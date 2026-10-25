// ═══════════════════════════════════════════════════════════════
//  SequenceButton.cs — یک گام از معمای توالی (آینه، شمع، بیت شعر...)
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • روی هر شیء تعاملیِ یک SequencePuzzle می‌نشیند.
//  • وقتی بازیکن با آن تعامل می‌کند، به پدرش (SequencePuzzle) خبر می‌دهد
//    که «من زده شدم» — و پدر چک می‌کند ترتیب درست بوده یا نه.
//
//  کجا وصل می‌شود؟
//  • روی هر شیء: آینه، شمع، تابلوی شعر، قاب عکس...
//  • در Inspector:
//      معما  ← شیئی که SequencePuzzle دارد
//      ترتیب ← جایگاه درست این شیء (اولین = 0)
//  • برای واکنش بصری از رویداد onSelected استفاده کن
//      (مثلاً روشن شدن شعله‌ی شمع، باز شدن پارچه‌ی آینه، چراغ شدن قاب)
// ═══════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.Events;

public class SequenceButton : Interactable
{
    [Header("اتصال به معما")]
    [Tooltip("معمای پدر (شیئی که SequencePuzzle دارد)")]
    [SerializeField] SequencePuzzle puzzle;

    [Tooltip("جایگاه درست این گام — اولین گام = 0، دومی = 1 و...")]
    [SerializeField] int answerIndex = 0;

    [Header("صدا")]
    [SerializeField] AudioClip selectSfx;

    [Header("واکنش بصری (در Inspector وصل کن)")]
    [Tooltip("وقتی این گام درست انتخاب شد — مثلاً روشن شدن شعله")]
    public UnityEvent onSelected;

    bool used = false;

    public override bool CanInteract()
    {
        // بعد از حل شدن معما، دیگر قابل تعامل نیست
        if (puzzle != null && puzzle.IsSolved) return false;
        return base.CanInteract();
    }

    public override string InteractionPrompt
    {
        get
        {
            if (used) return base.InteractionPrompt; // متن دلخواه سازنده
            return base.InteractionPrompt;
        }
    }

    protected override void OnInteracted()
    {
        if (puzzle == null)
        {
            Debug.LogWarning("این گام به هیچ معمايی وصل نیست: " + name);
            return;
        }

        used = true;

        // صدا و واکنش بصری
        if (selectSfx != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfxAt(selectSfx, transform.position);
        onSelected?.Invoke();

        // خبر به معما — پدرش تشخیص می‌دهد درست بود یا اشتباه
        puzzle.SubmitStep(answerIndex);
    }

    /// <summary>برای ریست بصری (وقتی معما اشتباه می‌شود) — از UnityEvent onMistake وصل کن</summary>
    public void ResetVisual()
    {
        used = false;
    }
}
