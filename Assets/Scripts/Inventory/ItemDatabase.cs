// ═══════════════════════════════════════════════════════════════
//  ItemDatabase.cs — دفترچه‌ی همه‌ی آیتم‌های بازی
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • همه‌ی فایل‌های ItemData را یکجا نگه می‌دارد.
//  • موقع بارگذاری ذخیره‌ی بازی، از روی شناسه‌ی آیتم (مثل key_basement)
//    فایل آیتم اصلی را پیدا می‌کند.
//
//  کجا وصل می‌شود؟
//  • راست‌کلیک در Project → Create → خانه نذر → دفترچه‌ی آیتم‌ها
//  • همه‌ی آیتم‌هایی که ساختی را داخل لیستش بکش.
//  • مرجعش را در InventoryManager (صحنه‌ی House) بده.
// ═══════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "خانه نذر/دفترچه‌ی آیتم‌ها")]
public class ItemDatabase : ScriptableObject
{
    [Tooltip("همه‌ی آیتم‌های بازی را اینجا بگذار")]
    public List<ItemData> items = new List<ItemData>();

    /// <summary>پیدا کردن آیتم با شناسه — برای بارگذاری ذخیره</summary>
    public ItemData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (var item in items)
        {
            if (item != null && item.id == id)
                return item;
        }
        return null;
    }
}
