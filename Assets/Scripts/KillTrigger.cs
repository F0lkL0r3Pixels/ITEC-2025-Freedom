using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerManager.Instance.RespawnAtCheckpoint();
        }
    }
}
