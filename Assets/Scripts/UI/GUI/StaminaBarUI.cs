using UnityEngine;
using UnityEngine.UI; // Keep this for Slider, Image
using TMPro; // Add this if using TextMeshPro
using System.Collections;

[RequireComponent(typeof(Slider), typeof(CanvasGroup))]
public class StaminaBarUI : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("Duration of the fade in/out animation for the bar in seconds.")]
    public float fadeDuration = 0.3f;
    [Tooltip("Delay in seconds after reaching full stamina before starting the fade out for the bar.")]
    public float hideDelay = 1.5f;
    [Tooltip("Small tolerance for checking if stamina is full.")]
    public float fullTolerance = 0.01f;

    [Header("Color Settings")]
    [Tooltip("The default color of the stamina bar fill.")]
    public Color defaultColor = Color.green;
    [Tooltip("The color when stamina is zero or below the critical threshold during regen.")]
    public Color criticalColor = Color.red;
    [Tooltip("Percentage (0-1) of max stamina below which the critical color is used during regen.")]
    [Range(0f, 1f)]
    public float criticalThresholdPercentage = 0.1f; // 10%

    // --- NEW: Critical Warning Settings ---
    [Header("Critical Warning Settings")]
    [Tooltip("Assign the CanvasGroup of the Text element used for the critical warning.")]
    public CanvasGroup criticalWarningCanvasGroup; // Assign in Inspector
    // If using TextMeshPro, use this instead:
    // public TextMeshProUGUI criticalWarningText; // Assign TextMeshPro component
    // public Text criticalWarningText; // Or assign regular Text component
    [Tooltip("Duration of one full fade in/out pulse cycle for the warning text.")]
    public float warningPulseDuration = 0.8f;
    [Tooltip("The minimum alpha value during the pulse (slightly visible).")]
    [Range(0f, 1f)]
    public float warningMinAlpha = 0.1f;
    [Tooltip("The maximum alpha value during the pulse (fully visible).")]
    [Range(0f, 1f)]
    public float warningMaxAlpha = 1.0f;
    [Tooltip("Duration for the warning text to fade out completely when exiting critical state.")]
    public float warningFadeOutDuration = 0.2f;


    // --- Component References ---
    private Slider staminaSlider;
    private CanvasGroup staminaBarCanvasGroup; // Renamed for clarity
    private Image fillImage; // Reference to the Slider's Fill Image

    // --- Internal State Tracking ---
    private float previousStaminaValue = -1f;
    private float currentMaxValue = 100f;
    private bool isBarVisible = false;
    private bool wasPreviouslyFull = true;
    private bool hideTimerRunning = false;
    private bool isInCriticalState = false; // Tracks the critical state
    private bool isWarningPulsing = false; // Tracks if the warning is currently pulsing

    // --- Coroutine References ---
    private Coroutine activeBarFadeCoroutine;
    private Coroutine activeHideDelayCoroutine;
    private Coroutine activeWarningPulseCoroutine; // Coroutine for the warning pulse
    private Coroutine activeWarningFadeCoroutine; // Coroutine for warning fade in/out


    void Awake()
    {
        staminaSlider = GetComponent<Slider>();
        staminaBarCanvasGroup = GetComponent<CanvasGroup>(); // Use specific name

        // Find the Fill Image component
        Transform fillTransform = staminaSlider.fillRect;
        if (fillTransform != null)
        {
            fillImage = fillTransform.GetComponentInChildren<Image>();
        }
        if (fillImage == null)
        {
            Debug.LogError("StaminaBarUI: Could not find the 'Fill' Image component within the Slider's fillRect.", this);
        }

        // --- Null Check for Critical Warning ---
        if (criticalWarningCanvasGroup == null)
        {
            Debug.LogError("StaminaBarUI: Critical Warning Canvas Group is not assigned in the Inspector!", this);
        }
        else
        {
            criticalWarningCanvasGroup.alpha = 0f; // Ensure warning is hidden initially
            criticalWarningCanvasGroup.interactable = false;
            criticalWarningCanvasGroup.blocksRaycasts = false;
        }
        // ---

        staminaBarCanvasGroup.alpha = 0f;
        staminaBarCanvasGroup.interactable = false;
        staminaBarCanvasGroup.blocksRaycasts = false;
        isBarVisible = false;

        staminaSlider.interactable = false;
        isInCriticalState = false;
        isWarningPulsing = false;

        previousStaminaValue = -1f;

        // Set initial color
        UpdateFillColor(); // Assume full initially
    }

    public void UpdateStamina(float currentValue, float maxValue)
    {
        // --- Clamping and Max Value Update ---
        maxValue = Mathf.Max(0.1f, maxValue);
        currentValue = Mathf.Clamp(currentValue, 0f, maxValue);

        bool maxChanged = false;
        if (!Mathf.Approximately(currentMaxValue, maxValue))
        {
            currentMaxValue = maxValue;
            staminaSlider.maxValue = currentMaxValue;
            wasPreviouslyFull = IsFull(previousStaminaValue);
            maxChanged = true;
        }

        bool valueChanged = !Mathf.Approximately(previousStaminaValue, currentValue);

        // --- Update Critical State Logic & Warning Trigger ---
        float criticalThresholdValue = currentMaxValue * criticalThresholdPercentage;
        bool previouslyCritical = isInCriticalState; // Store state before check

        if (Mathf.Approximately(currentValue, 0f))
        {
            isInCriticalState = true;
        }
        else if (isInCriticalState && currentValue > criticalThresholdValue)
        {
            isInCriticalState = false;
        }

        // --- Handle Warning State Change ---
        if (isInCriticalState && !previouslyCritical)
        {
            // Just entered critical state
            StartWarningPulse();
        }
        else if (!isInCriticalState && previouslyCritical)
        {
            // Just exited critical state
            StopWarningPulse(true); // Fade out the warning
        }
        // ---

        // --- Update UI Elements ---
        if (valueChanged)
        {
            staminaSlider.value = currentValue;
        }

        if (valueChanged || maxChanged || (isInCriticalState != previouslyCritical) /* Update color if critical state changed */ )
        {
            UpdateFillColor();
        }

        // --- Bar Visibility Logic ---
        bool isCurrentlyFull = IsFull(currentValue);
        bool staminaDecreased = currentValue < previousStaminaValue;

        if (staminaDecreased || (!isCurrentlyFull && !isBarVisible))
        {
            if (staminaDecreased)
            {
                StopHideProcess();
            }
            RequestBarFade(true);
        }

        if (isCurrentlyFull && !wasPreviouslyFull)
        {
            StartHideProcess();
        }
        else if (!isCurrentlyFull && wasPreviouslyFull)
        {
            StopHideProcess();
            RequestBarFade(true); // Show bar if it dipped below full
        }

        // --- Update Previous Value ---
        if (valueChanged)
        {
            previousStaminaValue = currentValue;
        }
        wasPreviouslyFull = isCurrentlyFull;
    }

    private void UpdateFillColor()
    {
        if (fillImage == null) return;
        Color targetColor = isInCriticalState ? criticalColor : defaultColor;
        if (fillImage.color != targetColor)
        {
            fillImage.color = targetColor;
        }
    }

    // --- Bar Fading Logic ---
    private void RequestBarFade(bool fadeIn)
    {
        float targetAlpha = fadeIn ? 1f : 0f;

        // Check if already at target or no fade needed
        if (activeBarFadeCoroutine == null && Mathf.Approximately(staminaBarCanvasGroup.alpha, targetAlpha))
        {
            if (fadeIn) isBarVisible = true;
            // If already faded out and requested fade out again, do nothing.
            // If already faded in and requested fade in again, do nothing but ensure state.
            return;
        }

        if (activeBarFadeCoroutine != null)
        {
            StopCoroutine(activeBarFadeCoroutine);
            activeBarFadeCoroutine = null;
        }

        isBarVisible = fadeIn; // Update visibility state *before* starting fade
        activeBarFadeCoroutine = StartCoroutine(FadeCanvasGroup(staminaBarCanvasGroup, targetAlpha, fadeDuration, () => activeBarFadeCoroutine = null));
    }


    // --- Hide Delay Logic ---
    private void StartHideProcess()
    {
        if (isBarVisible && activeHideDelayCoroutine == null && !hideTimerRunning)
        {
            activeHideDelayCoroutine = StartCoroutine(HideBarAfterDelay());
        }
    }

    private IEnumerator HideBarAfterDelay()
    {
        hideTimerRunning = true;
        yield return new WaitForSeconds(hideDelay);

        // Double-check if still full *after* the delay
        if (IsFull(staminaSlider.value)) // Use current slider value for check
        {
            RequestBarFade(false);
        }

        hideTimerRunning = false;
        activeHideDelayCoroutine = null;
    }

    private void StopHideProcess()
    {
        if (activeHideDelayCoroutine != null)
        {
            StopCoroutine(activeHideDelayCoroutine);
            activeHideDelayCoroutine = null;
        }
        hideTimerRunning = false;

        // If a fade-out was in progress, stop it.
        if (activeBarFadeCoroutine != null && !isBarVisible)
        {
            StopCoroutine(activeBarFadeCoroutine);
            activeBarFadeCoroutine = null;
            // Don't force alpha=0 here, let RequestBarFade(true) handle showing it again if needed
        }
    }

    // --- NEW: Critical Warning Logic ---

    private void StartWarningPulse()
    {
        if (criticalWarningCanvasGroup == null || isWarningPulsing) return; // Exit if no group or already pulsing

        isWarningPulsing = true;

        // Stop any previous fade out coroutine for the warning
        if (activeWarningFadeCoroutine != null)
        {
            StopCoroutine(activeWarningFadeCoroutine);
            activeWarningFadeCoroutine = null;
        }
        // Stop any existing pulse (shouldn't happen if isWarningPulsing is checked, but safe)
        if (activeWarningPulseCoroutine != null)
        {
            StopCoroutine(activeWarningPulseCoroutine);
        }

        // Start the new pulse
        activeWarningPulseCoroutine = StartCoroutine(WarningPulseCoroutine());
    }

    private void StopWarningPulse(bool fadeOut)
    {
        if (criticalWarningCanvasGroup == null || !isWarningPulsing) return; // Exit if no group or not pulsing

        isWarningPulsing = false;

        // Stop the pulse coroutine
        if (activeWarningPulseCoroutine != null)
        {
            StopCoroutine(activeWarningPulseCoroutine);
            activeWarningPulseCoroutine = null;
        }

        // Stop any other fade coroutine targeting the warning group
        if (activeWarningFadeCoroutine != null)
        {
            StopCoroutine(activeWarningFadeCoroutine);
            activeWarningFadeCoroutine = null;
        }


        if (fadeOut)
        {
            // Start fade out using the generic fade coroutine
            activeWarningFadeCoroutine = StartCoroutine(FadeCanvasGroup(criticalWarningCanvasGroup, 0f, warningFadeOutDuration, () => activeWarningFadeCoroutine = null));
        }
        else
        {
            // Instantly hide if not fading out
            criticalWarningCanvasGroup.alpha = 0f;
        }
    }

    private IEnumerator WarningPulseCoroutine()
    {
        if (criticalWarningCanvasGroup == null) yield break;

        float halfPulseDuration = warningPulseDuration / 2f;
        if (halfPulseDuration <= 0) halfPulseDuration = 0.1f; // Avoid division by zero

        // Initial fade in before starting the loop
        yield return StartCoroutine(FadeCanvasGroup(criticalWarningCanvasGroup, warningMaxAlpha, halfPulseDuration, null)); // Fade in initially


        while (isWarningPulsing) // Loop while in critical state
        {
            // Fade Out to Min
            yield return StartCoroutine(FadeCanvasGroup(criticalWarningCanvasGroup, warningMinAlpha, halfPulseDuration, null));
            if (!isWarningPulsing) break; // Check again after fade

            // Fade In to Max
            yield return StartCoroutine(FadeCanvasGroup(criticalWarningCanvasGroup, warningMaxAlpha, halfPulseDuration, null));
            if (!isWarningPulsing) break; // Check again after fade
        }
        // Coroutine finishes naturally when isWarningPulsing becomes false outside
        activeWarningPulseCoroutine = null; // Ensure reference is cleared if loop exits
    }


    // --- Generic Helper Coroutine ---
    // Modified to accept the target CanvasGroup and an optional callback Action
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration, System.Action onComplete = null)
    {
        if (cg == null) yield break; // Safety check

        float startAlpha = cg.alpha;
        float elapsedTime = 0f;

        // Instantly set alpha if duration is zero or less
        if (duration <= 0f)
        {
            cg.alpha = targetAlpha;
            onComplete?.Invoke();
            yield break;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            cg.alpha = newAlpha;
            yield return null;
        }

        cg.alpha = targetAlpha; // Ensure target alpha is reached
        onComplete?.Invoke(); // Execute the callback
    }

    // --- Utility Functions ---
    private bool IsFull(float value)
    {
        // Use currentMaxValue from the class member
        return value >= currentMaxValue - fullTolerance;
    }

    public void SetCriticalThresholdPercentage(float newThresholdPercentage)
    {
        criticalThresholdPercentage = Mathf.Clamp01(newThresholdPercentage);
        // Potentially re-evaluate critical state immediately if needed, though UpdateStamina handles it
        // UpdateFillColor(); // Color might change based on new threshold
    }

    // --- Cleanup ---
    void OnDisable()
    {
        // Stop all coroutines when the object is disabled to prevent errors
        StopAllCoroutines();
        // Reset flags just in case
        activeBarFadeCoroutine = null;
        activeHideDelayCoroutine = null;
        activeWarningPulseCoroutine = null;
        activeWarningFadeCoroutine = null;
        hideTimerRunning = false;
        isWarningPulsing = false;
        // Optionally reset alpha states if needed upon re-enable,
        // but Awake handles initial state which is usually sufficient.
        // if (staminaBarCanvasGroup) staminaBarCanvasGroup.alpha = 0;
        // if (criticalWarningCanvasGroup) criticalWarningCanvasGroup.alpha = 0;
    }
}