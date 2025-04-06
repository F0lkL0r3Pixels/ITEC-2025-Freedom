using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FinalGameTrigger : MonoBehaviour
{
    public GameObject objectToDisable;
    public DialogueTrigger dialogueTrigger;

    private Collider triggerCollider;
    private bool alreadyTriggered = false;

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();

        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning($"Collider on {gameObject.name} is not set to 'Is Trigger'. ConditionTrigger requires a trigger collider.", this);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the trigger hasn't fired yet AND if the colliding object is the Player
        if (!alreadyTriggered && other.CompareTag("Player")) // Make sure your player GameObject has the tag "Player"
        {
            alreadyTriggered = true;

            objectToDisable.SetActive(false);

            dialogueTrigger.isStory1Trigger = false;
            dialogueTrigger.isStory2Trigger = true;

            // 3. Change the GameProgress flag
            GameProgress.canStartSecondStory = true;
            Debug.Log("ConditionTrigger: GameProgress.canStartStory2 has been set to TRUE.");
        }
    }
}