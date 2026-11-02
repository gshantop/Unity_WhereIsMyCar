using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ObjectTransformationScript : MonoBehaviour
{
    public float rotationSpeed = 90f;
    public float scaleSpeed = 0.5f;

    public float minFactor = 0.5f;
    public float maxFactor = 1.5f;

    [Header("Pogu nosaukumi ainā")]
    public string rotateCWButton = "Clokwise";
    public string rotateCCWButton = "Clokwise (1)";
    public string scaleUpYButton = "xUp";
    public string scaleDownYButton = "yUp";
    public string scaleUpXButton = "xDown";
    public string scaleDownXButton = "yDown";
    public string flipButton = "Button (Legacy)";

    private bool rotateCW, rotateCCW, scaleUpY, scaleDownY, scaleUpX, scaleDownX;
    public static bool isTransforming = false;

    private readonly Dictionary<GameObject, Vector2> baseScales = new Dictionary<GameObject, Vector2>();
    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
    private PointerEventData pointerData;
    private int[] lastTouchIds;

    void Update()
    {
        // Katru kadru paši pārbaudām, kura poga ir nospiesta (pirksts vai pele).
        // Tas strādā vienādi redaktorā un uz telefona, neatkarīgi no EventTrigger.
        ReadHeldButtons();
        CheckFlipPressed();

        if (GameManagerScript.GameOver || GameObjectsScript.lastDragged == null)
        {
            isTransforming = false;
            return;
        }

        GameObject obj = GameObjectsScript.lastDragged;
        RectTransform rc = obj.GetComponent<RectTransform>();
        if (rc == null)
        {
            isTransforming = false;
            return;
        }

        if (!baseScales.ContainsKey(obj))
        {
            Vector3 ls = rc.localScale;
            baseScales[obj] = new Vector2(
                Mathf.Max(0.01f, Mathf.Abs(ls.x)),
                Mathf.Max(0.01f, Mathf.Abs(ls.y)));
        }
        Vector2 b = baseScales[obj];

        float dt = Time.unscaledDeltaTime;

        float rot = (rotateCCW ? 1f : 0f) - (rotateCW ? 1f : 0f);
        if (rot != 0f)
        {
            rc.Rotate(0, 0, rot * rotationSpeed * dt);
        }

        float dx = (scaleUpX ? 1f : 0f) - (scaleDownX ? 1f : 0f);
        float dy = (scaleUpY ? 1f : 0f) - (scaleDownY ? 1f : 0f);

        if (dx != 0f || dy != 0f)
        {
            Vector3 s = rc.localScale;

            float signX = s.x < 0f ? -1f : 1f;
            float signY = s.y < 0f ? -1f : 1f;

            float absX = Mathf.Abs(s.x);
            float absY = Mathf.Abs(s.y);

            if (dx != 0f)
                absX = Mathf.Clamp(absX + dx * scaleSpeed * b.x * dt,
                                   b.x * minFactor, b.x * maxFactor);
            if (dy != 0f)
                absY = Mathf.Clamp(absY + dy * scaleSpeed * b.y * dt,
                                   b.y * minFactor, b.y * maxFactor);

            s.x = absX * signX;
            s.y = absY * signY;
            rc.localScale = s;
        }

        isTransforming =
            rotateCW || rotateCCW || scaleUpY || scaleDownY || scaleUpX || scaleDownX;
    }

    // Nosaka, kuras transformācijas pogas pašlaik tiek turētas
    private void ReadHeldButtons()
    {
        rotateCW = rotateCCW = scaleUpY = scaleDownY = scaleUpX = scaleDownX = false;

        if (EventSystem.current == null)
            return;

        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.isPressed)
                    ApplyButtonAt(touch.position.ReadValue());
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            ApplyButtonAt(Mouse.current.position.ReadValue());
    }

    private void ApplyButtonAt(Vector2 screenPos)
    {
        if (pointerData == null)
            pointerData = new PointerEventData(EventSystem.current);

        pointerData.position = screenPos;
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        if (raycastResults.Count == 0)
            return;

        // Augšējais UI elements zem pirksta (vai tā vecāks, ja trāpīts pogas bērnā)
        Transform t = raycastResults[0].gameObject.transform;
        for (int i = 0; i < 2 && t != null; i++, t = t.parent)
        {
            string n = t.name;

            if (n == rotateCWButton) { rotateCW = true; return; }
            if (n == rotateCCWButton) { rotateCCW = true; return; }
            if (n == scaleUpYButton) { scaleUpY = true; return; }
            if (n == scaleDownYButton) { scaleDownY = true; return; }
            if (n == scaleUpXButton) { scaleUpX = true; return; }
            if (n == scaleDownXButton) { scaleDownX = true; return; }
        }
    }

    // Apgriešanas poga: viens pieskāriens = viena apgriešana (pieskāriena sākumā).
    // Jaunu pieskārienu nosakām pēc touchId, tāpēc nepazūd arī ļoti ātri pieskārieni.
    private void CheckFlipPressed()
    {
        if (EventSystem.current == null)
            return;

        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            if (lastTouchIds == null || lastTouchIds.Length != touches.Count)
                lastTouchIds = new int[touches.Count];

            for (int i = 0; i < touches.Count; i++)
            {
                int id = touches[i].touchId.ReadValue();
                if (id != 0 && id != lastTouchIds[i])
                {
                    lastTouchIds[i] = id;
                    if (IsOverButton(touches[i].startPosition.ReadValue(), flipButton))
                        DoFlip();
                }
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
            IsOverButton(Mouse.current.position.ReadValue(), flipButton))
        {
            DoFlip();
        }
    }

    private bool IsOverButton(Vector2 screenPos, string buttonName)
    {
        if (pointerData == null)
            pointerData = new PointerEventData(EventSystem.current);

        pointerData.position = screenPos;
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        if (raycastResults.Count == 0)
            return false;

        Transform t = raycastResults[0].gameObject.transform;
        for (int i = 0; i < 2 && t != null; i++, t = t.parent)
        {
            if (t.name == buttonName)
                return true;
        }
        return false;
    }

    // Atstāta pogas On Click saitei ainā; apgriešanu tagad veic CheckFlipPressed
    // (citādi redaktorā mašīna apgrieztos divreiz un paliktu kā bija)
    public void FlipX() { }

    private void DoFlip()
    {
        if (GameManagerScript.GameOver)
            return;

        GameObject obj = GameObjectsScript.lastDragged;
        if (obj == null)
            return;

        RectTransform rc = obj.GetComponent<RectTransform>();
        if (rc == null)
            return;

        Vector3 s = rc.localScale;
        s.x = -s.x;
        rc.localScale = s;
    }

    // Atstātas, lai EventTrigger ainā neradītu kļūdas (pogas tagad nosaka Update)
    public void StartRotateCW(BaseEventData eventData) { }
    public void StopRotateCW(BaseEventData eventData) { }
    public void StartRotateCCW(BaseEventData eventData) { }
    public void StopRotateCCW(BaseEventData eventData) { }
    public void StartScaleUpY(BaseEventData eventData) { }
    public void StopScaleUpY(BaseEventData eventData) { }
    public void StartScaleDownY(BaseEventData eventData) { }
    public void StopScaleDownY(BaseEventData eventData) { }
    public void StartScaleUpX(BaseEventData eventData) { }
    public void StopScaleUpX(BaseEventData eventData) { }
    public void StartScaleDownX(BaseEventData eventData) { }
    public void StopScaleDownX(BaseEventData eventData) { }
}