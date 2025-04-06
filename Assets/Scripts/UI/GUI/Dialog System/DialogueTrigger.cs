using UnityEngine;

[RequireComponent(typeof(Collider))] // Ensure there's a collider
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Setup")]
    public DialogueStory storyToTrigger;
    public DialogueStory repeatStory;

    [Header("Trigger Conditions")]
    public bool isStory1Trigger = false;
    public bool isStory2Trigger = false;

    private Collider triggerCollider;
    private bool playerInside = false;

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Use tags or layers for more robust player detection
        if (!playerInside && other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log($"Player entered trigger: {gameObject.name}");

            // Determine which story to play based on conditions
            DialogueStory story = SelectStoryToPlay();

            if (story != null)
            {
                // Tell the manager whether this is the special auto-play zone (Story 1)
                DialogueManager.Instance.StartDialogue(story, isStory1Trigger);
            }

            // If it's the Story 1 trigger, also tell the manager the player entered the *specific* zone
            if (isStory1Trigger)
            {
                DialogueManager.Instance.PlayerEnteredZone(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (playerInside && other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log($"Player exited trigger: {gameObject.name}");

            if (isStory1Trigger)
            {
                DialogueManager.Instance.PlayerExitedZone(true);
                // We don't necessarily end the dialogue here, manager handles visibility.
            }
        }
    }

    private DialogueStory SelectStoryToPlay()
    {
        if (isStory1Trigger)
        {
            // If Story 1 is already done, show repeat message
            if (GameProgress.firstStoryCompleted)
            {
                Debug.Log("Story 1 already completed, showing repeat message.");
                return repeatStory;
            }
            else
            {
                // Play Story 1
                Debug.Log("Conditions met for Story 1.");
                return storyToTrigger;
            }
        }
        else if (isStory2Trigger)
        {
            // Must have completed Story 1 AND met the special condition
            if (GameProgress.firstStoryCompleted && GameProgress.canStartSecondStory)
            {
                Debug.Log("Conditions met for Story 2.");
                return storyToTrigger; // Play Story 2
            }
            else
            {
                // Conditions not met, show repeat message
                Debug.Log("Conditions not met for Story 2, showing repeat message.");
                return repeatStory;
            }
        }
        else
        {
            // Default trigger behaviour (if you add more triggers later)
            return storyToTrigger;
        }
    }
}