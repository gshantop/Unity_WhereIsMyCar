using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAds = false;
    [SerializeField] bool showInterstitialOnStart = true;
    private bool firstAdShown = false;

    public RewardedAds rewardedAds;
    [SerializeField] bool turnOffRewardedAds = false;

    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAds = false;

    public static AdManager Instance { get; private set; }

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
        if (interstitialAd != null)
            interstitialAd.OnInterstitialAdReady -= HandleInterstitialReady;
    }

    private void FindComponents()
    {
        if (adsInitializer == null) adsInitializer = FindFirstObjectByType<AdsInitializer>();
        if (interstitialAd == null) interstitialAd = FindFirstObjectByType<InterstitialAd>();
        if (rewardedAds == null) rewardedAds = FindFirstObjectByType<RewardedAds>();
        if (bannerAd == null) bannerAd = FindFirstObjectByType<BannerAd>();
    }

    private void HandleAdsInitialized()
    {
        if (!turnOffInterstitialAds && interstitialAd != null)
        {
            // Atrakstāmies pirms pierakstīšanās, lai nebūtu dubultu izsaukumu pēc ainas pārlādes
            interstitialAd.OnInterstitialAdReady -= HandleInterstitialReady;
            interstitialAd.OnInterstitialAdReady += HandleInterstitialReady;
            interstitialAd.LoadAd();
        }

        if (!turnOffRewardedAds && rewardedAds != null)
            rewardedAds.LoadAd();

        if (!turnOffBannerAds && bannerAd != null)
            bannerAd.LoadBanner();
    }

    private void HandleInterstitialReady()
    {
        if (showInterstitialOnStart && !firstAdShown && interstitialAd != null)
        {
            Debug.Log("Showing interstitial ad for the first time!");
            firstAdShown = true;
            interstitialAd.ShowAd();
        }
        else
        {
            Debug.Log("Interstitial ad is ready to be shown again!");
        }
    }

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

        FindComponents();

        Button interstitialButton = FindButtonByTag("InterstitialAdButton");
        if (interstitialAd != null && interstitialButton != null)
            interstitialAd.SetButton(interstitialButton);

        Button rewardedAdButton = FindButtonByTag("RewardedAdButton");
        if (rewardedAds != null && rewardedAdButton != null)
            rewardedAds.SetButton(rewardedAdButton);

        Button bannerAdButton = FindButtonByTag("BannerAdButton");
        if (bannerAd != null && bannerAdButton != null)
            bannerAd.SetButton(bannerAdButton);

        if (adsInitializer != null && adsInitializer.IsInitialized)
            HandleAdsInitialized();
    }

    // Droša meklēšana: ja tags nav izveidots vai objekta nav - atgriež null, nevis kļūdu
    private Button FindButtonByTag(string tag)
    {
        try
        {
            GameObject go = GameObject.FindGameObjectWithTag(tag);
            return go != null ? go.GetComponent<Button>() : null;
        }
        catch (UnityException)
        {
            Debug.LogWarning($"AdManager: tags \"{tag}\" nav izveidots (Tags & Layers).");
            return null;
        }
    }
}