using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineBoundaryNotify : MonoBehaviour
{
    public PremiumWarningUI warningUI; // Sử dụng Script Premium thay vì GameObject đơn thuần
    private CinemachineVirtualCamera vcam;

    public static bool canShowWarning = false; // Mặc định là tắt để tránh hiện lúc Cutscene

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
    }

    void LateUpdate()
    {
        if (vcam == null || warningUI == null) return;

        Vector3 rawPos = vcam.State.RawPosition;
        Vector3 finalPos = vcam.State.FinalPosition;

        // Nếu camera bị chặn ở biên
        bool isTouchingBoundary = Vector3.Distance(rawPos, finalPos) > 0.1f;

        // CHỈ HIỆN CẢNH BÁO NẾU ĐÃ VÀO GAME (canShowWarning = true)
        if (canShowWarning)
            warningUI.ShowWarning(isTouchingBoundary);
        else
            warningUI.ShowWarning(false);
    }
}
