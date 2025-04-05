using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class InfoBarController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI infoTextTMP;

    [Header("Timing Settings")]
    [SerializeField] private float displayDuration = 3.0f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private CanvasGroup infoCanvasGroup;
    private Coroutine activeDisplayCoroutine = null;

    void Awake()
    {
        infoCanvasGroup = GetComponent<CanvasGroup>();

        if (infoCanvasGroup != null)
        {
            infoCanvasGroup.alpha = 0f;
            infoCanvasGroup.interactable = false;
            infoCanvasGroup.blocksRaycasts = false;
        }
    }

    public void ShowMessage(string message)
    {
        if (activeDisplayCoroutine != null)
        {
            StopCoroutine(activeDisplayCoroutine);
            activeDisplayCoroutine = null;
        }

        SetText(message);
        infoCanvasGroup.alpha = 1f;
        activeDisplayCoroutine = StartCoroutine(DisplayAndFadeCoroutine());
    }

    private void SetText(string message)
    {
        if (infoTextTMP != null)
        {
            infoTextTMP.text = message;
        }
    }

    private IEnumerator DisplayAndFadeCoroutine()
    {
        yield return new WaitForSeconds(displayDuration);

        float elapsedTime = 0f;
        float startAlpha = infoCanvasGroup.alpha;

        if (fadeOutDuration <= 0f)
        {
            infoCanvasGroup.alpha = 0f;
        }
        else
        {
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float currentAlpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
                infoCanvasGroup.alpha = currentAlpha;
                yield return null;
            }
            infoCanvasGroup.alpha = 0f;
        }

        activeDisplayCoroutine = null;
    }

    void OnDisable()
    {
        if (activeDisplayCoroutine != null)
        {
            StopCoroutine(activeDisplayCoroutine);
            activeDisplayCoroutine = null;
        }
    }
}
