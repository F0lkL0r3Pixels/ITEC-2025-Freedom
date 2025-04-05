using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerMovement Movement;
    public Grappling Grappling;

    [TextArea(2, 5)]
    public string[] upgradeMessage;

    public static PlayerManager Instance;

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
    }

    public void GetUpgrade(int index)
    {
       GameManager.UIManager.infoBarController.ShowMessage(upgradeMessage[index]);

        switch (index)
        {
            case 0:
                Movement.CanChargeJump = true;
                break;
            case 1:
                Movement.CanDash = true;
                break;
            case 2:
                Grappling.enabled = true;
                break;
        }
    }

}
