using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;

    [Header("Pilnekrāna reklāma (ainu maiņas brīdī)")]
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAds = false;
    [Tooltip("Minimālais laiks starp pilnekrāna reklāmām sekundēs (0 = katrā ainu maiņā)")]
    [SerializeField] float minSecondsBetweenInterstitials = 0f;

    [Header("Apbalvojošā reklāma (pēc lietotāja izvēles)")]
    public RewardedAds rewardedAds;
    [SerializeField] bool turnOffRewardedAds = false;

    [Header("Banera reklāma")]
    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAds = false;

    public static AdManager Instance { get; private set; }

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

        bool cooldownPassed =
            Time.unscaledTime - lastInterstitialTime >= minSecondsBetweenInterstitials;

        if (!turnOffInterstitialAds && interstitialAd != null &&
            interstitialAd.isReady && cooldownPassed)
        {
            isChangingScene = true;

            // Aina tiek ielādēta tikai pēc tam, kad reklāma aizvērta
            if (interstitialAd.ShowAd(() => FinishSceneChange(buildIndex)))
            {
                lastInterstitialTime = Time.unscaledTime;
                return;
            }
        }

        // Reklāma nav gatava - ainu mainām uzreiz, lietotājs negaida
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

        if (bannerAd == null) bannerAd = FindFirstObjectByType<BannerAd>();

        Button rewardedAdButton = FindButton("RewardedAdButton");
        if (rewardedAds != null && rewardedAdButton != null)
            rewardedAds.SetButton(rewardedAdButton);

        Button bannerAdButton = FindButton("BannerAdButton");
        if (bannerAd != null && bannerAdButton != null)
            bannerAd.SetButton(bannerAdButton);

        if (adsInitializer != null && adsInitializer.IsInitialized)
            HandleAdsInitialized();
    }

    // Meklē pogu pēc taga, ja tāda taga nav - pēc objekta nosaukuma
    private Button FindButton(string key)
    {
        GameObject go = null;

        try
        {
            go = GameObject.FindGameObjectWithTag(key);
        }
        catch (UnityException)
        {
            // Tags nav izveidots - meklējam pēc nosaukuma
        }

        if (go == null)
            go = GameObject.Find(key);

        return go != null ? go.GetComponent<Button>() : null;
    }
}