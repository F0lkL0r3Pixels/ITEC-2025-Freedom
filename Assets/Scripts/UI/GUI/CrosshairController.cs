using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Elements")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private RectTransform crosshairRectTransform;

    [Header("Appearance Settings")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color alternateColor = Color.red;
    [SerializeField] private float scaleIncreaseFactor = 0.1f;

    private Vector3 defaultScale;
    private Vector3 alternateScale;

    void Awake()
    {
        if (crosshairRectTransform == null)
        {
            Debug.LogError("CrosshairController: Crosshair RectTransform is not assigned!", this);
            this.enabled = false;
            return;
        }
        if (crosshairImage == null)
        {
            Debug.LogError("CrosshairController: No Crosshair Images assigned!", this);
            this.enabled = false;
            return;
        }

        defaultScale = crosshairRectTransform.localScale;
        alternateScale = defaultScale * (1f + scaleIncreaseFactor);
        ApplyState(false);
    }

    public void ApplyState(bool isAlternateState)
    {
        Color targetColor = isAlternateState ? alternateColor : defaultColor;
        Vector3 targetScale = isAlternateState ? alternateScale : defaultScale;

        crosshairImage.color = targetColor;

        if (crosshairRectTransform != null)
        {
            crosshairRectTransform.localScale = targetScale;
        }
    }
}
