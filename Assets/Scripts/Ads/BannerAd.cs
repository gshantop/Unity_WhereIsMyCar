using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

// Banera reklāma: tiek rādīta nepārtraukti fiksētā pozīcijā (augšā centrā),
// netraucējot spēlei. Pēc pilnekrāna reklāmām atgriežas automātiski.
public class BannerAd : MonoBehaviour
{
    [SerializeField] string _AndroidAdUnitId = "BP_Banner_Android";
    string _adUnitId;

    [Tooltip("Fiksētā banera pozīcija ekrānā (augšā centrā - tur netraucē pogām un mašīnām)")]
    [SerializeField] BannerPosition bannerPosition = BannerPosition.TOP_CENTER;

    [SerializeField] float retryDelay = 10f;
    [SerializeField] int maxRetries = 5;

    public bool isBannerVisible = false;

    private int retryCount = 0;
    private bool isLoading = false;
    private bool isLoaded = false;
    private bool showRequested = false;

    // Lietotājs pats paslēpa baneri ar pogu "BannerAd" - tad automātiski neatgriežam
    private bool hiddenByUser = false;

    void Awake()
    {
        _adUnitId = _AndroidAdUnitId;
        Advertisement.Banner.SetPosition(bannerPosition);
    }

    void OnEnable()
    {
        StartCoroutine(KeepBannerVisible());
    }

    // Ik pēc brīža pārbaudām: ja baneris ielādēts, bet paslēpts
    // (piem., pēc pilnekrāna reklāmas) - parādām to atkal
    IEnumerator KeepBannerVisible()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.5f);

        while (true)
        {
            yield return wait;

            if (isLoaded && !isBannerVisible && !showRequested && !hiddenByUser &&
                Advertisement.isInitialized)
                ShowBannerAd();
        }
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
            if (!hiddenByUser)
                ShowBannerAd();
            return;
        }

        if (isLoading)
            return;

        isLoading = true;

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
        isLoading = false;
        isLoaded = true;
        retryCount = 0;
        if (!hiddenByUser)
            ShowBannerAd();
    }

    void OnBannerError(string message)
    {
        Debug.Log($"Banner error: {message}");
        isLoading = false;
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
        if (!isLoaded || isBannerVisible || showRequested)
            return;

        showRequested = true;

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
        showRequested = false;
    }

    void OnBannerShown()
    {
        Debug.Log("Banner ad is visible! " + _adUnitId);
        isBannerVisible = true;
        showRequested = false;
    }

    // Poga "BannerAd": paslēpj / atkal parāda baneri
    public void ToggleBanner()
    {
        if (isBannerVisible || showRequested)
        {
            hiddenByUser = true;
            HideBannerAd();
        }
        else
        {
            hiddenByUser = false;
            if (isLoaded)
                ShowBannerAd();
            else
                LoadBanner();
        }
    }

    // Izmanto pilnekrāna reklāmas; pēc tām baneris atgriežas pats (KeepBannerVisible)
    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
        isBannerVisible = false;
        showRequested = false;
    }
}