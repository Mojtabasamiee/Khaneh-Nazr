// ═══════════════════════════════════════════════════════════════
//  MainMenuUI.cs — منوی اصلی بازی
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • دکمه‌های منوی اصلی: بازی جدید / ادامه / تنظیمات / خروج
//  • دکمه‌ی «ادامه» فقط وقتی فعال است که ذخیره‌ای موجود باشد
//  • نسخه‌ی بازی را هم نشان می‌دهد
//
//  کجا وصل می‌شود؟
//  صحنه‌ی MainMenu — ساختار Canvas:
//
//    TitleText        (عنوان: خانه‌ی نذر)
//    NewGameButton    → MainMenuUI.OnNewGame
//    ContinueButton   → MainMenuUI.OnContinue
//    SettingsButton   → باز کردن پنل تنظیمات (خودت در Inspector وصل کن)
//    QuitButton       → MainMenuUI.OnQuit
//    VersionText
//
//  • این کد روی یک GameObject خالی (مثلاً GameManager صحنه‌ی منو) بگذار
//    و مراجع را بده. اتصال دکمه‌ها را در Inspector (onClick) انجام می‌دهی.
// ═══════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("دکمه‌های منو")]
    [SerializeField] Button newGameButton;
    [SerializeField] Button continueButton;
    [SerializeField] Button quitButton;

    [Header("نمایش نسخه")]
    [SerializeField] Text versionText;

    [Header("ظاهر دکمه‌ی ادامه")]
    [Tooltip("رنگ کم‌رنگ برای وقتی ذخیره‌ای نیست")]
    [SerializeField] Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);

    void Start()
    {
        // دکمه‌ها را به توابع وصل کن (نیازی به Inspector نیست!)
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGame);
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);

        // فعال بودن دکمه‌ی «ادامه» بسته به وجود ذخیره
        bool hasSave = SaveSystem.HasSave();
        if (continueButton != null)
        {
            continueButton.interactable = hasSave;
            var img = continueButton.GetComponent<Image>();
            if (img != null && !hasSave) img.color = disabledColor;
            var lbl = continueButton.GetComponentInChildren<Text>();
            if (lbl != null && !hasSave) lbl.color = disabledColor;
        }

        // نمایش نسخه
        if (versionText != null)
            versionText.text = "نسخه‌ی " + Application.version;
    }

    // ═══ عملکرد دکمه‌ها ══════════════════════════════════════════

    public void OnNewGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartNewGame();
    }

    public void OnContinue()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ContinueGame();
    }

    public void OnQuit()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
    }
}
