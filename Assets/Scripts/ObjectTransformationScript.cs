using UnityEngine;

public class ObjectTransformationScript : MonoBehaviour
{
    public GameObjectsScript gameObjectsScript;

    public float minScale = 0.4f;
    public float maxScale = 1.6f;


    void Awake()
    {
        gameObjectsScript = FindFirstObjectByType<GameObjectsScript>();
    }


    void Update()
    {
        if (GameObjectsScript.lastDragged != null)
        {
            if (Input.GetKey(KeyCode.Z))
            {
                GameObjectsScript.lastDragged.GetComponent<RectTransform>().Rotate(
                    0, 0, Time.deltaTime * 12);
            }

            if (Input.GetKey(KeyCode.X))
            {
                GameObjectsScript.lastDragged.GetComponent<RectTransform>().Rotate(
                    0, 0, -Time.deltaTime * 12);
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                if (GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y < maxScale)
                {
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale =
                    new Vector3(GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x,
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y + 0.001f, 1f);
                }
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                if (GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y > minScale)
                {
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale =
                    new Vector3(GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x,
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y - 0.001f, 1f);
                }
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                float x = GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x;
                if (Mathf.Abs(x) > minScale)
                {
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale =
                    new Vector3(Mathf.Sign(x) * (Mathf.Abs(x) - 0.001f),
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y, 1f);
                }
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                float x = GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x;
                if (Mathf.Abs(x) < maxScale)
                {
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale =
                    new Vector3(Mathf.Sign(x) * (Mathf.Abs(x) + 0.001f),
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y, 1f);
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale =
                    new Vector3(-GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x,
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y, 1f);

            }
        }
    }
}