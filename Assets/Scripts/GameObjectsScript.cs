using UnityEngine;
using System.Collections.Generic;

public class GameObjectsScript : MonoBehaviour
{
    public RectTransform[] spawnPoints;
    public RectTransform[] dropPlaces;

    public GameObject garbageTruck;
    public GameObject medicine;
    public GameObject schoolBus;
    public GameObject traktors;
    public GameObject traktors2;
    public GameObject policija;
    public GameObject b2;
    public GameObject e46;
    public GameObject e61;
    public GameObject ugunsdzeseji;
    public GameObject eskavators;
    public GameObject cementamasina;

    [HideInInspector]
    public Vector2 garbageTruckCoord;
    [HideInInspector]
    public Vector2 medicineCoord;
    [HideInInspector]
    public Vector2 schoolBusCoord;
    [HideInInspector]
    public Vector2 traktorsCoord;
    [HideInInspector]
    public Vector2 traktors2Coord;
    [HideInInspector]
    public Vector2 policijaCoord;
    [HideInInspector]
    public Vector2 b2Coord;
    [HideInInspector]
    public Vector2 e46Coord;
    [HideInInspector]
    public Vector2 e61Coord;
    [HideInInspector]
    public Vector2 ugunsdzesejiCoord;
    [HideInInspector]
    public Vector2 eskavatorsCoord;
    [HideInInspector]
    public Vector2 cementamasinaCoord;

    public Canvas canvas;
    public AudioSource carSoundSource;
    public AudioClip[] sounds;

    [HideInInspector]
    public bool inRightPlace = false;

    public static GameObject lastDragged = null;
    public static bool isDragging = false;


    void Awake()
    {
        SpawnDropPlacesRandomly();

        garbageTruckCoord = garbageTruck.GetComponent<RectTransform>().localPosition;
        medicineCoord = medicine.GetComponent<RectTransform>().localPosition;
        schoolBusCoord = schoolBus.GetComponent<RectTransform>().localPosition;
        traktorsCoord = traktors.GetComponent<RectTransform>().localPosition;
        traktors2Coord = traktors2.GetComponent<RectTransform>().localPosition;
        policijaCoord = policija.GetComponent<RectTransform>().localPosition;
        b2Coord = b2.GetComponent<RectTransform>().localPosition;
        e46Coord = e46.GetComponent<RectTransform>().localPosition;
        e61Coord = e61.GetComponent<RectTransform>().localPosition;
        ugunsdzesejiCoord = ugunsdzeseji.GetComponent<RectTransform>().localPosition;
        eskavatorsCoord = eskavators.GetComponent<RectTransform>().localPosition;
        cementamasinaCoord = cementamasina.GetComponent<RectTransform>().localPosition;
    }


    private void SpawnDropPlacesRandomly()
    {
        if (spawnPoints == null || dropPlaces == null)
        {
            Debug.LogError("SpawnPoints vai DropPlaces nav pievienoti!");
            return;
        }

        if (spawnPoints.Length < dropPlaces.Length)
        {
            Debug.LogError("SpawnPoint nepietiek visām DropPlace vietām!");
            return;
        }

        List<RectTransform> availablePoints =
            new List<RectTransform>(spawnPoints);

        foreach (RectTransform dropPlace in dropPlaces)
        {
            if (dropPlace == null)
            {
                Debug.LogWarning("Viens DropPlace nav pievienots!");
                continue;
            }

            int randomIndex = Random.Range(0, availablePoints.Count);

            RectTransform randomPoint = availablePoints[randomIndex];

            dropPlace.position = randomPoint.position;

            availablePoints.RemoveAt(randomIndex);
        }
    }