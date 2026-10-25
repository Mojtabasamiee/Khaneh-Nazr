// ═══════════════════════════════════════════════════════════════
//  RandomSoundPlayer.cs — صداهای تصادفی ترسناک خانه 🎃
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • در فواصل زمانی تصادفی، یکی از صداهای لیست را از مکان خودش پخش می‌کند
//    (جیرجیر، ضربه، نفس، زمزمه...) — خانه را «زنده» می‌کند
//
//  کجا وصل می‌شود؟
//  • روی هر GameObject خالی در جای مناسب خونه (راهرو، اتاق، سرداب)
//  • لیست صداها را پر کن و بازه‌ی زمانی بده.
//  • می‌توانی شرط پرچم بدهی: مثلاً فقط بعد از «وصل کردن فیوز» شروع شود!
// ═══════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;

public class RandomSoundPlayer : MonoBehaviour
{
    [Header("لیست صداها (یکی تصادفی انتخاب می‌شود)")]
    [SerializeField] AudioClip[] sounds;

    [Header("زمان‌بندی")]
    [Tooltip("کمترین فاصله بین دو صدا (ثانیه)")]
    [SerializeField] float minInterval = 12f;
    [Tooltip("بیشترین فاصله بین دو صدا (ثانیه)")]
    [SerializeField] float maxInterval = 30f;
    [Tooltip("تاخیر شروع بعد از بالا آمدن صحنه")]
    [SerializeField] float startDelay = 8f;

    [Header("صدا")]
    [Range(0f, 1f)]
    [SerializeField] float volume = 0.7f;
    [Tooltip("شعاع شنیده شدن (متر)")]
    [SerializeField] float hearingRange = 14f;

    [Header("شرط داستانی (اختیاری)")]
    [Tooltip("فقط بعد از روشن شدن این پرچم شروع به کار کن — خالی = همیشه")]
    [SerializeField] string requireFlag = "";
    [Tooltip("بعد از روشن شدن این پرچم، دیگر کار نکن — خالی = هیچ‌وقت")]
    [SerializeField] string stopFlag = "";

    AudioSource source;
    bool running = false;

    void Start()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 1.5f;
        source.maxDistance = hearingRange;

        StartCoroutine(StartLater());
    }

    IEnumerator StartLater()
    {
        yield return new WaitForSeconds(startDelay);
        running = true;
        StartCoroutine(SoundLoop());
    }

    IEnumerator SoundLoop()
    {
        while (running)
        {
            // چک کردن شرط‌های داستانی
            bool allowed = true;
            if (PuzzleManager.Instance != null)
            {
                if (!string.IsNullOrEmpty(requireFlag) && !PuzzleManager.Instance.IsFlagSet(requireFlag))
                    allowed = false;
                if (!string.IsNullOrEmpty(stopFlag) && PuzzleManager.Instance.IsFlagSet(stopFlag))
                    allowed = false;
            }

            if (allowed && sounds != null && sounds.Length > 0)
            {
                var clip = sounds[Random.Range(0, sounds.Length)];
                if (clip != null)
                    source.PlayOneShot(clip, volume * GameSettings.SfxVolume * GameSettings.MasterVolume);
            }

            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);
        }
    }

    void OnDisable() { running = false; }
}
