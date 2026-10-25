// ═══════════════════════════════════════════════════════════════
//  HorrorEventManager.cs — مرکز کنترل رویدادهای ترسناک 👻
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • همه‌ی «رویدادهای ترسناک» بازی را با شناسه نگه می‌دارد و صدا می‌زند.
//  • یک HorrorEvent می‌تواند شامل: صدا + لرزش دوربین + چراغ‌های سوسوزن + UnityEvent باشد.
//  • مثال: HorrorEventManager.Instance.Trigger("drawer_slam")
//
//  کجا وصل می‌شود؟
//  • یک GameObject خالی به اسم «HorrorManager» در صحنه‌ی House
//  • در لیست «رویدادها» برای هر ترس یک HorrorEvent جدید اضافه کن
//    (شناسه + صدای ترس + شدت لرزش + رویدادهای دلخواه)
//  • از Triggerهای صحنه، معماها و داستان: Trigger("شناسه")
// ═══════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>تعریف یک رویداد ترسناک — داخل HorrorEventManager ساخته می‌شود</summary>
[Serializable]
public class HorrorEventData
{
    [Tooltip("شناسه‌ی یکتا — مثل scare_drawer")]
    public string id = "scare_1";

    [Tooltip("صدای ترسناک")]
    public AudioClip scareSfx;

    [Range(0f, 1f)]
    [Tooltip("بلندی صدای ترس")]
    public float sfxVolume = 1f;

    [Tooltip("صدای ترس ۳بعدی از این نقطه باشد؟ (خالی = ۲بعدی از همه‌جا)")]
    public Transform sfxPosition;

    [Range(0f, 1f)]
    [Tooltip("شدت لرزش دوربین — ۰ یعنی بدون لرزش")]
    public float shakeAmount = 0.15f;

    [Tooltip("مدت لرزش دوربین (ثانیه)")]
    public float shakeDuration = 0.6f;

    [Tooltip("چراغ‌هایی که موقع این رویداد سوسو می‌زنند")]
    public FlickerLight[] flickeringLights;

    [Tooltip("رویدادهای اضافی دلخواه (مثلاً ظاهر شدن شبح)")]
    public UnityEventString onTriggered;
}

/// <summary>UnityEvent با پارامتر رشته‌ای — برای رویدادهای انعطاف‌پذیر</summary>
[Serializable]
public class UnityEventString : UnityEngine.Events.UnityEvent<string> { }

public class HorrorEventManager : MonoBehaviour
{
    public static HorrorEventManager Instance { get; private set; }

    [Header("رویدادهای ترسناک بازی")]
    [SerializeField] List<HorrorEventData> events = new List<HorrorEventData>();

    /// <summary>وقتی رویدادی اجرا می‌شود (شناسه)</summary>
    public event Action<string> OnEventTriggered;

    void Awake()
    {
        Instance = this;
    }

    // ═══ اجرای رویداد با شناسه ═══════════════════════════════════
    public void Trigger(string eventId)
    {
        HorrorEventData e = events.Find(ev => ev.id == eventId);
        if (e == null)
        {
            Debug.LogWarning("رویداد ترسناک «" + eventId + "» پیدا نشد!");
            return;
        }
        StartCoroutine(ExecuteEvent(e));
    }

    /// <summary>اجرای رویداد با تاخیر — برای زمان‌بندی ترس</summary>
    public void TriggerAfterDelay(string eventId, float delay)
    {
        StartCoroutine(DelayedTrigger(eventId, delay));
    }

    IEnumerator DelayedTrigger(string eventId, float delay)
    {
        yield return new WaitForSeconds(delay);
        Trigger(eventId);
    }

    IEnumerator ExecuteEvent(HorrorEventData e)
    {
        // ۱) صدای ترس
        if (e.scareSfx != null && AudioManager.Instance != null)
        {
            if (e.sfxPosition != null)
                AudioManager.Instance.PlaySfxAt(e.scareSfx, e.sfxPosition.position, e.sfxVolume);
            else
                AudioManager.Instance.PlaySfx2D(e.scareSfx, e.sfxVolume);
        }

        // ۲) لرزش دوربین
        if (e.shakeAmount > 0f && PlayerController.Instance != null)
        {
            PlayerController.Instance.Shake(e.shakeAmount);
        }

        // ۳) سوسوزدن چراغ‌ها
        if (e.flickeringLights != null)
        {
            foreach (var fl in e.flickeringLights)
                if (fl != null) fl.StartFlicker();
        }

        // ۴) رویدادهای دلخواه سازنده‌ی بازی
        e.onTriggered?.Invoke(e.id);

        OnEventTriggered?.Invoke(e.id);

        // پایان لرزش بعد از مدت مشخص
        yield return new WaitForSeconds(e.shakeDuration);
    }
}
