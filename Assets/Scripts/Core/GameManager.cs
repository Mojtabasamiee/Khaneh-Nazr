// ═══════════════════════════════════════════════════════════════
//  GameManager.cs — قلب مرکزی بازی «خانه‌ی نذر»
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • یک شیء Singleton است یعنی در همه‌جای بازی فقط یکی از آن وجود دارد
//    و از هر کد دیگری با GameManager.Instance به آن دسترسی داری.
//  • وضعیت فعلی بازی را نگه می‌دارد (منو / در حال بازی / مکث / معما / پایان)
//  • مدیریت سوییچ بین صحنه‌ها (منوی اصلی ↔ خانه)
//  • وقتی بازی Pause است، مانع حرکت پلیر می‌شود.
//
//  کجا وصل می‌شود؟
//  • یک GameObject خالی در صحنه‌های MainMenu و House بساز به اسم «GameManager»
//    و این کد را رویش بگذار. (در هر دو صحنه باید باشد — خودش بین صحنه‌ها حفظ می‌شود)
// ═══════════════════════════════════════════════════════════════

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>وضعیت‌های مختلف بازی — همه‌ی سیستم‌های دیگر به این نگاه می‌کنند</summary>
public enum GameState
{
    Menu,       // منوی اصلی
    Playing,    // در حال بازی (پلیر آزاد است)
    Paused,     // بازی متوقف شده (منوی مکث)
    Puzzle,     // یک پنل معما (مثل قفل کد) باز است — پلیر قفل می‌شود
    Ending,     // سکانس پایان بازی
}

public class GameManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── نام صحنه‌ها (اگر اسم صحنه‌ها را عوض کردی اینجا هم عوض کن) ──
    public const string SCENE_MAIN_MENU = "MainMenu";
    public const string SCENE_HOUSE     = "House";

    // ── وضعیت فعلی ───────────────────────────────────────────────
    public GameState State { get; private set; } = GameState.Menu;

    /// <summary>هر وقت وضعیت بازی عوض شد این رویداد (event) صدا زده می‌شود</summary>
    public event Action<GameState> OnStateChanged;

    /// <summary>فقط برای نمایش در Inspector — دست نزن</summary>
    [SerializeField] string debugState = "Menu";

    void Awake()
    {
        // اطمینان از اینکه فقط یک GameManager در کل بازی داریم
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // بین صحنه‌ها از بین نرو

        // تنظیمات ذخیره‌شده (زیرنویس، صدا، حساسیت) را بارگذاری کن
        GameSettings.Load();
    }

    void Start()
    {
        // اگر تازه وارد صحنه‌ای شدیم، وضعیت را با نوع صحنه هماهنگ کن
        string scene = SceneManager.GetActiveScene().name;
        if (scene == SCENE_HOUSE)
            SetState(GameState.Playing);
        else
            SetState(GameState.Menu);
    }

    // ═══ تغییر وضعیت ════════════════════════════════════════════

    public void SetState(GameState newState)
    {
        if (State == newState) return;
        State = newState;
        debugState = newState.ToString();
        OnStateChanged?.Invoke(newState);

        // در مکث، زمان بازی متوقف می‌شود (انیمیشن‌ها و فیزیک می‌ایستند)
        Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;
    }

    /// <summary>آیا پلیر اجازه‌ی حرکت و نگاه دارد؟</summary>
    public bool CanPlayerAct()
    {
        return State == GameState.Playing;
    }

    // ═══ شروع / ادامه‌ی بازی ═════════════════════════════════════

    /// <summary>دکمه‌ی «بازی جدید» منو — به صحنه‌ی خانه می‌رود</summary>
    public void StartNewGame()
    {
        SaveSystem.DeleteSave();            // بازی از صفر شروع می‌شود
        StartCoroutine(LoadSceneRoutine(SCENE_HOUSE));
    }

    /// <summary>دکمه‌ی «ادامه‌ی بازی» — از آخرین ذخیره ادامه می‌دهد</summary>
    public void ContinueGame()
    {
        StartCoroutine(LoadSceneRoutine(SCENE_HOUSE, applySave: true));
    }

    /// <summary>برگشت به منوی اصلی (از منوی مکث یا پایان)</summary>
    public void BackToMenu()
    {
        // اگر وسط بازی هستیم قبل از رفتن، ذخیره کن
        if (SaveSystem.HasSave() || SceneManager.GetActiveScene().name == SCENE_HOUSE)
            SaveSystem.SaveGame();

        Time.timeScale = 1f;
        StartCoroutine(LoadSceneRoutine(SCENE_MAIN_MENU));
    }

    /// <summary>خروج کامل از بازی (دکمه‌ی خروج منو)</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ═══ بارگذاری صحنه ═══════════════════════════════════════════

    IEnumerator LoadSceneRoutine(string sceneName, bool applySave = false)
    {
        // قبل از عوض کردن صحنه، وضعیت را تمیز کن
        SetState(GameState.Menu);
        Time.timeScale = 1f;

        // اگر صفحه‌ی بارگذاری در صحنه موجود بود، از آن استفاده کن
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene(sceneName); // خودش صحنه را عوض می‌کند
        }
        else
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (op != null && !op.isDone)
                yield return null;
        }

        // صبر تا صحنه‌ی جدید واقعاً فعال شود
        float timeout = 20f;
        while (SceneManager.GetActiveScene().name != sceneName && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }
        yield return null; // یک فریم صبر تا تک‌تک سیستم‌های صحنه Awake شوند

        if (applySave)
        {
            // ذخیره‌ی بازی را روی صحنه‌ی تازه اعمال کن
            SaveSystem.ApplyLoadedSave();
            SetState(GameState.Playing);
        }
        else if (sceneName == SCENE_HOUSE)
        {
            SetState(GameState.Playing);
        }
        else
        {
            SetState(GameState.Menu);
        }
    }
}
