// ═══════════════════════════════════════════════════════════════
//  PeripheralGhost.cs — شبحِ گوشه‌ی چشم 👁️
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • شبح فقط وقتی دیده می‌شود که بازیکن «مستقیم نگاهش نکند»!
//    (وقتی چشم بازیکن به سمتش می‌چرخد، محو می‌شود — وحشتناک‌ترین ترس)
//  • دو حالت:
//    گوشه‌ی چشم  = فقط در حاشیه‌ی دید دیده می‌شود
//    ظاهر و غیب = ظاهر می‌شود و بعد از چند ثانیه یا با نگاه بازیکن غیب می‌شود
//
//  کجا وصل می‌شود؟
//  • یک مدل شبح (سیلیوئت تاریک/سایه) بساز و این کد رویش بگذار
//  • شیء را در جای خودش بگذار و غیرفعال کن.
//  • فعال‌سازی از HorrorEventManager یا UnityEvent: Appear()
// ═══════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;

public class PeripheralGhost : MonoBehaviour
{
    /// <summary>حالت ظاهر شدن شبح</summary>
    public enum GhostMode
    {
        CornerOfEye,   // فقط وقتی نگاه نمی‌کنی دیده می‌شود
        AppearAndVanish, // ظاهر می‌شود، بعد از مدتی غیب می‌شود
    }

    [Header("حالت")]
    [SerializeField] GhostMode mode = GhostMode.CornerOfEye;

    [Header("گوشه‌ی چشم")]
    [Tooltip("زاویه (درجه) — اگر نگاه بازیکن داخل این زاویه باشد، شبح محو می‌شود")]
    [SerializeField] float vanishAngle = 35f;

    [Header("ظاهر و غیب")]
    [Tooltip("حداکثر مدت ظاهر ماندن (ثانیه)")]
    [SerializeField] float maxVisibleTime = 6f;
    [Tooltip("صدای ظاهر شدن (زمزمه و...)")]
    [SerializeField] AudioClip appearSfx;
    [Tooltip("صدای غیب شدن")]
    [SerializeField] AudioClip vanishSfx;

    [Header("محو شدن نرم")]
    [SerializeField] float fadeTime = 0.6f;

    Renderer[] renderers;
    Coroutine activeRoutine;
    bool isVisible = false;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        SetVisibleImmediate(false);
    }

    // ═══ ظاهر شدن ════════════════════════════════════════════════

    /// <summary>شبح ظاهر شود — از رویدادها صدا زده می‌شود</summary>
    public void Appear()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        SetVisibleImmediate(true);
        isVisible = true;

        if (appearSfx != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfxAt(appearSfx, transform.position, 0.8f);

        activeRoutine = StartCoroutine(GhostLogic());
    }

    /// <summary>غیب شدن دستی</summary>
    public void Vanish()
    {
        if (activeRoutine != null) { StopCoroutine(activeRoutine); activeRoutine = null; }
        StartCoroutine(FadeOut());
    }

    // ═══ منطق شبح ════════════════════════════════════════════════
    IEnumerator GhostLogic()
    {
        float timer = 0f;

        while (isVisible)
        {
            Camera cam = PlayerController.Instance != null
                ? PlayerController.Instance.GetComponentInChildren<Camera>()
                : Camera.main;

            if (cam != null)
            {
                Vector3 dirToGhost = (transform.position - cam.transform.position).normalized;
                float angle = Vector3.Angle(cam.transform.forward, dirToGhost);

                // فاصله هم مهم است — شبح نزدیک که شد، فرار می‌کند!
                float distance = Vector3.Distance(cam.transform.position, transform.position);

                if (mode == GhostMode.CornerOfEye)
                {
                    // اگر بازیکن مستقیم نگاهش کرد یا خیلی نزدیک شد → غیب
                    if (angle < vanishAngle || distance < 2.5f)
                    {
                        Vanish();
                        yield break;
                    }
                }
                else // AppearAndVanish
                {
                    // اگر بازیکن مستقیم نگاهش کرد → غیب
                    if (angle < vanishAngle * 0.5f)
                    {
                        Vanish();
                        yield break;
                    }
                }
            }

            timer += Time.deltaTime;
            if (timer > maxVisibleTime)
            {
                Vanish();
                yield break;
            }

            yield return null;
        }
    }

    // ═══ نمایش / محو ═════════════════════════════════════════════
    void SetVisibleImmediate(bool visible)
    {
        if (renderers == null) return;
        foreach (var r in renderers)
        {
            if (r == null) continue;
            // با تغییر سایه‌ها هم می‌شود بازی کرد؛ ساده نگه می‌داریم
            r.enabled = visible;
        }
    }

    IEnumerator FadeOut()
    {
        // برای سادگی، محو با کوچک شدن — اگر متریال شفاف داری بهتر جواب می‌دهد
        if (vanishSfx != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfxAt(vanishSfx, transform.position, 0.6f);

        yield return new WaitForSeconds(fadeTime);
        SetVisibleImmediate(false);
        isVisible = false;
    }
}
