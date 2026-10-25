// ═══════════════════════════════════════════════════════════════
//  TouchLook.cs — چرخش دوربین با سوایپ انگشت (سمت راست صفحه)
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • لمس‌های نیمه‌ی راست صفحه را می‌گیرد و میزان کشیدن انگشت را
//    به صورت Delta به PlayerController می‌دهد تا دوربین بچرخد.
//  • اگر انگشت سریع و کم‌جابجایی صفحه را لمس کند (تپ)، رویداد OnTap
//    می‌فرستد که PlayerInteractor با آن روی شیء لمس‌شده تعامل می‌کند.
//  • در ادیتور، ماوس هم کار می‌کند (کلیک چپ نگه‌دار و بکش) تا راحت تست کنی.
//
//  کجا وصل می‌شود؟
//  • روی GameObject پلیر (همان شیئی که PlayerController دارد) بگذار.
//  • UI نباید روی این لمس‌ها اثر بگذارد — خودش لمس روی دکمه‌ها را نادیده می‌گیرد.
// ═══════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using UnityEngine.EventSystems;

// این کد باید قبل از PlayerController در هر فریم اجرا شود
[DefaultExecutionOrder(-100)]
public class TouchLook : MonoBehaviour
{
    // ── خروجی برای PlayerController (ایستا) ──────────────────────
    //  Delta.x = جابجایی افقی انگشت  → چرخش راست/چپ
    //  Delta.y = جابجایی عمودی انگشت → چرخش بالا/پایین
    public static Vector2 Delta { get; private set; }

    /// <summary>وقتی کاربر روی یک شیء تپ کرد (نه سوایپ) این رویداد با مکان صفحه صدا زده می‌شود</summary>
    public static event Action<Vector2> OnTap;

    [Header("تنظیمات ناحیه‌ی نگاه")]
    [Tooltip("از چه کسری از عرض صفحه به بعد = ناحیه‌ی نگاه؟ (0.55 یعنی راستِ صفحه)")]
    [Range(0.3f, 0.8f)]
    [SerializeField] float lookAreaStartX = 0.55f;

    [Header("تشخیص تپ")]
    [Tooltip("حداکثر جابجایی انگشت که هنوز «تپ» حساب شود (پیکسل)")]
    [SerializeField] float maxTapDistance = 15f;
    [Tooltip("حداکثر زمان لمس که هنوز «تپ» حساب شود (ثانیه)")]
    [SerializeField] float maxTapTime = 0.25f;

    // وضعیت ردیابی لمس فعال
    int   trackedFingerId = -1;
    Vector2 lastPosition;
    Vector2 startPosition;   // نقطه‌ی شروع لمس (برای تشخیص تپ)
    float   touchStartTime;

#if UNITY_EDITOR
    // ── پشتیبانی ماوس در ادیتور (برای تست راحت) ──────────────────
    Vector2 mouseLastPos;
    bool    mouseDragging;
    float   mouseDownTime;
    Vector2 mouseDownPos;
#endif

    void Update()
    {
        // اگر بازی در حال بازی نیست، دلتا صفر شود
        if (GameManager.Instance == null || !GameManager.Instance.CanPlayerAct())
        {
            Delta = Vector2.zero;
            trackedFingerId = -1;
            return;
        }

        Vector2 frameDelta = Vector2.zero;
        ProcessTouches(ref frameDelta);

#if UNITY_EDITOR
        ProcessMouse(ref frameDelta);
#endif

        Delta = frameDelta;
    }

    // ═══ پردازش لمس‌های واقعی گوشی ═══════════════════════════════
    void ProcessTouches(ref Vector2 frameDelta)
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            // ۱) شروع لمس: ببین آیا این لمس متعلق به ناحیه‌ی نگاه است؟
            if (t.phase == TouchPhase.Began)
            {
                // نیمه‌ی چپ صفحه مال جوی‌استیک است — نادیده بگیر
                if (t.position.x < Screen.width * lookAreaStartX) continue;

                // لمس روی دکمه‌ها و UI — نادیده بگیر
                if (EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject(t.fingerId)) continue;

                trackedFingerId = t.fingerId;
                lastPosition    = t.position;
                startPosition   = t.position;
                touchStartTime  = Time.unscaledTime;
                continue;
            }

            // ۲) فقط همان انگشتی که ردیابی می‌کنیم ادامه داده شود
            if (t.fingerId != trackedFingerId) continue;

            if (t.phase == TouchPhase.Moved)
            {
                Vector2 move = t.position - lastPosition;
                frameDelta  += move;
                lastPosition = t.position;
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                // ۳) تشخیص تپ: جابجایی کم از نقطه‌ی شروع + زمان کم
                float elapsed = Time.unscaledTime - touchStartTime;
                float moved   = (t.position - startPosition).magnitude;
                if (moved < maxTapDistance && elapsed < maxTapTime)
                    OnTap?.Invoke(t.position);

                trackedFingerId = -1;
            }
        }
    }

#if UNITY_EDITOR
    // ═══ پردازش ماوس فقط در ادیتور ═══════════════════════════════
    void ProcessMouse(ref Vector2 frameDelta)
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mp = Input.mousePosition;
            // ناحیه‌ی چپ + UI را همان‌طور رد کن
            if (mp.x < Screen.width * lookAreaStartX) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            mouseDragging = true;
            mouseDownTime = Time.unscaledTime;
            mouseDownPos  = mp;
            mouseLastPos  = mp;
        }
        else if (Input.GetMouseButton(0) && mouseDragging)
        {
            Vector2 mp = Input.mousePosition;
            frameDelta += (Vector2)(mp - mouseLastPos);
            mouseLastPos = mp;
        }
        else if (Input.GetMouseButtonUp(0) && mouseDragging)
        {
            mouseDragging = false;
            float elapsed = Time.unscaledTime - mouseDownTime;
            float dist    = ((Vector2)Input.mousePosition - mouseDownPos).magnitude;
            if (dist < maxTapDistance && elapsed < maxTapTime)
                OnTap?.Invoke(Input.mousePosition);
        }
    }
#endif

    void OnDisable()
    {
        Delta = Vector2.zero;
        trackedFingerId = -1;
    }
}
