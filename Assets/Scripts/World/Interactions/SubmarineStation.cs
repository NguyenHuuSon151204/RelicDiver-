using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SubmarineStation : MonoBehaviour
{
    [Header("Cấu hình Tương tác")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private GameObject interactionUI;

    [Header("Chỉ số Tàu ngầm")]
    [SerializeField] private float subMoveSpeed = 20f; // Tăng lên 20
    [SerializeField] private float subAcceleration = 15f; // Tăng lên 15
    [SerializeField] private float flipSpeed = 3f; 
    [SerializeField] private float energyConsumptionMove = 5f; // Tiêu tốn khi di chuyển (tại tốc độ mặc định)
    [SerializeField] private float energyConsumptionLight = 2f; // Tiêu tốn của đèn (tại độ sáng tối đa)

    [Header("Cấu hình Hồi phục")]
    [SerializeField] private float oxygenRegenRate = 20f;
    [SerializeField] private float batteryRegenRate = 5f;
    [SerializeField] private float baseRegenMultiplier = 5f; // Sạc nhanh gấp 5 lần tại căn cứ
    [SerializeField] private float maxBattery = 100f;
    private float currentBattery;
    private bool isAtBase = false; // Trạng thái ở căn cứ
    
    [Header("Cơ chế Lưới đánh cá")]
    [SerializeField] private int tangledNetsCount = 0;
    private int maxTangledNets = 5;
    [SerializeField] private GameObject netRemovalUI;       // Kéo trực tiếp cái bảng NetRemovalPanel vào đây
    [SerializeField] private GameObject[] visualNets;    // Kéo 5 lưới gắn sẵn trên tàu vào đây
    
    public System.Action<float, float> OnBatteryChanged;
    
    [SerializeField] private ParticleSystem refillEffect; 
    [SerializeField] private AudioClip refillSound;        // Âm thanh nạp năng lượng

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
    private float baseSubLightIntensity = 1f;

    [Header("Khởi đầu")]
    public bool startInside = false;

    void Start()
    {
        subRb = GetComponent<Rigidbody2D>();
        subRb.bodyType = RigidbodyType2D.Dynamic;
        subRb.mass = 1000f; // Khối lượng siêu nặng để không bị đẩy đi
        subRb.gravityScale = 0;
        subRb.drag = 1.0f; // Giảm drag để tàu lướt tự nhiên hơn
        subRb.angularDrag = 3f;
        subRb.interpolation = RigidbodyInterpolation2D.Interpolate;

        subMoveSpeed = 15f; 
        subAcceleration = 10000f; // Lực đẩy siêu mạnh (10,000) để kéo khối lượng 1000

        if (submarineGraphics == null) submarineGraphics = transform.Find("Graphics");
        if (submarineGraphics == null) submarineGraphics = transform;

        currentBattery = maxBattery;
        tangledNetsCount = 0; // Reset lưới lúc bắt đầu
        
        // Ẩn tất cả lưới lúc khởi đầu
        if (visualNets != null) {
            foreach(var net in visualNets) if(net) net.SetActive(false);
        }

        InitializePlayer();
        if (submarineLight != null) baseSubLightIntensity = submarineLight.intensity;
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
        EnterSubmarine(true);
    }

    void Update()
    {
        if (isInside)
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

            // HIỆN THÔNG BÁO NẾU BỊ KẸT TRONG TÀU
            if (tangledNetsCount >= maxTangledNets)
            {
                if (interactionUI)
                {
                    interactionUI.SetActive(true);
                    var text = interactionUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (text) text.text = "<color=red>TÀU BỊ KẸT!</color> Nhấn [F] ra ngoài gỡ lưới";
                }
            }

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
            
            // --- Hệ thống Lưới (Khi đã ở ngoài) ---
            if (tangledNetsCount > 0 && isPlayerInRange)
            {
                if (interactionUI)
                {
                    interactionUI.SetActive(true);
                    var text = interactionUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (text) text.text = $"[F] Vào tàu | [R] Gỡ {tangledNetsCount} lưới";
                }

                if (Input.GetKeyDown(KeyCode.R)) 
                {
                    OpenNetRemovalUI();
                }
            }
            
            // --- Vào tàu (Luôn ưu tiên F) ---
            if (isPlayerInRange && Input.GetKeyDown(interactKey))
            {
                EnterSubmarine(false);
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
        // CHẶN DI CHUYỂN NẾU MẮC 5 LƯỚI
        if (tangledNetsCount >= maxTangledNets)
        {
            subRb.velocity = Vector2.zero; // Dừng hẳn tàu
            return;
        }

        if (moveInput.magnitude > 0.1f)
        {
            // Tính toán tốc độ dựa trên công suất bình điện và số lượng lưới
            float powerFactor = currentBattery > 0 ? 1f : 0.2f; // Nếu hết pin thì trôi cực chậm
            float netFactor = 1f - (tangledNetsCount / (float)maxTangledNets); // Lưới quấn làm giảm tốc
            netFactor = Mathf.Clamp(netFactor, 0, 1f);

            float leverSpeed = LeverUIController.Instance != null ? LeverUIController.Instance.GetSpeedLeverValue() : 1f;
            float speedMultiplier = Mathf.Lerp(1f, 8f, leverSpeed); // Tăng dải tốc độ tối đa lên 8 lần cho "bốc"
            
            float finalSpeed = subMoveSpeed * speedMultiplier * powerFactor * netFactor; 
            float finalAccel = subAcceleration * speedMultiplier * powerFactor * netFactor;

            // QUAN TRỌNG: Loại bỏ Time.fixedDeltaTime để lực đẩy đạt công suất tối đa
            subRb.AddForce(moveInput * finalAccel, ForceMode2D.Force);

            // Giới hạn vận tốc tối đa
            if (subRb.velocity.magnitude > finalSpeed)
            {
                subRb.velocity = subRb.velocity.normalized * finalSpeed;
            }
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
        
        submarineGraphics.localScale = Vector3.Lerp(submarineGraphics.localScale, new Vector3(targetScaleX, submarineGraphics.localScale.y, submarineGraphics.localScale.z), Time.fixedDeltaTime * flipSpeed);

        // 3. KHÓA HƯỚNG UI
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
        
        // 4. ĐỒNG BỘ ĐỘ SÁNG ĐÈN TÀU
        if (submarineLight != null && AtmosphereMaster.Instance != null)
        {
            submarineLight.intensity = baseSubLightIntensity * AtmosphereMaster.Instance.GetBrightness();
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

    private void EnterSubmarine(bool snapToCenter = false)
    {
        isInside = true;
        isPlayerInRange = false; 
        if (interactionUI) interactionUI.SetActive(false);

        if (visualsToHide) visualsToHide.SetActive(false);
        playerController.enabled = false;
        
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        playerRb.interpolation = RigidbodyInterpolation2D.None;
        playerRb.simulated = false;

        if (LeverUIController.Instance != null)
        {
            LeverUIController.Instance.SwitchLightTarget(submarineLight);
            LeverUIController.Instance.ShowSpeedSlider(true);
        }

        player.transform.SetParent(transform);
        if (snapToCenter) player.transform.localPosition = Vector3.zero;

        // Thông báo cho HUD dùng pin của tàu
        HUDController hud = FindObjectOfType<HUDController>();
        if (hud != null) hud.SubscribeToSubmarine(this);

        if (refillEffect) refillEffect.Play();
        if (refillSound && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(refillSound);
    }

    private void ExitSubmarine()
    {
        isInside = false;
        if (visualsToHide) visualsToHide.SetActive(true);

        if (refillEffect) refillEffect.Stop();

        playerController.enabled = true;
        player.GetComponent<Rigidbody2D>().simulated = true;

        if (LeverUIController.Instance != null)
        {
            LeverUIController.Instance.SwitchLightTarget(playerLight);
            LeverUIController.Instance.ShowSpeedSlider(false);
        }

        player.transform.SetParent(null);
        player.transform.position = transform.position + Vector3.up * 1.5f;

        // Trả lại quyền hiển thị pin cho thợ lặn
        HUDController hud = FindObjectOfType<HUDController>();
        if (hud != null) hud.UnsubscribeFromSubmarine();

        subRb.velocity = Vector2.zero;
        Debug.Log("<color=yellow>Đã rời tàu.</color>");
    }

    private void HandleSystems()
    {
        if (playerStatus != null)
        {
            playerStatus.RestoreOxygen(oxygenRegenRate * Time.deltaTime);

            // Lấy giá trị từ các cần gạt UI
            float speedLever = LeverUIController.Instance != null ? LeverUIController.Instance.GetSpeedLeverValue() : 0f;
            float lightLever = LeverUIController.Instance != null ? LeverUIController.Instance.GetLightLeverValue() : 0.5f;

            float totalDrain = 0f;

            // 1. Tính toán tiêu hao do Di Chuyển
            if (moveInput.magnitude > 0.1f)
            {
                // Tốc độ càng cao, càng tốn pin (tốn thêm dựa trên vị trí cần gạt)
                totalDrain += energyConsumptionMove * Mathf.Max(0.5f, speedLever);
            }

            // 2. Tính toán tiêu hao do Đèn
            totalDrain += energyConsumptionLight * lightLever;

            // 3. Tính toán Hồi phục (Regen)
            float totalRegen = 0f;
            if (moveInput.magnitude < 0.1f && isAtBase) // Chỉ hồi pin khi đứng yên VÀ ở căn cứ
            {
                totalRegen = batteryRegenRate * baseRegenMultiplier;
            }

            // Cập nhật Pin thực tế: Drain - Regen
            currentBattery += (totalRegen - totalDrain) * Time.deltaTime;
            currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);
            
            OnBatteryChanged?.Invoke(currentBattery, maxBattery);

            // Nếu hết pin hoàn toàn, tàu sẽ bị trôi tự do (giảm tốc nhanh)
            if (currentBattery <= 0) subRb.velocity *= 0.95f;
        }
    }

    // --- Các hàm hỗ trợ Lưới ---
    public void AddTangledNet()
    {
        if (tangledNetsCount >= maxTangledNets) return;

        tangledNetsCount++;
        Debug.Log($"<color=red>Tàu bị dính lưới! Tổng số: {tangledNetsCount}/{maxTangledNets}</color>");
        
        // HIỆN NGẪU NHIÊN 1 CÁI LƯỚI TRÊN THÂN TÀU
        if (visualNets != null && visualNets.Length > 0)
        {
            // Tìm các lưới đang ẩn
            System.Collections.Generic.List<GameObject> hiddenNets = new System.Collections.Generic.List<GameObject>();
            foreach (var net in visualNets)
            {
                if (net != null && !net.activeSelf) hiddenNets.Add(net);
            }

            // Bật ngẫu nhiên 1 cái
            if (hiddenNets.Count > 0)
            {
                int randomIndex = Random.Range(0, hiddenNets.Count);
                hiddenNets[randomIndex].SetActive(true);
            }
        }

        // Nếu đủ 5 cái thì khựng lại
        if (tangledNetsCount >= maxTangledNets)
        {
            Debug.Log("<color=orange>⚠️ TÀU ĐÃ BỊ KẸT CỨNG VÌ LƯỚI! CẦN GỠ NGAY!</color>");
            subRb.velocity = Vector2.zero;
        }
    }

    private void OpenNetRemovalUI()
    {
        if (netRemovalUI == null) return;

        // BẬT BẢNG CÓ SẴN (Không dùng Instantiate)
        netRemovalUI.SetActive(true);
        
        NetRemovalUI uiScript = netRemovalUI.GetComponent<NetRemovalUI>();
        if (uiScript)
        {
            uiScript.Setup(tangledNetsCount, this);
        }
    }

    public void ClearNets()
    {
        tangledNetsCount = 0;

        // Ẩn tất cả lưới hình ảnh
        if (visualNets != null)
        {
            foreach (var net in visualNets) if (net) net.SetActive(false);
        }

        Debug.Log("<color=green>Đã gỡ sạch lưới! Tàu hoạt động bình thường.</color>");
    }

    public float GetCurrentBattery() => currentBattery;
    public float GetMaxBattery() => maxBattery;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra bằng Tag hoặc bằng Component HomeBaseTrigger (để chắc cú 100%)
        if (collision.CompareTag("HomeBase") || collision.GetComponent<HomeBaseTrigger>() != null)
        {
            isAtBase = true;
            if (refillEffect) refillEffect.Play();
            Debug.Log("<color=cyan><b>🚀 Tàu đã vào trạm: Chế độ sạc nhanh KÍCH HOẠT!</b></color>");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("HomeBase") || collision.GetComponent<HomeBaseTrigger>() != null)
        {
            isAtBase = false;
            if (refillEffect) refillEffect.Stop();
            Debug.Log("<color=orange><b>⚠️ Tàu đã rời trạm: Chế độ sạc nhanh TẮT!</b></color>");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
