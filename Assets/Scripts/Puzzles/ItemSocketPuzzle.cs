// ═══════════════════════════════════════════════════════════════
//  ItemSocketPuzzle.cs — جاگذاری آیتم‌ها در جای خودشان 🖼️
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • «جای خالی» می‌سازد: قاب عکس خالی، جاکلیدی، جای فیوز و...
//  • اگر بازیکن آیتم درست را در انبار داشته باشد، با تعامل، آیتم
//    «در جای خودش قرار می‌گیرد» (آیتم مصرف می‌شود + ویژوال ظاهر می‌شود)
//  • وقتی همه‌ی جاها پر شد، معما حل می‌شود.
//
//  کاربردها در بازی ما:
//      → فیوز داخل جعبه‌ی برق
//      → عکس‌های خانوادگی در قاب‌های اتاق
//      → هر آیتمی که باید «به جایش برگردد»
//
//  کجا وصل می‌شود؟
//  • روی هر جای خالی + Collider
//  • آیتم مورد نیاز + ویژوال ظاهرشونده (مدل فیوز، عکس و...) را بده.
//  • رویداد onFilled را برای افکت وصل کن (صدای جا خوردن و...)
// ═══════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemSocketPuzzle : Interactable
{
    [Header("تنظیمات جای خالی")]
    [Tooltip("شناسه‌ی آیتمی که باید اینجا گذاشته شود (مثل fuse)")]
    [SerializeField] string requiredItemId = "fuse";
    [Tooltip("آیتم مصرف شود؟ (فیوز: بله — کلید: مصرف چون استفاده شد)")]
    [SerializeField] bool consumeItem = true;
    [Tooltip("ویژوالی که بعد از جاگذاری ظاهر می‌شود (مدل عکس داخل قاب و...)")]
    [SerializeField] GameObject filledVisual;

    [Header("رویدادها")]
    [Tooltip("وقتی این جای خالی پر شد")]
    public UnityEvent onFilled;

    [Header("پیام‌ها")]
    [SerializeField] string filledHint = "جای خودش نشست...";

    [Tooltip("پرچمی که وقتی «همه‌ی سوکت‌های گروه» پر شدند روشن می‌شود (اختیاری)")]
    [SerializeField] string groupCompleteFlag = "";

    bool filled = false;

    public bool IsFilled => filled;

    public override bool CanInteract() => !filled && base.CanInteract();

    public override string InteractionPrompt
    {
        get
        {
            if (filled) return "";
            // اگر آیتم را دارد → جاگذاری، وگرنه راهنما
            if (HasRequiredItem())
                return "قرار دادن";
            return "جای خالی... چیزی اینجا جا می‌شود";
        }
    }

    protected override void OnInteracted()
    {
        if (filled) return;

        // چک کردن آیتم
        if (!HasRequiredItem())
        {
            SubtitleUI.Instance?.ShowHint("جای خالی... چیزی اینجا جا می‌شود");
            return;
        }

        // جاگذاری!
        filled = true;
        if (consumeItem) ConsumeRequiredItem();

        // ویژوال ظاهر شود
        if (filledVisual != null) filledVisual.SetActive(true);

        // صدا و رویداد
        if (AudioManager.Instance != null && interactSfxOverride != null)
            AudioManager.Instance.PlaySfxAt(interactSfxOverride, transform.position);
        onFilled?.Invoke();
        SubtitleUI.Instance?.ShowHint(filledHint);

        // آیا گروه سوکت‌ها کامل شد؟ (با رویداد گروهی چک می‌شود — پایین ببین)
        CheckGroupCompletion();
    }

    [Header("صدای جاگذاری")]
    [SerializeField] AudioClip interactSfxOverride;

    [Header("گروه سوکت‌ها (اختیاری)")]
    [Tooltip("سوکت‌های دیگرِ همین معما — وقتی همه پر شدند پرچم روشن می‌شود")]
    [SerializeField] List<ItemSocketPuzzle> groupSockets = new List<ItemSocketPuzzle>();

    void CheckGroupCompletion()
    {
        if (groupSockets == null || groupSockets.Count == 0) return;

        foreach (var socket in groupSockets)
        {
            if (socket == null) continue;
            if (!socket.IsFilled)
            {
                // یکی هنوز خالی است — یکبار به آن یادآوری می‌دهیم
                return;
            }
        }

        // همه پر شدند!
        if (!string.IsNullOrEmpty(groupCompleteFlag) && PuzzleManager.Instance != null)
            PuzzleManager.Instance.SetFlag(groupCompleteFlag);
    }
}
