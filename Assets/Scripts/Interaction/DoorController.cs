// ═══════════════════════════════════════════════════════════════
//  DoorController.cs — درهای بازی (باز/بسته/قفل)
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • در با تعامل، به آرامی باز و بسته می‌شود (انیمیشن چرخش لولایی)
//  • می‌توانی در را قفل کنی که فقط با کلیدِ مشخص باز شود
//  • موقع قفل بودن، متن «قفل است...» و صدای قفل پخش می‌شود
//
//  کجا وصل می‌شود؟
//  ساختار مهم — در باید Pivot داشته باشد (لولا):
//
//    DoorPivot   ← این کد + Collider روی پنل در
//    └── DoorPanel (Mesh در)
//
//  Pivot را دقیقاً سر جای لولا (لبه‌ی در) بگذار!
// ═══════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;

public class DoorController : Interactable
{
    [Header("تنظیمات در")]
    [Tooltip("زاویه‌ی باز شدن در (درجه) — معمولاً ۹۰ یا منفی ۹۰")]
    [SerializeField] float openAngle = 90f;
    [Tooltip("سرعت باز/بسته شدن در")]
    [SerializeField] float openSpeed = 2.5f;

    [Header("قفل و کلید")]
    [Tooltip("آیا در در شروع بازی قفل است؟")]
    [SerializeField] bool startLocked = false;
    [Tooltip("شناسه‌ی کلیدی که بازش می‌کند (خالی = بدون کلید، فقط قفل/باز با پرچم)")]
    [SerializeField] string keyItemId = "";
    [Tooltip("با این پرچم داستان، در آزاد می‌شود (مثلاً بعد از حل معما) — خالی = بی‌ربط")]
    [SerializeField] string unlockFlag = "";
    [Tooltip("بعد از باز شدن، دوباره قفل شود؟ (برای درهایی که فقط یکبار باز می‌شوند)")]
    [SerializeField] bool lockAgainAfterClose = false;

    [Header("صداها")]
    [SerializeField] AudioClip openSfx;
    [SerializeField] AudioClip closeSfx;
    [SerializeField] AudioClip lockedSfx2;
    [SerializeField] AudioClip unlockSfx;

    [Header("گویندگی (اختیاری)")]
    [Tooltip("شناسه‌ی خط گویندگی وقتی در قفل است و کلید نداری")]
    [SerializeField] string lockedVoiceId = "";
    [Tooltip("شناسه‌ی خط گویندگی وقتی در با کلید باز می‌شود")]
    [SerializeField] string unlockVoiceId = "";

    bool isOpen = false;
    bool isAnimating = false;
    Quaternion closedRotation;
    Quaternion openRotation;

    protected virtual void Awake()
    {
        closedRotation = transform.localRotation;
        openRotation = Quaternion.Euler(0f, openAngle, 0f) * closedRotation;
    }

    public override bool CanInteract() => !isAnimating && base.CanInteract();

    public override string InteractionPrompt
    {
        get
        {
            // آیا در قفل است؟
            if (IsLocked())
            {
                // اگر کلیدش را داریم؟
                if (!string.IsNullOrEmpty(keyItemId) && HasRequiredItem())
                    return "باز کردن در با " + GetKeyName();
                return "در قفل است";
            }
            return isOpen ? "بستن در" : "باز کردن در";
        }
    }

    bool IsLocked()
    {
        if (!startLocked) return false;

        // پرچم آزادسازی؟
        if (!string.IsNullOrEmpty(unlockFlag) &&
            PuzzleManager.Instance != null &&
            PuzzleManager.Instance.IsFlagSet(unlockFlag))
            return false;

        return true;
    }

    string GetKeyName()
    {
        // اگر دیتابیس آیتم موجود باشد، نام فارسی کلید را نشان بده
        if (InventoryManager.Instance != null)
        {
            foreach (var item in InventoryManager.Instance.GetAllItems())
                if (item.id == keyItemId) return item.displayName;
        }
        return "کلید";
    }

    public override void Interact()
    {
        // ── قفل چک ────────────────────────────────────────────────
        if (IsLocked())
        {
            // کلید را دارد؟
            if (!string.IsNullOrEmpty(keyItemId) && HasRequiredItem())
            {
                // باز کردن با کلید
                ConsumeRequiredItem(); // کلید مصرف می‌شود
                startLocked = false;
                PlaySfx(unlockSfx);
                if (!string.IsNullOrEmpty(unlockVoiceId) && NarrationManager.Instance != null)
                    NarrationManager.Instance.PlayById(unlockVoiceId);
            }
            else
            {
                // قفل مانده
                PlaySfx(lockedSfx2);
                if (!string.IsNullOrEmpty(lockedVoiceId) && NarrationManager.Instance != null)
                    NarrationManager.Instance.PlayById(lockedVoiceId);
                else
                    SubtitleUI.Instance?.ShowHint("در قفل است...");
                return;
            }
        }

        // ── باز/بسته ──────────────────────────────────────────────
        if (isAnimating) return;
        isOpen = !isOpen;
        PlaySfx(isOpen ? openSfx : closeSfx);
        StartCoroutine(RotateDoor());
    }

    IEnumerator RotateDoor()
    {
        isAnimating = true;
        Quaternion from = transform.localRotation;
        Quaternion to = isOpen ? openRotation : closedRotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            transform.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        transform.localRotation = to;
        isAnimating = false;

        // اگر می‌خواهی بعد از بسته شدن دوباره قفل شود
        if (!isOpen && lockAgainAfterClose)
            startLocked = true;
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfxAt(clip, transform.position);
    }

    /// <summary>اگر از کد دیگری می‌خواهی در را آزاد کنی</summary>
    public void ForceUnlock()
    {
        startLocked = false;
        PlaySfx(unlockSfx);
    }
}
