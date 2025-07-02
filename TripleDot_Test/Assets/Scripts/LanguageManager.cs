using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageCycler : MonoBehaviour
{
    private int currentLocaleIndex = 0;
    private bool isChanging = false;

    public void CycleToNextLanguage()
    {
        if (isChanging) return;
        isChanging = true;

        int totalLocales = LocalizationSettings.AvailableLocales.Locales.Count;
        currentLocaleIndex++;

        if (currentLocaleIndex >= totalLocales)
            currentLocaleIndex = 0;

        var nextLocale = LocalizationSettings.AvailableLocales.Locales[currentLocaleIndex];

        LocalizationSettings.SelectedLocale = nextLocale;

        isChanging = false;
    }
}