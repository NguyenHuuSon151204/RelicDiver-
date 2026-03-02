using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class DiverController : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float acceleration = 10f;
    
    [Header("Cấu hình xoay")]
    [SerializeField] private float rotationSpeed = 8f;
    [Tooltip("Góc bù trừ để đèn pin nhìn đúng hướng bơi. Hãy thử -70 hoặc 70.")]
    [SerializeField] private float lightOffset = 0f; 

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private bool isSprinting;
    private float leverSpeedMultiplier = 1f;
    private bool hasPower = true;
    private float targetZAngle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; 
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.drag = 2f; 
    }

    private void Start()
    {
        targetZAngle = rb.rotation;
    }

    public void SetLeverSpeedMultiplier(float value)
    {
        leverSpeedMultiplier = Mathf.Lerp(0.5f, 2.5f, value);
    }

    private void Update()
    {
        if ((DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive()) || !hasPower)
        {
            moveInput = Vector2.zero;
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;
        isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (moveInput.magnitude > 0.1f)
        {
            // 1. Tính góc dựa trên hướng phím bấm
            float moveAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;

            // 2. Lật Toàn Bộ Player dựa trên phím bấm (Giả sử Sprite gốc nhìn TRÁI)
            Vector3 scale = transform.localScale;
            if (moveInput.x < -0.1f) scale.x = 1f;  // Nhấn A (Trái) -> scale 1 (Nhìn trái)
            else if (moveInput.x > 0.1f) scale.x = -1f; // Nhấn D (Phải) -> scale -1 (Nhìn phải)
            transform.localScale = scale;

            // 3. Tính toán targetZAngle để đầu/đèn luôn nhìn đúng hướng bơi
            // Nếu bơi Trái (moveAngle = 180) và sprite nhìn Trái (scale.x = 1), ta cần bù 180 để ra 0.
            // Nếu bơi Phải (moveAngle = 0) và sprite nhìn Phải (scale.x = -1), ta để 0.
            float flipBonus = (scale.x > 0) ? 180f : 0f;
            targetZAngle = moveAngle + flipBonus + lightOffset;
        }
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
        rb.drag = 3f;
    }

    public bool IsSprinting() => isSprinting && moveInput.magnitude > 0.1f;
}
