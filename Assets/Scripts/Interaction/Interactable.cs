// ═══════════════════════════════════════════════════════════════
//  Interactable.cs — کلاس پایه‌ی همه‌ی اشیای تعاملی بازی
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • هر چیزی که بازیکن می‌تواند لمس/بررسی/باز کند از این کلاس ارث می‌برد.
//    (در، کشو، آیتم، چراغ، شمع، قفل و...)
//  • متن راهنما (مثل «باز کردن در») را می‌دهد که روی HUD نمایش داده می‌شود.
//  • اگر شیء به آیتمی نیاز داشته باشد (مثل کلید)، خودش چک می‌کند.
//  • یک UnityEvent دارد که می‌توانی بدون کدنویسی در Inspector بهش کار بدی.
//
//  کجا وصل می‌شود؟
//  • این کد را مستقیم استفاده نمی‌کنی؛ کلاس‌های دیگر (Door, Pickup...)
//    از آن ارث می‌برند. ولی برای «شیء بررسی‌پذیر ساده» می‌توانی همین را
//    روی هر GameObject بگذاری و فقط onInteract را در Inspector پر کنی.
//  • مهم: هر شیء تعاملی باید یک Collider داشته باشد.
// ═══════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.Events;

/// <summary>قرارداد همه‌ی اشیای تعاملی — PlayerInteractor با این کار می‌کند</summary>
public interface IInteractable
{
    string InteractionPrompt { get; } // متنی که روی HUD نشان داده می‌شود
    bool   CanInteract();             // آیا الان می‌شود تعامل کرد؟
    void   Interact();                // انجام تعامل
}

public class Interactable : MonoBehaviour, IInteractable
{
    [Header("متن راهنما")]
    [Tooltip("متنی که وقتی به شیء نگاه می‌کنی روی HUD می‌آید")]
    [SerializeField] string prompt = "بررسی";

    [Header("نیاز به آیتم (مثل کلید)")]
    [Tooltip("آیا برای تعامل، بازیکن باید آیتمی در انبار داشته باشد؟")]
    [SerializeField] bool requiresItem = false;
    [Tooltip("شناسه‌ی آیتم مورد نیاز (مثلاً key_basement)")]
    [SerializeField] string requiredItemId = "";
    [Tooltip("متنی که اگر آیتم را نداشته باشد نشان داده می‌شود")]
    [TextArea(1, 2)]
    [SerializeField] string missingItemHint = "چیزی کم دارم...";

    [Header("صداها")]
    [Tooltip("صدایی که موقع تعامل با شیء پخش می‌شود (اختیاری)")]
    [SerializeField] AudioClip interactSfx;
    [Tooltip("صدای «قفل بودن» — وقتی آیتم لازم را نداری")]
    [SerializeField] AudioClip lockedSfx;

    [Header("رویداد تعامل (بدون کدنویسی!)")]
    [Tooltip("هر کاری که می‌خواهی موقع تعامل انجام شود را اینجا در Inspector وصل کن")]
    public UnityEvent onInteract;

    // ── پیاده‌سازی IInteractable ──────────────────────────────────
    public virtual string InteractionPrompt
    {
        get
        {
            // اگر به آیتم نیاز است و نداریم، نشان بده که قفل است
            if (requiresItem && !HasRequiredItem())
                return missingItemHint;
            return prompt;
        }
    }

    public virtual bool CanInteract()
    {
        return isActiveAndEnabled;
    }

    public virtual void Interact()
    {
        // چک کردن آیتم مورد نیاز
        if (requiresItem && !HasRequiredItem())
        {
            OnMissingItem();
            return;
        }

        // صدای تعامل
        PlaySfx(interactSfx);

        // رویداد Inspector + تابع داخلی برای کلاس‌های فرزند
        onInteract?.Invoke();
        OnInteracted();
    }

    // ── ابزارهای قابل استفاده در کلاس‌های فرزند ───────────────────

    /// <summary>آیا بازیکن آیتم مورد نیاز را دارد؟</summary>
    protected bool HasRequiredItem()
    {
        if (string.IsNullOrEmpty(requiredItemId)) return true;
        return InventoryManager.Instance != null &&
               InventoryManager.Instance.HasItem(requiredItemId);
    }

    /// <summary>آیتم مورد نیاز را از انبار مصرف کن (مثل کلید)</summary>
    protected void ConsumeRequiredItem()
    {
        if (!string.IsNullOrEmpty(requiredItemId) && InventoryManager.Instance != null)
            InventoryManager.Instance.RemoveItem(requiredItemId);
    }

    /// <summary>کلاس‌های فرزند این را بازنویسی (override) می‌کنند</summary>
    protected virtual void OnInteracted() { }

    /// <summary>وقتی آیتم لازم را ندارد چه شود؟</summary>
    protected virtual void OnMissingItem()
    {
        PlaySfx(lockedSfx);
        SubtitleUI.Instance?.ShowHint(missingItemHint);
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfxAt(clip, transform.position);
    }
}
