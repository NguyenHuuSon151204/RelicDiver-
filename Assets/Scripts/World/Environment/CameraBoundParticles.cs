using UnityEngine;

public class CameraBoundParticles : MonoBehaviour
{
    private Camera mainCam;
    private ParticleSystem ps;
    
    [Header("Cấu hình tối ưu")]
    public float margin = 15f; // Tăng lề rộng hơn nữa để bao phủ toàn bộ Camera
    public bool autoResizeShape = true;
    public float predictionPower = 0.5f; // Độ "nhô" về phía trước khi bơi nhanh

    private float lastOrthoSize;
    private Vector3 lastCamPos;
    private Vector3 camVelocity;

    void Start()
    {
        mainCam = Camera.main;
        ps = GetComponent<ParticleSystem>();

        // Tự động cấu hình Renderer để luôn hiện trên cùng
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 10;
        }

        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        // Chế độ Culling Mode để hạt không bao giờ bị ẩn đột ngột
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
        
        lastCamPos = mainCam.transform.position;
        UpdateShapeScale();
    }

    void LateUpdate()
    {
        if (mainCam == null) return;

        // 1. Tính toán vận tốc Camera (Tránh chia cho 0 khi game pause)
        if (Time.deltaTime > 0)
        {
            camVelocity = (mainCam.transform.position - lastCamPos) / Time.deltaTime;
        }
        else
        {
            camVelocity = Vector3.zero;
        }
        
        lastCamPos = mainCam.transform.position;

        // 2. Vị trí Particle system: Có đón đầu hướng đi
        Vector3 targetPos = mainCam.transform.position + (camVelocity * predictionPower);
        targetPos.z = -0.5f; // Đưa trục Z ra phía trước (Negative Z trong Unity 2D) để tránh Z-fighting với Background
        transform.position = targetPos;

        // 3. Chỉ cập nhật Shape khi cần
        if (autoResizeShape && !Mathf.Approximately(mainCam.orthographicSize, lastOrthoSize))
        {
            UpdateShapeScale();
        }
    }

    void UpdateShapeScale()
    {
        lastOrthoSize = mainCam.orthographicSize;
        float aspect = mainCam.aspect;
        
        float height = lastOrthoSize * 2f + margin;
        float width = height * aspect + margin;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(width, height, 1f);
        transform.rotation = Quaternion.identity;
    }
}
