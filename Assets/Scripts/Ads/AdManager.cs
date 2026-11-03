using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;

    [Header("Pilnekrāna reklāma (ainu maiņas brīdī)")]
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAds = false;
    [Tooltip("Minimālais laiks starp pilnekrāna reklāmām sekundēs (0 = katrā ainu maiņā)")]
    [SerializeField] float minSecondsBetweenInterstitials = 0f;
    [Tooltip("Cik sekundes gaidīt, ja reklāma vēl lādējas (piem., uzreiz pēc spēles palaišanas)")]
    [SerializeField] float maxWaitForInterstitial = 4f;

    [Header("Apbalvojošā reklāma (pēc lietotāja izvēles)")]
    public RewardedAds rewardedAds;
    [SerializeField] bool turnOffRewardedAds = false;

    [Header("Banera reklāma (nepārtraukti, fiksētā pozīcijā)")]
    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAds = false;

    [Header("Reklāmas pogu nosaukumi ainā")]
    [SerializeField] string interstitialButtonName = "InterstitialAd";
    [SerializeField] string[] rewardedButtonNames = { "RewardedAd", "RewardedAdButton" };
    [SerializeField] string bannerButtonName = "BannerAd";

    public static AdManager Instance { get; private set; }

    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
    private PointerEventData pointerData;
    private int[] lastTouchIds;

    private bool isChangingScene = false;
    private float lastInterstitialTime = -9999f;

    // Ja nevienā ainā nav AdManager - izveidojam to automātiski,
    // lai reklāma strādātu jau no StartScene
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateIfMissing()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("AdManager (auto)");
        go.AddComponent<AdsInitializer>();
        go.AddComponent<InterstitialAd>();
        go.AddComponent<RewardedAds>();
        go.AddComponent<BannerAd>();
        go.AddComponent<AdManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // DontDestroyOnLoad strādā tikai saknes objektiem
        if (transform.parent != null)
            transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        FindComponents();

        if (adsInitializer != null)
        {
            adsInitializer.OnAdsInitialized += HandleAdsInitialized;
            if (adsInitializer.IsInitialized)
                HandleAdsInitialized();
        }
        else
        {
            Debug.LogWarning("AdManager: AdsInitializer nav atrasts!");
        }
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        if (adsInitializer != null)
            adsInitializer.OnAdsInitialized -= HandleAdsInitialized;
    }

    private void FindComponents()
    {
        if (adsInitializer == null) adsInitializer = GetComponent<AdsInitializer>();
        if (interstitialAd == null) interstitialAd = GetComponent<InterstitialAd>();

        if (rewardedAds == null) rewardedAds = GetComponent<RewardedAds>();
        if (rewardedAds == null && !turnOffRewardedAds)
            rewardedAds = gameObject.AddComponent<RewardedAds>();

        if (bannerAd == null) bannerAd = GetComponent<BannerAd>();
        if (bannerAd == null && !turnOffBannerAds)
            bannerAd = gameObject.AddComponent<BannerAd>();
    }

    private void HandleAdsInitialized()
    {
        if (!turnOffInterstitialAds && interstitialAd != null)
            interstitialAd.LoadAd();

        if (!turnOffRewardedAds && rewardedAds != null)
            rewardedAds.LoadAd();

        if (!turnOffBannerAds && bannerAd != null)
            bannerAd.LoadBanner();
    }

    // ===== Ainu maiņa ar pilnekrāna reklāmu =====

    // Visas ainu maiņas spēlē iet caur šo metodi
    public static void LoadScene(int buildIndex)
    {
        if (Instance == null)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(buildIndex);
            return;
        }

        Instance.LoadSceneWithAd(buildIndex);
    }

    private void LoadSceneWithAd(int buildIndex)
    {
        if (isChangingScene)
            return;

        isChangingScene = true;
        StartCoroutine(LoadSceneRoutine(buildIndex));
    }

    private IEnumerator LoadSceneRoutine(int buildIndex)
    {
        bool cooldownPassed =
            Time.unscaledTime - lastInterstitialTime >= minSecondsBetweenInterstitials;
        bool useAd = !turnOffInterstitialAds && interstitialAd != null && cooldownPassed;

        // Ja reklāma vēl nav gatava (piem., spēle tikko palaista vai iepriekšējā tikko parādīta) -
        // īsu brīdi pagaidām, lai tā tiktu parādīta katrā ainu maiņā, nevis "pa reizei"
        if (useAd && !interstitialAd.isReady)
        {
            float waited = 0f;
            while (waited < maxWaitForInterstitial && !interstitialAd.isReady)
            {
                if (Advertisement.isInitialized)
                    interstitialAd.LoadAd(); // nedara neko, ja jau lādējas

                waited += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (useAd && interstitialAd.isReady &&
            interstitialAd.ShowAd(() => FinishSceneChange(buildIndex)))
        {
            // Aina tiek ielādēta tikai pēc tam, kad reklāma aizvērta
            lastInterstitialTime = Time.unscaledTime;
            yield break;
        }

        // Reklāma tā arī neielādējās - ainu mainām bez tās
        FinishSceneChange(buildIndex);
    }

    private void FinishSceneChange(int buildIndex)
    {
        isChangingScene = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(buildIndex);
    }

    // ===== Pogu pieslēgšana pēc ainas ielādes =====

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this) return;

        if (adsInitializer != null && adsInitializer.IsInitialized)
            HandleAdsInitialized();
    }

    // ===== Reklāmas pogas =====
    // Pogas nosakām paši (pēc pieskāriena sākuma un nosaukuma), jo telefonā
    // Button On Click ātrus pieskārienus mēdz nepamanīt

    private void Update()
    {
        if (Instance != this || EventSystem.current == null)
            return;

        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            if (lastTouchIds == null || lastTouchIds.Length != touches.Count)
                lastTouchIds = new int[touches.Count];

            for (int i = 0; i < touches.Count; i++)
            {
                int id = touches[i].touchId.ReadValue();
                if (id != 0 && id != lastTouchIds[i])
                {
                    lastTouchIds[i] = id;
                    HandleTap(touches[i].startPosition.ReadValue());
                }
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            HandleTap(Mouse.current.position.ReadValue());
    }

    private void HandleTap(Vector2 screenPos)
    {
        string name = GetUIButtonName(screenPos);
        if (name == null)
            return;

        if (name == interstitialButtonName)
        {
            if (!turnOffInterstitialAds && interstitialAd != null)
                interstitialAd.ShowManually();
        }
        else if (System.Array.IndexOf(rewardedButtonNames, name) >= 0)
        {
            if (!turnOffRewardedAds && rewardedAds != null)
                rewardedAds.ShowAd();
        }
        else if (name == bannerButtonName)
        {
            if (!turnOffBannerAds && bannerAd != null)
                bannerAd.ToggleBanner();
        }
    }

    // Augšējā UI elementa (vai tā vecāka) nosaukums, ja tā ir viena no reklāmas pogām
    private string GetUIButtonName(Vector2 screenPos)
    {
        if (pointerData == null)
            pointerData = new PointerEventData(EventSystem.current);

        pointerData.position = screenPos;
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        if (raycastResults.Count == 0)
            return null;

        Transform t = raycastResults[0].gameObject.transform;
        for (int i = 0; i < 2 && t != null; i++, t = t.parent)
        {
            string n = t.name;
            if (n == interstitialButtonName || n == bannerButtonName ||
                System.Array.IndexOf(rewardedButtonNames, n) >= 0)
                return n;
        }
        return null;
    }
}