using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] string _androidGameId = "800388732";
    [SerializeField] bool _testMode = true;
    private string _gameId;
    public event Action OnAdsInitialized;

    // Lai inicializācija netiktu palaista divreiz (piem., ja ainā ir otrs AdScriptHolder)
    private static bool initializationStarted = false;

    public bool IsInitialized => Advertisement.isInitialized;

    void Awake()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        _gameId = _androidGameId;
#endif

        if (string.IsNullOrEmpty(_gameId))
        {
            Debug.LogWarning("AdsInitializer: Game ID nav norādīts!");
            return;
        }

        if (initializationStarted || Advertisement.isInitialized || !Advertisement.isSupported)
            return;

        initializationStarted = true;
        Advertisement.Initialize(_gameId, _testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        OnAdsInitialized?.Invoke();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        initializationStarted = false;
        Debug.LogError($"Unity Ads Initialization Failed: {error} - {message}");
    }
}