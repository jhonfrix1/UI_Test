using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float typingSpeed = 0.05f;
    [TextArea]
    public string fullText;

    public void StartAnim()
    {
        textComponent.enabled = false;
        StartCoroutine(ShowText());
    }

    IEnumerator ShowText()
    {
        yield return null;
        textComponent.text = "";
        textComponent.enabled = true;

        foreach (char c in fullText)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
