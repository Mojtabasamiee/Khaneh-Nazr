// ═══════════════════════════════════════════════════════════════
//  SubtitleUI.cs — زیرنویس فارسی و پیام‌های کوتاه
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • زیرنویس گویندگی‌ها را پایین صفحه نشان می‌دهد
//    ⚠️ اگر در تنظیمات، زیرنویس خاموش باشد → نشان نمی‌دهد (فقط صدا پخش می‌شود)
//  • دو نوع پیام دارد:
//    زیرنویس (sub) = حرف‌زدن‌های داستان — پایین صفحه
//    راهنما (hint) = پیام‌های کوتاه سیستم — کمی بالاتر
//
//  کجا وصل می‌شود؟
//  ساختار Canvas:
//
//    SubtitleText  ← متن زیرنویس (پایین وسط، فونت فارسی بزرگ)
//    HintText      ← متن راهنما (کمی بالاتر، کوچک‌تر)
//
//  • این کد روی یک GameObject خالی به اسم «SubtitleUI» در Canvas بگذار
//    و دو مرجع متن را بده. (در صحنه‌ی House)
//
//  ⚠️ نکته‌ی خیلی مهم: برای متن فارسی از «Text» معمولی استفاده کن،
//  نه TextMeshPro — چون TMP حروف فارسی را جدا-جدا نشان می‌دهد!
// ═══════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleUI : MonoBehaviour
{
    public static SubtitleUI Instance { get; private set; }

    [Header("مرجع‌های متن")]
    [Tooltip("متن زیرنویس گویندگی (پایین صفحه)")]
    [SerializeField] Text subtitleText;
    [Tooltip("متن راهنمای سیستم (مثل: در قفل است...)")]
    [SerializeField] Text hintText;

    [Header("ظاهر")]
    [SerializeField] Color subtitleColor = new Color(1f, 0.95f, 0.85f);
    [SerializeField] Color hintColor = new Color(0.8f, 0.8f, 0.75f);

    Coroutine subtitleRoutine;
    Coroutine hintRoutine;

    void Awake()
    {
        Instance = this;

        if (subtitleText != null)
        {
            subtitleText.text = "";
            subtitleText.color = subtitleColor;
        }
        if (hintText != null)
        {
            hintText.text = "";
            hintText.color = hintColor;
        }
    }

    // ═══ زیرنویس گویندگی ═════════════════════════════════════════
    /// <summary>نمایش زیرنویس به مدت طول صدا (NarrationManager صدا می‌زند)</summary>
    public void ShowSubtitle(string text, float duration)
    {
        // ★ قلب سیستم زیرنویس: اگر در تنظیمات خاموش است، هیچ!
        if (!GameSettings.SubtitlesEnabled) return;

        if (subtitleRoutine != null) StopCoroutine(subtitleRoutine);
        subtitleRoutine = StartCoroutine(ShowRoutine(subtitleText, text, duration));
    }

    /// <summary>مخفی کردن زیرنویس (وقتی خط تمام می‌شود)</summary>
    public void HideSubtitle()
    {
        if (subtitleRoutine != null) { StopCoroutine(subtitleRoutine); subtitleRoutine = null; }
        if (subtitleText != null) subtitleText.text = "";
    }

    // ═══ راهنمای کوتاه (مثل: «در قفل است») ═══════════════════════
    /// <summary>نمایش پیام کوتاه — همیشه نشان داده می‌شود (زیرنویس نیست)</summary>
    public void ShowHint(string text, float duration = 2.5f)
    {
        if (hintText == null || string.IsNullOrEmpty(text)) return;

        if (hintRoutine != null) StopCoroutine(hintRoutine);
        hintRoutine = StartCoroutine(ShowRoutine(hintText, text, duration));
    }

    // ═══ اجرای نمایش با محو شدن ══════════════════════════════════
    IEnumerator ShowRoutine(Text target, string text, float duration)
    {
        if (target == null) yield break;

        target.text = text;
        target.CrossFadeAlpha(1f, 0.15f, true);

        yield return new WaitForSeconds(duration);

        // محو شدن نرم
        target.CrossFadeAlpha(0f, 0.5f, true);
        yield return new WaitForSeconds(0.5f);
        target.text = "";
        target.CrossFadeAlpha(1f, 0f, true);
    }
}
