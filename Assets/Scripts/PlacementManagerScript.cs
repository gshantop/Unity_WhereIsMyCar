using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Randomly places car spawn points (17 pcs) and drop-off points (17 pcs)
/// every time the game starts, but ONLY from a predefined set of valid
/// points (predefinedPoints) - not completely random (e.g. can't end up
/// on a rooftop or similar invalid spot).
/// At each car spawn point a random car model is picked from carPrefabs,
/// so the same location is never used twice, but the car model can repeat.
/// </summary>
public class PlacementManager : MonoBehaviour
{
    [Header("Predefined valid points on the map")]
    [Tooltip("Drag empty GameObjects here that were manually placed at valid map locations")]
    public List<Transform> predefinedPoints = new List<Transform>();

    [Header("How many points to pick")]
    public int carSpawnCount = 17;
    public int dropOffCount = 17;

    [Header("Prefabs")]
    [Tooltip("All available car models (e.g. 12 different cars). One is picked at random for each spawn point.")]
    public List<GameObject> carPrefabs = new List<GameObject>();
    public GameObject dropOffMarkerPrefab;

    private readonly List<Transform> carSpawnPoints = new List<Transform>();
    private readonly List<Transform> dropOffPoints = new List<Transform>();
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        GeneratePlacement();
    }

    /// <summary>
    /// Recalculates the random placement. Can be called again
    /// (e.g. when restarting a level/round).
    /// </summary>
    public void GeneratePlacement()
    {
        ClearPrevious();

        int required = carSpawnCount + dropOffCount;
        if (predefinedPoints.Count < required)
        {
            Debug.LogError($"Not enough predefined points: need {required}, have {predefinedPoints.Count}");
            return;
        }

        if (carPrefabs.Count == 0)
        {
            Debug.LogError("carPrefabs list is empty - add at least one car prefab.");
            return;
        }

        // Copy the list so the original order in the Inspector isn't affected
        List<Transform> shuffled = new List<Transform>(predefinedPoints);
        ShuffleList(shuffled);

        // First carSpawnCount points go to cars,
        // next dropOffCount points go to drop-off locations.
        // This guarantees no overlap between car and drop-off points.
        carSpawnPoints.AddRange(shuffled.GetRange(0, carSpawnCount));
        dropOffPoints.AddRange(shuffled.GetRange(carSpawnCount, dropOffCount));

        SpawnCars(carSpawnPoints);
        SpawnAt(dropOffPoints, dropOffMarkerPrefab);
    }

    private void SpawnCars(List<Transform> points)
    {
        foreach (Transform point in points)
        {
            // Pick a random car model for this point (models can repeat, locations can't)
            GameObject prefab = carPrefabs[Random.Range(0, carPrefabs.Count)];
            GameObject obj = Instantiate(prefab, point.position, point.rotation);
            spawnedObjects.Add(obj);
        }
    }

    private void SpawnAt(List<Transform> points, GameObject prefab)
    {
        if (prefab == null) return;

        foreach (Transform point in points)
        {
            GameObject obj = Instantiate(prefab, point.position, point.rotation);
            spawnedObjects.Add(obj);
        }
    }

    private void ClearPrevious()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();
        carSpawnPoints.Clear();
        dropOffPoints.Clear();
    }

    // Fair list shuffling - Fisher-Yates algorithm
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}