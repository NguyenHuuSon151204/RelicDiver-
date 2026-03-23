using UnityEngine;

public class SubmarineHook : MonoBehaviour
{
    [Header("--- Trạng thái ---")]
    public bool hasHook = true;         // Ô Tick chọn tàu có móc hay không

    [Header("--- Cấu hình Móc ---")]
    public float launchSpeed = 8f;      // Tốc độ lao xuống
    public float retractSpeed = 6f;     // Tốc độ kéo lên
    public float maxLength = 10f;       // Độ dài tối đa
    public KeyCode hookKey = KeyCode.Space; // Phím bắn móc

    [Header("--- Hoạt họa Càng móc ---")]
    public Transform leftClaw;          // Càng bên trái
    public Transform rightClaw;         // Càng bên phải
    public float openAngle = 0f;        // Góc khi mở (mặc định)
    public float closedAngle = 35f;     // Góc khi khép lại để quắp (ví dụ 35 độ)

    [Header("--- Tham chiếu ---")]
    public Transform hookHead;          // Cái đầu móc (GameObject)
    public LineRenderer rope;           // Sợi dây nối (Sử dụng LineRenderer)
    public Transform launchPoint;       // Điểm xuất phát (Bụng tàu)

    private Vector3 startPos;
    private bool isLaunching = false;
    private bool isRetracting = false;
    private Transform caughtItem = null;

    enum HookState { IDLE, LAUNCHING, RETRACTING }
    HookState currentState = HookState.IDLE;

    void Start()
    {
        // Luôn bật để người chơi nhìn thấy
        if (hookHead) hookHead.gameObject.SetActive(true);
        if (rope) {
            rope.enabled = true;
            rope.positionCount = 2;
        }
    }

    void Update()
    {
        // Kiểm tra quyền hạn của tàu
        if (!hasHook) {
            if (hookHead) hookHead.gameObject.SetActive(false);
            if (rope) rope.enabled = false;
            return;
        }

        // 1. Nhấn phím để bắn
        if (Input.GetKeyDown(hookKey) && currentState == HookState.IDLE)
        {
            LaunchHook();
        }

        // 2. Xử lý trạng thái
        switch (currentState)
        {
            case HookState.IDLE:
                // Ở trạng thái nghỉ, móc sẽ treo lơ lửng tại điểm bắn
                hookHead.position = launchPoint.position;
                UpdateRope();
                break;

            case HookState.LAUNCHING:
                MoveHook(Vector3.down * launchSpeed * Time.deltaTime);
                UpdateRope();
                
                // Kiểm tra nếu quá độ dài tối đa
                if (Vector3.Distance(launchPoint.position, hookHead.position) >= maxLength)
                {
                    StartRetracting();
                }
                break;

            case HookState.RETRACTING:
                // Quay về điểm xuất phát
                Vector3 direction = (launchPoint.position - hookHead.position).normalized;
                hookHead.position += direction * retractSpeed * Time.deltaTime;
                UpdateRope();

                // Kiểm tra nếu đã về đích
                if (Vector3.Distance(launchPoint.position, hookHead.position) <= 0.1f)
                {
                    ResetHook();
                }
                break;
        }
    }

    void LaunchHook()
    {
        currentState = HookState.LAUNCHING;
        Debug.Log("<color=cyan><b>⚓ BẮN MÓC TÀU NGẦM!</b></color>");
    }

    void MoveHook(Vector3 amount)
    {
        hookHead.position += amount;
    }

    void StartRetracting()
    {
        currentState = HookState.RETRACTING;
    }

    void ResetHook()
    {
        currentState = HookState.IDLE;
        
        // Giữ nguyên hiển thị, không ẩn đi nữa
        SetClawAnimation(false);

        if (caughtItem != null)
        {
            Debug.Log("<color=green><b>📦 ĐÃ THU HOẠCH VẬT PHẨM!</b></color>");
            Destroy(caughtItem.gameObject); 
            caughtItem = null;
        }
    }

    void UpdateRope()
    {
        if (rope)
        {
            rope.SetPosition(0, launchPoint.position);
            rope.SetPosition(1, hookHead.position);
        }
    }

    // --- KIỂM TRA CHẠM VẬT PHẨM ---
    // Gắn đoạn này vào script của HookHead hoặc check Collision ở đây
    public void CatchItem(Transform item)
    {
        if (currentState == HookState.LAUNCHING)
        {
            caughtItem = item;
            caughtItem.SetParent(hookHead); // Quắp vật phẩm đi theo đầu móc
            caughtItem.localPosition = Vector3.down * 0.5f; // Chỉnh vị trí vật phẩm dưới móc
            
            // Khép càng móc lại
            SetClawAnimation(true);

            StartRetracting();
            Debug.Log("<color=yellow><b>✨ TRÚNG THƯỞNG! ĐANG KÉO LÊN...</b></color>");
        }
    }

    void SetClawAnimation(bool isClosed)
    {
        if (leftClaw == null || rightClaw == null) return;

        float angle = isClosed ? closedAngle : openAngle;
        
        // Càng trái xoay theo chiều kim đồng hồ, càng phải xoay ngược lại
        leftClaw.localEulerAngles = new Vector3(0, 0, -angle);
        rightClaw.localEulerAngles = new Vector3(0, 0, angle);
    }
}
