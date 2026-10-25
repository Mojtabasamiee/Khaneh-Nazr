// ═══════════════════════════════════════════════════════════════
//  VirtualJoystick.cs — جوی‌استیک مجازی لمسی برای حرکت پلیر
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • یک جوی‌استیک روی صفحه‌ی گوشی می‌سازد (زمین ثابت + دسته‌ی متحرک)
//  • هر جا انگشتت را روی ناحیه‌ی جوی‌استیک بکشی، دسته دنبالت می‌آید
//    و بردار حرکت (InputVector) را به PlayerController می‌دهد.
//
//  کجا وصل می‌شود؟
//  • در Canvas دو تا Image بگذار (زمین + دسته، دسته فرزند زمین)
//  • این کد روی «زمین» جوی‌استیک می‌رود و مرجع دسته را در Inspector می‌دهی.
//  • PlayerController خودش VirtualJoystick.InputVector را می‌خواند.
//
//  نکته: Image زمین حتماً باید Raycast Target باشد.
// ═══════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // ── بردار حرکت فعلی (ایستا = از هر جای کد قابل خواندن) ───────
    //  مقدار x: چپ/راست (منفی=چپ) — مقدار y: جلو/عقب (مثبت=جلو)
    //  همیشه بین -1 تا +1 است.
    public static Vector2 InputVector { get; private set; }

    [Header("تنظیمات جوی‌استیک")]
    [Tooltip("دسته‌ی کوچک داخل جوی‌استیک (باید فرزند زمین باشد)")]
    [SerializeField] RectTransform handle;
    [Tooltip("حداکثر فاصله‌ی حرکت دسته بر حسب پیکسل")]
    [SerializeField] float radius = 90f;
    [Tooltip("جوی‌استیک تا وقتی انگشت رویش نیست شفاف باشد؟")]
    [SerializeField] float idleAlpha = 0.55f;
    [SerializeField] float activeAlpha = 0.9f;

    CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        ResetJoystick();
    }

    // ── وقتی انگشت روی جوی‌استیک می‌گذاری ─────────────────────────
    public void OnPointerDown(PointerEventData eventData)
    {
        canvasGroup.alpha = activeAlpha;
        OnDrag(eventData); // همان لحظه هم حرکت اولیه را اعمال کن
    }

    // ── وقتی انگشتت را می‌کشی ─────────────────────────────────────
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        // مکان انگشت را به مختصات محلی زمین جوی‌استیک تبدیل کن
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint);

        // دسته را در شعاع مجاز نگه دار
        Vector2 clamped = Vector2.ClampMagnitude(localPoint, radius);
        handle.anchoredPosition = clamped;

        // بردار حرکت نرمال‌شده (۰ تا ۱) برای PlayerController
        InputVector = clamped / radius;
    }

    // ── وقتی انگشت را برمی‌داری ───────────────────────────────────
    public void OnPointerUp(PointerEventData eventData)
    {
        ResetJoystick();
    }

    void ResetJoystick()
    {
        if (handle != null) handle.anchoredPosition = Vector2.zero;
        InputVector = Vector2.zero;
        if (canvasGroup != null) canvasGroup.alpha = idleAlpha;
    }

    void OnDisable()
    {
        // اگر جوی‌استیک خاموش شد (مثلاً منو باز شد) حرکت هم صفر شود
        InputVector = Vector2.zero;
    }
}
