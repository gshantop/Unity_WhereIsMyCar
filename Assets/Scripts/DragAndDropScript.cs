using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropScript : MonoBehaviour, 
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObjectsScript gameObjectsScript;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    public ScreenBounderScript screenBounderScript;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null ){
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rectTransform = GetComponent<RectTransform>();
        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
        screenBounderScript = Object.FindFirstObjectByType<ScreenBounderScript>();


    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            GameObjectsScript.isDragging = true;
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
            int lastIndex = transform.parent.childCount - 1;
            int positionIndex = Mathf.Max(0, lastIndex - 1);
            transform.SetSiblingIndex(positionIndex);

            Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x, Input.mousePosition.y, 
                screenBounderScript.screenPoint.z));
            rectTransform.position = cursorWorldPos;
            screenBounderScript.screenPoint = Camera.main.WorldToScreenPoint(rectTransform.localPosition);

            screenBounderScript.offset = rectTransform.localPosition -
                Camera.main.ScreenToWorldPoint(new Vector3(
                    Input.mousePosition.x, Input.mousePosition.y,
                    screenBounderScript.screenPoint.z));
            GameObjectsScript.lastDragged = eventData.pointerDrag;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2)){
            Vector3 cursScreenPoint = new Vector3(Input.mousePosition.x,
                Input.mousePosition.y,
                screenBounderScript.screenPoint.z);
            Vector3 cursPoint = Camera.main.ScreenToWorldPoint(cursScreenPoint) +
                screenBounderScript.offset;

            rectTransform.position = screenBounderScript.GetClampedPosition(cursPoint);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Input.GetMouseButtonUp(0))
        {
            GameObjectsScript.isDragging = false;
            Debug.Log("OnEndDrag called for " + gameObject.name);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            if (gameObjectsScript.inRightPlace)
            {
                canvasGroup.blocksRaycasts = false;
                GameObjectsScript.lastDragged = null;
            }

            gameObjectsScript.inRightPlace = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            Debug.Log("Left mouse button clicked on " + gameObject.name);
            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[0]);
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
