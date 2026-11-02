using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

// Apbalvojošā reklāma: lietotājs pats izvēlas to skatīties (poga),
// bonuss - visi lidojošie objekti (lidmašīnas, mākoņi, bumbas) tiek iznīcināti.
public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    // Tiek izsaukts, kad reklāma noskatīta līdz galam (citas ainas var dot savu bonusu)
    public static event Action OnRewardGranted;

    [SerializeField] string _androidAdUnitId = "BP_Rewarded_Android";
    string _adUnitId;

    [SerializeField] Button _rewardedAdButton;

    [SerializeField] float retryDelay = 10f;
    [SerializeField] int maxRetries = 5;
    private int retryCount = 0;
    private bool isLoading = false;
    private bool isReady = false;
    private bool isShowing = false;
    private bool pendingShow = false;

    void Awake()
    {
        _adUnitId = _androidAdUnitId;
    }

    void Start()
    {
        if (_rewardedAdButton != null)
            SetButton(_rewardedAdButton);
    }

    void Update()
    {
        // Pogu nevar spiest, kamēr reklāma rādās vai spēle beigusies
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = !isShowing && !GameManagerScript.GameOver;
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

        // Lietotājs jau nospieda pogu, kamēr reklāma lādējās
        if (pendingShow)
        {
            pendingShow = false;
            ShowAd();
        }
    }

    // Pogas On Click
    public void ShowAd()
    {
        if (isShowing || GameManagerScript.GameOver)
            return;

        if (!isReady)
        {
            Debug.Log("Rewarded ad not ready - loading, will show when loaded.");
            pendingShow = true;
            retryCount = 0;
            LoadAd();
            return;
        }

        if (AdManager.Instance != null && AdManager.Instance.bannerAd != null &&
            AdManager.Instance.bannerAd.isBannerVisible)
        {
            AdManager.Instance.bannerAd.HideBannerAd();
        }

        isReady = false;
        isShowing = true;
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
        isShowing = false;
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
        isShowing = false;
        RestoreTime();

        // Bonusu dodam tikai, ja reklāma noskatīta līdz galam
        if (placementId == _adUnitId &&
            showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Rewarded ad completed - giving reward!");
            GiveReward();
            OnRewardGranted?.Invoke();
        }

        retryCount = 0;
        LoadAd();
    }

    // Bonuss: iznīcina visus lidojošos objektus ainā
    private void GiveReward()
    {
        if (GameManagerScript.GameOver)
            return;

        FlyingObjectControllerScript[] flyingObjects =
            FindObjectsByType<FlyingObjectControllerScript>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (FlyingObjectControllerScript obj in flyingObjects)
        {
            if (obj == null)
                continue;

            if (obj.CompareTag("Bomb"))
                obj.TriggerExplosion();
            else
                obj.StartAndDestroy(Color.gray);
        }
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

    public void SetButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowAd);
        _rewardedAdButton = button;
    }
}