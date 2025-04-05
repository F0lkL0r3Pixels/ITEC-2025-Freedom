using UnityEngine;
using UnityEngine.UI;

public class CrosshairControllerTester : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Y;

    private bool isAlternate = false;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isAlternate = !isAlternate;
            GameManager.UIManager.crosshairController.ApplyState(isAlternate);
        }
    }
}
