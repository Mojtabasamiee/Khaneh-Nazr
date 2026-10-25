// ═══════════════════════════════════════════════════════════════
//  PlayerInteractor.cs — چشمِ تعاملی بازی: دیدن و لمس اشیای محیط
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • از وسط صفحه یک پرتو (Raycast) به جلو می‌تابد.
//  • اگر به شیء تعاملی (چیزی که کدمع Interactable دارد) بخورد و
//    در برد باشد، متن راهنما را روی HUD نشان می‌دهد.
//  • دکمه‌ی «تعامل» HUD یا تپ روی شیء، کار تعامل را انجام می‌دهد.
//
//  کجا وصل می‌شود؟
//  • روی دوربین اصلی (فرزند Player) بگذار.
//  • دکمه‌ی تعامل HUD → onClick → PlayerInteractor.TryInteract
//  • تپ روی اشیاء: خودش به TouchLook.OnTap وصل می‌شود (نیازی به کار نیست)
// ═══════════════════════════════════════════════════════════════

using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public static PlayerInteractor Instance { get; private set; }

    [Header("تنظیمات برد تعامل")]
    [Tooltip("حداکثر فاصله‌ی تعامل بر حسب متر")]
    [SerializeField] float interactRange = 2.6f;
    [Tooltip("لایه‌هایی که تعاملی هستند — همه را بگذار مگر دلیل داشته باشی")]
    [SerializeField] LayerMask interactableMask = ~0;

    [Header("تپ روی شیء")]
    [Tooltip("برای تپ روی شیء، برد کمی بیشتر مجاز باشد")]
    [SerializeField] float tapRangeBonus = 1.5f;

    // هدف فعلی که کاربر به آن نگاه می‌کند
    IInteractable currentTarget;

    void Awake()
    {
        Instance = this;

        // تپ‌های TouchLook را گوش می‌زنیم
        TouchLook.OnTap += HandleTap;
    }

    void OnDestroy()
    {
        TouchLook.OnTap -= HandleTap;
    }

    void Update()
    {
        // فقط وقتی در حال بازی هستیم هدف‌یابی کن
        if (GameManager.Instance != null && !GameManager.Instance.CanPlayerAct())
        {
            SetTarget(null);
            return;
        }

        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableMask, QueryTriggerInteraction.Collide))
        {
            // اگر شیء چندلایه بود، بالاترین Interactable را پیدا کن
            IInteractable it = hit.collider.GetComponentInParent<IInteractable>();
            SetTarget(it);
        }
        else
        {
            SetTarget(null);
        }

#if UNITY_EDITOR
        // تست با کلید E در ادیتور
        if (Input.GetKeyDown(KeyCode.E)) TryInteract();
#endif
    }

    // ═══ مدیریت هدف فعلی ═════════════════════════════════════════
    void SetTarget(IInteractable target)
    {
        if (currentTarget == target) return;
        currentTarget = target;

        // نمایش/مخفی کردن متن راهنما روی HUD
        if (InteractPromptUI.Instance != null)
        {
            if (target != null && target.CanInteract())
                InteractPromptUI.Instance.Show(target.InteractionPrompt);
            else
                InteractPromptUI.Instance.Hide();
        }
    }

    // ═══ تعامل با هدف فعلی (دکمه‌ی HUD یا کلید E) ═══════════════
    public void TryInteract()
    {
        if (GameManager.Instance != null && !GameManager.Instance.CanPlayerAct()) return;

        if (currentTarget != null && currentTarget.CanInteract())
            currentTarget.Interact();
    }

    // ═══ تپ روی شیء در صفحه — برای UX موبایل ═════════════════════
    void HandleTap(Vector2 screenPos)
    {
        if (GameManager.Instance == null || !GameManager.Instance.CanPlayerAct()) return;

        Camera cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        float range = interactRange + tapRangeBonus;

        if (Physics.Raycast(ray, out RaycastHit hit, range, interactableMask, QueryTriggerInteraction.Collide))
        {
            IInteractable it = hit.collider.GetComponentInParent<IInteractable>();
            if (it != null && it.CanInteract())
            {
                it.Interact();
                return;
            }
        }

        // اگر تپ به شیئی نخورد، به دکمه‌ی تعامل معنایی نده — فقط نادیده بگیر
    }

    // گیزموی قرمز در ادیتور برای دیدن برد تعامل
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * interactRange);
    }
}
