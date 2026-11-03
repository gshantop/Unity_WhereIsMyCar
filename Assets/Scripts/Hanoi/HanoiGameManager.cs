using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Hanojas torņa spēles loģika.
// Stieņi ir uzzīmēti fona bildē (towerOfHanoi.png), tāpēc to vietas rēķinām no BackgroundImage.
// Bloku var pārvietot divos veidos (kā mašīnu spēlē - vilkt, vai ar pieskārieniem):
//   1) Vilkt augšējo bloku uz citu stieni un atlaist.
//   2) Pieskarties stienim (bloks paceļas), tad pieskarties citam stienim.
// Ievadi apstrādājam paši (bez EventSystem / On Click), lai telefonā nepazustu pieskārieni.
public class HanoiGameManager : MonoBehaviour
{
    [Header("Laukums")]
    [Tooltip("Ainas BackgroundImage ar towerOfHanoi.png")]
    public RectTransform background;

    [Tooltip("Bloki. Ja tukšs - tiek atrasti visi Canvas objekti, kuru nosaukums sākas ar \"block\"")]
    public List<HanoiBlock> blocks = new List<HanoiBlock>();

    [Header("Stieņu vietas fona bildē (0..1, no kreisās / no apakšas)")]
    public float[] pegU = { 249.5f / 1280f, 673.5f / 1280f, 1098.5f / 1280f };
    public float groundV = 46f / 600f;
    public float pegTopV = 497f / 600f;

    [Header("Zvaigžņu laika sliekšņi (sekundēs) - kā mašīnu spēlē")]
    public float threeStarTime = 90f;
    public float twoStarTime = 180f;

    [Header("Kustība")]
    public float moveStepDuration = 0.15f;
    [Tooltip("Cik pikseļus jāpabīda pirksts, lai sāktos vilkšana")]
    public float dragThresholdPixels = 25f;

    [Header("Teksti (nav obligāti). Ja tukši - meklē pēc nosaukuma")]
    [Tooltip("Ja tukšs - objekts \"Timer\" (kā CityScene)")]
    public TMP_Text timerText;
    public TMP_Text movesText;

    [Header("Pogas (On Click NEVAJAG - skripts pats nosaka pieskārienu)")]
    public RectTransform restartButton;
    [Tooltip("Ja tukšs - objekts, kura nosaukums sākas ar \"MenuButton\" (ne uzvaras logā)")]
    public RectTransform menuButton;

    [Header("Uzvaras logs (WinPanel no CityScene). Ja tukši - meklē pēc nosaukuma")]
    [Tooltip("Ja tukšs - objekts \"WinPanel\"")]
    public GameObject winPanel;
    [Tooltip("Ja tukšs - \"FinalTimeText\" WinPanel iekšā")]
    public TMP_Text finalTimeText;
    [Tooltip("Ja tukšs - \"star1\", \"star2\", \"star3\" WinPanel iekšā")]
    public GameObject[] stars = new GameObject[3];
    [Tooltip("Nav obligāts: teksts ar gājienu skaitu uzvaras logā")]
    public TMP_Text winMovesText;
    [Tooltip("Ja tukšs - \"Restart\" WinPanel iekšā")]
    public RectTransform winRestartButton;
    [Tooltip("Ja tukšs - \"MenuButton\" WinPanel iekšā")]
    public RectTransform winMenuButton;

    [Header("Skaņas")]
    public AudioSource musicSource;
    public AudioSource effectsSource;
    public AudioClip pickSound;     // onButton
    public AudioClip dropSound;     // drop
    public AudioClip clickSound;    // click
    public AudioClip victorySound;  // victory

    [Header("Ainas")]
    public int menuSceneIndex = 0;

    // ---- Stāvoklis ----
    private readonly List<HanoiBlock>[] pegs =
        { new List<HanoiBlock>(), new List<HanoiBlock>(), new List<HanoiBlock>() };
    private readonly List<HanoiBlock> activeBlocks = new List<HanoiBlock>();
    private Canvas canvas;
    private Camera uiCamera;

    private int moves;
    private float elapsed;
    private bool timerRunning;
    private bool won;
    private bool busy;              // notiek animācija
    private bool leavingScene;

    // Pacelts bloks (pieskāriena režīmā) vai vilkts bloks
    private HanoiBlock heldBlock;
    private int heldFromPeg = -1;
    private bool selectedByTap;

    // Pirksts / pele
    private bool pointerDown;
    private bool dragging;
    private Vector2 pressPos;
    private Vector3 dragOffset;
    private int lastTouchId;

    // ===================== Sākums =====================

    void Awake()
    {
        if (background == null)
        {
            GameObject bg = GameObject.Find("BackgroundImage");
            if (bg != null) background = bg.GetComponent<RectTransform>();
        }

        if (background == null)
        {
            Debug.LogError("HanoiGameManager: nav norādīts Background (BackgroundImage)!");
            enabled = false;
            return;
        }

        canvas = background.GetComponentInParent<Canvas>().rootCanvas;
        uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        FindBlocks();
        FindUi();
        PanelOnTop.Apply(winPanel);

        if (Touchscreen.current != null)
            lastTouchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
    }

    void OnEnable()
    {
        RewardedAds.OnRewardGranted += OnRewardGranted;
    }

    void OnDisable()
    {
        RewardedAds.OnRewardGranted -= OnRewardGranted;
    }

    IEnumerator Start()
    {
        // Pagaidām vienu kadru, līdz Canvas mērogs un izkārtojums ir gatavs
        yield return null;
        Canvas.ForceUpdateCanvases();
        NewGame();
    }

    void FindBlocks()
    {
        blocks.RemoveAll(b => b == null);

        if (blocks.Count == 0)
        {
            foreach (Image img in background.parent.GetComponentsInChildren<Image>(true))
            {
                if (!img.name.ToLower().StartsWith("block")) continue;

                HanoiBlock b = img.GetComponent<HanoiBlock>();
                if (b == null) b = img.gameObject.AddComponent<HanoiBlock>();
                blocks.Add(b);
            }
        }

        // Kārtojam pēc redzamā platuma: 0 = mazākais
        blocks.Sort((a, b) => a.WorldWidth.CompareTo(b.WorldWidth));
        for (int i = 0; i < blocks.Count; i++)
            blocks[i].size = i;

        if (blocks.Count == 0)
            Debug.LogError("HanoiGameManager: nav atrasts neviens bloks!");
    }

    // Tukšos laukus aizpildām pēc objektu nosaukumiem (kā CityScene)
    void FindUi()
    {
        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (winPanel == null)
        {
            Transform t = FindByName(all, "WinPanel", null);
            if (t != null) winPanel = t.gameObject;
        }
        Transform win = winPanel != null ? winPanel.transform : null;

        if (finalTimeText == null)
        {
            Transform t = FindByName(all, "FinalTimeText", win);
            if (t != null) finalTimeText = t.GetComponent<TMP_Text>();
        }

        if (stars == null || stars.Length != 3) stars = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            if (stars[i] != null) continue;
            Transform t = FindByName(all, "star" + (i + 1), win);
            if (t != null) stars[i] = t.gameObject;
        }

        if (winRestartButton == null) winRestartButton = FindByName(all, "Restart", win) as RectTransform;
        if (winMenuButton == null) winMenuButton = FindByName(all, "MenuButton", win, true) as RectTransform;

        if (timerText == null)
        {
            Transform t = FindByName(all, "Timer", null);
            if (t != null) timerText = t.GetComponent<TMP_Text>();
        }

        if (menuButton == null)
        {
            foreach (Transform t in all)
            {
                if (!t.name.Trim().StartsWith("MenuButton")) continue;
                if (win != null && t.IsChildOf(win)) continue;
                menuButton = t as RectTransform;
                break;
            }
        }
    }

    // Meklē objektu pēc nosaukuma; ja "parent" norādīts - tikai tā iekšā, citādi - ārpus WinPanel
    Transform FindByName(Transform[] all, string objName, Transform parent, bool startsWith = false)
    {
        Transform win = winPanel != null ? winPanel.transform : null;

        foreach (Transform t in all)
        {
            string n = t.name.Trim();
            bool match = startsWith ? n.StartsWith(objName) : n == objName;
            if (!match) continue;

            if (parent != null && !t.IsChildOf(parent)) continue;
            if (parent == null && win != null && t != win && t.IsChildOf(win)) continue;
            return t;
        }
        return null;
    }

    // ===================== Stieņu ģeometrija =====================

    Vector3 BgPoint(float u, float v)
    {
        Rect r = background.rect;
        return background.TransformPoint(new Vector3(r.xMin + u * r.width, r.yMin + v * r.height, 0f));
    }

    float PegWorldX(int peg) => BgPoint(pegU[peg], 0f).x;
    float GroundWorldY => BgPoint(0f, groundV).y;
    float PegTopWorldY => BgPoint(0f, pegTopV).y;
    float PlaneZ => background.position.z;

    // Bloka centrs, ja tas stāv stienī norādītajā līmenī
    Vector3 StackCenter(int peg, int level, HanoiBlock block)
    {
        float y = GroundWorldY;
        List<HanoiBlock> stack = pegs[peg];
        for (int i = 0; i < level && i < stack.Count; i++)
            y += stack[i].WorldHeight;

        return new Vector3(PegWorldX(peg), y + block.WorldHeight / 2f, PlaneZ);
    }

    // Paceltā bloka vieta (stieņa augšā)
    Vector3 HoverCenter(int peg, HanoiBlock block)
    {
        return new Vector3(PegWorldX(peg), PegTopWorldY - block.WorldHeight * 0.5f, PlaneZ);
    }

    // Augstums, pa kuru bloks pārlido virs stieņiem
    Vector3 TravelPoint(int peg, HanoiBlock block)
    {
        return new Vector3(PegWorldX(peg), PegTopWorldY + block.WorldHeight * 0.7f, PlaneZ);
    }

    // ===================== Spēle =====================

    public void NewGame()
    {
        StopAllCoroutines();
        busy = false;
        won = false;
        timerRunning = false;
        moves = 0;
        elapsed = 0f;
        heldBlock = null;
        heldFromPeg = -1;
        selectedByTap = false;
        pointerDown = false;
        dragging = false;

        if (winPanel != null) winPanel.SetActive(false);
        if (musicSource != null && musicSource.clip != null && !musicSource.isPlaying) musicSource.Play();

        foreach (List<HanoiBlock> p in pegs) p.Clear();
        activeBlocks.Clear();

        // Visi bloki uz kreisā stieņa, lielākais apakšā
        for (int i = blocks.Count - 1; i >= 0; i--)
        {
            HanoiBlock b = blocks[i];
            b.gameObject.SetActive(true);
            b.SetLifted(false);
            b.peg = 0;
            pegs[0].Add(b);
            activeBlocks.Add(b);
            b.SetCenter(StackCenter(0, pegs[0].Count - 1, b));
        }

        UpdateHud();
    }

    public void GoToMenu()
    {
        if (leavingScene) return;
        leavingScene = true;
        PlaySound(clickSound);
        AdManager.LoadScene(menuSceneIndex);
    }

    HanoiBlock Top(int peg)
    {
        List<HanoiBlock> p = pegs[peg];
        return p.Count > 0 ? p[p.Count - 1] : null;
    }

    bool CanPlace(HanoiBlock block, int peg)
    {
        HanoiBlock top = Top(peg);
        return top == null || top == block || top.size > block.size;
    }

    // Paņem augšējo bloku no stieņa
    void Pick(int peg)
    {
        heldBlock = Top(peg);
        heldFromPeg = peg;
        heldBlock.SetLifted(true);
        PlaySound(pickSound);
    }

    // Noliek paņemto bloku uz stieņa (vai atpakaļ, ja nedrīkst)
    void Place(int targetPeg)
    {
        HanoiBlock block = heldBlock;
        int from = heldFromPeg;
        bool invalid = targetPeg >= 0 && targetPeg != from && !CanPlace(block, targetPeg);

        // Pieskāriena režīmā nepareizs mērķis: bloks paliek pacelts, var izvēlēties citu stieni
        if (invalid && selectedByTap)
        {
            block.ShowError();
            return;
        }

        heldBlock = null;
        heldFromPeg = -1;
        selectedByTap = false;

        if (invalid || targetPeg < 0 || targetPeg == from)
        {
            // Atpakaļ uz savu stieni
            if (invalid) block.ShowError();
            StartCoroutine(AnimateTo(block, false, StackCenter(from, pegs[from].Count - 1, block)));
            return;
        }

        pegs[from].Remove(block);
        pegs[targetPeg].Add(block);
        block.peg = targetPeg;
        moves++;
        timerRunning = true;
        UpdateHud();

        Vector3 end = StackCenter(targetPeg, pegs[targetPeg].Count - 1, block);
        if (dragging)
            StartCoroutine(AnimateTo(block, true, end));
        else
            StartCoroutine(AnimateTo(block, true,
                TravelPoint(from, block), TravelPoint(targetPeg, block), end));
    }

    IEnumerator AnimateTo(HanoiBlock block, bool countsAsMove, params Vector3[] points)
    {
        busy = true;
        yield return block.MoveThrough(moveStepDuration, points);
        block.SetLifted(false);
        PlaySound(dropSound);
        busy = false;

        if (countsAsMove) CheckWin();
    }

    void CheckWin()
    {
        // Uzvara, ja viss tornis pārvietots uz vidējo VAI labējo stieni
        if (pegs[1].Count != activeBlocks.Count && pegs[2].Count != activeBlocks.Count) return;

        won = true;
        timerRunning = false;

        // Zvaigznes pēc laika (kā mašīnu spēlē)
        int starCount = elapsed <= threeStarTime ? 3 : (elapsed <= twoStarTime ? 2 : 1);

        for (int i = 0; i < stars.Length; i++)
            if (stars[i] != null) stars[i].SetActive(i < starCount);

        if (finalTimeText != null) finalTimeText.text = GameManagerScript.FormatTime(elapsed);
        if (winMovesText != null) winMovesText.text = "Gājieni: " + moves;

        if (musicSource != null) musicSource.Stop();
        PlaySound(victorySound);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            winPanel.transform.SetAsLastSibling();
        }
    }

    void UpdateHud()
    {
        if (movesText != null) movesText.text = "Gājieni: " + moves;
        if (timerText != null) timerText.text = GameManagerScript.FormatTime(elapsed);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && effectsSource != null) effectsSource.PlayOneShot(clip);
    }

    // ===================== Reklāmas bonuss: padoms =====================

    // Pēc noskatītas apbalvojošās reklāmas spēle pati izdara nākamo pareizo gājienu
    void OnRewardGranted()
    {
        if (won || activeBlocks.Count == 0) return;
        StartCoroutine(HintMove());
    }

    IEnumerator HintMove()
    {
        while (busy) yield return null;

        // Ja bloks bija pacelts - nolikt atpakaļ
        if (heldBlock != null)
        {
            HanoiBlock b = heldBlock;
            int from = heldFromPeg;
            heldBlock = null;
            heldFromPeg = -1;
            selectedByTap = false;
            dragging = false;
            pointerDown = false;
            yield return AnimateTo(b, false, StackCenter(from, pegs[from].Count - 1, b));
        }

        int[] pos = new int[activeBlocks.Count];
        for (int p = 0; p < 3; p++)
            foreach (HanoiBlock b in pegs[p]) pos[b.size] = p;

        Vector2Int? move = NextOptimalMove(pos, activeBlocks.Count - 1, 2);
        if (!move.HasValue) yield break;

        Pick(move.Value.x);
        Place(move.Value.y);
    }

    // Optimālais nākamais gājiens no jebkura stāvokļa: bloki 0..d jānovieto uz "target"
    Vector2Int? NextOptimalMove(int[] pos, int d, int target)
    {
        if (d < 0) return null;
        if (pos[d] == target) return NextOptimalMove(pos, d - 1, target);

        int aux = 3 - pos[d] - target;
        Vector2Int? smaller = NextOptimalMove(pos, d - 1, aux);
        return smaller ?? new Vector2Int(pos[d], target);
    }

    // ===================== Ievade =====================

    void Update()
    {
        if (timerRunning && !won)
        {
            elapsed += Time.unscaledDeltaTime;
            if (timerText != null) timerText.text = GameManagerScript.FormatTime(elapsed);
        }

        ReadPointer();
    }

    void ReadPointer()
    {
        // ---- Telefons ----
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            int id = touch.touchId.ReadValue();
            bool pressed = touch.press.isPressed;

            if (id != 0 && id != lastTouchId)
            {
                // Jauns pieskāriens (nepazūd arī ļoti ātrs pieskāriens)
                lastTouchId = id;
                OnPress(touch.startPosition.ReadValue());

                if (!pressed)
                {
                    OnRelease(touch.position.ReadValue());
                    return;
                }
            }

            if (pointerDown)
            {
                if (pressed) OnHold(touch.position.ReadValue());
                else OnRelease(touch.position.ReadValue());
                return;
            }
        }

        // ---- Pele (redaktorā / datorā) ----
        if (Mouse.current != null)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            if (Mouse.current.leftButton.wasPressedThisFrame) OnPress(pos);
            else if (pointerDown && Mouse.current.leftButton.isPressed) OnHold(pos);
            else if (pointerDown && Mouse.current.leftButton.wasReleasedThisFrame) OnRelease(pos);
        }
    }

    void OnPress(Vector2 screenPos)
    {
        pointerDown = false;
        dragging = false;

        // 1) Pogas
        if (HandleButtons(screenPos)) return;

        if (won || busy) return;

        int peg = PegAt(screenPos);

        // 2) Bloks jau pacelts ar pieskārienu - šis pieskāriens izvēlas mērķi
        if (selectedByTap && heldBlock != null)
        {
            if (peg >= 0) Place(peg);
            return;
        }

        // 3) Paņemam augšējo bloku
        if (peg < 0 || Top(peg) == null) return;

        Pick(peg);
        pointerDown = true;
        pressPos = screenPos;
        dragOffset = heldBlock.WorldCenter - ScreenToWorld(screenPos);
    }

    void OnHold(Vector2 screenPos)
    {
        if (heldBlock == null) { pointerDown = false; return; }

        if (!dragging && (screenPos - pressPos).magnitude > dragThresholdPixels)
            dragging = true;

        if (dragging)
            heldBlock.SetCenter(ScreenToWorld(screenPos) + dragOffset);
    }

    void OnRelease(Vector2 screenPos)
    {
        pointerDown = false;
        if (heldBlock == null) return;

        if (dragging)
        {
            // Vilkšana: noliekam uz stieņa, virs kura atlaida
            Place(PegAtX(ScreenToWorld(screenPos).x));
            dragging = false;
        }
        else
        {
            // Pieskāriens: bloks paceļas stieņa augšā un gaida nākamo pieskārienu
            selectedByTap = true;
            StartCoroutine(LiftAnimation(heldBlock, heldFromPeg));
        }
    }

    IEnumerator LiftAnimation(HanoiBlock block, int peg)
    {
        busy = true;
        yield return block.MoveThrough(moveStepDuration, HoverCenter(peg, block));
        busy = false;
    }

    bool HandleButtons(Vector2 screenPos)
    {
        bool winOpen = winPanel != null && winPanel.activeSelf;

        if (winOpen)
        {
            if (Hit(winRestartButton, screenPos)) { PlaySound(clickSound); NewGame(); return true; }
            if (Hit(winMenuButton, screenPos)) { GoToMenu(); return true; }
            return true; // kamēr atvērts uzvaras logs, citas pogas nestrādā
        }

        if (Hit(menuButton, screenPos)) { GoToMenu(); return true; }
        if (Hit(restartButton, screenPos)) { PlaySound(clickSound); NewGame(); return true; }
        return false;
    }

    static bool Hit(RectTransform rt, Vector2 screenPos)
    {
        if (rt == null || !rt.gameObject.activeInHierarchy) return false;

        Canvas c = rt.GetComponentInParent<Canvas>();
        Camera cam = null;
        if (c != null)
        {
            c = c.rootCanvas;
            cam = c.renderMode == RenderMode.ScreenSpaceOverlay ? null : c.worldCamera;
        }
        return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam);
    }

    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 world;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(background, screenPos, uiCamera, out world);
        return world;
    }

    // Stienis zem pieskāriena (vai -1)
    int PegAt(Vector2 screenPos)
    {
        Vector3 w = ScreenToWorld(screenPos);
        float margin = (PegTopWorldY - GroundWorldY) * 0.15f;
        if (w.y < GroundWorldY - margin || w.y > PegTopWorldY + margin * 2f) return -1;
        return PegAtX(w.x);
    }

    // Tuvākais stienis pēc X (vai -1, ja pārāk tālu)
    int PegAtX(float worldX)
    {
        int best = -1;
        float bestDist = float.MaxValue;
        for (int i = 0; i < 3; i++)
        {
            float d = Mathf.Abs(worldX - PegWorldX(i));
            if (d < bestDist) { bestDist = d; best = i; }
        }

        float halfSpacing = Mathf.Abs(PegWorldX(1) - PegWorldX(0)) / 2f;
        return bestDist <= halfSpacing ? best : -1;
    }
}