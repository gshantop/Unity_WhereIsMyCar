using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    private float placeZRot, carZRot, diffZRot;
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
                RectTransform carRect = eventData.pointerDrag.GetComponent<RectTransform>();
                RectTransform placeRect = GetComponent<RectTransform>();

                // Rotācijas un izmēra starpība
                placeZRot = carRect.transform.eulerAngles.z;
                carZRot = placeRect.transform.eulerAngles.z;
                diffZRot = Mathf.Abs(placeZRot - carZRot);
                Debug.Log("Diff Z Rot: " + diffZRot);

                placeSize = carRect.localScale;
                carSize = placeRect.localScale;
                xSizeDiff = Mathf.Abs(placeSize.x - carSize.x);
                ySizeDiff = Mathf.Abs(placeSize.y - carSize.y);
                Debug.Log("Diff X Size: " + xSizeDiff);
                Debug.Log("Diff Y Size: " + ySizeDiff);

                if ((diffZRot <= 7 || (diffZRot >= 353 && diffZRot <= 360)) &&
                    (xSizeDiff <= 0.08f && ySizeDiff <= 0.08f))
                {
                    Debug.Log("Car placed correctly!");
                    gameObjectsScript.inRightPlace = true;
                    gameObjectsScript.NotifyCarPlacedCorrectly();

                    carRect.anchoredPosition = placeRect.anchoredPosition;
                    carRect.localScale = placeRect.localScale;
                    carRect.localRotation = placeRect.localRotation;

                    PlayCarSound(eventData.pointerDrag.tag);
                }
                else
                {
                    // Pareizā vieta, bet nepareizs izmērs vai rotācija - atgriežam sākumā
                    gameObjectsScript.inRightPlace = false;
                    gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[4]);
                    ResetCar(eventData.pointerDrag.tag);
                }
            }
            else
            {
                // Nepareiza vieta
                gameObjectsScript.inRightPlace = false;
                gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[4]);
                ResetCar(eventData.pointerDrag.tag);
            }
        }
    }

    // Atskaņo mašīnas skaņu pēc taga
    private void PlayCarSound(string carTag)
    {
        int soundIndex;

        switch (carTag)
        {
            case "Garbage": soundIndex = 1; break;
            case "Ambulance": soundIndex = 2; break;
            case "School": soundIndex = 3; break;
            case "CementaMasina": soundIndex = 12; break;
            case "b2": soundIndex = 11; break;
            case "Traktors": soundIndex = 13; break;
            case "Policija": soundIndex = 15; break;
            case "e61": soundIndex = 9; break;
            case "Traktors2": soundIndex = 14; break;
            case "e46": soundIndex = 8; break;
            case "Eskavators": soundIndex = 10; break;
            case "Ugunsdzeseji": soundIndex = 7; break;
            default:
                Debug.Log("No matching tag found for the dropped object.");
                return;
        }

        gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[soundIndex]);
    }

    private void ResetCar(string carTag)
    {
        switch (carTag)
        {
            case "Garbage":
                gameObjectsScript.garbageTruck.GetComponent<RectTransform>().localPosition =
                    gameObjectsScript.garbageTruckCoord;
                break;

            case "Ambulance":
                gameObjectsScript.medicine.GetComponent<RectTransform>().localPosition =
                    gameObjectsScript.medicineCoord;
                break;

            case "School":
                gameObjectsScript.schoolBus.GetComponent<RectTransform>().localPosition =
                    gameObjectsScript.schoolBusCoord;
                break;

            case "CementaMasina":
                gameObjectsScript.cementamasina.GetComponent<RectTransform>().localPosition =
                    gameObjectsScript.cementamasinaCoord;
                break;

            case "b2":
                gameObjectsScript.b2.GetComponent<RectTransform>().localPosition =
                    gameObjectsScript.b2Coord;
                break;

            case "Traktors":
                gameObjectsScript.traktors.GetComponent<RectTransform>().localPosition =
                    gameObjectsScript.traktorsCoord;
                break;

            case "Policija":
                gameObjectsScript.policija.GetComponent<RectTransform>().localPosition =
                    gameObjectsScript.policijaCoord;
                break;

            case "e61":
                gameObjectsScript.e61.GetComponent<RectTransform>().localPosition =
                    gameObjectsScript.e61Coord;
                break;

            case "Traktors2":
                gameObjectsScript.traktors2.GetComponent<RectTransform>().localPosition =
                    gameObjectsScript.traktors2Coord;
                break;

            case "e46":
                gameObjectsScript.e46.GetComponent<RectTransform>().localPosition =
                    gameObjectsScript.e46Coord;
                break;

            case "Eskavators":
                gameObjectsScript.eskavators.GetComponent<RectTransform>().localPosition =
                    gameObjectsScript.eskavatorsCoord;
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