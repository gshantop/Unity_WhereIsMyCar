using UnityEngine;

public class FlyingObjectSpawnScript : MonoBehaviour
{
    ScreenBoundariesScript screenBoundariesScript;
    [Header("Cloud Prefabs")]
    public GameObject[] cloudPrefabs;
    [Header("Plain Prefabs")]
    public GameObject[] plainPrefabs;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    [Header("Spawn Interval")]
    public float cloudSpawnInterval = 2f;
    public float plainSpawnInterval = 3f;

    private float minY, maxY;

    [Header("Cloud Speed")]
    public float cloudMinSpeed = 1.5f;
    public float cloudMaxSpeed = 150f;

    [Header("Plain Speed")]
    public float plainMinSpeed = 2f;
    public float plainMaxSpeed = 100f;


    void Start()
    {
        KeepFlyingObjectsAboveCars();

        screenBoundariesScript = Object.FindFirstObjectByType<ScreenBoundariesScript>();
        minY = screenBoundariesScript.worldBounds.yMin;
        maxY = screenBoundariesScript.worldBounds.yMax;
        InvokeRepeating(nameof(SpawnCloud), 0f, cloudSpawnInterval);
        InvokeRepeating(nameof(SpawnPlain), 0f, plainSpawnInterval);
    }

    // Mākoņi, lidmašīnas un bumbas ir SpawnPoint bērni. Piešķiram SpawnPoint savu Canvas ar
    // augstāku kārtas numuru - tad tie vienmēr zīmējas virs mašīnām (arī velkot mašīnu),
    // bet zem uzvaras/zaudējuma paneļiem (PanelOnTop = 100).
    void KeepFlyingObjectsAboveCars()
    {
        if (spawnPoint == null) return;

        Canvas parentCanvas = spawnPoint.parent != null
            ? spawnPoint.parent.GetComponentInParent<Canvas>()
            : null;

        Canvas c = spawnPoint.GetComponent<Canvas>();
        if (c == null) c = spawnPoint.gameObject.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 50;
        if (parentCanvas != null) c.sortingLayerID = parentCanvas.sortingLayerID;
    }

    void SpawnCloud()
    {
        if (cloudPrefabs.Length == 0) return;

        GameObject prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
        float y = Random.Range(minY, maxY);
        Vector3 pos = new Vector3(spawnPoint.position.x, y, spawnPoint.position.z);
        GameObject cloud = Instantiate(prefab, pos, Quaternion.identity, spawnPoint);
        float speed = Random.Range(cloudMinSpeed, cloudMaxSpeed);

        FlyingObjectControllerScript controller =
            cloud.GetComponent<FlyingObjectControllerScript>();

        if (controller != null)
            controller.speed = speed;
    }

    void SpawnPlain()
    {
        if (plainPrefabs.Length == 0) return;
        GameObject prefab = plainPrefabs[Random.Range(0, plainPrefabs.Length)];
        float y = Random.Range(minY, maxY);
        Vector3 pos = new Vector3(-spawnPoint.position.x, y, spawnPoint.position.z);
        GameObject plain = Instantiate(prefab, pos, Quaternion.identity, spawnPoint);
        float speed = Random.Range(plainMinSpeed, plainMaxSpeed);
        FlyingObjectControllerScript controller =
            plain.GetComponent<FlyingObjectControllerScript>();
        if (controller != null)
            controller.speed = -speed;
    }
}