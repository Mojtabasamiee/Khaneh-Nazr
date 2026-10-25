// ═══════════════════════════════════════════════════════════════
//  LoadingScreen.cs — صفحه‌ی بارگذاری با نکته‌های ترسناک 🌑
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • موقع رفتن از منو به خانه (یا برعکس)، یک صفحه‌ی سیاه با نکته نشان می‌دهد
//  • صحنه را در پس‌زمینه بارگذاری می‌کند (async) تا بازی قشنگ بالا بیاید
//
//  کجا وصل می‌شود؟
//  Canvas صحنه‌ی MainMenu:
//
//    LoadingPanel (تمام‌صفحه، سیاه، غیرفعال در شروع)
//    ├── TipText     (نکته‌ی تصادفی)
//    └── ProgressBar (اختیاری — Image با Fill Mode=Filled)
//
//  • این کد روی LoadingPanel می‌رود.
//  • نکته‌ها: خودت فارسی بنویس (لیست پایین را عوض کن!)
// ═══════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("مرجع‌ها")]
    [SerializeField] Text tipText;
    [SerializeField] Image progressBar;

    [Header("نکته‌های بارگذاری — خودت عوضش کن!")]
    [TextArea(1, 2)]
    [SerializeField] string[] tips = new string[]
    {
        "صدای زمزمه را روشن نگه دار... شاید راهنمایی کند.",
        "به آینه‌ها اعتماد نکن.",
        "نذر نیمه‌کاره، هرگز کهنه نمی‌شود...",
        "جمعه‌شب‌ها به سرداب نرو.",
        "بعضی درها فقط با یادآوری باز می‌شوند.",
        "شمع‌ها را بی‌ترتیب روشن نکن.",
    };

    [Header("حداقل مدت نمایش")]
    [SerializeField] float minDisplayTime = 1.2f;

    Coroutine loadRoutine;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    // ═══ بارگذاری با صفحه‌ی نکته ═════════════════════════════════
    /// <summary>از منو صدا بزن: LoadingScreen.Instance.LoadScene("House")</summary>
    public void LoadScene(string sceneName)
    {
        if (loadRoutine != null) return;
        loadRoutine = StartCoroutine(LoadRoutine(sceneName));
    }

    IEnumerator LoadRoutine(string sceneName)
    {
        gameObject.SetActive(true);

        // نکته‌ی تصادفی
        if (tipText != null && tips != null && tips.Length > 0)
            tipText.text = tips[Random.Range(0, tips.Length)];

        if (progressBar != null) progressBar.fillAmount = 0f;

        // شروع بارگذاری در پس‌زمینه
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float elapsed = 0f;
        while (op.progress < 0.9f)
        {
            elapsed += Time.unscaledDeltaTime;
            if (progressBar != null)
                progressBar.fillAmount = Mathf.MoveTowards(progressBar.fillAmount, op.progress / 0.9f, Time.unscaledDeltaTime);
            yield return null;
        }

        // حداقل زمان نمایش (که نکته خوانده شود)
        while (elapsed < minDisplayTime)
        {
            elapsed += Time.unscaledDeltaTime;
            if (progressBar != null)
                progressBar.fillAmount = Mathf.MoveTowards(progressBar.fillAmount, 1f, Time.unscaledDeltaTime);
            yield return null;
        }

        // ورود به صحنه
        op.allowSceneActivation = true;
        yield return null;

        loadRoutine = null;
        // خودمان دیگر نیازی به غیرفعال کردن نیست — صحنه عوض می‌شود
    }
}
