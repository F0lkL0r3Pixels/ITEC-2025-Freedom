using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static UIManager UIManager;

    public static InputMaster InputMaster;

    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InputMaster = new();

        UIManager = GetComponentInChildren<UIManager>();
    }

    private void OnEnable()
    {
        InputMaster.Enable();
    }

    private void OnDisable()
    {
        InputMaster.Disable();
    }

    private void Start()
    {
        UIManager = GetComponentInChildren<UIManager>();
    }
}
