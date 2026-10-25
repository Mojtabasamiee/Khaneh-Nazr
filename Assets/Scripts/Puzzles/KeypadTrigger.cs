// ═══════════════════════════════════════════════════════════════
//  KeypadTrigger.cs — در/قفلی که پنل کد را باز می‌کند 🔐
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • وقتی بازیکن به این شیء (در قفل‌دار، جعبه‌ی رمز...) تعامل می‌کند،
//    پنل قفل کد باز می‌شود و کنترل پلیر موقتاً قفل می‌شود.
//  • اگر معما قبلاً حل شده باشد، به جای پنل، رویداد موفقیت می‌رود.
//
//  کجا وصل می‌شود؟
//  • روی در یا قفلی که با کد باز می‌شود + Collider
//  • مرجع پنل قفل (KeypadPanel) و مرجع در (DoorController) را بده.
// ═══════════════════════════════════════════════════════════════

using UnityEngine;

public class KeypadTrigger : Interactable
{
    [Header("مرجع‌ها")]
    [Tooltip("پنل قفل کد (شیئی که KeypadPuzzle دارد)")]
    [SerializeField] GameObject keypadPanel;

    [Tooltip("در یا جعبه‌ای که بعد از حل باز می‌شود (اختیاری)")]
    [SerializeField] DoorController linkedDoor;

    [Tooltip("پرچم حل این قفل — باید با solvedFlag در KeypadPuzzle یکی باشد!")]
    [SerializeField] string solveFlag = "grandma_room_open";

    [Header("گویندگی")]
    [Tooltip("خط گویندگی وقتی پنل باز می‌شود (اولین بار)")]
    [SerializeField] string firstTimeVoiceId = "";
    bool voicePlayed = false;

    public override string InteractionPrompt =>
        (PuzzleManager.Instance != null && PuzzleManager.Instance.IsFlagSet(solveFlag))
            ? "باز کردن"
            : "بررسی قفل کد";

    protected override void OnInteracted()
    {
        bool alreadySolved = PuzzleManager.Instance != null && PuzzleManager.Instance.IsFlagSet(solveFlag);

        if (alreadySolved)
        {
            // قفل قبلاً باز شده — در را باز کن
            if (linkedDoor != null) linkedDoor.ForceUnlock();
            return;
        }

        // پنل قفل کد باز شود
        if (keypadPanel != null)
        {
            keypadPanel.SetActive(true);
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Puzzle); // پلیر قفل شود
        }

        // گویندگی اولین بار
        if (!voicePlayed && !string.IsNullOrEmpty(firstTimeVoiceId) && NarrationManager.Instance != null)
        {
            NarrationManager.Instance.PlayById(firstTimeVoiceId);
            voicePlayed = true;
        }
    }
}
