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
            SoundManager.Instance.PlayBGM(testBgmName);
            Debug.Log("BGM played: " + testBgmName);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SoundManager.Instance.StopBGM();
            Debug.Log("BGM stopped.");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SoundManager.Instance.PlaySFX(testSfxName);
            Debug.Log("SFX played: " + testSfxName);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            SoundManager.Instance.PlaySFXAtPoint(testSfxName, Camera.main.transform.position);
            Debug.Log("SFX played at camera position: " + testSfxName);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SoundManager.Instance.SetBGMVolume(0.2f);
            Debug.Log("BGM volume set to 0.2");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SoundManager.Instance.SetBGMVolume(0.8f);
            Debug.Log("BGM volume set to 0.8");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SoundManager.Instance.SetSFXVolume(0.1f);
            Debug.Log("SFX volume set to 0.1");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SoundManager.Instance.SetSFXVolume(1.0f);
            Debug.Log("SFX volume set to 1.0");
        }
    }
}
