using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class DiverController : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float acceleration = 10f;
    
    [Header("Hình ảnh & Xoay")]
    [SerializeField] private Transform visualContainer; 
    [SerializeField] private float flipSpeed = 10f; 
    [SerializeField] private float rotationSpeed = 8f; // Thêm lại biến bị thiếu
    [Tooltip("Góc bù trừ để đèn pin nhìn đúng hướng bơi. Hãy thử -70 hoặc 70.")]
    [SerializeField] private float lightOffset = 0f; 

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private bool isSprinting;
    private float leverSpeedMultiplier = 1f;
    private float targetZAngle;
    private float targetScaleX = 1f;
    private PlayerStatusManager playerStatus;
    private bool hasPower = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerStatus = GetComponent<PlayerStatusManager>();

        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; 
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearDamping = 2f; 
        
        if (visualContainer == null) visualContainer = transform.Find("Graphics");
        if (visualContainer == null) visualContainer = transform; // Fail-safe
    }

    private void Start()
    {
        targetZAngle = rb.rotation;
        targetScaleX = transform.localScale.x;
    }

    public void SetLeverSpeedMultiplier(float value)
    {
        leverSpeedMultiplier = Mathf.Lerp(0.5f, 2.5f, value);
        if (playerStatus != null) playerStatus.SetSpeedBatteryDrain(value);
    }

    private void Update()
    {
        hasPower = playerStatus == null || playerStatus.currentBattery > 0;
        if ((DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive()) || !hasPower)
        {
            moveInput = Vector2.zero;
            if (!hasPower) HandlePowerLoss();
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;
        
        // Chạy nhanh bằng Shift
        isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (moveInput.magnitude > 0.1f)
        {
            // 1. Tính góc dựa trên hướng phím bấm
            float moveAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;

            // 2. TÍNH TOÁN FLIP MƯỢT (Smooth Flip)
            // Xác định hướng X mục tiêu dựa trên phím bấm (Giả sử Sprite gốc nhìn TRÁI)
            if (moveInput.x < -0.1f) targetScaleX = Mathf.Abs(transform.localScale.y); // Nhìn TRÁI
            else if (moveInput.x > 0.1f) targetScaleX = -Mathf.Abs(transform.localScale.y); // Nhìn PHẢI

            // 3. Tính toán targetZAngle để đầu/đèn luôn nhìn đúng hướng bơi
            float currentFlipDir = (visualContainer.localScale.x > 0) ? 1f : -1f;
            float flipBonus = (currentFlipDir > 0) ? 180f : 0f;
            targetZAngle = moveAngle + flipBonus + lightOffset;
        }

        // Thực hiện Lật (Flip) mượt mà bằng Lerp lên nốt Hình ảnh (Visuals)
        float newX = Mathf.Lerp(visualContainer.localScale.x, targetScaleX, Time.deltaTime * flipSpeed);
        visualContainer.localScale = new Vector3(newX, visualContainer.localScale.y, visualContainer.localScale.z);
    }

    private void FixedUpdate()
    {
        if (moveInput.magnitude > 0.1f && hasPower)
        {
            float currentSpeed = moveSpeed * leverSpeedMultiplier * (isSprinting ? sprintMultiplier : 1f);
            rb.AddForce(moveInput * currentSpeed * acceleration);
        }

        float currentAngle = rb.rotation;
        float smoothedAngle = Mathf.LerpAngle(currentAngle, targetZAngle, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothedAngle);
    }

    private void HandlePowerLoss()
    {
        hasPower = false;
        rb.linearDamping = 3f;
    }

    public bool IsSprinting() => isSprinting && moveInput.magnitude > 0.1f;
}
