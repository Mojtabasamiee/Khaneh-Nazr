// ═══════════════════════════════════════════════════════════════
//  PickupItem.cs — برداشتن اشیاء از محیط (کلید، فیوز، عکس و...)
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • وقتی بازیکن روی این شیء تعامل می‌کند، آیتمش وارد انبار می‌شود
//    و شیء از صحنه محو می‌شود.
//
//  کجا وصل می‌شود؟
//  • روی هر شیء کوچک قابل برداشتن (کلید روی میز و...)
//  • در Inspector فایل ItemData مربوطه را به «آیتم» بده.
//  • شیء باید Collider داشته باشد.
// ═══════════════════════════════════════════════════════════════

using UnityEngine;

public class PickupItem : Interactable
{
    [Header("تنظیمات برداشتن")]
    [Tooltip("کدام آیتم برداشته می‌شود؟ (فایل ItemData)")]
    [SerializeField] ItemData item;

    [Tooltip("صدای برداشتن (بدون نیاز به ItemData)")]
    [SerializeField] AudioClip pickupSfx;

    [Tooltip("به جای محو شدن، فقط کوچک و نامرئی شود؟ (برای مواقعی که ممکن است دوباره ظاهر شود)")]
    [SerializeField] bool hideInsteadOfDestroy = false;

    bool taken = false;

    public override bool CanInteract() => !taken && base.CanInteract();

    protected override void OnInteracted()
    {
        if (taken || item == null || InventoryManager.Instance == null) return;

        // صدای برداشتن
        if (pickupSfx != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfxAt(pickupSfx, transform.position);

        // اضافه شدن به انبار (گویندگی pickupVoiceId خودکار پخش می‌شود)
        InventoryManager.Instance.AddItem(item);
        taken = true;

        // محو شدن شیء از دنیای بازی
        if (hideInsteadOfDestroy)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);
    }
}
