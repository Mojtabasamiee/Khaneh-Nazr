// ═══════════════════════════════════════════════════════════════
//  KeypadPuzzle.cs — قفل کد دیجیتال 🔢 (در اتاق مادربزرگ)
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • یک صفحه‌ی قفل کد با دکمه‌های 0-9 روی صفحه‌ی گوشی
//  • بازیکن کد را می‌زند؛ درست بود → پرچم حل + رویداد موفقیت
//  • اشتباه بود → صدای خطا + پاک شدن
//
//  کجا وصل می‌شود؟
//  ساختار Canvas (پنل تمام‌صفحه یا بزرگ):
//
//    KeypadPanel (غیرفعال در شروع)
//    ├── DisplayText   ← کد زده‌شده (****)
//    ├── Buttons       ← دکمه‌های 0-9 + پاک کردن + تایید
//    └── CloseButton
//
//  • این کد روی KeypadPanel می‌رود.
//  • دکمه‌ها در Inspector onClick → KeypadPuzzle.Press("5") و...
//  • برای باز کردن پنل: KeypadTrigger روی در/قفل می‌گذاری.
//  • کد قفل (مثلاً تاریخ نذر: 1348) را اینجا تنظیم کن!
// ═══════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class KeypadPuzzle : MonoBehaviour
{
    [Header("تنظیمات قفل")]
    [Tooltip("کد صحیح — مثلاً تاریخ نذر مادربزرگ! (فقط رقم)")]
    [SerializeField] string correctCode = "1348";
    [Tooltip("پرچمی که بعد از حل روشن می‌شود")]
    [SerializeField] string solvedFlag = "grandma_room_open";

    [Header("مرجع‌های UI")]
    [Tooltip("متن نمایش کد زده‌شده")]
    [SerializeField] Text displayText;

    [Header("صداها")]
    [SerializeField] AudioClip buttonSfx;
    [SerializeField] AudioClip errorSfx;
    [SerializeField] AudioClip successSfx;

    [Header("رویدادها")]
    [Tooltip("وقتی قفل باز شد — مثلاً صدای قفل، باز شدن در، گویندگی")]
    public UnityEvent onUnlocked;

    [Header("واکنش خطا")]
    [Tooltip("شناسه‌ی رویداد ترسناک موقع اشتباه (اختیاری — ترس از شکست!)")]
    [SerializeField] string errorScareEvent = "";
    [Tooltip("متن راهنمای خطا")]
    [SerializeField] string errorHint = "درست نیست...";

    // ── وضعیت ────────────────────────────────────────────────────
    string entered = "";
    bool solved = false;
    bool errorLock = false;

    void OnEnable()
    {
        entered = "";
        errorLock = false;
        UpdateDisplay();
    }

    // ═══ دکمه‌های عددی ═══════════════════════════════════════════
    /// <summary>فشار یک رقم — از دکمه‌های UI صدا زده می‌شود (با متن دکمه)</summary>
    public void PressDigit(string digit)
    {
        if (solved || errorLock) return;
        if (entered.Length >= correctCode.Length) return;

        PlaySfx(buttonSfx);
        entered += digit;
        UpdateDisplay();

        // اگر تعداد رقم‌ها کامل شد، خودکار چک کن
        if (entered.Length == correctCode.Length)
            StartCoroutine(CheckCodeAfterDelay(0.35f));
    }

    // ═══ دکمه‌های کنترل ══════════════════════════════════════════
    /// <summary>پاک کردن (دکمه‌ی C)</summary>
    public void Clear()
    {
        if (solved || errorLock) return;
        PlaySfx(buttonSfx);
        entered = "";
        UpdateDisplay();
    }

    /// <summary>دکمه‌ی تایید (اگر بخواهی خودکار چک نکنی)</summary>
    public void Submit()
    {
        if (solved || errorLock) return;
        CheckCode();
    }

    /// <summary>بستن پنل — دکمه‌ی ضربدر</summary>
    public void Close()
    {
        PlaySfx(buttonSfx);
        gameObject.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.Playing);
    }

    // ═══ چک کردن کد ══════════════════════════════════════════════
    IEnumerator CheckCodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CheckCode();
    }

    void CheckCode()
    {
        if (entered == correctCode)
        {
            // ═══ موفقیت! ═══
            solved = true;
            PlaySfx(successSfx);
            UpdateDisplay(); // کد را کامل نشان بده

            if (PuzzleManager.Instance != null && !string.IsNullOrEmpty(solvedFlag))
                PuzzleManager.Instance.SetFlag(solvedFlag);

            onUnlocked?.Invoke();

            // بعد از لحظه‌ای پنل بسته شود
            Invoke(nameof(Close), 0.8f);
        }
        else
        {
            // ═══ اشتباه ═══
            PlaySfx(errorSfx);
            SubtitleUI.Instance?.ShowHint(errorHint);

            if (!string.IsNullOrEmpty(errorScareEvent) && HorrorEventManager.Instance != null)
                HorrorEventManager.Instance.Trigger(errorScareEvent);

            // قفل موقت + پاک شدن
            errorLock = true;
            entered = "";
            UpdateDisplay();
            Invoke(nameof(RemoveErrorLock), 0.6f);
        }
    }

    void RemoveErrorLock() { errorLock = false; }

    void UpdateDisplay()
    {
        if (displayText == null) return;

        if (solved)
            displayText.text = "باز شد";
        else
            // به جای رقم‌های زده‌شده، ستاره نشان بده (راز!)
            displayText.text = new string('*', entered.Length);
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx2D(clip);
    }
}
