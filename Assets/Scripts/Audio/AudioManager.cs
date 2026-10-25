// ═══════════════════════════════════════════════════════════════
//  AudioManager.cs — مدیریت کل صداهای بازی
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • سه کانال صدا دارد: موسیقی / محیط / افکت — هر کدام ولوم جدا از تنظیمات
//  • موسیقی و صدای محیط را با محو شدن (fade) عوض می‌کند
//  • افکت‌های ۲بعدی و ۳بعدی را پخش می‌کند
//
//  کجا وصل می‌شود؟
//  • یک GameObject خالی به اسم «AudioManager» در هر دو صحنه (منو + خانه)
//    بساز و این کد را رویش بگذار. (خودش بین صحنه‌ها حفظ می‌شود)
//  • لیست «صداهای محیط» را با نام پر کن: مثلاً house_quiet → فایل صدای خونه
//  • هر جا صدایی لازم شد: AudioManager.Instance.PlaySfxAt(...) و ...
// ═══════════════════════════════════════════════════════════════

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class AmbientEntry
    {
        public string name;      // نام محیط: house_quiet, basement, rain...
        public AudioClip clip;   // فایل صدا
        public float volume = 1f;
    }

    [Header("صداهای محیط (با نام)")]
    [Tooltip("فایل‌های صدای محیط — با نامشان صدا زده می‌شوند")]
    [SerializeField] List<AmbientEntry> ambientLibrary = new List<AmbientEntry>();
    [Tooltip("صدای محیط پیش‌فرض صحنه‌ی خانه — با ورود به خانه پخش می‌شود")]
    [SerializeField] string defaultAmbient = "house_quiet";
    [Tooltip("صدای محیط پیش‌فرض منو")]
    [SerializeField] string menuAmbient = "";
    [SerializeField] float ambientFadeSeconds = 2f;

    // ── منابع صوتی ───────────────────────────────────────────────
    AudioSource musicSource;
    AudioSource ambientSource;
    AudioSource sfx2DSource;

    Coroutine ambientFadeRoutine;
    string currentAmbientName = "";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ساخت سه پخش‌کننده
        musicSource   = gameObject.AddComponent<AudioSource>();
        ambientSource = gameObject.AddComponent<AudioSource>();
        sfx2DSource   = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        ambientSource.loop = true;
        musicSource.playOnAwake = false;
        ambientSource.playOnAwake = false;
        sfx2DSource.playOnAwake = false;

        ApplyVolumes();

        // شروع صدای محیط بسته به صحنه
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string startAmbient = (scene == GameManager.SCENE_HOUSE) ? defaultAmbient : menuAmbient;
        if (!string.IsNullOrEmpty(startAmbient))
            PlayAmbient(startAmbient);
    }

    // ═══ ولوم‌ها (از GameSettings) ═══════════════════════════════
    public void ApplyVolumes()
    {
        if (musicSource   != null) musicSource.volume   = GameSettings.MusicVolume  * GameSettings.MasterVolume;
        if (ambientSource != null) ambientSource.volume = GameSettings.SfxVolume    * GameSettings.MasterVolume;
        if (sfx2DSource   != null) sfx2DSource.volume   = GameSettings.SfxVolume    * GameSettings.MasterVolume;
    }

    // ═══ موسیقی ══════════════════════════════════════════════════
    public void PlayMusic(AudioClip clip, float fadeSeconds = 1.5f)
    {
        StartCoroutine(FadeSwapRoutine(musicSource, clip, fadeSeconds, GameSettings.MusicVolume * GameSettings.MasterVolume));
    }

    public void StopMusic(float fadeSeconds = 1f)
    {
        StartCoroutine(FadeSwapRoutine(musicSource, null, fadeSeconds, 0f));
    }

    // ═══ صدای محیط ═══════════════════════════════════════════════
    /// <summary>عوض کردن صدای محیط با نام — مثلاً PlayAmbient("basement")</summary>
    public void PlayAmbient(string name)
    {
        if (name == currentAmbientName) return;

        AmbientEntry entry = ambientLibrary.Find(a => a.name == name);
        if (entry == null || entry.clip == null)
        {
            Debug.LogWarning("صدای محیطی با نام «" + name + "» پیدا نشد.");
            return;
        }

        currentAmbientName = name;
        if (ambientFadeRoutine != null) StopCoroutine(ambientFadeRoutine);
        ambientFadeRoutine = StartCoroutine(FadeSwapRoutine(
            ambientSource, entry.clip, ambientFadeSeconds,
            entry.volume * GameSettings.SfxVolume * GameSettings.MasterVolume));
    }

    // ═══ افکت‌ها ═════════════════════════════════════════════════
    /// <summary>پخش افکت ۲بعدی (بدون مکان — مثل صدای UI)</summary>
    public void PlaySfx2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfx2DSource.PlayOneShot(clip, volume);
    }

    /// <summary>پخش افکت ۳بعدی در یک نقطه از دنیای بازی (در، کشو، قفل...)</summary>
    public void PlaySfxAt(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        GameObject go = new GameObject("TempSFX");
        go.transform.position = position;
        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 1f;              // صدای ۳بعدی
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 1f;
        src.maxDistance = 18f;
        src.volume = volume * GameSettings.SfxVolume * GameSettings.MasterVolume;
        src.Play();
        Destroy(go, clip.length + 0.1f);
    }

    // ═══ محو شدن نرم بین دو صدا ══════════════════════════════════
    IEnumerator FadeSwapRoutine(AudioSource src, AudioClip newClip, float fade, float targetVolume)
    {
        // محو شدن صدای قبلی
        if (src.clip != null)
        {
            float start = src.volume;
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.01f, fade))
            {
                src.volume = Mathf.Lerp(start, 0f, t);
                yield return null;
            }
        }
        src.Stop();

        // شروع صدای جدید و بالا آمدن نرم
        src.clip = newClip;
        if (newClip != null)
        {
            src.Play();
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.01f, fade))
            {
                src.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }
            src.volume = targetVolume;
        }
    }
}
