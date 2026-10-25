// ═══════════════════════════════════════════════════════════════
//  EndingManager.cs — پایان دوگانه‌ی بازی 🌘
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • دو پایان دارد:
//      پایان «ماندن» = بازیکن با جعبه‌ی نذر/قصه تعامل می‌کند و قصه را تمام می‌کند
//      پایان «فرار»  = بازیکن درِ اصلی را باز می‌کند و می‌رود
//  • اگر «خاطرات» (عکس‌های خانوادگی) جمع شده باشند، متن پایان کامل‌تر و بهتر است.
//  • پایان‌ها را ذخیره می‌کند (کدام‌ها را دیده‌ای)
//
//  کجا وصل می‌شود؟
//  • یک GameObject خالی به اسم «Endings» در صحنه‌ی House
//  • در Inspector: پنل پایان (Canvas) + متن عنوان و توضیح
//  • شیء «قصه» → Interactable → onInteract → Endings.ChooseStayEnding
//  • شیء «درِ اصلی» → Interactable → onInteract → Endings.ChooseEscapeEnding
// ═══════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance { get; private set; }

    [Header("پرچم‌های پایان")]
    [Tooltip("پرچمی که یعنی خاطرات/عکس‌ها کامل جمع شده (اختیاری — پایان بهتر)")]
    [SerializeField] string memoriesFlag = "photos_done";
    [Tooltip("پرچم ماندن/قصه — از ItemSocketPuzzle یا معمای آخر")]
    [SerializeField] string storyCompletedFlag = "story_done";

    [Header("پنل پایان")]
    [Tooltip("پنل تمام‌صفحه‌ی پایان (غیرفعال در شروع)")]
    [SerializeField] GameObject endingPanel;
    [SerializeField] Text endingTitleText;
    [SerializeField] Text endingBodyText;
    [SerializeField] Button backToMenuButton;

    [Header("متن پایان‌ها — حالت کامل (با خاطرات)")]
    [TextArea(3, 6)]
    [SerializeField] string stayTitleFull = "آرامش";
    [TextArea(6, 12)]
    [SerializeField] string stayBodyFull = "قصه تمام شد...";
    [TextArea(3, 6)]
    [SerializeField] string escapeTitleFull = "فرار";
    [TextArea(6, 12)]
    [SerializeField] string escapeBodyFull = "از در بیرون زدی...";

    [Header("متن پایان‌ها — حالت ناقص (بدون خاطرات)")]
    [TextArea(3, 6)]
    [SerializeField] string stayTitlePartial = "قصه‌ی ناتمام";
    [TextArea(6, 12)]
    [SerializeField] string stayBodyPartial = "می‌مانی، اما چیزی کم است...";
    [TextArea(3, 6)]
    [SerializeField] string escapeTitlePartial = "پشت در";
    [TextArea(6, 12)]
    [SerializeField] string escapeBodyPartial = "می‌روی، اما...";

    [Header("گویندگی پایان‌ها")]
    [Tooltip("شناسه‌ی گویندگی پایان ماندن (کامل / ناقص)")]
    [SerializeField] string stayVoiceFull = "v_end_stay_full";
    [SerializeField] string stayVoicePartial = "v_end_stay_partial";
    [SerializeField] string escapeVoiceFull = "v_end_escape_full";
    [SerializeField] string escapeVoicePartial = "v_end_escape_partial";

    [Header("موسیقی پایان")]
    [Tooltip("صدای محیط/موسیقی موقع پایان (اختیاری)")]
    [SerializeField] string endingAmbient = "";

    [Header("رویداد پایان")]
    public UnityEvent onEndingShown;

    // ── پایان‌های دیده‌شده (ذخیره می‌شوند) ────────────────────────
    List<string> endingsSeen = new List<string>();
    bool endingPlaying = false;

    void Awake()
    {
        Instance = this;
        if (endingPanel != null) endingPanel.SetActive(false);
    }

    void Start()
    {
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(GoToMenu);
    }

    // ═══ انتخاب پایان: ماندن (قصه را تمام کن) ════════════════════
    public void ChooseStayEnding()
    {
        if (endingPlaying) return;
        bool full = PuzzleManager.Instance != null && PuzzleManager.Instance.IsFlagSet(memoriesFlag);
        StartCoroutine(PlayEnding(
            full ? stayTitleFull : stayTitlePartial,
            full ? stayBodyFull : stayBodyPartial,
            full ? stayVoiceFull : stayVoicePartial,
            "stay"));
    }

    // ═══ انتخاب پایان: فرار (از در اصلی) ═════════════════════════
    public void ChooseEscapeEnding()
    {
        if (endingPlaying) return;
        bool full = PuzzleManager.Instance != null && PuzzleManager.Instance.IsFlagSet(memoriesFlag);
        StartCoroutine(PlayEnding(
            full ? escapeTitleFull : escapeTitlePartial,
            full ? escapeBodyFull : escapeBodyPartial,
            full ? escapeVoiceFull : escapeVoicePartial,
            "escape"));
    }

    // ═══ اجرای سکانس پایان ═══════════════════════════════════════
    IEnumerator PlayEnding(string title, string body, string voiceId, string endingId)
    {
        endingPlaying = true;

        // قفل کل بازی
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.Ending);
        if (NarrationManager.Instance != null)
            NarrationManager.Instance.StopAll();

        // محیط پایان
        if (!string.IsNullOrEmpty(endingAmbient) && AudioManager.Instance != null)
            AudioManager.Instance.PlayAmbient(endingAmbient);

        // گویندگی پایان
        if (!string.IsNullOrEmpty(voiceId) && NarrationManager.Instance != null)
        {
            NarrationManager.Instance.PlayNow(voiceId);
            yield return new WaitForSeconds(0.3f);
            while (NarrationManager.Instance.IsPlaying)
                yield return null;
        }

        // نمایش پنل متن پایان
        if (endingPanel != null)
        {
            if (endingTitleText != null) endingTitleText.text = title;
            if (endingBodyText != null) endingBodyText.text = body;
            endingPanel.SetActive(true);
        }

        // ذخیره اینکه این پایان دیده شد
        if (!endingsSeen.Contains(endingId))
        {
            endingsSeen.Add(endingId);
            if (SaveSystem.HasSave()) SaveSystem.SaveGame();
        }

        onEndingShown?.Invoke();
    }

    void GoToMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.BackToMenu();
    }

    // ═══ برای SaveSystem ═════════════════════════════════════════
    public List<string> GetEndingsSeen() => endingsSeen;

    public void LoadEndingsSeen(List<string> ids)
    {
        endingsSeen = ids != null ? ids : new List<string>();
    }
}
