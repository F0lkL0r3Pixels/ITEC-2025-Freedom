using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public CanvasGroup dialogueCanvasGroup;

    [Header("Dialogue Settings")]
    public float typeSpeed = 0.04f;
    public float sentenceDelay = 0.8f;
    public float fadeDuration = 0.5f;

    private Queue<string> sentenceQueue = new Queue<string>();
    private DialogueStory currentStory;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private bool isFading = false;
    private bool playerInAutoZone = false;
    private Coroutine typingCoroutine;
    private Coroutine fadeCoroutine;
    private Coroutine autoPlayCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (dialogueCanvasGroup != null)
        {
            dialogueCanvasGroup.alpha = 0f;
            dialogueCanvasGroup.interactable = false;
            dialogueCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("DialogueManager: Dialogue Canvas Group is not assigned!");
        }
        if (dialogueText == null)
        {
            Debug.LogError("DialogueManager: Dialogue Text is not assigned!");
        }
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    public void StartDialogue(DialogueStory story, bool isAutoPlay = false)
    {
        if (isDialogueActive || isFading)
        {
            Debug.LogWarning("DialogueManager: Tried to start new dialogue while one is already active or fading.");
            // If want to interrupt: Call StopCurrentDialogue the continue.
            return;
        }

        Debug.Log($"Starting dialogue: {story.storyName}");
        currentStory = story;
        playerInAutoZone = isAutoPlay;

        sentenceQueue.Clear();
        foreach (string sentence in story.sentences)
        {
            sentenceQueue.Enqueue(sentence);
        }

        isDialogueActive = true;

        // Stop any lingering fade and start fade in
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 1f, fadeDuration));

        // Start processing sentences
        // If it's auto play, use a specific coroutine, otherwise just display first sentence
        if (isAutoPlay)
        {
            if (autoPlayCoroutine != null) StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = StartCoroutine(AutoPlaySentences());
        }
        else
        {
            // Auto-advance once started, can be used to wait for Player input
            if (autoPlayCoroutine != null) StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = StartCoroutine(AutoPlaySentences());
            // DisplayNextSentence(); // Alternative: show first sentence and wait for input/trigger
        }
    }

    private IEnumerator AutoPlaySentences()
    {
        yield return fadeCoroutine;

        while (sentenceQueue.Count > 0)
        {
            // If it's an auto-play zone AND the player left, pause here but keep dialogue active
            // The loop continues internally, but waits for player reentry to *show* text again.
            // The UI fading is handled by PlayerExitedZone/PlayerEnteredZone
            yield return new WaitUntil(() => playerInAutoZone || !currentStoryRequiresPlayerPresence()); // Wait if player needs to be in zone and isn't

            string sentence = sentenceQueue.Dequeue();
            yield return DisplaySentence(sentence); // Type out the sentence

            // If it's an auto-play zone AND the player left *during* typing, we might still need to wait
            yield return new WaitUntil(() => playerInAutoZone || !currentStoryRequiresPlayerPresence());

            // Don't proceed to next sentence immediately if player is out of zone
            if (!playerInAutoZone && currentStoryRequiresPlayerPresence())
            {
                // Player left, wait indefinitely until they re-enter or dialogue is manually ended
                yield return new WaitUntil(() => playerInAutoZone || !isDialogueActive);
                if (!isDialogueActive) yield break; // Stop if dialogue ended externally
            }

            // Wait between sentences if player is present OR if presence isn't required
            if (playerInAutoZone || !currentStoryRequiresPlayerPresence())
            {
                yield return new WaitForSeconds(sentenceDelay);
            }
        }

        EndDialogue();
    }

    private bool currentStoryRequiresPlayerPresence()
    {
        // For now, ONLY the story started with isAutoPlay=true requires presence
        // Modify this logic if you have other types of zones/dialogues
        return playerInAutoZone;
    }

    private IEnumerator DisplaySentence(string sentence)
    {
        dialogueText.text = ""; // Clear previous text
        isTyping = true;
        typingCoroutine = StartCoroutine(TypeText(sentence));
        yield return typingCoroutine;
        typingCoroutine = null;
        isTyping = false;
    }


    private IEnumerator TypeText(string sentence)
    {
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    public void EndDialogue(bool triggeredByExit = false)
    {
        if (!isDialogueActive) return;

        Debug.Log($"Ending dialogue: {currentStory?.storyName ?? "N/A"}");

        // Stop all related coroutines cleanly
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }

        isTyping = false;
        isDialogueActive = false;

        // Update progress flags ONLY if the dialogue completed naturally (not by player leaving zone early)
        // Current AutoPlaySentences loop finishes even if player leaves, so EndDialogue is always natural completion.
        // Adjust this if you change the logic to abort dialogue on exit.
        if (currentStory != null && currentStory.name == "Story1") // Use asset name or a specific ID
        {
            GameProgress.firstStoryCompleted = true;
            GameProgress.canStartSecondStory= true;
            Debug.Log("Story 1 Marked as Completed.");
        }

        // Don't fade out immediately if triggered by exit and player is still out
        // Fade out handled by PlayerExitedZone in that case.
        // Only fade out here if ending naturally or player is back in the zone.
        if (!triggeredByExit || playerInAutoZone)
        {
            if (dialogueCanvasGroup.alpha > 0) // Only fade if visible
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0f, fadeDuration));
            }
        }

        // Clean up state AFTER potential fade starts
        sentenceQueue.Clear();


        if(currentStory?.storyName == "Second Story")
        {
            GameManager.UIManager.ToggleEnd();
        }

        currentStory = null;
        // Reset playerInAutoZone only when dialogue truly ends or a new one starts.
        // playerInAutoZone = false; // Let PlayerExitedZone handle this for its specific case
    }

    public void StopCurrentDialogue()
    {
        if (isDialogueActive)
        {
            EndDialogue(true);
        }
    }

    public void PlayerEnteredZone(bool isAutoPlayZone)
    {
        if (isAutoPlayZone)
        {
            playerInAutoZone = true;
            Debug.Log("Player entered Auto Zone.");
            // If dialogue is active but UI was hidden, fade it back in
            if (isDialogueActive && !isFading && dialogueCanvasGroup.alpha < 1f)
            {
                Debug.Log("Fading dialogue back in.");
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 1f, fadeDuration));
            }
        }
    }

    public void PlayerExitedZone(bool isAutoPlayZone)
    {
        if (isAutoPlayZone)
        {
            playerInAutoZone = false;
            Debug.Log("Player exited Auto Zone.");
            // If dialogue is active, fade out the UI but DON'T stop the dialogue logic yet
            if (isDialogueActive && !isFading && dialogueCanvasGroup.alpha > 0f)
            {
                Debug.Log("Fading dialogue out (player left zone).");
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0f, fadeDuration));
            }
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration)
    {
        isFading = true;
        float startAlpha = cg.alpha;
        float time = 0f;

        // Ensure interactability matches visibility goal
        cg.interactable = targetAlpha > 0;
        cg.blocksRaycasts = targetAlpha > 0;


        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null; // Wait for the next frame
        }

        cg.alpha = targetAlpha; // Ensure it reaches the target value
        isFading = false;
        fadeCoroutine = null; // Mark as completed
    }
}