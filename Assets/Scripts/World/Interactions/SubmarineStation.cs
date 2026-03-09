using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SubmarineStation : MonoBehaviour
{
    [Header("Cấu hình Tương tác")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private GameObject interactionUI;

    [Header("Chỉ số Tàu ngầm")]
    [SerializeField] private float subMoveSpeed = 10f;
    [SerializeField] private float subAcceleration = 5f;
    [SerializeField] private float flipSpeed = 3f; 
    [SerializeField] private float energyConsumptionMove = 2f; 

    [Header("Cấu hình Hồi phục")]
    [SerializeField] private float oxygenRegenRate = 20f;
    [SerializeField] private float batteryRegenRate = 5f;

    [Header("Ánh sáng")]
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D submarineLight;
    private UnityEngine.Rendering.Universal.Light2D playerLight;

    [Header("Hình ảnh Thợ lặn")]
    [SerializeField] private GameObject visualsToHide; 

    [Header("Hình ảnh Tàu")]
    [SerializeField] private Transform submarineGraphics; 

    private GameObject player;
    private DiverController playerController;
    private PlayerStatusManager playerStatus;
    
    private Rigidbody2D subRb;
    private Vector2 moveInput;
    private bool isPlayerInRange = false;
    private bool isInside = false;
    private float targetScaleX = 1f;

    [Header("Khởi đầu")]
    public bool startInside = false;

    void Start()
    {
        subRb = GetComponent<Rigidbody2D>();
        subRb.gravityScale = 0;
        subRb.drag = 2f;
        subRb.angularDrag = 3f;
        subRb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (submarineGraphics == null) submarineGraphics = transform.Find("Graphics");
        if (submarineGraphics == null) submarineGraphics = transform;

        InitializePlayer();
        if (player != null && startInside) ForceEnter();

        if (interactionUI) interactionUI.SetActive(false);
        targetScaleX = submarineGraphics.localScale.x;
    }

    private void InitializePlayer()
    {
        if (player != null) return; // Đã khởi tạo rồi

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<DiverController>();
            playerStatus = player.GetComponent<PlayerStatusManager>();
            playerLight = player.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
        }
    }

    public void ForceEnter()
    {
        InitializePlayer();
        EnterSubmarine();
    }

    void Update()
    {
        if (player == null) return;

        if (isInside)
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

            if (Input.GetKeyDown(interactKey))
            {
                ExitSubmarine();
            }
            else
            {
                HandleSystems();
            }
        }
        else
        {
            CheckForEntry();
            
            if (isPlayerInRange && Input.GetKeyDown(interactKey))
            {
                EnterSubmarine();
            }
        }
    }

    void FixedUpdate()
    {
        if (isInside) HandleMovement();
        HandleVisuals();
    }

    private void HandleMovement()
    {
        if (moveInput.magnitude > 0.1f)
        {
            float leverSpeed = LeverUIController.Instance != null ? LeverUIController.Instance.GetSpeedLeverValue() : 1f;
            float actualSpeed = subMoveSpeed * Mathf.Lerp(0.5f, 2f, leverSpeed);
            
            // NHÂN THÊM subRb.mass để tàu luôn di chuyển mượt dù nặng bao nhiêu
            subRb.AddForce(moveInput * actualSpeed * subAcceleration * subRb.mass);
        }
    }

    private void HandleVisuals()
    {
        Vector2 activeInput = isInside ? moveInput : Vector2.zero;

        // 1. NGHIÊNG TÀU (TILT)
        float targetAngle = activeInput.x * -12f;
        float currentAngle = Mathf.LerpAngle(subRb.rotation, targetAngle, Time.fixedDeltaTime * 2f);
        subRb.MoveRotation(currentAngle);

        // 2. QUAY ĐẦU (SMOOTH FLIP)
        if (isInside && activeInput.x != 0)
        {
            targetScaleX = (activeInput.x > 0) ? Mathf.Abs(transform.localScale.y) : -Mathf.Abs(transform.localScale.y);
        }

        float speedFactor = (subRb.velocity.magnitude / subMoveSpeed) + 0.5f;
        float currentScaleX = Mathf.Lerp(submarineGraphics.localScale.x, targetScaleX, Time.fixedDeltaTime * flipSpeed * speedFactor);
        submarineGraphics.localScale = new Vector3(currentScaleX, submarineGraphics.localScale.y, submarineGraphics.localScale.z);

        // KHÓA HƯỚNG UI
        if (interactionUI != null && interactionUI.activeInHierarchy)
        {
            Vector3 uiScale = interactionUI.transform.localScale;
            interactionUI.transform.localScale = new Vector3(Mathf.Abs(uiScale.x), uiScale.y, uiScale.z);
            
            if (interactionUI.transform.IsChildOf(submarineGraphics))
            {
                float parentScaleX = submarineGraphics.localScale.x;
                interactionUI.transform.localScale = new Vector3(Mathf.Abs(uiScale.x) * Mathf.Sign(parentScaleX), uiScale.y, uiScale.z);
            }
        }
    }

    private void CheckForEntry()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance <= interactionRange)
        {
            if (!isPlayerInRange) {
                isPlayerInRange = true;
                if (interactionUI) interactionUI.SetActive(true);
            }
        }
        else if (isPlayerInRange) {
            isPlayerInRange = false;
            if (interactionUI) interactionUI.SetActive(false);
        }
    }

    private void EnterSubmarine()
    {
        isInside = true;
        isPlayerInRange = false; 
        if (interactionUI) interactionUI.SetActive(false);

        if (visualsToHide) visualsToHide.SetActive(false);
        playerController.enabled = false;
        player.GetComponent<Rigidbody2D>().simulated = false;

        if (LeverUIController.Instance != null)
        {
            LeverUIController.Instance.SwitchLightTarget(submarineLight);
            LeverUIController.Instance.ShowSpeedSlider(true);
        }

        player.transform.SetParent(transform);
        player.transform.localPosition = Vector3.zero;
    }

    private void ExitSubmarine()
    {
        isInside = false;
        if (visualsToHide) visualsToHide.SetActive(true);

        playerController.enabled = true;
        player.GetComponent<Rigidbody2D>().simulated = true;

        if (LeverUIController.Instance != null)
        {
            LeverUIController.Instance.SwitchLightTarget(playerLight);
            LeverUIController.Instance.ShowSpeedSlider(false);
        }

        player.transform.SetParent(null);
        player.transform.position = transform.position + Vector3.up * 1.5f;
        subRb.velocity = Vector2.zero;
        Debug.Log("<color=yellow>Đã rời tàu.</color>");
    }

    private void HandleSystems()
    {
        if (playerStatus != null)
        {
            playerStatus.RestoreOxygen(oxygenRegenRate * Time.deltaTime);

            if (moveInput.magnitude > 0.1f)
                playerStatus.RechargeBattery(-energyConsumptionMove * Time.deltaTime);
            else
                playerStatus.RechargeBattery(batteryRegenRate * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
