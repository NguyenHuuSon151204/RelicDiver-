using UnityEngine;

public class SineSeaFloater : MonoBehaviour
{
    private SineWaveMesh waveManager;
    public float floatOffset = 0.2f; // Chỉnh cao thấp so với mặt sóng

    void Start()
    {
        waveManager = FindObjectOfType<SineWaveMesh>();
    }

    void LateUpdate()
    {
        if (waveManager == null) return;

        // Lấy tọa độ Y chính xác tại vị trí X của Player
        float waveY = waveManager.GetHeightAtX(transform.position.x);

        // Nếu Player đang ở gần mặt nước
        if (transform.position.y > waveY - 2.0f && transform.position.y < waveY + 2.0f)
        {
            // Ép vị trí Y của Player đi theo con sóng
            transform.position = new Vector3(transform.position.x, waveY + floatOffset, transform.position.z);
        }
    }
}
