using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrefsLoadingController : MonoBehaviour
{
    [Header("General Setting")]
    [SerializeField] private bool canUse = false;
    [SerializeField] private MenuController menuController;

    [Header("Volume Setting")]
    [SerializeField] private TMP_Text bgVolumeTextValue = null;
    [SerializeField] private TMP_Text sfxVolumeTextValue = null;
    [SerializeField] private Slider bgVolumeSlider = null;
    [SerializeField] private Slider sfxVolumeSlider = null;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private const string BGM_VOLUME_KEY = "BgmVolume";
    private const string SFX_VOLUME_KEY = "SfxVolume";

    private void Awake()
    {
        if (canUse)
        {
            if (PlayerPrefs.HasKey(BGM_VOLUME_KEY) && PlayerPrefs.HasKey(SFX_VOLUME_KEY))
            {
                float bgVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY);
                float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY);
                bgVolumeTextValue.text = bgVolume.ToString("0.00");
                sfxVolumeTextValue.text = sfxVolume.ToString("0.00");
                bgVolumeSlider.value = bgVolume;
                sfxVolumeSlider.value = sfxVolume;

                bgmSource.volume = bgVolume;
                sfxSource.volume = sfxVolume;
            }
            else
            {
                menuController.ResetButton("Audio");
            }
        }
    }
}
