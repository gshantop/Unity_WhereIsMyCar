using UnityEngine;
using UnityEngine.SceneManagement;
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

    private GameObjectsScript gameObjectsScript;
    private float elapsedTime = 0f;
    private bool gameActive = true;
    private int totalCars;
    private int placedCars = 0;

    void Start()
    {
        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
        gameObjectsScript.gameManagerScript = this;

        totalCars = gameObjectsScript.dropPlaces.Length;
        placedCars = 0;
        elapsedTime = 0f;
        gameActive = true;

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        Time.timeScale = 1f;
        UpdateTimerText();
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

    // Sauc DropPlaceScript, kad mašīna nolikta pareizajā vietā
    public void RegisterCarPlaced()
    {
        if (!gameActive) return;

        placedCars++;
        if (placedCars >= totalCars)
        {
            Win();
        }
    }

    // Sauc FlyingObjectControllerScript, kad mašīna tiek iznīcināta lidojoša objekta dēļ
    public void RegisterCarDestroyed()
    {
        if (!gameActive) return;
        Lose();
    }

    void Win()
    {
        gameActive = false;
        Time.timeScale = 0f;

        if (winPanel != null) winPanel.SetActive(true);
        if (finalTimeText != null) finalTimeText.text = FormatTime(elapsedTime);

        SetStars(CalculateStars(elapsedTime));
    }

    void Lose()
    {
        gameActive = false;
        Time.timeScale = 0f;

        if (losePanel != null) losePanel.SetActive(true);
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