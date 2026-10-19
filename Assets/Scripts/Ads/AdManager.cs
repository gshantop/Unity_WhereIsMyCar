using UnityEngine;

public class AdManager : MonoBehaviour
{
    public AdsInitializer AdsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAds = false;
    private bool firstAdShown = false;

    public static AdManager Instance { get; private set; }

    private void Awake()
    {
        
    }
}
