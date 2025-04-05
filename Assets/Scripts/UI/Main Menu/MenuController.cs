using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("Levels To Load")]
    public string _newGameLevel;
    private string levelToLoad;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private const string BGM_VOLUME_KEY = "BgmVolume";
    private const string SFX_VOLUME_KEY = "SfxVolume";

    [Header ("Volume Setting")]
    [SerializeField] private TMP_Text volumeBGTextValue = null;
    [SerializeField] private TMP_Text volumeSFXTextValue = null;
    [SerializeField] private Slider volumeBGSlider = null;
    [SerializeField] private Slider volumeSFXSlider = null;
    [SerializeField] private float defaultBGVolume = 0.5f;
    [SerializeField] private float defaultSFXVolume = 0.5f;

    [SerializeField] private GameObject noSavedGameDialog = null;
    [SerializeField] private GameObject comfirmationPrompt = null;
    [SerializeField] private float comfirmationPromptShowTime = 1f;

    public void StartNewGame()
    {
        SceneManager.LoadScene(_newGameLevel);
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SavedLevel"))
        {
            levelToLoad = PlayerPrefs.GetString("SavedLevel");
            SceneManager.LoadScene(levelToLoad);
        }
        else
        {
            noSavedGameDialog.SetActive(true);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void SetBGVolume(float volume)
    {
        bgmSource.volume = volume;
        volumeBGTextValue.text = FloatToVolumeString(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        volumeSFXTextValue.text = FloatToVolumeString(volume);
    }

    public void ResetButton(string menuType) {

        if (menuType == "Audio") {
            bgmSource.volume = defaultBGVolume;
            sfxSource.volume = defaultSFXVolume;
            volumeBGSlider.value = defaultBGVolume;
            volumeSFXSlider.value = defaultSFXVolume;
            volumeBGTextValue.text = FloatToVolumeString(defaultBGVolume);
            volumeSFXTextValue.text = FloatToVolumeString(defaultSFXVolume);
            VolumeApply();
        }
    }

    public void VolumeApply()
    {
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmSource.volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxSource.volume);
        StartCoroutine(ConfirmationBox());
    }

    public IEnumerator ConfirmationBox()
    {
        comfirmationPrompt.SetActive(true);
        yield return new WaitForSeconds(comfirmationPromptShowTime);
        comfirmationPrompt.SetActive(false);
    }

    private string FloatToVolumeString(float volume) {
        return volume.ToString("0.00");
    }
}