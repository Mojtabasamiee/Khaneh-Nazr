// ═══════════════════════════════════════════════════════════════
//  VoiceLibrary.cs — کتابخانه‌ی گویندگی‌های بازی 🎙️
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • یک ScriptableObject است که «همه‌ی خط‌های گویندگی بازی» را نگه می‌دارد.
//  • هر خط سه چیز دارد: شناسه‌ی یکتا + فایل صدای خودت + متن زیرنویس فارسی
//
//  کجا وصل می‌شود؟
//  • راست‌کلیک در Project → Create → خانه نذر → کتابخانه‌ی گویندگی
//  • لیست متن‌های گویندگی (با شناسه‌ها) در Docs/STORY.md آمده است!
//  • صداها را ضبط کردی، با شناسه‌ی همین جدول نام‌گذاری کن (v_intro_1.mp3)
//    و اینجا به هر خط، فایل صدا و متنش را بده.
//  • مرجعش را در NarrationManager (صحنه‌ی House) بده.
// ═══════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoiceLine
{
    [Tooltip("شناسه‌ی یکتا — مثل v_intro_1 (از جدول STORY.md)")]
    public string id = "";

    [TextArea(2, 4)]
    [Tooltip("متن زیرنویس فارسی — دقیقاً همان چیزی که گوینده می‌گوید")]
    public string subtitle = "";

    [Tooltip("فایل صدای ضبط‌شده‌ی این خط")]
    public AudioClip clip;

    [Tooltip("چند ثانیه بعد از این خط، خط بعدی پخش شود")]
    public float delayAfter = 0.5f;
}

[CreateAssetMenu(fileName = "VoiceLibrary", menuName = "خانه نذر/کتابخانه‌ی گویندگی")]
public class VoiceLibrary : ScriptableObject
{
    [Tooltip("همه‌ی خط‌های گویندگی بازی")]
    public List<VoiceLine> lines = new List<VoiceLine>();

    /// <summary>پیدا کردن خط با شناسه</summary>
    public VoiceLine GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (var line in lines)
        {
            if (line != null && line.id == id)
                return line;
        }
        Debug.LogWarning("خط گویندگی با شناسه «" + id + "» در کتابخانه پیدا نشد!");
        return null;
    }
}
