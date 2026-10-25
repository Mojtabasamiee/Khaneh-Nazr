// ═══════════════════════════════════════════════════════════════
//  StoryManager.cs — پیشبرد داستان با پرچم‌ها 📖
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • داستان را به صورت «مراحل» تعریف می‌کنی.
//  • هر مرحله می‌گوید: «وقتی این پرچم‌ها روشن شدند، این گویندگی‌ها
//    پخش شود و این رویدادها اجرا شود».
//  • یعنی داستانِ بازی را «بدون کدنویسی» در Inspector می‌چینی!
//
//  مثال:
//    مرحله‌ی 1 (ورود): بدون شرط — همون اول: گویندگی v_intro_1..3
//    مرحله‌ی 2: پرچم fuse_fixed → گویندگی v_power_on + ترسِ صدای قصه
//    مرحله‌ی 3: پرچم grandma_room_open → گویندگی های جدید...
//
//  کجا وصل می‌شود؟
//  • یک GameObject خالی به اسم «Story» در صحنه‌ی House
//  • مراحل را در Inspector اضافه کن (شرط پرچم + گویندگی + رویدادها)
//  • جدول کامل مراحل بازی در Docs/06-معماها.md و STORY.md آمده است.
// ═══════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    /// <summary>تعریف یک مرحله از داستان</summary>
    [Serializable]
    public class StoryStep
    {
        [Tooltip("نام مرحله (برای خودت — مثل «بیدار شدن در خانه»)")]
        public string stepName = "مرحله‌ی جدید";

        [Tooltip("این مرحله کی اجرا شود؟ هر وقت «همه‌ی این پرچم‌ها» روشن بودند")]
        public List<string> requiredFlags = new List<string>();

        [Tooltip("گویندگی‌هایی که به ترتیب پخش شوند (شناسه‌ها از VoiceLibrary)")]
        public List<string> narrationIds = new List<string>();

        [Tooltip("رویدادهای ترسناکی که همزمان اجرا شوند")]
        public List<string> horrorEventIds = new List<string>();

        [Tooltip("صدای محیطی که عوض شود (خالی = بدون تغییر)")]
        public string ambientChange = "";

        [Tooltip("چند ثانیه صبر قبل از شروع گویندگی")]
        public float startDelay = 0.5f;

        // اجرا شده؟ (داخلی)
        [NonSerialized] public bool played = false;
    }

    [Header("مراحل داستان (به ترتیب!)")]
    [SerializeField] List<StoryStep> steps = new List<StoryStep>();

    [Header("تنظیمات")]
    [Tooltip("فاصله بین پخش گویندگی‌های پشت‌سرهم (ثانیه)")]
    [SerializeField] float betweenLinesDelay = 0.3f;

    /// <summary>مرحله‌ی فعلی داستان (برای ذخیره)</summary>
    public int CurrentStep { get; private set; } = 0;

    bool storyRunning = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // اتصال رویداد در Start (نه OnEnable) تا PuzzleManager حتماً ساخته شده باشد
        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.OnFlagSet += OnFlagChanged;

        // شروع داستان — مرحله‌ی اول (معمولاً شرط ندارد)
        StartCoroutine(CheckStepsNextFrame());
    }

    void OnDestroy()
    {
        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.OnFlagSet -= OnFlagChanged;
    }

    IEnumerator CheckStepsNextFrame()
    {
        yield return null; // صبر تا همه چیز آماده شود
        EvaluateSteps();
    }

    // ═══ وقتی پرچمی عوض شد، مراحل را چک کن ═══════════════════════
    void OnFlagChanged(string flagId)
    {
        if (!storyRunning)
            EvaluateSteps();
    }

    void EvaluateSteps()
    {
        storyRunning = true;
        StartCoroutine(EvaluateRoutine());
    }

    IEnumerator EvaluateRoutine()
    {
        foreach (var step in steps)
        {
            if (step.played) continue;

            // آیا همه‌ی پرچم‌های لازم این مرحله روشن‌اند؟
            if (!AreFlagsSet(step.requiredFlags)) continue;

            step.played = true;
            CurrentStep = steps.IndexOf(step);

            // اجرای مرحله
            yield return StartCoroutine(PlayStep(step));
        }
        storyRunning = false;
    }

    bool AreFlagsSet(List<string> required)
    {
        if (PuzzleManager.Instance == null) return true;
        foreach (string f in required)
            if (!PuzzleManager.Instance.IsFlagSet(f)) return false;
        return true;
    }

    // ═══ اجرای یک مرحله ══════════════════════════════════════════
    IEnumerator PlayStep(StoryStep step)
    {
        Debug.Log("📖 مرحله‌ی داستان: " + step.stepName);

        // تاخیر شروع
        if (step.startDelay > 0f)
            yield return new WaitForSeconds(step.startDelay);

        // رویدادهای ترسناک همزمان
        foreach (string eventId in step.horrorEventIds)
            if (HorrorEventManager.Instance != null)
                HorrorEventManager.Instance.Trigger(eventId);

        // تغییر صدای محیط
        if (!string.IsNullOrEmpty(step.ambientChange) && AudioManager.Instance != null)
            AudioManager.Instance.PlayAmbient(step.ambientChange);

        // پخش گویندگی‌ها به ترتیب
        foreach (string voiceId in step.narrationIds)
        {
            if (NarrationManager.Instance == null) break;

            NarrationManager.Instance.PlayById(voiceId);

            // صبر تا پایان این خط + فاصله
            yield return new WaitForSeconds(0.2f);
            while (NarrationManager.Instance.IsPlaying)
                yield return null;
            yield return new WaitForSeconds(betweenLinesDelay);
        }
    }

    // ═══ برای SaveSystem ═════════════════════════════════════════

    /// <summary>بازگرداندن داستان تا مرحله‌ی ذخیره‌شده (بدون پخش مجدد گویندگی‌ها)</summary>
    public void SetStepWithoutPlaying(int stepIndex)
    {
        for (int i = 0; i < steps.Count && i <= stepIndex; i++)
            steps[i].played = true;

        CurrentStep = Mathf.Clamp(stepIndex, 0, steps.Count - 1);
    }
}
