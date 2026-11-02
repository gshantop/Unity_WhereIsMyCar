using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

// Pilnekrāna reklāma. Rāda AdManager ainu maiņas brīdī (AdManager.LoadScene).
public class InterstitialAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "BP_Interstitial_Android";
    string _adUnitId;
    public bool isReady = false;

    [Header("Atkārtota ielāde kļūdas gadījumā")]
    [SerializeField] float retryDelay = 10f;
    [SerializeField] int maxRetries = 5;
    private int retryCount = 0;
    private bool isLoading = false;
    private Coroutine retryRoutine;

    // Ko izsaukt, kad reklāma aizvērta (piem., ielādēt nākamo ainu)
    private Action onFinished;

    // Poga nospiesta, kamēr reklāma vēl lādējās - parādīsim, tiklīdz ielādēsies
    private bool pendingManualShow = false;

    void Awake()
    {
        _adUnitId = _androidAdUnitId;
    }

    // Poga "InterstitialAd": parāda reklāmu tūlīt vai uzreiz pēc ielādes
    public void ShowManually()
    {
        if (isReady)
        {
            ShowAd(null);
            return;
        }

        pendingManualShow = true;
        retryCount = 0;
        LoadAd();
    }

    public void OnInterstitialAdButtonClicked()
    {
        ShowManually();
    }

    // Atgriež true, ja reklāma sāk rādīties
    public bool ShowAd(Action onFinished)
    {
        if (!isReady || !Advertisement.isInitialized)
        {
            Debug.LogWarning("Interstitial ad is not ready yet!");
            return false;
        }

        if (AdManager.Instance != null && AdManager.Instance.bannerAd != null &&
            AdManager.Instance.bannerAd.isBannerVisible)
        {
            AdManager.Instance.bannerAd.HideBannerAd();
        }

        this.onFinished = onFinished;
        isReady = false;
        Advertisement.Show(_adUnitId, this);
        return true;
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

    // Nekad neielādējam uzreiz atkārtoti - tas var radīt bezgalīgu ciklu
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

    private void Finish()
    {
        Action callback = onFinished;
        onFinished = null;
        callback?.Invoke();
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId != _adUnitId) return;

        Debug.Log("Interstitial ad loaded successfully!");
        isLoading = false;
        isReady = true;
        retryCount = 0;

        if (pendingManualShow)
        {
            pendingManualShow = false;
            ShowAd(null);
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
        Debug.Log($"Interstitial ad closed: {showCompletionState}");
        RestoreTime();
        retryCount = 0;
        LoadAd();
        Finish();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Failed to show interstitial ad: {error} - {message}");
        RestoreTime();
        ScheduleRetry();
        Finish();
    }

    // Neatsaldējam spēli, ja tā jau beigusies (uzvaras/zaudējuma logs)
    private void RestoreTime()
    {
        if (!GameManagerScript.GameOver)
            Time.timeScale = 1f;
    }
}