using UnityEngine;

public class PlayerManager : MonoBehaviour
{
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
}
