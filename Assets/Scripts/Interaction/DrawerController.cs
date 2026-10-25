// ═══════════════════════════════════════════════════════════════
//  DrawerController.cs — کشوها (باز شدن کشویی + آیتم پنهان)
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • کشو با تعامل به آرامی بیرون می‌آید و با تعامل بعدی می‌رود داخل
//  • اگر داخلش چیزی پنهان کرده باشی (کلید و...) با باز شدن ظاهر می‌شود
//
//  کجا وصل می‌شود؟
//  ساختار:
//
//    Drawer (این کد + Collider)   ← کشو که حرکت می‌کند
//    └── HiddenItem (PickupItem)  ← آیتم داخل کشو — در ابتدای غیرفعال
//
//  • جهت کشیدن کشو با «آفست باز» تنظیم می‌شود (معمولاً محور Z محلی)
// ═══════════════════════════════════════════════════════════════

using System.Collections;
using UnityEngine;

public class DrawerController : Interactable
{
    [Header("تنظیمات کشو")]
    [Tooltip("چقدر کشو بیرون بیاید؟ (در مختصات محلی — مثلاً Z منفی یعنی رو به بیرون)")]
    [SerializeField] Vector3 openOffset = new Vector3(0f, 0f, -0.35f);
    [Tooltip("سرعت باز/بسته شدن")]
    [SerializeField] float slideSpeed = 2f;

    [Header("آیتم پنهان")]
    [Tooltip("چیزی که داخل کشو پنهان است (اختیاری) — بعد از باز شدن فعال می‌شود")]
    [SerializeField] GameObject hiddenItem;

    [Header("صداها")]
    [SerializeField] AudioClip openSfx;
    [SerializeField] AudioClip closeSfx;
    [Tooltip("صدای جیرجیر چوبی موقع کشیدن (اختیاری و هولناک!)")]
    [SerializeField] AudioClip creakSfx;

    bool isOpen = false;
    bool isAnimating = false;
    Vector3 closedPos;
    Vector3 openPos;
    bool itemRevealed = false;

    void Awake()
    {
        closedPos = transform.localPosition;
        openPos = closedPos + openOffset;
    }

    void Start()
    {
        // آیتم پنهان در شروع، نامرئی باشد
        if (hiddenItem != null) hiddenItem.SetActive(false);
    }

    public override bool CanInteract() => !isAnimating && base.CanInteract();

    public override string InteractionPrompt =>
        isOpen ? "بستن کشو" : "باز کردن کشو";

    protected override void OnInteracted()
    {
        if (isAnimating) return;
        isOpen = !isOpen;

        // صداها
        if (AudioManager.Instance != null)
        {
            if (creakSfx != null) AudioManager.Instance.PlaySfxAt(creakSfx, transform.position);
            AudioManager.Instance.PlaySfxAt(isOpen ? openSfx : closeSfx, transform.position);
        }

        // ظاهر شدن آیتم پنهان (با کمی تاخیر تا کشو باز شود)
        if (isOpen && hiddenItem != null && !itemRevealed)
        {
            itemRevealed = true;
            StartCoroutine(RevealItemAfterDelay(0.4f));
        }

        StartCoroutine(SlideDrawer());
    }

    IEnumerator RevealItemAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hiddenItem != null) hiddenItem.SetActive(true);
    }

    IEnumerator SlideDrawer()
    {
        isAnimating = true;
        Vector3 from = transform.localPosition;
        Vector3 to = isOpen ? openPos : closedPos;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * slideSpeed;
            transform.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        transform.localPosition = to;
        isAnimating = false;
    }
}
