// ═══════════════════════════════════════════════════════════════
//  GameSettings.cs — تنظیمات بازی (زیرنویس، صدا، حساسیت نگاه)
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • یک کلاس «استاتیک» است — یعنی لازم نیست روی هیچ شیئی بگذاری‌اش!
//    از هر جای کد با GameSettings.SubtitlesEnabled به آن دسترسی داری.
//  • تنظیمات را در PlayerPrefs گوشی ذخیره می‌کند (بین اجراها می‌ماند)
//
//  کجا وصل می‌شود؟
//  • خودکار توسط GameManager بارگذاری می‌شود.
//  • اسلایدرها و دکمه‌های سند «SettingsUI» مقادیر اینجا را عوض می‌کنند.
//  • AudioManager موقع پخش هر صدا، ولوم را از همین‌جا می‌خواند.
// ═══════════════════════════════════════════════════════════════

using UnityEngine;

public static class GameSettings
{
    // ── کلیدهای ذخیره در PlayerPrefs ─────────────────────────────
    const string KEY_SUBTITLES  = "khaneh_subtitles";
    const string KEY_SENSITIVITY = "khaneh_sensitivity";
    const string KEY_MASTER     = "khaneh_vol_master";
    const string KEY_MUSIC      = "khaneh_vol_music";
    const string KEY_SFX        = "khaneh_vol_sfx";
    const string KEY_VOICE      = "khaneh_vol_voice";

    // ── مقادیر پیش‌فرض ────────────────────────────────────────────
    public static bool  SubtitlesEnabled { get; set; } = true;
    public static float LookSensitivity  { get; set; } = 1.0f;   // 0.3 تا 3.0
    public static float MasterVolume     { get; set; } = 1.0f;
    public static float MusicVolume      { get; set; } = 0.8f;
    public static float SfxVolume        { get; set; } = 1.0f;
    public static float VoiceVolume      { get; set; } = 1.0f;

    /// <summary>آیا حداقل یک بار تنظیمات ذخیره شده؟ (برای تشخیص بار اول)</summary>
    public static bool WasLoaded { get; private set; } = false;

    // ═══ بارگذاری از حافظه‌ی گوشی ═══════════════════════════════
    public static void Load()
    {
        SubtitlesEnabled = PlayerPrefs.GetInt(KEY_SUBTITLES, 1) == 1;
        LookSensitivity  = PlayerPrefs.GetFloat(KEY_SENSITIVITY, 1.0f);
        MasterVolume     = PlayerPrefs.GetFloat(KEY_MASTER, 1.0f);
        MusicVolume      = PlayerPrefs.GetFloat(KEY_MUSIC, 0.8f);
        SfxVolume        = PlayerPrefs.GetFloat(KEY_SFX, 1.0f);
        VoiceVolume      = PlayerPrefs.GetFloat(KEY_VOICE, 1.0f);
        WasLoaded = true;
    }

    // ═══ ذخیره در حافظه‌ی گوشی ═══════════════════════════════════
    public static void Save()
    {
        PlayerPrefs.SetInt(KEY_SUBTITLES, SubtitlesEnabled ? 1 : 0);
        PlayerPrefs.SetFloat(KEY_SENSITIVITY, LookSensitivity);
        PlayerPrefs.SetFloat(KEY_MASTER, MasterVolume);
        PlayerPrefs.SetFloat(KEY_MUSIC, MusicVolume);
        PlayerPrefs.SetFloat(KEY_SFX, SfxVolume);
        PlayerPrefs.SetFloat(KEY_VOICE, VoiceVolume);
        PlayerPrefs.Save();

        // هر وقت تنظیمات عوض شد، ولوم‌های پخش‌کننده‌ی صدا هم فوری آپدیت شود
        if (AudioManager.Instance != null)
            AudioManager.Instance.ApplyVolumes();
    }

    /// <summary>بازگشت به تنظیمات پیش‌فرض</summary>
    public static void ResetToDefault()
    {
        SubtitlesEnabled = true;
        LookSensitivity  = 1.0f;
        MasterVolume     = 1.0f;
        MusicVolume      = 0.8f;
        SfxVolume        = 1.0f;
        VoiceVolume      = 1.0f;
        Save();
    }
}
