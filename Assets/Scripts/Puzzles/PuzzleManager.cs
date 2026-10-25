// ═══════════════════════════════════════════════════════════════
//  PuzzleManager.cs — مرکز پرچم‌های داستان و معماها 🚩
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • «پرچم» = یک رویداد مهم که اتفاق افتاده.
//    مثلاً: fuse_fixed (فیوز وصل شد)، grandma_room_open، poem_done...
//  • همه‌ی سیستم‌ها با پرچم با هم حرف می‌زنند:
//    معما حل شد → SetFlag → داستان جلو می‌رود → در باز می‌شود → ترس فعال می‌شود
//
//  کجا وصل می‌شود؟
//  • یک GameObject خالی به اسم «PuzzleManager» در صحنه‌ی House
//  • بقیه‌ی کدها خودشان از PuzzleManager.Instance استفاده می‌کنند.
//  • پرچم‌ها با SaveSystem ذخیره می‌شوند — بازی قابل ادامه است.
// ═══════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    // ── ذخیره‌ی پرچم‌ها ───────────────────────────────────────────
    readonly Dictionary<string, bool> flags = new Dictionary<string, bool>();

    /// <summary>وقتی پرچمی تازه روشن می‌شود (شناسه‌ی پرچم)</summary>
    public event Action<string> OnFlagSet;

    void Awake()
    {
        Instance = this;
    }

    // ═══ روشن کردن پرچم ══════════════════════════════════════════
    /// <summary>روشن کردن پرچم — فقط بار اول اثر دارد</summary>
    public void SetFlag(string flagId)
    {
        if (string.IsNullOrEmpty(flagId)) return;

        if (flags.ContainsKey(flagId) && flags[flagId]) return; // قبلاً روشن بوده

        flags[flagId] = true;
        Debug.Log("🚩 پرچم روشن شد: " + flagId);
        OnFlagSet?.Invoke(flagId);
    }

    // ═══ پرس‌وجو ═════════════════════════════════════════════════
    /// <summary>آیا این پرچم روشن است؟</summary>
    public bool IsFlagSet(string flagId)
    {
        if (string.IsNullOrEmpty(flagId)) return false;
        return flags.ContainsKey(flagId) && flags[flagId];
    }

    /// <summary>آیا همه‌ی این پرچم‌ها روشن هستند؟ (برای درهای چند-شرطی)</summary>
    public bool AreAllSet(params string[] flagIds)
    {
        foreach (string f in flagIds)
            if (!IsFlagSet(f)) return false;
        return true;
    }

    /// <summary>آیا حداقل یکی از این پرچم‌ها روشن است؟</summary>
    public bool IsAnySet(params string[] flagIds)
    {
        foreach (string f in flagIds)
            if (IsFlagSet(f)) return true;
        return false;
    }

    // ═══ ذخیره / بارگذاری ════════════════════════════════════════

    /// <summary>برای SaveSystem — لیست پرچم‌های روشن</summary>
    public List<string> GetAllFlags()
    {
        var result = new List<string>();
        foreach (var pair in flags)
            if (pair.Value) result.Add(pair.Key);
        return result;
    }

    /// <summary>برای SaveSystem — بازگرداندن پرچم‌ها از ذخیره (بدون رویداد!)</summary>
    public void LoadFlags(List<string> flagIds)
    {
        flags.Clear();
        if (flagIds == null) return;
        foreach (string f in flagIds)
            flags[f] = true;
    }
}
