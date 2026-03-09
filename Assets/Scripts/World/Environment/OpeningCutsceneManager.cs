using UnityEngine;
using System.Collections;

public class OpeningCutsceneManager : MonoBehaviour
{
    [Header("--- Tham chiếu đối tượng ---")]
    public Transform carrierBay;      // Toàn bộ tổ hợp khoang tàu mẹ
    public Transform leftDoor;        // Cánh cửa bên trái
    public Transform rightDoor;       // Cánh cửa bên phải
    public GameObject playerSubmarine; // Tàu ngầm SubmarineStation của người chơi
    public GameObject playerDiver;     // Thợ lặn (để ẩn/hiện nếu cần)

    [Header("--- Cấu hình Di chuyển ---")]
    public Vector3 startPosition = new Vector3(-30, 0, 0);
    public Vector3 targetPosition = new Vector3(0, 0, 0);
    public float moveSpeed = 5f;

    [Header("--- Cấu hình Cửa ---")]
    public float doorOpenDistance = 4f; // Khoảng cách cửa trượt ra
    public float doorOpenSpeed = 2f;

    [Header("--- Cấu hình Phóng tàu ---")]
    public float subLaunchForce = 5f;
    public float waitBeforeOpen = 1f;
    public float waitAfterOpen = 1f;

    [Header("--- Chế độ ---")]
    public bool playCutscene = true; // Nút bật/tắt cutscene trong Inspector

    [Header("--- Camera ---")]
    public Camera mainCamera;         
    public bool lockCameraAtStart = true;

    private bool isCutsceneActive = true;
    private Vector3 initialCameraPos;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        
        // Luôn ép người chơi vào tàu ngầm lúc đầu
        if (playerSubmarine != null) 
        {
            SubmarineStation sub = playerSubmarine.GetComponent<SubmarineStation>();
            if (sub != null) sub.ForceEnter();
        }

        if (playCutscene)
        {
            if (carrierBay != null) carrierBay.position = startPosition;
            SetupPlayerState(false);

            if (lockCameraAtStart && mainCamera != null)
            {
                initialCameraPos = new Vector3(targetPosition.x, targetPosition.y, mainCamera.transform.position.z);
                SetCameraFollowActive(false);
                mainCamera.transform.position = initialCameraPos;
            }

            StartCoroutine(StartOpeningSequence());
        }
        else
        {
            // BỎ QUA: Đặt mọi thứ ở trạng thái xong xuôi
            if (carrierBay != null) carrierBay.position = targetPosition;
            OpenDoorsImmediately();
            ReleaseSubmarine();
            EndCutscene();
        }
    }

    private void OpenDoorsImmediately()
    {
        if (leftDoor && rightDoor)
        {
            leftDoor.localPosition += Vector3.left * doorOpenDistance;
            rightDoor.localPosition += Vector3.right * doorOpenDistance;
        }
    }

    private void ReleaseSubmarine()
    {
        if (playerSubmarine != null)
        {
            playerSubmarine.transform.SetParent(null);
            Rigidbody2D rb = playerSubmarine.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = true;
        }
    }

    private IEnumerator StartOpeningSequence()
    {
        Debug.Log("🎬 Bắt đầu Cutscene...");
        
        while (Vector3.Distance(carrierBay.position, targetPosition) > 0.1f)
        {
            carrierBay.position = Vector3.MoveTowards(carrierBay.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        carrierBay.position = targetPosition;

        yield return new WaitForSeconds(waitBeforeOpen);

        // 2. Mở cửa khoang
        Vector3 leftTarget = leftDoor.localPosition + Vector3.left * doorOpenDistance;
        Vector3 rightTarget = rightDoor.localPosition + Vector3.right * doorOpenDistance;

        float t = 0;
        Vector3 leftStart = leftDoor.localPosition;
        Vector3 rightStart = rightDoor.localPosition;

        while (t < 1f)
        {
            t += Time.deltaTime * doorOpenSpeed;
            leftDoor.localPosition = Vector3.Lerp(leftStart, leftTarget, t);
            rightDoor.localPosition = Vector3.Lerp(rightStart, rightTarget, t);
            yield return null;
        }

        yield return new WaitForSeconds(waitAfterOpen);

        // 3. Phóng tàu
        ReleaseSubmarine();
        Rigidbody2D subRb = playerSubmarine.GetComponent<Rigidbody2D>();
        if (subRb != null) subRb.AddForce(Vector2.right * subLaunchForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(1.5f);
        EndCutscene();
    }

    private void EndCutscene()
    {
        isCutsceneActive = false;
        SetupPlayerState(true);
        SetCameraFollowActive(true);
        Debug.Log("🎮 Sẵn sàng chơi!");
    }

    private void SetCameraFollowActive(bool active)
    {
        if (mainCamera == null) return;
        MonoBehaviour[] scripts = mainCamera.GetComponents<MonoBehaviour>();
        foreach (var s in scripts)
        {
            if (s.GetType().Name.Contains("Follow") || s.GetType().Name.Contains("Cinemachine"))
                s.enabled = active;
        }
    }

    private void SetupPlayerState(bool canControl)
    {
        SubmarineStation subStation = playerSubmarine.GetComponent<SubmarineStation>();
        if (subStation != null) subStation.enabled = canControl;
        
        Rigidbody2D rb = playerSubmarine.GetComponent<Rigidbody2D>();
        if (rb != null && !canControl) rb.simulated = false;
    }
}
