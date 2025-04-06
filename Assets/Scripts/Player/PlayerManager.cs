using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Vector3 savedPosition;

    [TextArea(2, 5)]
    public string[] upgradeMessage;

    public PlayerMovement Movement;
    public Grappling Grappling;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RespawnAtCheckpoint();
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

    public void SavePosition(Vector3 pos)
    {
        savedPosition = pos;
    }

    public void RespawnAtCheckpoint()
    {
        transform.position = savedPosition;
    }
}
