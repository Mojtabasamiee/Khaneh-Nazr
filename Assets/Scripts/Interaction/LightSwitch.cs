// ═══════════════════════════════════════════════════════════════
//  LightSwitch.cs — کلید برق و چراغ‌ها
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • با تعامل، یک یا چند چراغ (Light) را روشن/خاموش می‌کند
//  • می‌تواند با یک پرچم داستان قفل باشد (مثلاً تا فیوز وصل نشده کار نکند)
//
//  کجا وصل می‌شود؟
//  • روی هر مدل ساده‌ی کلید برق + Collider
//  • چراغ‌هایی که باید کنترل شوند را در لیست «چراغ‌ها» بگذار
//    (یا اشیایی که چراغ‌شان فرزند است — گزینه‌ی «فرزندان»)
// ═══════════════════════════════════════════════════════════════

using UnityEngine;

public class LightSwitch : Interactable
{
    [Header("چراغ‌های کنترل‌شونده")]
    [Tooltip("چراغ‌هایی که این کلید مستقیم روشن/خاموش می‌کند")]
    [SerializeField] Light[] lights;
    [Tooltip("همه‌ی چراغ‌های فرزند هم کنترل شوند؟")]
    [SerializeField] bool includeChildren = true;

    [Header("قفل با داستان")]
    [Tooltip("تا وقتی این پرچم روشن نشده، کلید کار نمی‌کند (مثلاً فیوز وصل نیست) — خالی = همیشه کار می‌کند")]
    [SerializeField] string requiredFlag = "";
    [Tooltip("متن راهنما وقتی پرچم لازم هنوز نیست")]
    [SerializeField] string lockedHint = "برق نمی‌آید...";

    [Header("صدا")]
    [SerializeField] AudioClip clickSfx;

    bool isOn = false;

    public override string InteractionPrompt
    {
        get
        {
            if (!CanSwitch()) return lockedHint;
            return isOn ? "خاموش کردن" : "روشن کردن";
        }
    }

    bool CanSwitch()
    {
        if (string.IsNullOrEmpty(requiredFlag)) return true;
        return PuzzleManager.Instance != null &&
               PuzzleManager.Instance.IsFlagSet(requiredFlag);
    }

    protected override void OnInteracted()
    {
        // چک پرچم لازم
        if (!CanSwitch())
        {
            PlaySfx(clickSfx);
            SubtitleUI.Instance?.ShowHint(lockedHint);
            return;
        }

        isOn = !isOn;
        PlaySfx(clickSfx);
        ApplyState();
    }

    void ApplyState()
    {
        if (lights != null)
            foreach (var l in lights)
                if (l != null) l.enabled = isOn;

        if (includeChildren)
            foreach (var l in GetComponentsInChildren<Light>(true))
                l.enabled = isOn;

        // اگر می‌خواهی با روشن کردن کلید، رویدادی رخ دهد:
        // در Inspector بخش onInteract این را وصل کن
    }

    /// <summary>از کدهای دیگر (مثل فیوز) کلید را اجباری روشن کن</summary>
    public void ForceOn()
    {
        isOn = true;
        ApplyState();
    }

    public void ForceOff()
    {
        isOn = false;
        ApplyState();
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfxAt(clip, transform.position);
    }
}
