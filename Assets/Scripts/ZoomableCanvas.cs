using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Ļauj tuvināt/attālināt un bīdīt kameru mašīnu spēlē.
//
// Problēma: Canvas ir "Screen Space - Camera" - tas vienmēr seko kamerai un aizpilda ekrānu,
// tāpēc kameras tālummaiņa (CameraZoomScript) nav redzama.
// Risinājums: spēles laikā Canvas tiek pārslēgts uz "World Space" (paliek tieši tajā pašā vietā),
// bet saskarnes elementi (taimeris, sirsniņas, Menu, uzvaras/zaudējuma logi) tiek pārcelti
// uz atsevišķu "HUD" Canvas, kas paliek uz ekrāna un netiek tuvināts.
//
// Pievieno šo skriptu galvenajam Canvas objektam CityScene ainā.
[RequireComponent(typeof(Canvas))]
public class ZoomableCanvas : MonoBehaviour
{
    [Tooltip("Objekti (Canvas tiešie bērni), kas paliek uz ekrāna. Salīdzina nosaukuma sākumu.")]
    public List<string> hudObjectNames = new List<string>
    {
        "Timer", "heart", "MenuButton", "WinPanel", "LosePanel"
    };

    [Tooltip("HUD kārtas numurs (virs spēles laukuma)")]
    public int hudSortingOrder = 10;

    IEnumerator Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas.renderMode == RenderMode.WorldSpace) yield break;

        // Pagaidām, līdz Canvas izmērs un mērogs aprēķināti kameras režīmā
        yield return null;
        Canvas.ForceUpdateCanvases();

        RectTransform hud = CreateHud(canvas);
        MoveHudObjects(hud);

        // Pārslēdzam uz pasaules telpu - RectTransform saglabā pašreizējo vietu un mērogu
        Camera cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam; // vajadzīgs, lai mašīnas varētu vilkt
    }

    RectTransform CreateHud(Canvas source)
    {
        GameObject go = new GameObject("HUD", typeof(RectTransform));
        Canvas hudCanvas = go.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = hudSortingOrder;

        // Tāda pati mērogošana kā galvenajam Canvas - elementi paliek tajās pašās vietās
        CanvasScaler src = source.GetComponent<CanvasScaler>();
        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        if (src != null)
        {
            scaler.uiScaleMode = src.uiScaleMode;
            scaler.referenceResolution = src.referenceResolution;
            scaler.screenMatchMode = src.screenMatchMode;
            scaler.matchWidthOrHeight = src.matchWidthOrHeight;
            scaler.referencePixelsPerUnit = src.referencePixelsPerUnit;
        }

        go.AddComponent<GraphicRaycaster>(); // lai pogas (Menu, Restart) strādātu
        return go.GetComponent<RectTransform>();
    }

    void MoveHudObjects(RectTransform hud)
    {
        // Kopija, jo bērnu saraksts mainās pārvietošanas laikā
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform) children.Add(child);

        foreach (Transform child in children)
        {
            string n = child.name.Trim();
            foreach (string hudName in hudObjectNames)
            {
                if (!n.StartsWith(hudName)) continue;

                // false = saglabā enkurus un pozīciju (nevis pasaules koordinātas)
                child.SetParent(hud, false);
                break;
            }
        }
    }
}
