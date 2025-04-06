using UnityEngine;

public class AntennaPickup : MonoBehaviour
{

    [SerializeField] private DialogueTrigger dialogueTrigger;
    [SerializeField] private GameObject antennaOnRobot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, 360f * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (dialogueTrigger != null)
        { 
            if(other.CompareTag("Player"))
            {
                dialogueTrigger.isStory1Trigger = false;
                dialogueTrigger.isStory2Trigger = true;
                GameProgress.canStartSecondStory = true;

                GameManager.UIManager.infoBarController.ShowMessage("Antenna acquired!\nReturn to the robot");

                SoundManager.Instance.PlaySFX("upgrade");

                

                antennaOnRobot.SetActive(true);

                gameObject.SetActive(false);
            }
        }
    }
}
