using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ObjectTransformationScript : MonoBehaviour
{
    public float rotationSpeed = 90f;
    public float scaleSpeed = 0.5f;

    public float minFactor = 0.5f;
    public float maxFactor = 1.5f;

    private bool rotateCW, rotateCCW, scaleUpY, scaleDownY, scaleUpX, scaleDownX;
    public static bool isTransforming = false;

    private readonly Dictionary<GameObject, Vector2> baseScales = new Dictionary<GameObject, Vector2>();

    void Update()
    {
        if (!Input.GetMouseButton(0) && Input.touchCount == 0)
        {
            ResetFlags();
        }

        if (GameObjectsScript.lastDragged == null)
        {
            isTransforming = false;
            return;
        }

        GameObject obj = GameObjectsScript.lastDragged;
        RectTransform rc = obj.GetComponent<RectTransform>();

        if (!baseScales.ContainsKey(obj))
        {
            Vector3 ls = rc.localScale;
            baseScales[obj] = new Vector2(
                Mathf.Max(0.01f, Mathf.Abs(ls.x)),
                Mathf.Max(0.01f, Mathf.Abs(ls.y)));
        }
        Vector2 b = baseScales[obj];

        float rot = (rotateCCW ? 1f : 0f) - (rotateCW ? 1f : 0f);
        if (rot != 0f)
        {
            rc.Rotate(0, 0, rot * rotationSpeed * Time.deltaTime);
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
                absX = Mathf.Clamp(absX + dx * scaleSpeed * b.x * Time.deltaTime,
                                   b.x * minFactor, b.x * maxFactor);
            if (dy != 0f)
                absY = Mathf.Clamp(absY + dy * scaleSpeed * b.y * Time.deltaTime,
                                   b.y * minFactor, b.y * maxFactor);

            s.x = absX * signX;
            s.y = absY * signY;
            rc.localScale = s;
        }

        isTransforming =
            rotateCW || rotateCCW || scaleUpY || scaleDownY || scaleUpX || scaleDownX;
    }

    private void ResetFlags()
    {
        rotateCW = rotateCCW = false;
        scaleUpY = scaleDownY = scaleUpX = scaleDownX = false;
        isTransforming = false;
    }

    public void StartRotateCW(BaseEventData eventData) { rotateCW = true; }
    public void StopRotateCW(BaseEventData eventData) { rotateCW = false; }

    public void StartRotateCCW(BaseEventData eventData) { rotateCCW = true; }
    public void StopRotateCCW(BaseEventData eventData) { rotateCCW = false; }

    public void StartScaleUpY(BaseEventData eventData) { scaleUpY = true; }
    public void StopScaleUpY(BaseEventData eventData) { scaleUpY = false; }

    public void StartScaleDownY(BaseEventData eventData) { scaleDownY = true; }
    public void StopScaleDownY(BaseEventData eventData) { scaleDownY = false; }

    public void StartScaleUpX(BaseEventData eventData) { scaleUpX = true; }
    public void StopScaleUpX(BaseEventData eventData) { scaleUpX = false; }

    public void StartScaleDownX(BaseEventData eventData) { scaleDownX = true; }
    public void StopScaleDownX(BaseEventData eventData) { scaleDownX = false; }
}