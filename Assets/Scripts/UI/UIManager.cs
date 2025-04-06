using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Element References")]
    public StaminaBarUI staminaBarUI;
    public CrosshairController crosshairController;
    public InfoBarController infoBarController;
    public GameObject endScreen;
   

    public void ToggleEnd()
    {
        PlayerManager.Instance.Movement.enabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        endScreen.SetActive(true);
        StartCoroutine(End());
    }

    public IEnumerator End()
    {
        yield return new WaitForSeconds(5.25f);
        SceneManager.LoadScene(0);
    }
}