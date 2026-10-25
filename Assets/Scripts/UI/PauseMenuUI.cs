// ═══════════════════════════════════════════════════════════════
//  PauseMenuUI.cs — منوی مکث داخل بازی ⏸️
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • دکمه‌ی مکث HUD → بازی متوقف می‌شود (زمان می‌ایستد)
//  • منو: ادامه / تنظیمات / منوی اصلی / خروج
//  • موقع مکث، پلیر و ترس‌ها متوقف می‌شوند
//
//  کجا وصل می‌شود؟
//  Canvas صحنه‌ی House:
//
//    PausePanel (غیرفعال در شروع)
//    ├── ResumeButton  (ادامه)
//    ├── SettingsPanel (پنل تنظیمات — می‌توانی همان پنل مشترک را بگذاری)
//    ├── MenuButton    (منوی اصلی)
//    └── QuitButton    (خروج)
//
//  • این کد روی PausePanel می‌رود.
//  • دکمه‌ی مکث HUD → onClick → PauseMenuUI.PauseGame
// ═══════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance { get; private set; }

    [Header("پنل")]
    [SerializeField] GameObject panel;

    [Header("پنل تنظیمات (اختیاری)")]
    [Tooltip("اگر داخل منوی مکث، پنل تنظیمات جدا داری")]
    [SerializeField] GameObject settingsPanel;

    [Header("ذخیره‌ی خودکار")]
    [Tooltip("موقع مکث، بازی ذخیره شود؟ (پیشنهاد: بله)")]
    [SerializeField] bool saveOnPause = true;

    void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    void Start()
    {
        // اتصال در Start تا GameManager (که از صحنه‌ی قبل می‌آید) حتماً موجود باشد
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChange;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChange;
    }

    // ═══ نمایش/مخفی شدن خودکار با وضعیت بازی ═════════════════════
    void HandleStateChange(GameState newState)
    {
        if (panel == null) return;

        if (newState == GameState.Paused)
        {
            panel.SetActive(true);
            if (saveOnPause) SaveSystem.SaveGame();
        }
        else
        {
            panel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }
    }

    // ═══ دکمه‌ها ═════════════════════════════════════════════════

    /// <summary>دکمه‌ی مکث روی HUD → این صدا زده می‌شود</summary>
    public void PauseGame()
    {
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.Playing)
            GameManager.Instance.SetState(GameState.Paused);
    }

    /// <summary>دکمه‌ی ادامه</summary>
    public void ResumeGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.Playing);
    }

    /// <summary>نمایش/مخفی کردن تنظیمات داخل منوی مکث</summary>
    public void ToggleSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    /// <summary>برگشت به منوی اصلی (ذخیره‌ی خودکار دارد)</summary>
    public void BackToMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.BackToMenu();
    }

    /// <summary>خروج از بازی</summary>
    public void QuitGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
    }
}
