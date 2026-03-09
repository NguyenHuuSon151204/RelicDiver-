using UnityEngine;
using System.Collections.Generic;

public class FishAnimPremium : MonoBehaviour
{
    [Header("Cấu hình xương")]
    public Transform bodyBone;   // Xương thân
    public Transform tailBone;   // Xương đuôi (ngọn)

    [Header("Thông số quẫy")]
    public float swingSpeed = 10f;  // Tốc độ quẫy
    public float swingAmount = 15f; // Độ rộng của cú quẫy đuôi
    public float waveDelay = 0.2f;  // Độ trễ giữa thân và đuôi

    [Header("Hiệu ứng Co Giãn (3D Feel)")]
    public bool useSquash = true;
    public float squashAmount = 0.1f; // Độ bóp lại của thân khi quẫy cực đại

    private float initialBodyZ;
    private float initialTailZ;
    private Vector3 initialScale;
    private float timeOffset;

    void Start()
    {
        timeOffset = Random.Range(0f, 100f);
        initialScale = transform.localScale;
        
        if (bodyBone) initialBodyZ = bodyBone.localRotation.eulerAngles.z;
        if (tailBone) initialTailZ = tailBone.localRotation.eulerAngles.z;
    }

    void LateUpdate()
    {
        float t = Time.time * swingSpeed + timeOffset;

        // 1. Quẫy thân (Nhẹ nhàng)
        if (bodyBone) {
            float angle1 = Mathf.Sin(t) * (swingAmount * 0.3f);
            bodyBone.localRotation = Quaternion.Euler(0, 0, initialBodyZ + angle1);
        }

        // 2. Quẫy đuôi (Mạnh mẽ & Trễ nhịp)
        if (tailBone) {
            float angle2 = Mathf.Sin(t - waveDelay) * swingAmount;
            tailBone.localRotation = Quaternion.Euler(0, 0, initialTailZ + angle2);
        }

        // 3. HIỆU ỨNG TẠO ĐỘ SÂU (BÍ KÍP)
        // Khi đuôi quẫy sang hai bên, con cá trông sẽ hơi ngắn lại một chút
        if (useSquash) {
            float squash = Mathf.Abs(Mathf.Sin(t)) * squashAmount;
            transform.localScale = new Vector3(initialScale.x - squash, initialScale.y, initialScale.z);
        }
    }
}
