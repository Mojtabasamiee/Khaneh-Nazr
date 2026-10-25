// ═══════════════════════════════════════════════════════════════
//  InventoryUI.cs — نمایش انبار روی صفحه‌ی گوشی
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • پنل انبار را باز/بسته می‌کند (دکمه‌ی کوله‌پشتی HUD)
//  • خودش برای هر آیتم یک اسلوت (خانه) با آیکون می‌سازد
//  • با لمس هر آیتم، توضیح فارسی‌اش پایین پنل نمایش داده می‌شود
//
//  کجا وصل می‌شود؟
//  ساختار Canvas لازم:
//
//    InventoryPanel (پنل نیمه‌شفاف پایین/کل صفحه)
//    ├── Content       ← با Grid Layout Group (جا برای اسلوت‌ها)
//    └── DescriptionText ← متن توضیح آیتم انتخابی
//
//  • این کد روی InventoryPanel می‌رود؛ مراجع را بده.
//  • دکمه‌ی انبار HUD → onClick → InventoryUI.TogglePanel
// ═══════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("مرجع‌های UI")]
    [Tooltip("پنل اصلی انبار (همین شیء)")]
    [SerializeField] GameObject panel;
    [Tooltip("شیئی که Grid Layout Group دارد (جاها داخلش ساخته می‌شوند)")]
    [SerializeField] Transform content;
    [Tooltip("متن توضیح آیتم انتخاب‌شده")]
    [SerializeField] Text descriptionText;
    [Tooltip("قالب اسلوت: یک Button کوچک با Image داخلش — غیرفعال باشد")]
    [SerializeField] GameObject slotPrefab;
    [Tooltip("عکس پس‌زمینه‌ی اسلوت (اختیاری)")]
    [SerializeField] Image slotBackground;

    [Header("ظاهر اسلوت")]
    [SerializeField] Color normalSlotColor = new Color(0.15f, 0.12f, 0.1f, 0.9f);
    [SerializeField] Color selectedSlotColor = new Color(0.55f, 0.4f, 0.2f, 1f);

    ItemData selected;
    Image selectedSlotImage;
    Image previousSlotImage;

    void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    void Start()
    {
        // اتصال رویدادها در Start (نه OnEnable) تا InventoryManager حتماً ساخته شده باشد
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += Refresh;
            InventoryManager.Instance.OnItemAdded += HandleItemAdded;
        }
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
            InventoryManager.Instance.OnItemAdded -= HandleItemAdded;
        }
    }

    void HandleItemAdded(ItemData item)
    {
        Refresh();
        // اولین بار که آیتمی برمی‌داری، پنل کوتاهی باز شود تا ببینی
        if (!panel.activeSelf && item != null)
        {
            OpenPanel();
            descriptionText.text = item.displayName + "\n" + item.description;
            CancelInvoke(nameof(AutoClose));
            Invoke(nameof(AutoClose), 3.5f);
        }
    }

    void AutoClose() { if (!string.IsNullOrEmpty(selected?.id)) return; ClosePanel(); }

    // ═══ باز و بسته کردن ═════════════════════════════════════════
    public void TogglePanel()
    {
        if (panel.activeSelf) ClosePanel();
        else OpenPanel();
    }

    public void OpenPanel()
    {
        panel.SetActive(true);
        Refresh();
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        ClearSelection();
    }

    // ═══ بازسازی اسلوت‌ها ════════════════════════════════════════
    void Refresh()
    {
        if (content == null || slotPrefab == null || InventoryManager.Instance == null) return;

        // اسلوت‌های قبلی را پاک کن
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        // برای هر آیتم، یک اسلوت بساز
        foreach (var item in InventoryManager.Instance.GetAllItems())
        {
            GameObject slot = Instantiate(slotPrefab, content);
            slot.SetActive(true);

            Image iconImage = slot.GetComponentInChildren<Image>(true);
            Button btn = slot.GetComponent<Button>();

            if (iconImage != null && item.icon != null)
            {
                iconImage.sprite = item.icon;
                iconImage.preserveAspect = true;
            }

            if (btn != null)
            {
                ItemData captured = item; // مهم: کپی برای closure
                // عکس اسلوت: اول خود اسلوت، وگرنه پس‌زمینه‌ی پیش‌فرض
                Image slotImg = slot.GetComponent<Image>();
                if (slotImg == null) slotImg = slotBackground;
                btn.onClick.AddListener(() => SelectItem(captured, slotImg));
            }
        }
    }

    // ═══ انتخاب آیتم برای دیدن توضیح ═════════════════════════════
    void SelectItem(ItemData item, Image slotImage)
    {
        // رنگ قبلی را برگردان
        if (previousSlotImage != null)
            previousSlotImage.color = normalSlotColor;

        selected = item;
        selectedSlotImage = slotImage;
        previousSlotImage = slotImage;

        if (slotImage != null) slotImage.color = selectedSlotColor;
        if (descriptionText != null)
            descriptionText.text = item.displayName + "\n" + item.description;

        CancelInvoke(nameof(AutoClose));
    }

    void ClearSelection()
    {
        selected = null;
        if (previousSlotImage != null) previousSlotImage.color = normalSlotColor;
        if (descriptionText != null) descriptionText.text = "";
    }
}
