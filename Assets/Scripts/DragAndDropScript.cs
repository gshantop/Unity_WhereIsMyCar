using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropScript : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObjectsScript gameObjectsScript;
    private CanvasGroup canvasGroup;
    public ScreenBoundariesScript screenBoundariesScript;
    private Vector3 dragOffsetWorld;
    private Camera mainCamera;
    private Canvas canvas;

    // Vilkšanas stāvoklis un pēdējā pirksta/kursora pozīcija ekrānā
    private bool dragging = false;
    private Vector2 lastPointerScreenPos;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (gameObjectsScript == null)
        {
            gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
        }

        if (screenBoundariesScript == null)
        {
            screenBoundariesScript = Object.FindFirstObjectByType<ScreenBoundariesScript>();
        }

        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            mainCamera = canvas.worldCamera;
        }
        else
            Debug.LogWarning("Canvas not found in parent hierarchy of " + gameObject.name);

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Clicked on " + gameObject.name);
        if (gameObjectsScript.carSoundSource != null && gameObjectsScript.sounds != null)
        {
            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[0]);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Start drag!");
        GameObjectsScript.isDragging = true;
        dragging = true;
        lastPointerScreenPos = eventData.position;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        int lastIndex = transform.parent.childCount - 1;
        int positionIndex = Mathf.Max(0, lastIndex - 1);
        transform.SetSiblingIndex(positionIndex);

        Vector3 cursorWorldPos;
        if (ScreenToWorldPoint(eventData.position, out cursorWorldPos))
        {
            dragOffsetWorld = transform.position - cursorWorldPos;
            dragOffsetWorld.z = 0f;
        }
        else
        {
            dragOffsetWorld = Vector3.zero;
        }

        GameObjectsScript.lastDragged = eventData.pointerDrag;
    }

    public void OnDrag(PointerEventData eventData)
    {
        lastPointerScreenPos = eventData.position;
        FollowPointer();
    }

    // Katru kadru (arī tad, kad kamera kustas, bet kursors stāv) mašīna paliek zem kursora
    void LateUpdate()
    {
        if (dragging && GameObjectsScript.isDragging)
        {
            FollowPointer();
        }
    }

    void FollowPointer()
    {
        Vector3 pointerWorld;
        if (!ScreenToWorldPoint(lastPointerScreenPos, out pointerWorld))
        {
            return;
        }

        Vector3 desired = pointerWorld + dragOffsetWorld;
        desired.z = transform.position.z;

        Vector2 clamped = screenBoundariesScript.GetClampedPosition(desired);
        transform.position = new Vector3(clamped.x, clamped.y, desired.z);
    }

    private bool ScreenToWorldPoint(Vector2 screenPosition, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main camera is not assigned.");
            return false;
        }

        float z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, z);
        worldPosition = mainCamera.ScreenToWorldPoint(screenPoint);
        return true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        GameObjectsScript.isDragging = false;
        Debug.Log("OnEndDrag called for " + gameObject.name);

        if (gameObjectsScript.carSoundSource != null && gameObjectsScript.sounds != null)
        {
            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[0]);
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (gameObjectsScript.inRightPlace)
        {
            canvasGroup.blocksRaycasts = false;
            GameObjectsScript.lastDragged = null;
        }

        gameObjectsScript.inRightPlace = false;
    }

    void OnDisable()
    {
        dragging = false;
    }
}