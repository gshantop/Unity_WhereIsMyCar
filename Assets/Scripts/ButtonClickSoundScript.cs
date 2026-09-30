using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSoundScript : MonoBehaviour
{
    public AudioClip clickSound;
    [Range(0f, 1f)] public float volume = 1f;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        if (clickSound == null) return;

        GameObject go = new GameObject("ClickSound");
        DontDestroyOnLoad(go);

        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = clickSound;
        src.volume = volume;
        src.spatialBlend = 0f;
        src.Play();

        Destroy(go, clickSound.length + 0.1f);
    }
}