using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonsScript : MonoBehaviour
{
    public void PlayGame()
    {
        // Ainas maiņa caur AdManager - pirms tās tiek parādīta pilnekrāna reklāma
        AdManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Poga "Hanojas tornis" galvenajā izvēlnē
    public void PlayHanoi()
    {
        int index = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/HanoiScene.unity");
        if (index < 0)
        {
            Debug.LogError("HanoiScene nav pievienota File -> Build Settings sarakstam!");
            return;
        }

        AdManager.LoadScene(index);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}