using UnityEngine;
using UnityEngine.UI;

// Padara UI paneli (uzvara / zaudējums) vienmēr redzamu virs citiem elementiem tajā pašā Canvas -
// neatkarīgi no tā, kurā vietā hierarhijā tas atrodas un kādas mašīnas/bloki tiek pārvietoti.
public static class PanelOnTop
{
    public const int SortingOrder = 100;

    public static void Apply(GameObject panel)
    {
        if (panel == null) return;

        Canvas parentCanvas = panel.transform.parent != null
            ? panel.transform.parent.GetComponentInParent<Canvas>()
            : null;

        Canvas c = panel.GetComponent<Canvas>();
        if (c == null) c = panel.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = SortingOrder;
        if (parentCanvas != null) c.sortingLayerID = parentCanvas.sortingLayerID;

        // Lai pogas panelī joprojām reaģētu uz klikšķiem
        if (panel.GetComponent<GraphicRaycaster>() == null)
            panel.AddComponent<GraphicRaycaster>();
    }
}
