using System;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Rendering;
using UnityEngine.UI;

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
        throw new NotImplementedException();
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        throw new NotImplementedException();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        throw new NotImplementedException();
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        throw new NotImplementedException();
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        throw new NotImplementedException();
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        throw new NotImplementedException();
    }
}
