// ═══════════════════════════════════════════════════════════════
//  FlickerLight.cs — چراغ سوسوزن ترسناک 💡
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • چراغ را با الگوی تصادفی سوسو می‌دهد (مثل برق خراب خانه‌های قدیمی)
//  • دو حالت: سوسوی دائمی از شروع / سوسوی موقت (موقع رویداد ترسناک)
//
//  کجا وصل می‌شود؟
//  • روی هر GameObject که Light دارد (یا Light خودش را به مرجع بده)
//  • برای سوسوی دائمی: تیک «از شروع سوسو بزند» را بزن.
//  • HorrorEventManager هم موقع رویدادها StartFlicker را صدا می‌زند.
// ═══════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    [Header("مرجع چراغ")]
    [Tooltip("چراغ مورد نظر (خالی = خودِ این شیء)")]
    [SerializeField] Light targetLight;

    [Header("الگوی سوسو")]
    [Tooltip("از شروع بازی سوسو بزند؟")]
    [SerializeField] bool flickerFromStart = false;
    [Tooltip("کمترین فاصله بین دو پرش نور (ثانیه)")]
    [SerializeField] float minOffTime = 0.03f;
    [Tooltip("بیشترین فاصله بین دو پرش نور (ثانیه)")]
    [SerializeField] float maxOffTime = 0.25f;
    [Tooltip("احتمال خاموش ماندن طولانی‌تر (۰ تا ۱)")]
    [Range(0f, 1f)]
    [SerializeField] float longDarkChance = 0.12f;
    [SerializeField] float longDarkDuration = 1.2f;

    [Header("صدا (اختیاری)")]
    [Tooltip("صدای تق‌تق برق موقع سوسو")]
    [SerializeField] AudioClip buzzSfx;

    float baseIntensity;
    Coroutine flickerRoutine;
    bool baseEnabled;

    void Awake()
    {
        if (targetLight == null) targetLight = GetComponent<Light>();
        if (targetLight != null)
        {
            baseIntensity = targetLight.intensity;
            baseEnabled = targetLight.enabled;
        }
    }

    void Start()
    {
        if (flickerFromStart) StartFlicker();
    }

    // ═══ کنترل عمومی ═════════════════════════════════════════════

    /// <summary>شروع سوسو زدن</summary>
    public void StartFlicker()
    {
        if (targetLight == null) return;
        if (flickerRoutine != null) StopCoroutine(flickerRoutine);
        flickerRoutine = StartCoroutine(FlickerRoutine());
    }

    /// <summary>توقف سوسو و برگشت به حالت عادی</summary>
    public void StopFlicker()
    {
        if (flickerRoutine != null) { StopCoroutine(flickerRoutine); flickerRoutine = null; }
        if (targetLight != null)
        {
            targetLight.enabled = baseEnabled;
            targetLight.intensity = baseIntensity;
        }
    }

    /// <summary>چراغ را کاملاً خاموش کن</summary>
    public void TurnOff()
    {
        StopFlicker();
        if (targetLight != null) targetLight.enabled = false;
    }

    /// <summary>چراغ را روشن کن (بدون سوسو)</summary>
    public void TurnOn()
    {
        StopFlicker();
        if (targetLight != null) { targetLight.enabled = true; targetLight.intensity = baseIntensity; }
    }

    // ═══ حلقه‌ی سوسو ═════════════════════════════════════════════
    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            if (targetLight == null) yield break;

            // خاموش
            targetLight.enabled = false;
            if (buzzSfx != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySfxAt(buzzSfx, transform.position, 0.5f);

            // گاهی خاموشی طولانی (ناگهانی و ترسناک!)
            if (Random.value < longDarkChance)
                yield return new WaitForSeconds(longDarkDuration);
            else
                yield return new WaitForSeconds(Random.Range(minOffTime, maxOffTime));

            if (targetLight == null) yield break;

            // روشن با شدت تصادفی (پرش نور)
            targetLight.enabled = true;
            targetLight.intensity = baseIntensity * Random.Range(0.5f, 1.2f);
            yield return new WaitForSeconds(Random.Range(minOffTime * 2f, maxOffTime * 2f));
        }
    }

    void OnDisable() => StopFlicker();
}
