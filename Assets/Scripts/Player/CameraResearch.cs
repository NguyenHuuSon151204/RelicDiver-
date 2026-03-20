using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraResearch : MonoBehaviour
{
    [Header("--- Settings ---")]
    public KeyCode cameraKey = KeyCode.C;
    public float zoomSpeed = 2f;
    public float scanDuration = 1.5f;

    [Header("--- UI Elements ---")]
    public GameObject viewfinderUI;
    public Image scanProgressBar;
    public GameObject flashUI;
    public AudioClip shutterSound;

    [Header("--- References ---")]
    private bool isAiming = false;
    private float scanTimer = 0f;
    private PhotoTarget currentTarget;

    void Update()
    {
        // 1. Nhấn giữ phím để giơ máy ảnh
        if (Input.GetKey(cameraKey))
        {
            if (!isAiming) StartAiming();
            HandleAiming();
        }
        else
        {
            if (isAiming) StopAiming();
        }
    }

    void StartAiming()
    {
        isAiming = true;
        if (viewfinderUI) viewfinderUI.SetActive(true);
        scanTimer = 0f;
    }

    void StopAiming()
    {
        isAiming = false;
        if (viewfinderUI) viewfinderUI.SetActive(false);
        if (scanProgressBar) scanProgressBar.fillAmount = 0;
        currentTarget = null;
    }

    void HandleAiming()
    {
        // 2. Raycast kiểm tra xem có mục tiêu trong khung hình không
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, 1.5f, Vector2.zero, 0f);
        
        if (hit.collider != null)
        {
            PhotoTarget target = hit.collider.GetComponent<PhotoTarget>();
            if (target != null && !target.isPhotographed)
            {
                currentTarget = target;
                scanTimer += Time.deltaTime;
                
                if (scanProgressBar) scanProgressBar.fillAmount = scanTimer / scanDuration;

                if (scanTimer >= scanDuration)
                {
                    TakePhoto();
                }
                return;
            }
        }

        // Nếu không trúng mục tiêu nào, reset timer
        scanTimer = Mathf.MoveTowards(scanTimer, 0, Time.deltaTime);
        if (scanProgressBar) scanProgressBar.fillAmount = scanTimer / scanDuration;
    }

    void TakePhoto()
    {
        if (currentTarget != null)
        {
            currentTarget.OnPhotographed();
            
            // Hiệu ứng
            if (shutterSound && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(shutterSound);
            
            StartCoroutine(FlashEffect());
            
            Debug.Log($"<color=yellow>📸 Đã chụp ảnh thành công!</color> {currentTarget.targetName}");
            
            StopAiming();
        }
    }

    IEnumerator FlashEffect()
    {
        if (flashUI)
        {
            flashUI.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            flashUI.SetActive(false);
        }
    }
}
