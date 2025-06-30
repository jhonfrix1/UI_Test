using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BackgroundCover : MonoBehaviour
{
    private RectTransform rectTransform;
    private Image image;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        FitToScreen();
    }

    void FitToScreen()
    {
        if (image.sprite == null) return;

        float screenRatio = (float)Screen.width / Screen.height;

        float imageRatio = (float)image.sprite.texture.width / image.sprite.texture.height;

        Vector2 newSize = Vector2.zero;

        if (screenRatio > imageRatio)
        {
            newSize.x = Screen.width;
            newSize.y = Screen.width / imageRatio;
        }
        else
        {
            newSize.y = Screen.height;
            newSize.x = Screen.height * imageRatio;
        }

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);

        rectTransform.anchoredPosition = Vector2.zero;
    }
}
