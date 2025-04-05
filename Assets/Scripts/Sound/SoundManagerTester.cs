using UnityEngine;

public class SoundManagerTester : MonoBehaviour
{
    [Header("Test Clip Names")]
    [SerializeField] private string testBgmName = "bgm";
    [SerializeField] private string testSfxName = "click";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            GameManager.SoundManager.PlayBGM(testBgmName);
            Debug.Log("BGM played: " + testBgmName);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            GameManager.SoundManager.StopBGM();
            Debug.Log("BGM stopped.");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            GameManager.SoundManager.PlaySFX(testSfxName);
            Debug.Log("SFX played: " + testSfxName);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            GameManager.SoundManager.PlaySFXAtPoint(testSfxName, Camera.main.transform.position);
            Debug.Log("SFX played at camera position: " + testSfxName);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameManager.SoundManager.SetBGMVolume(0.2f);
            Debug.Log("BGM volume set to 0.2");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GameManager.SoundManager.SetBGMVolume(0.8f);
            Debug.Log("BGM volume set to 0.8");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GameManager.SoundManager.SetSFXVolume(0.1f);
            Debug.Log("SFX volume set to 0.1");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            GameManager.SoundManager.SetSFXVolume(1.0f);
            Debug.Log("SFX volume set to 1.0");
        }
    }
}
