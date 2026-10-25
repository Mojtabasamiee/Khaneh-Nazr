// ═══════════════════════════════════════════════════════════════
//  SaveSystem.cs — سیستم ذخیره و ادامه‌ی بازی
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • وضعیت بازی (پرچم‌های داستان، آیتم‌های انبار، مکان پلیر، فصل داستان)
//    را به صورت JSON داخل حافظه‌ی گوشی ذخیره می‌کند.
//  • موقع «ادامه‌ی بازی» همان وضعیت را برمی‌گرداند.
//
//  کجا وصل می‌شود؟
//  • یک GameObject خالی به اسم «SaveManager» در صحنه‌ی House بساز
//    و این کد را رویش بگذار.
//  • خودش به PuzzleManager و InventoryManager وصل می‌شود و موقع
//    هر تغییر مهم، ذخیره‌ی خودکار انجام می‌دهد.
// ═══════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    // ── داده‌هایی که ذخیره می‌شوند ────────────────────────────────
    [Serializable]
    public class SaveData
    {
        public List<string> flags = new List<string>();       // پرچم‌های داستان/معما که روشن شده‌اند
        public List<string> items = new List<string>();       // شناسه‌ی آیتم‌های انبار
        public float posX, posY, posZ;                        // مکان پلیر
        public float rotY;                                    // جهت نگاه پلیر
        public int storyStep = 0;                             // فصل فعلی داستان
        public List<string> endingsSeen = new List<string>(); // کدام پایان‌ها را دیده‌ای
    }

    const string FILE_NAME = "khaneh_save.json";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // اتصال رویدادها اینجا انجام می‌شود (نه OnEnable) تا مطمئن شویم
        // PuzzleManager و InventoryManager قبلاً ساخته شده‌اند
        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.OnFlagSet += OnFlagChangedHandler;
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += OnInventoryChangedHandler;
    }

    void OnDestroy()
    {
        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.OnFlagSet -= OnFlagChangedHandler;
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= OnInventoryChangedHandler;
    }

    void OnFlagChangedHandler(string _) => SaveGame();          // ذخیره‌ی خودکار
    void OnInventoryChangedHandler() => SaveGame();             // ذخیره‌ی خودکار

    // ═══ مسیر فایل ذخیره ═════════════════════════════════════════
    static string SavePath => System.IO.Path.Combine(Application.persistentDataPath, FILE_NAME);

    // ═══ ذخیره کردن ══════════════════════════════════════════════
    public static void SaveGame()
    {
        if (PuzzleManager.Instance == null) return; // هنوز در منوی اصلی هستیم

        try
        {
            SaveData data = new SaveData();

            data.flags = PuzzleManager.Instance.GetAllFlags();
            data.items = InventoryManager.Instance.GetAllItemIds();
            data.storyStep = StoryManager.Instance != null ? StoryManager.Instance.CurrentStep : 0;

            if (PlayerController.Instance != null)
            {
                Vector3 p = PlayerController.Instance.transform.position;
                data.posX = p.x; data.posY = p.y; data.posZ = p.z;
                data.rotY = PlayerController.Instance.transform.eulerAngles.y;
            }

            if (EndingManager.Instance != null)
                data.endingsSeen = EndingManager.Instance.GetEndingsSeen();

            System.IO.File.WriteAllText(SavePath, JsonUtility.ToJson(data, prettyPrint: true));
        }
        catch (Exception e)
        {
            Debug.LogWarning("ذخیره‌ی بازی ناموفق بود: " + e.Message);
        }
    }

    // ═══ آیا ذخیره وجود دارد؟ (برای دکمه‌ی «ادامه») ═════════════
    public static bool HasSave()
    {
        return System.IO.File.Exists(SavePath);
    }

    // ═══ اعمال ذخیره‌ی موجود روی صحنه ═══════════════════════════
    public static void ApplyLoadedSave()
    {
        if (!HasSave()) return;

        try
        {
            string json = System.IO.File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (PuzzleManager.Instance != null)
                PuzzleManager.Instance.LoadFlags(data.flags);

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.LoadItemIds(data.items);

            if (StoryManager.Instance != null)
                StoryManager.Instance.SetStepWithoutPlaying(data.storyStep);

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.transform.position =
                    new Vector3(data.posX, data.posY, data.posZ);
                PlayerController.Instance.transform.rotation =
                    Quaternion.Euler(0f, data.rotY, 0f);
            }

            if (EndingManager.Instance != null)
                EndingManager.Instance.LoadEndingsSeen(data.endingsSeen);

            Debug.Log("ذخیره‌ی بازی با موفقیت بارگذاری شد.");
        }
        catch (Exception e)
        {
            Debug.LogWarning("بارگذاری ذخیره ناموفق بود: " + e.Message);
        }
    }

    // ═══ حذف ذخیره (شروع بازی جدید) ══════════════════════════════
    public static void DeleteSave()
    {
        if (System.IO.File.Exists(SavePath))
            System.IO.File.Delete(SavePath);
    }
}
