using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "BP_Rewarded_Android";
    string _adUnitId;

    [SerializeField] Button _rewardedAdButton;
    public FlyingObjectManager flyingObjectManager;

    [SerializeField] float retryDelay = 10f;
    [SerializeField] int maxRetries = 5;
    private int retryCount = 0;
    private bool isLoading = false;
    private bool isReady = false;

    void Awake()
    {
        _adUnitId = _androidAdUnitId;
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.Log("Tried to load rewarded ad before Unity Ads was initialized.");
            return;
        }

        if (isLoading || isReady)
            return;

        isLoading = true;
        Debug.Log("Loading Rewarded ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId != _adUnitId) return;

        Debug.Log("Rewarded ad loaded: " + placementId);
        isLoading = false;
        isReady = true;
        retryCount = 0;
        SetButtonInteractable(true);
    }

    public void ShowAd()
    {
        if (!isReady)
        {
            Debug.LogWarning("Rewarded ad is not ready yet!");
            LoadAd();
            return;
        }

        if (AdManager.Instance != null && AdManager.Instance.bannerAd != null &&
            AdManager.Instance.bannerAd.isBannerVisible)
        {
            AdManager.Instance.bannerAd.HideBannerAd();
        }

        isReady = false;
        SetButtonInteractable(false);
        Advertisement.Show(_adUnitId, this);
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading rewarded ad {placementId} : {error} - {message}");
        isLoading = false;
        isReady = false;
        ScheduleRetry();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing rewarded ad: {placementId} : {error} - {message}");
        RestoreTime();
        ScheduleRetry();
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on rewarded ad: " + placementId);
    }

    public void OnUnityAdsShowComplete(string placementId,
        UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId == _adUnitId &&
            showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Rewarded ad completed!");

            // Meklējam katru reizi, jo pēc ainas pārlādes vecais objekts ir iznīcināts
            if (flyingObjectManager == null)
                flyingObjectManager = FindFirstObjectByType<FlyingObjectManager>();
            if (flyingObjectManager != null)
                flyingObjectManager.DestroyAllFlyingObjects();
        }

        RestoreTime();
        retryCount = 0;
        StartCoroutine(WaitAndLoadAd(retryDelay));
    }

    private void ScheduleRetry()
    {
        if (retryCount >= maxRetries)
        {
            Debug.LogWarning("Rewarded: sasniegts mēģinājumu limits, vairs nelādējam.");
            return;
        }

        retryCount++;
        StartCoroutine(WaitAndLoadAd(retryDelay));
    }

    private IEnumerator WaitAndLoadAd(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        LoadAd();
    }

    private void RestoreTime()
    {
        if (!GameManagerScript.GameOver)
            Time.timeScale = 1f;
    }

    private void SetButtonInteractable(bool value)
    {
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = value;
    }

    public void SetButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowAd);
        _rewardedAdButton = button;
        _rewardedAdButton.interactable = isReady;
    }
}