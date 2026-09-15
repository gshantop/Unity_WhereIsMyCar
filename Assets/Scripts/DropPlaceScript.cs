using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    private float placeZRot, carZRot, difZrot;
    private float diffZRot;
    private Vector3 placeSize, carSize;
    private float xSizeDiff, ySizeDiff;
    public GameObjectsScript gameObjectsScript;

    void Awake()
    {
        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if ((eventData.pointerDrag != null) && Input.GetMouseButtonUp(0) &&
            (!Input.GetMouseButton(2)))
        {
            if (eventData.pointerDrag.tag.Equals(tag))
            {
                placeZRot =
                    eventData.pointerDrag.GetComponent<RectTransform>().transform.eulerAngles.z;
                carZRot = GetComponent<RectTransform>().transform.eulerAngles.z;
                diffZRot = Mathf.Abs(placeZRot - carZRot);
                Debug.Log("Diff Z Rot: " + diffZRot);

                placeSize = eventData.pointerDrag.GetComponent<RectTransform>().localScale;
                carSize = GetComponent<RectTransform>().localScale;
                xSizeDiff = Mathf.Abs(placeSize.x - carSize.x);
                ySizeDiff = Mathf.Abs(placeSize.y - carSize.y);
                Debug.Log("Diff x Size: " + xSizeDiff);
                Debug.Log("Diff y Size: " + ySizeDiff);

                if ((diffZRot <= 7 || (diffZRot >= 353 && diffZRot <= 360)) &&
                    (xSizeDiff <= 0.08f && ySizeDiff <= 0.08f)) 
                {
                    Debug.Log("Car placed correctly!");
                    gameObjectsScript.inRightPlace = true;
                    eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition =
                        GetComponent<RectTransform>().anchoredPosition;

                    eventData.pointerDrag.GetComponent<RectTransform>().localScale =
                        GetComponent<RectTransform>().localScale;

                    eventData.pointerDrag.GetComponent<RectTransform>().localRotation =
                        GetComponent<RectTransform>().localRotation;

                    switch (eventData.pointerDrag.tag)
                    {
                        case "Garbage":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[1]);
                            break;

                        case "Ambulance":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[2]);
                            break;

                        case "SchoolBus":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[3]);
                            break;

                        case "b2":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[4]);
                            break;

                        case "Ugunsdzeseji":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[5]);
                            break;

                        case "Traktors":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[8]);
                            break;

                        case "Traktors2":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[9]);
                            break;

                        case "Policija":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[10]);
                            break;

                        case "CementaMasina":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[11]);
                            break;

                        case "e46":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[0]);
                            break;

                        case "e61":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[6]);
                            break;

                        case "Eskavators":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[7]);
                            break;

                        default:
                            Debug.Log("No matching tag found for the dropped object.");
                            break;
                    }
                }
            }
            else
            {
                gameObjectsScript.inRightPlace = false;
                gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[4]);

                switch (eventData.pointerDrag.tag)
                {
                    case "Garbage":
                        gameObjectsScript.garbageTruck.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.garbageTruckCoord;
                        break;

                    case "Ambulance":
                        gameObjectsScript.medicine.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.medicineCoord;
                        break;

                    case "SchoolBus":
                        gameObjectsScript.schoolBus.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.schoolBusCoord;
                        break;

                    case "b2":
                        gameObjectsScript.b2.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.b2Coord;
                        break;

                    case "CementaMasina":
                        gameObjectsScript.cementamasina.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.cementamasinaCoord;
                        break;

                    case "e46":
                        gameObjectsScript.e46.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.e46Coord;
                        break;

                    case "e61":
                        gameObjectsScript.e61.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.e61Coord;
                        break;

                    case "Eskavators":
                        gameObjectsScript.eskavators.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.eskavatorsCoord;
                        break;

                    case "Policija":
                        gameObjectsScript.policija.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.policijaCoord;
                        break;

                    case "Traktors":
                        gameObjectsScript.traktors.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.traktorsCoord;
                        break;

                    case "Traktors2":
                        gameObjectsScript.traktors2.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.traktors2Coord;
                        break;

                    case "Ugunsdzeseji":
                        gameObjectsScript.ugunsdzeseji.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.ugunsdzesejiCoord;
                        break;

                    default:
                        Debug.Log("No matching tag found for the dropped object.");
                        break;
                }
            }
        }
    }
}
