using UnityEngine;

public class Upgrade : MonoBehaviour
{
    public int index;

    void Update()
    {
        transform.Rotate(0f, 0f, 360f * Time.deltaTime);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<PlayerManager>().GetUpgrade(index);
            SoundManager.Instance.PlaySFX("upgrade");
            gameObject.SetActive(false);
        }
    }
}
