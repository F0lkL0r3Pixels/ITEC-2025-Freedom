using UnityEngine;

public class InfoBarTester : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.U;
    [SerializeField] private string testMessage = "Hello from InfoBarTester!";

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (GameManager.UIManager.infoBarController != null)
            {
                GameManager.UIManager.infoBarController.ShowMessage(testMessage);
                Debug.Log("Message shown: " + testMessage);
            }
            else
            {
                Debug.LogWarning("InfoBarController reference is missing.");
            }
        }
    }
}
