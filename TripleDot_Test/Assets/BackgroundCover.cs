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

        // Obtener ratio de la imagen (sprite)
        float imageRatio = (float)image.sprite.texture.width / image.sprite.texture.height;

        Vector2 newSize = Vector2.zero;

        if (screenRatio > imageRatio)
        {
            // Pantalla más ancha que la imagen
            newSize.x = Screen.width;
            newSize.y = Screen.width / imageRatio;
        }
        else
        {
            // Pantalla más alta que la imagen
            newSize.y = Screen.height;
            newSize.x = Screen.height * imageRatio;
        }

        // Convertir pixeles a unidades de Canvas
        // Canvas suele estar en modo Screen Space Overlay, donde 1 unidad = 1 pixel
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);

        // Centrar la imagen
        rectTransform.anchoredPosition = Vector2.zero;
    }
}
