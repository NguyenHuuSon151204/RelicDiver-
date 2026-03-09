using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineBoundaryNotify : MonoBehaviour
{
    public PremiumWarningUI warningUI; // Sử dụng Script Premium thay vì GameObject đơn thuần
    private CinemachineVirtualCamera vcam;

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

        // Báo cho script Premium thực hiện hiệu ứng
        warningUI.ShowWarning(isTouchingBoundary);
    }
}
