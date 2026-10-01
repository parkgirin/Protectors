using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeSettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject currencyPopup;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject colorPalettePanel;
    [SerializeField] private GameObject soundSettingsPanel;
    [SerializeField] private GameObject licensePanel;
    [SerializeField] private GameObject popupBackdrop;
    [SerializeField] private TMP_InputField customHexInput;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    void Awake()
    {
        BindVolumeSlider(masterVolumeSlider, SetMasterVolume);
        BindVolumeSlider(musicVolumeSlider, SetMusicVolume);
        BindVolumeSlider(sfxVolumeSlider, SetSfxVolume);
        RefreshVolumeControls();
    }

    void ShowMenu()
    {
        menuPanel?.SetActive(true);
        colorPalettePanel?.SetActive(false);
        soundSettingsPanel?.SetActive(false);
        licensePanel?.SetActive(false);
    }

    public void SelectStandardPalette() => PaletteSettings.SetMode(PaletteMode.Standard);
    public void SelectSoftPalette() => PaletteSettings.SetMode(PaletteMode.Soft);
    public void ApplyCustomAccent()
    {
        if (customHexInput == null) return;

        string hex = customHexInput.text.Trim().TrimStart('#');
        if (!PaletteSettings.TrySetCustomAccentHex("#" + hex))
            Debug.LogWarning("커스텀 색상은 #RRGGBB 형식으로 입력해야 합니다.", customHexInput);
    }

    public void OpenColorPalette() { menuPanel?.SetActive(false); colorPalettePanel?.SetActive(true); }
    public void CloseColorPalette()
    {
        colorPalettePanel?.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(true);
        else settingsPanel?.SetActive(false);
    }
    public void OpenSoundSettings()
    {
        menuPanel?.SetActive(false);
        soundSettingsPanel?.SetActive(true);
        RefreshVolumeControls();
    }
    public void CloseSoundSettings() { soundSettingsPanel?.SetActive(false); menuPanel?.SetActive(true); }
    public void OpenLicenses() { menuPanel?.SetActive(false); licensePanel?.SetActive(true); }
    public void SetMasterVolume(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(value);
        else
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(AudioManager.MasterVolumeKey, value);
            PlayerPrefs.Save();
        }
    }
    public void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);
        else SaveVolume(AudioManager.MusicVolumeKey, value);
    }

    public void SetSfxVolume(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(value);
        else SaveVolume(AudioManager.SfxVolumeKey, value);
    }
    public void OpenSettings()
    {
        currencyPopup?.SetActive(false);
        popupBackdrop?.SetActive(true);
        settingsPanel?.SetActive(true);
        settingsPanel?.transform.SetAsLastSibling();
        ShowMenu();
    }
    public void OpenCurrencyPopup()
    {
        settingsPanel?.SetActive(false);
        popupBackdrop?.SetActive(true);
        currencyPopup?.SetActive(true);
        currencyPopup?.transform.SetAsLastSibling();
    }
    public void CloseCurrencyPopup()
    {
        currencyPopup?.SetActive(false);
        popupBackdrop?.SetActive(false);
    }
    public void CloseSettings()
    {
        if (colorPalettePanel != null && colorPalettePanel.activeSelf) { CloseColorPalette(); return; }
        if (soundSettingsPanel != null && soundSettingsPanel.activeSelf) { CloseSoundSettings(); return; }
        if (licensePanel != null && licensePanel.activeSelf) { licensePanel.SetActive(false); menuPanel?.SetActive(true); return; }
        settingsPanel?.SetActive(false);
        popupBackdrop?.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currencyPopup != null && currencyPopup.activeSelf) CloseCurrencyPopup();
            else if (settingsPanel != null && settingsPanel.activeSelf) CloseSettings();
        }
    }

    private void RefreshVolumeControls()
    {
        float master = PlayerPrefs.GetFloat(AudioManager.MasterVolumeKey, 1f);
        float music = PlayerPrefs.GetFloat(AudioManager.MusicVolumeKey, 1f);
        float sfx = PlayerPrefs.GetFloat(AudioManager.SfxVolumeKey, 1f);
        AudioListener.volume = Mathf.Clamp01(master);
        masterVolumeSlider?.SetValueWithoutNotify(master);
        musicVolumeSlider?.SetValueWithoutNotify(music);
        sfxVolumeSlider?.SetValueWithoutNotify(sfx);
    }

    private static void BindVolumeSlider(Slider slider, UnityEngine.Events.UnityAction<float> onChanged)
    {
        if (slider == null) return;
        slider.onValueChanged.RemoveListener(onChanged);
        slider.onValueChanged.AddListener(onChanged);
    }

    private static void SaveVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }
}
