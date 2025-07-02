using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PopupBlurManager : MonoBehaviour
{
    public RawImage blurBackground;
    public GameObject popupPanel;
    public Material blurMaterial;

    private Texture2D capturedTexture;

    [Range(0f, 1f)]
    public float darkness = 0.6f;

    public void ShowPopup()
    {
        StartCoroutine(CaptureWithUI());
    }

    public void HidePopup()
    {
        popupPanel.SetActive(false);
        blurBackground.texture = null;
        blurBackground.gameObject.SetActive(false);

        if (capturedTexture != null)
        {
            Destroy(capturedTexture);
            capturedTexture = null;
        }
    }

    IEnumerator CaptureWithUI()
    {
        yield return new WaitForEndOfFrame();

        capturedTexture = ScreenCapture.CaptureScreenshotAsTexture();

        blurBackground.texture = capturedTexture;
        blurBackground.material = blurMaterial;
        blurMaterial.SetFloat("_Darkness", darkness);
        blurBackground.gameObject.SetActive(true);

        popupPanel.SetActive(true);
    }
}
