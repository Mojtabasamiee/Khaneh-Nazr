// ═══════════════════════════════════════════════════════════════
//  JumpScare.cs — جامپ‌اسکر تمام‌صفحه 💀
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • یک تصویر ترسناک را ناگهانی تمام صفحه نشان می‌دهد + صدای شوک + لرزش
//  • بعد از مدتی محو می‌شود
//
//  کجا وصل می‌شود؟
//  ساختار در Canvas:
//
//    JumpScarePanel (Full Screen — شفاف و غیرفعال در شروع)
//    └── ScareImage (تصویر ترسناک وسط صفحه)
//
//  • این کد روی JumpScarePanel می‌رود.
//  • برای فعال‌سازی: از HorrorEventManager یا UnityEvent هر شیء:
//    JumpScare.Instance.Play()  ← در رویداد onTriggered بگذار
//  • یک تصویر ترسناک (بدون پس‌زمینه/PNG شفاف) به «تصویر ترس» بده.
// ═══════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JumpScare : MonoBehaviour
{
    public static JumpScare Instance { get; private set; }

    [Header("اجزای جامپ‌اسکر")]
    [Tooltip("تصویر ترسناک (فرزند پنل)")]
    [SerializeField] Image scareImage;
    [Tooltip("صدای شوک")]
    [SerializeField] AudioClip scareSfx;
    [Range(0f, 1f)]
    [SerializeField] float sfxVolume = 1f;

    [Header("زمان‌بندی")]
    [Tooltip("مدت نمایش کامل تصویر (ثانیه)")]
    [SerializeField] float holdDuration = 0.8f;
    [Tooltip("مدت محو شدن (ثانیه)")]
    [SerializeField] float fadeDuration = 0.4f;
    [Tooltip("شدت لرزش دوربین")]
    [SerializeField] float shakeIntensity = 0.35f;

    [Header("افکت ورود تصویر")]
    [Tooltip("تصویر با زوم بزرگ شروع شود و برود داخل؟")]
    [SerializeField] bool zoomIn = true;
    [SerializeField] float zoomStartScale = 2.2f;
    [SerializeField] float zoomEndScale = 1f;

    CanvasGroup canvasGroup;
    RectTransform imageRect;
    bool playing = false;

    void Awake()
    {
        Instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (scareImage != null) imageRect = scareImage.rectTransform;

        // در شروع، کاملاً نامرئی و غیرقابل کلیک
        // (نکته: با alpha کار می‌کنیم نه SetActive — تا فراخوانی Play همیشه درست باشد)
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    /// <summary>اجرای جامپ‌اسکر — از هر جایی صدا زده می‌شود</summary>
    public void Play()
    {
        if (playing) return;
        gameObject.SetActive(true);
        playing = true;
        StartCoroutine(ScareRoutine());
    }

    IEnumerator ScareRoutine()
    {
        // صدا + لرزش — فوری و بی‌ملاحظه!
        if (scareSfx != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx2D(scareSfx, sfxVolume);
        if (PlayerController.Instance != null)
            PlayerController.Instance.Shake(shakeIntensity);

        // آماده‌سازی تصویر
        canvasGroup.alpha = 1f;
        if (zoomIn && imageRect != null)
            imageRect.localScale = Vector3.one * zoomStartScale;

        // زوم داخل شدن
        if (zoomIn && imageRect != null)
        {
            float t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime / 0.15f;
                imageRect.localScale = Vector3.Lerp(
                    Vector3.one * zoomStartScale, Vector3.one * zoomEndScale, t);
                yield return null;
            }
        }

        // نگه داشتن
        yield return new WaitForSeconds(holdDuration);

        // محو شدن
        float ft = 0f;
        while (ft < 1f)
        {
            ft += Time.deltaTime / Mathf.Max(0.01f, fadeDuration);
            canvasGroup.alpha = 1f - ft;
            yield return null;
        }

        // پاکسازی
        canvasGroup.alpha = 0f;
        playing = false;
        gameObject.SetActive(false);
    }
}
