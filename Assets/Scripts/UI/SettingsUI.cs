// ═══════════════════════════════════════════════════════════════
//  SettingsUI.cs — پنل تنظیمات ⚙️
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • همه‌ی تنظیمات بازی را وصل می‌کند:
//      ★ زیرنویس فارسی: روشن / خاموش (Toggle)
//      ★ حساسیت نگاه (Slider)
//      ★ صدای کل / موسیقی / افکت / گفتار (Slider)
//  • تغییرات فوری اعمال و ذخیره می‌شوند
//
//  کجا وصل می‌شود؟
//  پنل تنظیمات (در منوی اصلی و منوی مکث — می‌تواند یک پنل مشترک باشد):
//
//    SubtitlesToggle   → این کد را خودش در Start وصل می‌کند
//    SensitivitySlider → خودش وصل می‌شود
//    MasterSlider / MusicSlider / SfxSlider / VoiceSlider → خودش وصل می‌شود
//
//  • فقط مراجع را در Inspector بده — کد خودش همه را متصل می‌کند!
//  • این کد روی پنل تنظیمات بگذار.
// ═══════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("زیرنویس")]
    [Tooltip("دکمه‌ی Toggle زیرنویس")]
    [SerializeField] Toggle subtitlesToggle;

    [Header("حساسیت نگاه")]
    [SerializeField] Slider sensitivitySlider;
    [Tooltip("متن عدد حساسیت (اختیاری)")]
    [SerializeField] Text sensitivityValueText;

    [Header("ولوم‌ها")]
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider voiceSlider;

    [Header("دکمه‌ی پیش‌فرض")]
    [SerializeField] Button resetButton;

    void Start()
    {
        // ── زیرنویس ──
        if (subtitlesToggle != null)
        {
            subtitlesToggle.isOn = GameSettings.SubtitlesEnabled;
            subtitlesToggle.onValueChanged.AddListener(OnSubtitlesChanged);
        }

        // ── حساسیت ──
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0.3f;
            sensitivitySlider.maxValue = 3.0f;
            sensitivitySlider.value = GameSettings.LookSensitivity;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            UpdateSensitivityText();
        }

        // ── ولوم‌ها ──
        if (masterSlider != null)
        {
            masterSlider.value = GameSettings.MasterVolume;
            masterSlider.onValueChanged.AddListener(OnMasterChanged);
        }
        if (musicSlider != null)
        {
            musicSlider.value = GameSettings.MusicVolume;
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = GameSettings.SfxVolume;
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }
        if (voiceSlider != null)
        {
            voiceSlider.value = GameSettings.VoiceVolume;
            voiceSlider.onValueChanged.AddListener(OnVoiceChanged);
        }

        // ── بازگشت به پیش‌فرض ──
        if (resetButton != null)
            resetButton.onClick.AddListener(OnReset);
    }

    // ═══ رویدادهای تغییر ═════════════════════════════════════════

    void OnSubtitlesChanged(bool value)
    {
        GameSettings.SubtitlesEnabled = value;
        GameSettings.Save();
    }

    void OnSensitivityChanged(float value)
    {
        GameSettings.LookSensitivity = value;
        UpdateSensitivityText();
        GameSettings.Save();
    }

    void UpdateSensitivityText()
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = GameSettings.LookSensitivity.ToString("0.0");
    }

    void OnMasterChanged(float v) { GameSettings.MasterVolume = v; GameSettings.Save(); }
    void OnMusicChanged(float v)  { GameSettings.MusicVolume  = v; GameSettings.Save(); }
    void OnSfxChanged(float v)    { GameSettings.SfxVolume    = v; GameSettings.Save(); }
    void OnVoiceChanged(float v)  { GameSettings.VoiceVolume  = v; GameSettings.Save(); }

    void OnReset()
    {
        GameSettings.ResetToDefault();

        // آپدیت UI
        if (subtitlesToggle != null) subtitlesToggle.isOn = GameSettings.SubtitlesEnabled;
        if (sensitivitySlider != null) sensitivitySlider.value = GameSettings.LookSensitivity;
        if (masterSlider != null) masterSlider.value = GameSettings.MasterVolume;
        if (musicSlider != null) musicSlider.value = GameSettings.MusicVolume;
        if (sfxSlider != null) sfxSlider.value = GameSettings.SfxVolume;
        if (voiceSlider != null) voiceSlider.value = GameSettings.VoiceVolume;
        UpdateSensitivityText();
    }
}
