// ═══════════════════════════════════════════════════════════════
//  SequencePuzzle.cs — معمای «ترتیب درست» 🧩 (مغز همه‌ی معماها)
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • یک معمای توالی می‌سازد: چند چیز باید «به ترتیب درست» تعامل شوند.
//  • با همین یک سیستم، این‌ها ساخته می‌شوند:
//      → ترتیب باز کردن آینه‌ها
//      → ترتیب روشن کردن شمع‌ها
//      → ترتیب بیت‌های فال حافظ
//      → ترتیب جاگذاری عکس‌های خانوادگی
//  • اشتباه = کل معما ریست می‌شود (+ رویداد ترسناک اختیاری!)
//
//  کجا وصل می‌شود؟
//  • روی یک GameObject خالی به اسم معما (مثل Puzzle_Mirrors)
//  • تعداد گام‌ها را بده + پرچم پایان
//  • هر SequenceButton (آینه، شمع، بیت شعر...) باید به همین شیء اشاره کند
//    و «ترتیب صحیح» خودش را داشته باشد (0 = اولین گام)
//  • در Inspector می‌توانی به رویدادها کار بدهی:
//      onStepCorrect → مثلاً روشن شدن چراغ آن گام
//      onMistake → صدای ریست + رویداد ترسناک
//      onSolved → صدای موفقیت + باز شدن قفل بعدی
// ═══════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using UnityEngine.Events;

public class SequencePuzzle : MonoBehaviour
{
    [Header("مشخصات معما")]
    [Tooltip("نام معما (فقط برای نمایش)")]
    [SerializeField] string puzzleName = "معمای جدید";
    [Tooltip("تعداد گام‌های معما (چند چیز باید به ترتیب تعامل شوند؟)")]
    [SerializeField] int stepCount = 3;
    [Tooltip("پرچمی که بعد از حل معما روشن می‌شود (برای داستان و درها)")]
    [SerializeField] string solvedFlag = "";

    [Header("پاداش و مجازات")]
    [Tooltip("شناسه‌ی رویداد ترسناک موقع اشتباه (خالی = بدون ترس)")]
    [SerializeField] string mistakeScareEvent = "";
    [Tooltip("شناسه‌ی گویندگی موقع حل شدن (خالی = بدون گویندگی)")]
    [SerializeField] string solvedVoiceId = "";

    [Header("رویدادها (در Inspector وصل کن)")]
    /// <summary>یک گام درست زده شد (شماره‌ی گام از ۰)</summary>
    public IntEvent onStepCorrect;
    /// <summary>اشتباه شد — همه چیز ریست می‌شود</summary>
    public UnityEvent onMistake;
    /// <summary>معما کامل حل شد!</summary>
    public UnityEvent onSolved;

    /// <summary>UnityEvent با عدد</summary>
    [Serializable]
    public class IntEvent : UnityEvent<int> { }

    // ── وضعیت ────────────────────────────────────────────────────
    int currentStep = 0;
    bool solved = false;

    public bool IsSolved => solved;
    public int CurrentStep => currentStep;

    // ═══ یک گام ثبت شد (SequenceButton این را صدا می‌زند) ════════
    public void SubmitStep(int answerIndex)
    {
        if (solved) return;

        // ── درست؟ ──
        if (answerIndex == currentStep)
        {
            onStepCorrect?.Invoke(answerIndex);
            currentStep++;

            // معما تمام شد؟
            if (currentStep >= stepCount)
                CompletePuzzle();
        }
        // ── اشتباه! ──
        else
        {
            currentStep = 0;
            onMistake?.Invoke();

            if (!string.IsNullOrEmpty(mistakeScareEvent) && HorrorEventManager.Instance != null)
                HorrorEventManager.Instance.Trigger(mistakeScareEvent);

            SubtitleUI.Instance?.ShowHint("مثل اینکه ترتیبش این نبود...");
        }
    }

    // ═══ پایان موفق ══════════════════════════════════════════════
    void CompletePuzzle()
    {
        solved = true;
        onSolved?.Invoke();

        // پرچم داستانی روشن شود
        if (!string.IsNullOrEmpty(solvedFlag) && PuzzleManager.Instance != null)
            PuzzleManager.Instance.SetFlag(solvedFlag);

        // گویندگی پیروزی
        if (!string.IsNullOrEmpty(solvedVoiceId) && NarrationManager.Instance != null)
            NarrationManager.Instance.PlayById(solvedVoiceId);
    }

    /// <summary>ریست دستی معما (اگر لازم شد)</summary>
    public void ResetPuzzle() => currentStep = 0;
}
