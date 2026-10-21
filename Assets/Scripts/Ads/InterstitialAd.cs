using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class InterstitialAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{

    [SerializeField] string _androidAdUnityId = "Interstitial_Android";
    string _adUnitAd;
    public event Action OnIntertitialAdReady;
    public bool isReady = false;
    [SerializeField] Button _interstitialAdButton;
    
    void Awake()
    {
        _androidAdUnityId = _androidAdUnityId;
    }

    private void Update()
    {
        if(AdManager.Instance != null && AdManager.Instance.interstitialAd != null) 
        {
            _interstitialAdButton.interactable = AdManager.Instance.interstitialAd.isReady;
        }
    }

    public void OnInterstitialAdbuttonClicked()
    {
        Debug.Log("Interstitial Ad button Clicked");
        ShowInterstitial();
    }

    public void ShowInterstitial()
    {
        if(AdManager.Instance.interstitialAd != null &&
            AdManager.Instance.interstitialAd.isReady)
        {
            Debug.Log("Showing interstitial ad manually!");
            ShowAd();
        }
        else
        {
            Debug.LogWarning("Interstitial ad is not ready yet, loading again!");
            LoadAd();
        }
    }

    public void ShowAd()
    {
        if (isReady)
        {
            Advertisement.Show(_androidAdUnityId, this);
            isReady = false;

        }
        else
        {
            Debug.LogWarning("Interstitial ad is not ready yet!");
        }
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Tried to load interstitial ad before Unity Ads was initialized!");
            return;
        }

        Debug.Log("Loading interstitial ad...");
        Advertisement.Load(_androidAdUnityId, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Interstitial ad loaded successfully!");
        _interstitialAdButton.interactable = true;
        isReady = true;
        OnIntertitialAdReady?.Invoke();
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Failed to load interstitial ad: {error.ToString()} - {message}");
        LoadAd();
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("Interstitial ad clicked!");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Interstitial ad completed successfully!");
            StartCoroutine(SlowDownTimeTemporarily(30f));
            LoadAd();
        }
        else
        {
            Debug.LogWarning($"Interstitial ad was not completed: {showCompletionState.ToString()}");
            Time.timeScale = 1f;
            LoadAd();
        }
    }

    private IEnumerator SlowDownTimeTemporarily(float duration)
    {
        Time.timeScale = 0.3f;
        Debug.Log("Time slowed down for " + duration + " seconds.");
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        Debug.Log("Time restored to normal.");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Failed to show interstitial ad: {error.ToString()} - {message}");
        LoadAd();
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log("Interstitial ad startet showing!");
        Time.timeScale = 0f;
    }

    public void setButton(Button button)
    {
        if (button = null)
            return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnInterstitialAdbuttonClicked);
        _interstitialAdButton = button;
        _interstitialAdButton.interactable = false;
    }
}
