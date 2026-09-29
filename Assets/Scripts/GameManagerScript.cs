using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManagerScript : MonoBehaviour
{
    [Header("UI paneļi")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Laika teksts (spēles laikā)")]
    public TMP_Text timerText;

    [Header("Uzvaras loga elementi")]
    public TMP_Text finalTimeText;
    public GameObject[] stars;

    [Header("Zvaigžņu laika sliekšņi (sekundēs)")]
    public float threeStarTime = 30f;
    public float twoStarTime = 60f;

    [Header("Dzīvības (sirsniņas)")]
    public int maxLives = 3;
    public Image[] hearts;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    // Globāls karogs: citi skripti var pārbaudīt, vai spēle ir beigusies
    public static bool GameOver { get; private set; }

    private GameObjectsScript gameObjectsScript;
    private float elapsedTime = 0f;
    private bool gameActive = true;
    private int totalCars;
    private int placedCars = 0;
    private int lostCars = 0;

    void Start()
    {
        GameOver = false; // Atiestatām karogu jaunai spēlei

        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
        gameObjectsScript.gameManagerScript = this;

        totalCars = gameObjectsScript.dropPlaces.Length;
        placedCars = 0;
        lostCars = 0;
        elapsedTime = 0f;
        gameActive = true;

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        Time.timeScale = 1f;
        UpdateTimerText();
        SetHeartsVisible(true);
        UpdateHearts();
    }

    void Update()
    {
        if (!gameActive) return;

        elapsedTime += Time.deltaTime;
        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        if (timerText != null)
            timerText.text = FormatTime(elapsedTime);
    }

    public static string FormatTime(float seconds)
    {
        int h = Mathf.FloorToInt(seconds / 3600f);
        int m = Mathf.FloorToInt((seconds % 3600f) / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
    }

    public void RegisterCarPlaced()
    {
        if (!gameActive) return;

        placedCars++;
        if (placedCars >= totalCars)
        {
            Win();
        }
    }

    public void RegisterCarDestroyed()
    {
        if (!gameActive) return;

        lostCars++;
        totalCars--;
        UpdateHearts();

        if (lostCars >= maxLives)
        {
            Lose();
            return;
        }

        if (placedCars >= totalCars)
        {
            Win();
        }
    }

    void Win()
    {
        gameActive = false;
        FreezeGame();
        SetHeartsVisible(false);

        if (winPanel != null) winPanel.SetActive(true);
        if (finalTimeText != null) finalTimeText.text = FormatTime(elapsedTime);

        SetStars(CalculateStars(elapsedTime));
    }

    void Lose()
    {
        gameActive = false;
        FreezeGame();
        SetHeartsVisible(false);

        if (losePanel != null) losePanel.SetActive(true);
    }

    // Pilnībā iesaldē spēli: fiziku, animācijas, daļiņas un vilkšanu
    void FreezeGame()
    {
        GameOver = true;
        Time.timeScale = 0f;

        // 2D fizika: apturam visus objektus
        foreach (Rigidbody2D rb in Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
        {
            rb.simulated = false;
        }

        // 3D fizika: apturam visus objektus
        foreach (Rigidbody rb in Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None))
        {
            rb.isKinematic = true;
        }

        // Animācijas
        foreach (Animator anim in Object.FindObjectsByType<Animator>(FindObjectsSortMode.None))
        {
            anim.speed = 0f;
        }

        // Daļiņu efekti
        foreach (ParticleSystem ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
        {
            ps.Pause();
        }

        // Izslēdzam vilkšanas/klikšķu skriptus (bet ne pogas un uzvaras/zaudējuma paneļus)
        foreach (MonoBehaviour mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb == this) continue;
            if (mb is BaseInputModule) continue;
            if (mb is EventSystem) continue;
            if (mb is Selectable) continue;
            if (winPanel != null && mb.transform.IsChildOf(winPanel.transform)) continue;
            if (losePanel != null && mb.transform.IsChildOf(losePanel.transform)) continue;

            if (mb is IDragHandler ||
                mb is IBeginDragHandler ||
                mb is IEndDragHandler ||
                mb is IPointerDownHandler ||
                mb is IPointerUpHandler)
            {
                mb.enabled = false;
            }
        }
    }

    int CalculateStars(float time)
    {
        if (time <= threeStarTime) return 3;
        if (time <= twoStarTime) return 2;
        return 1;
    }

    void SetStars(int count)
    {
        if (stars == null) return;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
                stars[i].SetActive(i < count);
        }
    }

    void UpdateHearts()
    {
        if (hearts == null) return;

        int livesLeft = Mathf.Max(0, maxLives - lostCars);

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            Sprite sprite = (i < livesLeft) ? fullHeartSprite : emptyHeartSprite;
            if (sprite != null)
                hearts[i].sprite = sprite;
        }
    }

    void SetHeartsVisible(bool visible)
    {
        if (hearts == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
                hearts[i].gameObject.SetActive(visible);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}