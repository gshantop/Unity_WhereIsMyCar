using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Viens Hanojas torņa bloks (UI Image ainā).
// Pievienojas automātiski visiem objektiem, kuru nosaukums sākas ar "block",
// bet to var pievienot arī ar roku.
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class HanoiBlock : MonoBehaviour
{
    [Tooltip("Izmērs: 0 = mazākais bloks. Aizpilda HanoiGameManager pēc bloka platuma.")]
    public int size;

    [HideInInspector] public int peg;

    public RectTransform Rect { get; private set; }
    public Image Image { get; private set; }

    private Vector3 baseScale;
    private Color baseColor;
    private Coroutine moveRoutine;

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
        Image = GetComponent<Image>();
        baseScale = Rect.localScale;
        baseColor = Image.color;
    }

    // Redzamais bloka izmērs lokālajās vienībās.
    // Ja ieslēgts "Preserve Aspect", bilde aizņem tikai daļu no RectTransform.
    public Vector2 VisualLocalSize
    {
        get
        {
            Vector2 box = Rect.rect.size;
            Sprite sprite = Image.sprite;
            if (!Image.preserveAspect || sprite == null)
                return box;

            float aspect = sprite.rect.width / sprite.rect.height;
            if (box.x / box.y > aspect)
                return new Vector2(box.y * aspect, box.y);
            return new Vector2(box.x, box.x / aspect);
        }
    }

    // Mērogs bez "pacelšanas" palielinājuma
    Vector3 ParentScale => Rect.parent != null ? Rect.parent.lossyScale : Vector3.one;

    // Redzamais augstums pasaules koordinātās
    public float WorldHeight => VisualLocalSize.y * ParentScale.y * baseScale.y;

    // Redzamais platums (pēc tā nosaka, kurš bloks ir lielāks)
    public float WorldWidth => VisualLocalSize.x * ParentScale.x * baseScale.x;

    // Bloka redzamā centra nobīde no transform.position
    Vector3 CenterOffset => Rect.TransformPoint(Rect.rect.center) - Rect.position;

    public Vector3 WorldCenter => Rect.position + CenterOffset;

    // Uzreiz noliek bloku tā, lai tā centrs būtu norādītajā punktā
    public void SetCenter(Vector3 worldCenter)
    {
        StopMove();
        Rect.position = worldCenter - CenterOffset;
    }

    // Pārvieto bloku pa punktiem (pasaules koordinātās, bloka centrs)
    public Coroutine MoveThrough(float durationPerStep, params Vector3[] centers)
    {
        StopMove();
        moveRoutine = StartCoroutine(MoveRoutine(durationPerStep, centers));
        return moveRoutine;
    }

    public bool IsMoving => moveRoutine != null;

    void StopMove()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }

    IEnumerator MoveRoutine(float durationPerStep, Vector3[] centers)
    {
        foreach (Vector3 target in centers)
        {
            Vector3 from = WorldCenter;
            if ((from - target).sqrMagnitude < 0.0001f) continue;

            float t = 0f;
            while (t < 1f)
            {
                // unscaledDeltaTime - strādā arī tad, ja reklāma mainījusi Time.timeScale
                t += Time.unscaledDeltaTime / Mathf.Max(0.01f, durationPerStep);
                Rect.position = Vector3.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t)) - CenterOffset;
                yield return null;
            }
            Rect.position = target - CenterOffset;
        }
        moveRoutine = null;
    }

    // Pacelts bloks - nedaudz lielāks un virs citiem
    public void SetLifted(bool lifted)
    {
        Rect.localScale = lifted ? baseScale * 1.08f : baseScale;
        if (lifted) Rect.SetAsLastSibling();
    }

    // Nepareizs gājiens - bloks nosarkst un pakratās
    public void ShowError()
    {
        StartCoroutine(ErrorRoutine());
    }

    IEnumerator ErrorRoutine()
    {
        Image.color = new Color(1f, 0.45f, 0.45f, baseColor.a);
        Vector3 start = Rect.position;
        Vector3 expected = start;
        float shake = 10f * ParentScale.x;
        bool interrupted = false;

        for (int i = 0; i < 6; i++)
        {
            // Ja bloku tikmēr pārvieto kas cits - vairs nekratām, tikai nosarkst
            if (moveRoutine != null || Rect.position != expected)
                interrupted = true;

            if (!interrupted)
            {
                expected = start + new Vector3(i % 2 == 0 ? shake : -shake, 0, 0);
                Rect.position = expected;
            }
            yield return new WaitForSecondsRealtime(0.05f);
        }

        if (!interrupted && Rect.position == expected)
            Rect.position = start;
        Image.color = baseColor;
    }
}
