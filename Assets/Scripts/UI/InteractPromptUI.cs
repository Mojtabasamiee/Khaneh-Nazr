// ═══════════════════════════════════════════════════════════════
//  InteractPromptUI.cs — متن راهنمای تعامل (پایین وسط صفحه)
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • وقتی بازیکن به شیء تعاملی نگاه می‌کند، متن راهنما (مثل «باز کردن در»)
//    با محو شدن نرم ظاهر می‌شود؛ وقتی از دید خارج شد، محو می‌شود.
//  • PlayerInteractor به این دستور می‌دهد.
//
//  کجا وصل می‌شود؟
//  Canvas صحنه‌ی House:
//
//    InteractPrompt (Text فارسی — پایین وسط، کمی بالاتر از زیرنویس)
//
//  • این کد روی خود Text می‌رود (یا روی یک خالی و مرجعش را بده).
// ═══════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InteractPromptUI : MonoBehaviour
{
    public static InteractPromptUI Instance { get; private set; }

    [Header("مرجع متن")]
    [SerializeField] Text promptText;

    [Header("ظاهر")]
    [SerializeField] float fadeIn = 0.15f;
    [SerializeField] float fadeOut = 0.25f;

    Coroutine routine;

    void Awake()
    {
        Instance = this;

        if (promptText == null) promptText = GetComponent<Text>();
        if (promptText != null)
        {
            promptText.text = "";
            promptText.CrossFadeAlpha(0f, 0f, true);
        }
    }

    // ═══ نمایش ═══════════════════════════════════════════════════
    public void Show(string text)
    {
        if (promptText == null || string.IsNullOrEmpty(text)) return;

        // اگر همان متن قبلی است، کاری نکن (پرت‌کردن UI ممنوع)
        if (promptText.text == text) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(text));
    }

    // ═══ مخفی کردن ═══════════════════════════════════════════════
    public void Hide()
    {
        if (promptText == null) return;

        if (routine != null) { StopCoroutine(routine); routine = null; }

        if (promptText.text != "")
        {
            promptText.CrossFadeAlpha(0f, fadeOut, true);
            promptText.text = "";
        }
    }

    IEnumerator ShowRoutine(string text)
    {
        promptText.text = text;
        promptText.CrossFadeAlpha(1f, fadeIn, true);
        yield return null;
        routine = null;
    }
}
