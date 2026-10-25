// ═══════════════════════════════════════════════════════════════
//  NarrationManager.cs — پخش گویندگی‌ها با صف 🎙️
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • خط‌های گویندگی را در یک صف نگه می‌دارد و یکی‌یکی پخش می‌کند.
//  • حین پخش هر خط، زیرنویس فارسی‌اش را روی صفحه نشان می‌دهد.
//  • اگر خط تازه‌ای «فوری» باشد (PlayNow)، صف را پاک می‌کند و همان را می‌گوید.
//  • رویدادهای شروع/پایان هر خط را اعلام می‌کند (برای هماهنگی رویدادها)
//
//  کجا وصل می‌شود؟
//  • یک GameObject خالی به اسم «Narration» در صحنه‌ی House بساز
//    + این کد + مرجع VoiceLibrary
//  • از هر جای بازی: NarrationManager.Instance.PlayById("v_intro_1")
//  • در Inspector رویدادهای داستان (StoryManager) هم همین‌طور صدا زده می‌شود.
// ═══════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance { get; private set; }

    [Header("مرجع")]
    [Tooltip("کتابخانه‌ی گویندگی‌ها (فایل VoiceLibrary)")]
    [SerializeField] VoiceLibrary library;

    [Header("تنظیمات")]
    [Tooltip("اگر بازیکن دور شد/صدا قطع شد، بعد از این مدت خط را رها کن")]
    [SerializeField] float maxLineDuration = 30f;

    // ── صف پخش ───────────────────────────────────────────────────
    readonly Queue<VoiceLine> queue = new Queue<VoiceLine>();
    AudioSource voiceSource;
    VoiceLine currentLine;
    float lineTimer;

    /// <summary>شروع پخش یک خط (شناسه، متن زیرنویس)</summary>
    public event Action<string, string> OnLineStart;
    /// <summary>پایان پخش یک خط (شناسه)</summary>
    public event Action<string> OnLineEnd;
    /// <summary>صف کاملاً خالی شد</summary>
    public event Action OnQueueEmpty;

    public bool IsPlaying => voiceSource != null && voiceSource.isPlaying || queue.Count > 0;

    void Awake()
    {
        Instance = this;

        voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;
        voiceSource.spatialBlend = 0f; // صدای «ذهن» بازیکن — ۲بعدی
    }

    void Update()
    {
        // اگر چیزی در حال پخش نیست و صف پر است → خط بعدی
        if ((voiceSource.clip == null || !voiceSource.isPlaying) && queue.Count > 0)
        {
            VoiceLine next = queue.Dequeue();
            PlayLine(next);
        }

        // محافظت: اگر صدا به هر دلیلی تمام نشد، بعد از مدتی رها کن
        if (currentLine != null)
        {
            lineTimer += Time.deltaTime;
            if (lineTimer > maxLineDuration)
                FinishLine();
        }
    }

    // ═══ API عمومی ═══════════════════════════════════════════════

    /// <summary>اضافه کردن یک خط به آخر صف (با شناسه از کتابخانه)</summary>
    public void PlayById(string id)
    {
        if (library == null) { Debug.LogWarning("کتابخانه‌ی گویندگی تنظیم نشده!"); return; }
        VoiceLine line = library.GetById(id);
        if (line != null) queue.Enqueue(line);
    }

    /// <summary>اضافه کردن چند خط پشت‌سرهم — ترتیب حفظ می‌شود</summary>
    public void PlaySequence(params string[] ids)
    {
        foreach (string id in ids) PlayById(id);
    }

    /// <summary>پخش فوری — هر چه در صف بود پاک می‌شود (برای لحظه‌های مهم)</summary>
    public void PlayNow(string id)
    {
        if (library == null) return;
        ClearQueue();
        VoiceLine line = library.GetById(id);
        if (line != null)
        {
            if (currentLine != null) FinishLine();
            PlayLine(line);
        }
    }

    /// <summary>پاک کردن کل صف (خط جاری تمام می‌شود)</summary>
    public void ClearQueue() => queue.Clear();

    /// <summary>ساکت کردن فوری همه چیز</summary>
    public void StopAll()
    {
        ClearQueue();
        if (currentLine != null) FinishLine();
        if (voiceSource != null) voiceSource.Stop();
    }

    // ═══ پخش داخلی ═══════════════════════════════════════════════
    void PlayLine(VoiceLine line)
    {
        currentLine = line;
        lineTimer = 0f;

        if (line.clip != null)
        {
            voiceSource.clip = line.clip;
            voiceSource.volume = GameSettings.VoiceVolume * GameSettings.MasterVolume;
            voiceSource.Play();
        }

        // زیرنویس نشان داده شود؟ (بستگی به تنظیمات دارد)
        SubtitleUI.Instance?.ShowSubtitle(line.subtitle, line.clip != null ? line.clip.length : 3f);

        OnLineStart?.Invoke(line.id, line.subtitle);
    }

    void FinishLine()
    {
        string id = currentLine?.id ?? "";
        float delay = currentLine?.delayAfter ?? 0.3f;
        currentLine = null;

        SubtitleUI.Instance?.HideSubtitle();
        OnLineEnd?.Invoke(id);

        // فاصله‌ی کوتاه قبل از خط بعدی
        if (queue.Count == 0)
            Invoke(nameof(FireQueueEmpty), delay);
    }

    void FireQueueEmpty() => OnQueueEmpty?.Invoke();
}
