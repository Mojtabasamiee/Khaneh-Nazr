// ═══════════════════════════════════════════════════════════════
//  InventoryManager.cs — انبار بازیکن (چیزهایی که برداشته‌ای)
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • آیتم‌هایی که برمی‌داری را نگه می‌دارد و اضافه/حذف می‌کند.
//  • بقیه‌ی سیستم‌ها (در، قفل، معما) از همین‌جا می‌پرسند
//    «بازیکن کلید را دارد؟» — InventoryManager.Instance.HasItem(...)
//  • InventoryUI به رویدادهای این کلاس گوش می‌دهد و آیکون‌ها را آپدیت می‌کند.
//
//  کجا وصل می‌شود؟
//  • یک GameObject خالی به اسم «Inventory» در صحنه‌ی House بساز
//    و این کد را رویش بگذار + مرجع ItemDatabase را بده.
//  • HUD دکمه‌ی انبار → onClick → InventoryUI.TogglePanel
// ═══════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("مرجع‌ها")]
    [Tooltip("دفترچه‌ی آیتم‌های بازی")]
    [SerializeField] ItemDatabase database;

    // ── وضعیت انبار ──────────────────────────────────────────────
    readonly List<ItemData> items = new List<ItemData>();

    /// <summary>هر وقت انبار تغییر کرد (برداشت/حذف) این رویداد صدا زده می‌شود</summary>
    public event Action OnInventoryChanged;

    /// <summary>وقتی آیتمی برداشته می‌شود (برای نمایشش فوری) — آیتم جدید</summary>
    public event Action<ItemData> OnItemAdded;

    void Awake()
    {
        Instance = this;
    }

    // ═══ اضافه کردن آیتم ═════════════════════════════════════════
    public void AddItem(ItemData item)
    {
        if (item == null) return;

        // آیتم‌های تک‌نسخه: اگر قبلاً داریم دوباره اضافه نکن
        if (HasItem(item.id)) return;

        items.Add(item);
        OnItemAdded?.Invoke(item);
        OnInventoryChanged?.Invoke();

        // گویندگی برداشتن آیتم (اگر تعریف شده)
        if (!string.IsNullOrEmpty(item.pickupVoiceId) && NarrationManager.Instance != null)
            NarrationManager.Instance.PlayById(item.pickupVoiceId);

        Debug.Log("آیتم برداشته شد: " + item.displayName);
    }

    // ═══ حذف آیتم (مصرف کلید و...) ══════════════════════════════
    public void RemoveItem(string id)
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].id == id)
            {
                items.RemoveAt(i);
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

    // ═══ پرس‌وجوها ═══════════════════════════════════════════════
    /// <summary>آیا بازیکن این آیتم را دارد؟</summary>
    public bool HasItem(string id)
    {
        foreach (var item in items)
            if (item.id == id) return true;
        return false;
    }

    public List<ItemData> GetAllItems() => items;

    /// <summary>برای SaveSystem — فقط شناسه‌ها</summary>
    public List<string> GetAllItemIds()
    {
        var ids = new List<string>();
        foreach (var item in items) ids.Add(item.id);
        return ids;
    }

    /// <summary>برای SaveSystem — بازسازی انبار از شناسه‌ها</summary>
    public void LoadItemIds(List<string> ids)
    {
        items.Clear();
        if (ids == null || database == null) return;

        foreach (string id in ids)
        {
            ItemData item = database.GetById(id);
            if (item != null) items.Add(item);
        }
        OnInventoryChanged?.Invoke();
    }
}
