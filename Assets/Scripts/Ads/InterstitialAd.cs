using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class InterstitialAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "BP_Interstitial_Android";
    string _adUnitId;
    public event Action OnInterstitialAdReady;
    public bool isReady = false;
    [SerializeField] Button _interstitialAdButton;

    // false = poga vienmēr spiežama; ja reklāma vēl nav ielādēta, tā tiks parādīta, tiklīdz ielādēsies
    [SerializeField] bool disableButtonUntilReady = false;
    private bool pendingShow = false;

    [Header("Atkārtota ielāde kļūdas gadījumā")]
    [SerializeField] float retryDelay = 10f;
    [SerializeField] int maxRetries = 5;
    private int retryCount = 0;
    private bool isLoading = false;
    private Coroutine retryRoutine;

    [Header("Bonuss pēc reklāmas")]
    [SerializeField] float slowTimeScale = 0.3f;
    [SerializeField] float slowDuration = 30f;

    void Awake()
    {
        _adUnitId = _androidAdUnitId;
    }

    private void Update()
    {
        if (_interstitialAdButton != null)
            _interstitialAdButton.interactable = !disableButtonUntilReady || isReady;
    }

    public void OnInterstitialAdButtonClicked()
    {
        Debug.Log("Interstitial Ad Button Clicked");
        ShowInterstitial();
    }

    public void ShowInterstitial()
    {
        if (isReady)
        {
            Debug.Log("Showing interstitial ad manually!");
            ShowAd();
        }
        else
        {
            Debug.LogWarning("Interstitial ad is not ready yet, loading - will show when loaded!");
            pendingShow = true;
            retryCount = 0;
            LoadAd();
        }
    }

    public void ShowAd()
    {
        if (!isReady)
        {
            Debug.LogWarning("Interstitial ad is not ready yet!");
            return;
        }

        if (AdManager.Instance != null && AdManager.Instance.bannerAd != null &&
            AdManager.Instance.bannerAd.isBannerVisible)
        {
            AdManager.Instance.bannerAd.HideBannerAd();
        }

        isReady = false;
        Advertisement.Show(_adUnitId, this);
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Tried to load interstitial ad before Unity Ads was initialized!");
            return;
        }

        if (isLoading || isReady)
            return;

        isLoading = true;
        Debug.Log("Loading interstitial ad...");
        Advertisement.Load(_adUnitId, this);
    }

    // Nekad neielādējam uzreiz atkārtoti - tas var radīt bezgalīgu ciklu un "uzkārt" Unity
    private void ScheduleRetry()
    {
        if (retryCount >= maxRetries)
        {
            Debug.LogWarning("Interstitial: sasniegts mēģinājumu limits, vairs nelādējam.");
            return;
        }

        retryCount++;
        if (retryRoutine != null)
            StopCoroutine(retryRoutine);
        retryRoutine = StartCoroutine(RetryAfterDelay());
    }

    private IEnumerator RetryAfterDelay()
    {
        yield return new WaitForSecondsRealtime(retryDelay);
        retryRoutine = null;
        LoadAd();
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId != _adUnitId) return;

        Debug.Log("Interstitial ad loaded successfully!");
        isLoading = false;
        isReady = true;
        retryCount = 0;
        OnInterstitialAdReady?.Invoke();

        if (pendingShow)
        {
            pendingShow = false;
            ShowAd();
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Failed to load interstitial ad: {error} - {message}");
        isLoading = false;
        isReady = false;
        ScheduleRetry();
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("Interstitial ad clicked!");
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log("Interstitial ad started showing!");
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Interstitial ad completed successfully!");
            if (!GameManagerScript.GameOver)
                StartCoroutine(SlowDownTimeTemporarily(slowDuration));
        }
        else
        {
            Debug.LogWarning($"Interstitial ad was not completed: {showCompletionState}");
            RestoreTime();
        }

        retryCount = 0;
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Failed to show interstitial ad: {error} - {message}");
        RestoreTime();
        ScheduleRetry();
    }

    private IEnumerator SlowDownTimeTemporarily(float duration)
    {
        Time.timeScale = slowTimeScale;
        Debug.Log("Time slowed down for " + duration + " seconds.");
        yield return new WaitForSecondsRealtime(duration);
        RestoreTime();
        Debug.Log("Time restored to normal.");
    }

    // Neatsaldējam spēli, ja tā jau beigusies (uzvaras/zaudējuma logs)
    private void RestoreTime()
    {
        if (!GameManagerScript.GameOver)
            Time.timeScale = 1f;
    }

    public void SetButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnInterstitialAdButtonClicked);
        _interstitialAdButton = button;
        _interstitialAdButton.interactable = !disableButtonUntilReady || isReady;
    }
}