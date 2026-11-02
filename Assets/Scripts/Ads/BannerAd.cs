using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class BannerAd : MonoBehaviour
{
    [SerializeField] string _AndroidAdUnitId = "BP_Banner_Android";
    string _adUnitId;

    [SerializeField] Button _bannerButton;
    public bool isBannerVisible = false;

    [SerializeField] BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;

    [SerializeField] float retryDelay = 10f;
    [SerializeField] int maxRetries = 5;
    private int retryCount = 0;
    private bool isLoaded = false;

    void Awake()
    {
        _adUnitId = _AndroidAdUnitId;
        Advertisement.Banner.SetPosition(bannerPosition);
    }

    public void LoadBanner()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.Log("Tried to load banner ad before Unity ads was initialized");
            return;
        }

        if (isLoaded)
        {
            SetButtonInteractable(true);
            return;
        }

        BannerLoadOptions bannerOptions = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(_adUnitId, bannerOptions);
    }

    void OnBannerLoaded()
    {
        Debug.Log("Banner ad loaded!");
        isLoaded = true;
        retryCount = 0;
        SetButtonInteractable(true);
    }

    // Bez tūlītējas atkārtotas ielādes - tikai ar aizturi un limitu
    void OnBannerError(string message)
    {
        Debug.Log($"Banner error: {message}");
        isLoaded = false;

        if (retryCount >= maxRetries)
        {
            Debug.LogWarning("Banner: sasniegts mēģinājumu limits, vairs nelādējam.");
            return;
        }

        retryCount++;
        StartCoroutine(RetryAfterDelay());
    }

    IEnumerator RetryAfterDelay()
    {
        yield return new WaitForSecondsRealtime(retryDelay);
        LoadBanner();
    }

    public void ShowBannerAd()
    {
        if (isBannerVisible)
        {
            HideBannerAd();
            return;
        }

        BannerOptions bannerOptions = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };
        Advertisement.Banner.Show(_adUnitId, bannerOptions);
    }

    void OnBannerClicked()
    {
        Debug.Log("User clicked on banner ad: " + _adUnitId);
    }

    void OnBannerHidden()
    {
        Debug.Log("Banner ad is hidden! " + _adUnitId);
        isBannerVisible = false;
    }

    void OnBannerShown()
    {
        Debug.Log("Banner ad is visible! " + _adUnitId);
        isBannerVisible = true;
    }

    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
        isBannerVisible = false;
    }

    private void SetButtonInteractable(bool value)
    {
        if (_bannerButton != null)
            _bannerButton.interactable = value;
    }

    public void SetButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowBannerAd);
        _bannerButton = button;
        _bannerButton.interactable = isLoaded;
    }
}