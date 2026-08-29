using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class UICoverImage : MonoBehaviour
{
    [SerializeField] private RectTransform viewport;

    private RectTransform rectTransform;
    private Image image;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        if (viewport == null && transform.parent != null)
            viewport = transform.parent as RectTransform;

        ApplyCover();
    }

    private void Start()
    {
        ApplyCover();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyCover();
    }

    public void ApplyCover()
    {
        if (viewport == null || image == null || image.sprite == null)
            return;

        Rect spriteRect = image.sprite.rect;
        float spriteW = spriteRect.width;
        float spriteH = spriteRect.height;

        float parentW = viewport.rect.width;
        float parentH = viewport.rect.height;

        if (spriteW <= 0f || spriteH <= 0f || parentW <= 0f || parentH <= 0f)
            return;

        float scale = Mathf.Max(parentW / spriteW, parentH / spriteH);

        float finalW = spriteW * scale;
        float finalH = spriteH * scale;

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(finalW, finalH);
        rectTransform.anchoredPosition = Vector2.zero;
    }
}