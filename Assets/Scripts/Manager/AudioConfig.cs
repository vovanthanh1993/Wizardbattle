using UnityEngine;

public class AudioConfig : MonoBehaviour
{
    public void PlayButtonCloseSound()
    {
        AudioManager.Instance.PlayButtonCloseSound();
    }

    public void PlayButtonClickSound() {
        AudioManager.Instance.PlayButtonClickSound();
    }

    public void PlayButtonChangeSound()
    {
        AudioManager.Instance.PlayButtonChangeSound();
    }
}
