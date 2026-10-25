// ═══════════════════════════════════════════════════════════════
//  PlayerController.cs — حرکت و نگاهِ دوربین (پلیر بدون کاراکتر)
// ───────────────────────────────────────────────────────────────
//  این کد چه می‌کند؟
//  • دوربین اول‌شخص را حرکت می‌دهد: جوی‌استیک = حرکت، سوایپ = نگاه
//  • لرزش طبیعی سر موقع راه رفتن (Head Bob) — حس حضور می‌دهد
//  • لرزش دوربین موقع جامپ‌اسکر (Shake) را هم انجام می‌دهد
//  • در ادیتور با WASD + ماوس هم می‌توانی تست کنی
//
//  کجا وصل می‌شود؟
//  ساختار توپولوژی درست (خیلی مهم!):
//
//    Player            ← کپسول/شیء خالی + CharacterController + این کد + TouchLook
//    └── Camera        ← دوربین اصلی (MainCamera) — مرجعش را در Inspector بده
//
//  • جوی‌استیک و HUD جداگانه در Canvas ساخته می‌شوند (سند 03).
// ═══════════════════════════════════════════════════════════════

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[DefaultExecutionOrder(-90)] // بعد از TouchLook (-100) اجرا شود
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("مرجع دوربین (فرزند Player)")]
    [SerializeField] Transform cameraTransform;

    [Header("سرعت حرکت")]
    [Tooltip("سرعت قدم زدن — در بازی ترسناک معمولاً آرام")]
    [SerializeField] float walkSpeed = 2.0f;

    [Header("نگاه (سوایپ)")]
    [Tooltip("ضریب حساسیت سوایپ — از تنظیمات هم ضرب می‌شود")]
    [SerializeField] float lookSpeed = 0.12f;
    [SerializeField] float minPitch = -60f;  // حداکثر نگاه به پایین
    [SerializeField] float maxPitch = 70f;   // حداکثر نگاه به بالا

    [Header("لرزش سر موقع راه رفتن")]
    [SerializeField] float bobAmount = 0.03f;
    [SerializeField] float bobSpeed  = 8f;

    [Header("صداها")]
    [Tooltip("صدای قدم — هر چند قدم یکبار پخش می‌شود (اختیاری)")]
    [SerializeField] AudioClip[] footstepClips;
    [SerializeField] float footstepInterval = 0.55f;

    // ── وضعیت داخلی ──────────────────────────────────────────────
    CharacterController controller;
    float yaw;      // چرخش افقی بدنه
    float pitch;    // چرخش عمودی دوربین
    float bobTimer;
    float footstepTimer;
    float shakeIntensity;         // شدت لرزش دوربین (جامپ‌اسکر)
    Vector3 cameraBaseLocalPos;
    AudioSource footstepSource;
    bool controlsEnabled = true;

    void Awake()
    {
        Instance = this;
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && GetComponentInChildren<Camera>() != null)
            cameraTransform = GetComponentInChildren<Camera>().transform;

        if (cameraTransform != null)
            cameraBaseLocalPos = cameraTransform.localPosition;

        yaw = transform.eulerAngles.y;

        // پخش‌کننده‌ی صدای قدم
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
        footstepSource.spatialBlend = 0f; // صدا ۲بعدی چون از خود پلیر می‌آید
    }

    void Update()
    {
        // اگر GameManager هست ولی بازی در حال اجرا نیست → پلیر قفل
        bool canAct = GameManager.Instance == null || GameManager.Instance.CanPlayerAct();
        if (!canAct || !controlsEnabled)
        {
            return;
        }

        HandleLook();
        HandleMovement();
        HandleHeadBob();
        HandleShakeDecay();
    }

    // ═══ نگاه (چرخش دوربین) ══════════════════════════════════════
    void HandleLook()
    {
        Vector2 look = TouchLook.Delta * GameSettings.LookSensitivity;

        yaw  += look.x * lookSpeed;
        pitch = Mathf.Clamp(pitch - look.y * lookSpeed, minPitch, maxPitch);

        // بدنه فقط افقی می‌چرخد، دوربین عمودی
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    // ═══ حرکت ════════════════════════════════════════════════════
    void HandleMovement()
    {
        Vector2 moveInput = VirtualJoystick.InputVector;

#if UNITY_EDITOR
        // تست با کیبورد در ادیتور
        if (moveInput == Vector2.zero)
        {
            float h = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
            float v = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
            moveInput = new Vector2(h, v);
        }
#endif

        // حرکت نسبت به جهت نگاه
        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize(); // جلوگیری از حرکت اریبِ سریع‌تر

        // جاذبه — برای پایین آمدن از پله‌ها و سرداب
        float velY = controller.isGrounded ? -2f : -9.8f;
        Vector3 velocity = moveDir * walkSpeed + Vector3.up * velY;

        controller.Move(velocity * Time.deltaTime);

        // صدای قدم
        bool isWalking = moveInput.sqrMagnitude > 0.05f && controller.isGrounded;
        if (isWalking && footstepClips != null && footstepClips.Length > 0)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                footstepTimer = 0f;
                var clip = footstepClips[Random.Range(0, footstepClips.Length)];
                if (clip != null)
                {
                    footstepSource.volume = GameSettings.SfxVolume * GameSettings.MasterVolume * 0.7f;
                    footstepSource.PlayOneShot(clip);
                }
            }
        }
        else footstepTimer = footstepInterval * 0.5f; // آماده باش برای قدم بعدی
    }

    // ═══ لرزش طبیعی سر ═══════════════════════════════════════════
    void HandleHeadBob()
    {
        if (cameraTransform == null) return;
        bool walking = VirtualJoystick.InputVector.sqrMagnitude > 0.05f && controller.isGrounded;

        if (walking) bobTimer += Time.deltaTime * bobSpeed;
        else bobTimer = Mathf.Lerp(bobTimer, 0f, Time.deltaTime * 4f);

        float bobOffset = Mathf.Sin(bobTimer) * bobAmount * (walking ? 1f : 0f);
        cameraTransform.localPosition =
            cameraBaseLocalPos + Vector3.up * bobOffset;
    }

    // ═══ لرزش دوربین (برای جامپ‌اسکر و رویدادها) ═════════════════
    void HandleShakeDecay()
    {
        if (shakeIntensity <= 0.001f || cameraTransform == null) return;

        Vector3 shake = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0f) * shakeIntensity;

        cameraTransform.localPosition = cameraBaseLocalPos + shake;
        shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, Time.deltaTime * 5f);

        // وقتی لرزش تمام شد، دوربین به جای اصلی برگردد
        if (shakeIntensity <= 0.01f)
        {
            shakeIntensity = 0f;
            cameraTransform.localPosition = cameraBaseLocalPos;
        }
    }

    /// <summary>لرزش دوربین — HorrorEventManager و JumpScare صدا می‌زنند</summary>
    public void Shake(float intensity) => shakeIntensity = Mathf.Max(shakeIntensity, intensity);

    /// <summary>قفل/آزاد کردن کنترل‌ها (مثلاً وسط سکانس داستانی)</summary>
    public void SetControlsEnabled(bool enabled) => controlsEnabled = enabled;

    /// <summary>چرخش پلیر را (مثلاً بعد از بارگذاری ذخیره) تنظیم کن</summary>
    public void SetYaw(float newYaw) { yaw = newYaw; }
}
