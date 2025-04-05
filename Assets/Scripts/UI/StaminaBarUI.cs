using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Slider), typeof(CanvasGroup))]
public class StaminaBarUI : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("Duration of the fade in/out animation in seconds.")]
    public float fadeDuration = 0.3f;
    [Tooltip("Delay in seconds after reaching full stamina before starting the fade out.")]
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

    // --- Component References ---
    private Slider staminaSlider;
    private CanvasGroup staminaCanvasGroup;
    private Image fillImage; // Reference to the Slider's Fill Image

    // --- Internal State Tracking ---
    private float previousStaminaValue = -1f;
    private float currentMaxValue = 100f;
    private bool isBarVisible = false;
    private bool wasPreviouslyFull = true;
    private bool hideTimerRunning = false;
    private bool isInCriticalState = false;
    private Coroutine activeFadeCoroutine;
    private Coroutine activeHideDelayCoroutine;


    void Awake()
    {
        staminaSlider = GetComponent<Slider>();
        staminaCanvasGroup = GetComponent<CanvasGroup>();

        // Find the Fill Image component
        Transform fillTransform = staminaSlider.fillRect; // RectTransform of the fill area
        if (fillTransform != null)
        {
            fillImage = fillTransform.GetComponentInChildren<Image>(); // Usually the Image is on a child named "Fill"
        }

        if (fillImage == null)
        {
            Debug.LogError("StaminaBarPollingUI: Could not find the 'Fill' Image component within the Slider's fillRect.", this);
        }

        staminaCanvasGroup.alpha = 0f;
        staminaCanvasGroup.interactable = false;
        staminaCanvasGroup.blocksRaycasts = false;
        isBarVisible = false;
        isInCriticalState = false;

        staminaSlider.interactable = false;

        previousStaminaValue = -1f;

        UpdateFillColor();
    }

    public void UpdateStamina(float currentValue, float maxValue)
    {
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

        float criticalThresholdValue = currentMaxValue * criticalThresholdPercentage;

        if (Mathf.Approximately(currentValue, 0f))
        {
            isInCriticalState = true;
        }
        else if (isInCriticalState && currentValue > criticalThresholdValue)
        {
            isInCriticalState = false;
        }

        if (valueChanged)
        {
            staminaSlider.value = currentValue;
        }

        if (valueChanged || maxChanged)
        {
            UpdateFillColor();
        }

        bool isCurrentlyFull = IsFull(currentValue);
        bool staminaDecreased = currentValue < previousStaminaValue;

        if (staminaDecreased || (!isCurrentlyFull && !isBarVisible))
        {
            if (staminaDecreased)
            {
                StopHideProcess();
            }
            RequestFade(true);
        }

        if (isCurrentlyFull && !wasPreviouslyFull)
        {
            StartHideProcess();
        }
        else if (!isCurrentlyFull && wasPreviouslyFull)
        {
            StopHideProcess();
            RequestFade(true);
        }

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

    private void RequestFade(bool fadeIn)
    {
        float targetAlpha = fadeIn ? 1f : 0f;

        if (activeFadeCoroutine == null && Mathf.Approximately(staminaCanvasGroup.alpha, targetAlpha))
        {
            if (fadeIn) isBarVisible = true;
            return;
        }

        if (activeFadeCoroutine != null)
        {
            StopCoroutine(activeFadeCoroutine);
        }

        isBarVisible = fadeIn;
        activeFadeCoroutine = StartCoroutine(FadeCanvasGroup(targetAlpha, fadeDuration));
    }

    private IEnumerator FadeCanvasGroup(float targetAlpha, float duration)
    {
        float startAlpha = staminaCanvasGroup.alpha;
        float elapsedTime = 0f;

        if (duration <= 0f)
        {
            staminaCanvasGroup.alpha = targetAlpha;
            activeFadeCoroutine = null;
            yield break;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            staminaCanvasGroup.alpha = newAlpha;
            yield return null;
        }

        staminaCanvasGroup.alpha = targetAlpha;
        activeFadeCoroutine = null;
    }

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

        if (IsFull(previousStaminaValue))
        {
            RequestFade(false);
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

        if (activeFadeCoroutine != null && !isBarVisible)
        {
            StopCoroutine(activeFadeCoroutine);
            activeFadeCoroutine = null;
        }
    }

    private bool IsFull(float value)
    {
        return value >= currentMaxValue - fullTolerance;
    }

    public void SetCriticalThresholdPercentage(float newThresholdPercentage)
    {
        criticalThresholdPercentage = Mathf.Clamp01(newThresholdPercentage);
    }
}