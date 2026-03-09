using UnityEngine;
using System.Collections.Generic;

public class SeaweedSkeletalSway : MonoBehaviour
{
    [Header("Danh sách Xương muốn KHÓA (Đứng yên)")]
    public List<Transform> anchorsToLock = new List<Transform>();

    [Header("Cấu hình Uốn lượn")]
    public float speed = 3f;
    public float amount = 10f;
    public float waveDelay = 0.3f;

    [Header("Tương tác Người chơi")]
    public float interactionRadius = 2.5f; // Khoảng cách nhận biết người chơi
    public float pushAmount = 30f;        // Độ nghiêng khi bị đẩy
    public float smoothTime = 5f;          // Tốc độ hồi phục về tư thế cũ

    private struct BoneInfo
    {
        public Transform transform;
        public float initialZRotation;
        public float distance;
        public float currentPushVelocity; // Dùng cho mượt mà
        public float currentPushAngle;
    }

    private List<BoneInfo> bonesToSway = new List<BoneInfo>();
    private float timeOffset;
    private Transform player;

    void Start()
    {
        timeOffset = Random.Range(0f, 100f);
        
        // Tìm người chơi trong màn chơi
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach (Transform t in allChildren)
        {
            if (t == transform) continue;
            if (anchorsToLock.Contains(t)) continue; 

            BoneInfo info = new BoneInfo();
            info.transform = t;
            info.initialZRotation = t.localRotation.eulerAngles.z;
            info.distance = Vector3.Distance(transform.position, t.position);
            info.currentPushAngle = 0f;
            bonesToSway.Add(info);
        }
    }

    void LateUpdate()
    {
        if (bonesToSway.Count == 0) return;

        float time = Time.time * speed + timeOffset;
        float playerDist = player != null ? Vector3.Distance(transform.position, player.position) : float.MaxValue;

        for (int i = 0; i < bonesToSway.Count; i++)
        {
            BoneInfo bone = bonesToSway[i];
            if (bone.transform == null) continue;

            // 1. Tính toán uốn lượn tự nhiên (Sine Wave)
            float phase = time - (bone.distance * waveDelay);
            float swayAngle = Mathf.Sin(phase) * amount;

            // 2. Tính toán lực đẩy của người chơi
            float targetPushAngle = 0f;
            if (player != null && playerDist < interactionRadius)
            {
                // Hướng từ người chơi tới cây rêu
                float direction = transform.position.x - player.position.x;
                float force = 1f - (playerDist / interactionRadius); // Càng gần lực càng mạnh
                
                // Đẩy dạt sang hướng ngược lại với người chơi
                targetPushAngle = (direction > 0 ? 1 : -1) * force * pushAmount;
            }

            // Làm mượt chuyển động đẩy (Smooth Damp)
            bone.currentPushAngle = Mathf.Lerp(bone.currentPushAngle, targetPushAngle, Time.deltaTime * smoothTime);

            // 3. Áp dụng tổng hợp các góc xoay
            float finalAngle = bone.initialZRotation + swayAngle + bone.currentPushAngle;
            bone.transform.localRotation = Quaternion.Euler(0, 0, finalAngle);
            
            // Cập nhật lại giá trị trong danh sách
            bonesToSway[i] = bone;
        }
    }
}
