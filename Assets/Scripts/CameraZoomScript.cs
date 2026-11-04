using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

// Kartes tuvināšana / attālināšana un bīdīšana mašīnu spēlē.
//
// Canvas ir "Screen Space - Camera", tāpēc kameras tālummaiņa nav redzama.
// Tāpēc tuvinām nevis kameru, bet pašu karti: spēles sākumā visi spēles objekti
// (fons, mašīnas, vietas, mākoņi, lidmašīnas) tiek ielikti objektā "World", ko mērogojam un bīdām.
// Taimeris, sirsniņas, Menu un uzvaras/zaudējuma logi paliek uz vietas.
//
// Telefons: 2 pirksti - tuvināt/attālināt un bīdīt, 1 pirksts pa tukšu vietu - bīdīt,
//           dubultskāriens - atpakaļ uz pilnu skatu.
// Dators:   ritenītis vai +/- - tuvināt/attālināt, labā/vidējā poga vai kreisā pa tukšu vietu - bīdīt.
// Ja karte pietuvināta un velk mašīnu pie ekrāna malas - karte pati bīdās līdzi.
public class CameraZoomScript : MonoBehaviour
{
    [Header("Tuvināšana")]
    [Tooltip("Cik reizes var pietuvināt")]
    public float maxZoomFactor = 3f;
    [Range(0.05f, 0.4f)] public float scrollZoomStep = 0.15f;

    [Tooltip("Rāda ekrānā ritenīša vērtības (pārbaudei emulatorā)")]
    public bool showScrollDebug = false;

    [Header("Kartes bīdīšana, velkot mašīnu pie malas")]
    [Range(0.05f, 0.3f)] public float edgeZone = 0.12f;     // malas platums (daļa no ekrāna)
    public float edgeScrollSpeed = 1200f;                   // vienības sekundē

    [Header("Dubultskāriens")]
    public float doubleTapMaxDelay = 0.35f;
    public float doubleTapMaxDistance = 80f;

    [Header("Objekti, kas NEtiek tuvināti (nosaukuma sākums)")]
    public List<string> hudObjectNames = new List<string>
    {
        "Timer", "heart", "MenuButton", "WinPanel", "LosePanel"
    };

    RectTransform canvasRect;
    Camera uiCamera;
    RectTransform world;
    float zoom = 1f;
    bool ready;

    bool panning;
    Vector2 lastPanPos;
    int lastTouchCount;
    int lastTouchId;
    float lastPinchDistance;
    Vector2 lastPinchMid;
    float lastTapTime = -1f;
    Vector2 lastTapPos;
    Coroutine resetRoutine;

    readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    // Ritenītis no notikumiem (Android emulatorā vērtība dažreiz pazūd, ja lasa tikai kadra stāvokli)
    float pendingScroll;
    string scrollDebug = "";

    void OnEnable()
    {
        InputSystem.onEvent += OnInputEvent;
    }

    void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
    }

    void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        Mouse mouse = device as Mouse;
        if (mouse == null) return;
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()) return;

        if (mouse.scroll.y.ReadValueFromEvent(eventPtr, out float v) && Mathf.Abs(v) > 0.0001f)
            pendingScroll += v;
    }

    IEnumerator Start()
    {
        GameObject bg = GameObject.Find("BackgroundImage");
        if (bg == null) { Debug.LogError("CameraZoomScript: nav atrasts BackgroundImage"); yield break; }

        Canvas canvas = bg.GetComponentInParent<Canvas>().rootCanvas;
        canvasRect = canvas.GetComponent<RectTransform>();
        uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        // Pagaidām, līdz citi skripti (GameObjectsScript u.c.) ir salikuši mašīnas
        yield return null;
        Canvas.ForceUpdateCanvases();

        CreateWorld();
        ready = true;
    }

    // Pārliek visus spēles objektus objektā "World" (izskats nemainās)
    void CreateWorld()
    {
        GameObject go = new GameObject("World", typeof(RectTransform));
        world = go.GetComponent<RectTransform>();
        world.SetParent(canvasRect, false);
        world.anchorMin = Vector2.zero;
        world.anchorMax = Vector2.one;
        world.offsetMin = Vector2.zero;
        world.offsetMax = Vector2.zero;
        world.pivot = new Vector2(0.5f, 0.5f);
        world.SetAsFirstSibling(); // HUD paliek virs kartes

        List<Transform> children = new List<Transform>();
        foreach (Transform child in canvasRect) if (child != world) children.Add(child);

        foreach (Transform child in children)
        {
            if (IsHud(child.name)) continue;
            child.SetParent(world, true);
        }
    }

    bool IsHud(string objName)
    {
        string n = objName.Trim();
        foreach (string h in hudObjectNames)
            if (n.StartsWith(h)) return true;
        return false;
    }

    void Update()
    {
        if (!ready) return;

        HandleScroll();

        if (Touchscreen.current != null && ActiveTouches(out Vector2 p0, out Vector2 p1) > 0)
            HandleTouch(p0, p1);
        else
        {
            lastTouchCount = 0;
            HandleMouse();
        }

        EdgeScrollWhileDragging();
    }

    // ===================== Ievade =====================

    int ActiveTouches(out Vector2 p0, out Vector2 p1)
    {
        p0 = p1 = Vector2.zero;
        int count = 0;
        foreach (var t in Touchscreen.current.touches)
        {
            if (!t.press.isPressed) continue;
            if (count == 0) p0 = t.position.ReadValue();
            else if (count == 1) p1 = t.position.ReadValue();
            count++;
        }
        return count;
    }

    bool Blocked => GameObjectsScript.isDragging || ObjectTransformationScript.isTransforming;

    void HandleTouch(Vector2 p0, Vector2 p1)
    {
        int count = Touchscreen.current.touches.Count > 0 ? ActiveTouches(out p0, out p1) : 0;

        if (count >= 2)
        {
            if (!Blocked)
            {
                float dist = Vector2.Distance(p0, p1);
                Vector2 mid = (p0 + p1) / 2f;

                if (lastTouchCount >= 2 && lastPinchDistance > 1f)
                {
                    StopReset();
                    ZoomAt(lastPinchMid, zoom * dist / lastPinchDistance);
                    Pan(mid - lastPinchMid);
                }
                lastPinchDistance = dist;
                lastPinchMid = mid;
            }
            panning = false;
            lastTouchCount = count;
            return;
        }

        int id = Touchscreen.current.primaryTouch.touchId.ReadValue();
        if (lastTouchCount == 0 || id != lastTouchId)
        {
            lastTouchId = id;
            lastPanPos = p0;
            panning = !Blocked && IsEmptySpace(p0);
            if (panning) CheckDoubleTap(p0);
        }
        else if (panning && !Blocked)
        {
            Pan(p0 - lastPanPos);
            lastPanPos = p0;
        }
        lastTouchCount = 1;
    }

    void HandleMouse()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;
        Vector2 pos = mouse.position.ReadValue();

        if (mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame ||
            (mouse.leftButton.wasPressedThisFrame && IsEmptySpace(pos)))
        {
            panning = !Blocked;
            lastPanPos = pos;
            if (panning && mouse.leftButton.wasPressedThisFrame) CheckDoubleTap(pos);
        }

        if (!mouse.leftButton.isPressed && !mouse.rightButton.isPressed && !mouse.middleButton.isPressed)
            panning = false;

        if (panning && !Blocked)
        {
            Pan(pos - lastPanPos);
            lastPanPos = pos;
        }
    }

    // Ritenītis uz augšu / "+" - tuvina; uz leju / "-" - attālina.
    // Vērtību ņemam no vairākiem avotiem, jo Android emulatorā katrs strādā citādi.
    void HandleScroll()
    {
        float fromEvents = pendingScroll;
        pendingScroll = 0f;

        float fromState = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
        float fromLegacy = 0f;
#if ENABLE_LEGACY_INPUT_MANAGER
        fromLegacy = Input.mouseScrollDelta.y;
        if (Mathf.Abs(fromLegacy) < 0.0001f) fromLegacy = Input.GetAxis("Mouse ScrollWheel");
#endif
        // Ņemam spēcīgāko signālu
        float scroll = fromEvents;
        if (Mathf.Abs(fromState) > Mathf.Abs(scroll)) scroll = fromState;
        if (Mathf.Abs(fromLegacy) > Mathf.Abs(scroll)) scroll = fromLegacy;

        if (showScrollDebug && (fromEvents != 0f || fromState != 0f || fromLegacy != 0f))
            scrollDebug = $"events: {fromEvents:0.###}  state: {fromState:0.###}  legacy: {fromLegacy:0.###}";

        Vector2 pos = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (Keyboard.current != null)
        {
            if (Keyboard.current.equalsKey.wasPressedThisFrame || Keyboard.current.numpadPlusKey.wasPressedThisFrame)
                scroll = 1f;
            if (Keyboard.current.minusKey.wasPressedThisFrame || Keyboard.current.numpadMinusKey.wasPressedThisFrame)
                scroll = -1f;
        }

        if (Mathf.Abs(scroll) < 0.0001f) return;

        // Ja kursora pozīcija nav zināma (emulators) - tuvinām uz ekrāna centru
        if (pos.x <= 0f && pos.y <= 0f || pos.x > Screen.width || pos.y > Screen.height)
            pos = new Vector2(Screen.width / 2f, Screen.height / 2f);

        StopReset();
        float factor = scroll > 0f ? 1f / (1f - scrollZoomStep) : 1f - scrollZoomStep;
        ZoomAt(pos, zoom * factor);
    }

    void OnGUI()
    {
        if (!showScrollDebug || string.IsNullOrEmpty(scrollDebug)) return;
        GUI.Label(new Rect(20, Screen.height - 60, Screen.width, 50), scrollDebug,
            new GUIStyle(GUI.skin.label) { fontSize = 32, normal = { textColor = Color.red } });
    }

    void CheckDoubleTap(Vector2 pos)
    {
        if (Time.unscaledTime - lastTapTime <= doubleTapMaxDelay &&
            Vector2.Distance(pos, lastTapPos) <= doubleTapMaxDistance)
        {
            ResetView();
            lastTapTime = -1f;
            panning = false;
        }
        else
        {
            lastTapTime = Time.unscaledTime;
            lastTapPos = pos;
        }
    }

    // Velkot mašīnu pie ekrāna malas, karte bīdās līdzi (tikai, ja pietuvināts)
    void EdgeScrollWhileDragging()
    {
        if (!GameObjectsScript.isDragging || zoom <= 1.01f) return;

        Vector2 pos;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            pos = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null)
            pos = Mouse.current.position.ReadValue();
        else return;

        float zoneX = Screen.width * edgeZone;
        float zoneY = Screen.height * edgeZone;
        Vector2 dir = Vector2.zero;

        if (pos.x < zoneX) dir.x = 1f - pos.x / zoneX;                       // pa kreisi
        else if (pos.x > Screen.width - zoneX) dir.x = -(1f - (Screen.width - pos.x) / zoneX);
        if (pos.y < zoneY) dir.y = 1f - pos.y / zoneY;
        else if (pos.y > Screen.height - zoneY) dir.y = -(1f - (Screen.height - pos.y) / zoneY);

        if (dir == Vector2.zero) return;

        // Karte bīdās pretēji pirkstam - tātad "skats" iet pirksta virzienā
        world.anchoredPosition += dir * edgeScrollSpeed * Time.unscaledDeltaTime;
        ClampWorld();
    }

    // ===================== Tuvināšana / bīdīšana =====================

    Vector2 ScreenToCanvas(Vector2 screen)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, uiCamera, out Vector2 local);
        return local;
    }

    // Tuvina tā, lai punkts zem kursora/pirkstiem paliek uz vietas
    void ZoomAt(Vector2 screenPos, float newZoom)
    {
        newZoom = Mathf.Clamp(newZoom, 1f, maxZoomFactor);
        Vector2 anchor = ScreenToCanvas(screenPos);
        Vector2 pos = world.anchoredPosition;
        Vector2 pointInWorld = (anchor - pos) / zoom;

        zoom = newZoom;
        world.localScale = new Vector3(zoom, zoom, 1f);
        world.anchoredPosition = anchor - pointInWorld * zoom;
        ClampWorld();
    }

    void Pan(Vector2 screenDelta)
    {
        if (screenDelta == Vector2.zero) return;
        Vector2 a = ScreenToCanvas(Vector2.zero);
        Vector2 b = ScreenToCanvas(screenDelta);
        world.anchoredPosition += b - a;
        ClampWorld();
    }

    // Karte nedrīkst atstāt tukšu vietu ekrānā
    void ClampWorld()
    {
        Vector2 size = canvasRect.rect.size;
        float maxX = size.x * (zoom - 1f) / 2f;
        float maxY = size.y * (zoom - 1f) / 2f;
        Vector2 p = world.anchoredPosition;
        world.anchoredPosition = new Vector2(Mathf.Clamp(p.x, -maxX, maxX), Mathf.Clamp(p.y, -maxY, maxY));
    }

    public void ResetView()
    {
        StopReset();
        resetRoutine = StartCoroutine(ResetRoutine());
    }

    void StopReset()
    {
        if (resetRoutine != null) { StopCoroutine(resetRoutine); resetRoutine = null; }
    }

    IEnumerator ResetRoutine()
    {
        float startZoom = zoom;
        Vector2 startPos = world.anchoredPosition;

        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.25f)
        {
            float k = Mathf.SmoothStep(0f, 1f, t);
            zoom = Mathf.Lerp(startZoom, 1f, k);
            world.localScale = new Vector3(zoom, zoom, 1f);
            world.anchoredPosition = Vector2.Lerp(startPos, Vector2.zero, k);
            yield return null;
        }

        zoom = 1f;
        world.localScale = Vector3.one;
        world.anchoredPosition = Vector2.zero;
        resetRoutine = null;
    }

    // Tukša vieta = nav mašīnas, pogas vai ekrāna (HUD) elementa zem pirksta
    bool IsEmptySpace(Vector2 screenPos)
    {
        if (EventSystem.current == null) return true;

        PointerEventData data = new PointerEventData(EventSystem.current) { position = screenPos };
        raycastResults.Clear();
        EventSystem.current.RaycastAll(data, raycastResults);

        foreach (RaycastResult r in raycastResults)
        {
            GameObject go = r.gameObject;
            if (go.GetComponentInParent<DragAndDropScript>() != null) return false;
            if (go.GetComponentInParent<Selectable>() != null) return false;
            if (world != null && !go.transform.IsChildOf(world)) return false; // HUD, pogas
        }
        return true;
    }
}