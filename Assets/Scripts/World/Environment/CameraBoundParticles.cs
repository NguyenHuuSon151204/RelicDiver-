using UnityEngine;

public class CameraBoundParticles : MonoBehaviour
{
    private Camera mainCam;
    private ParticleSystem ps;
    
    [Header("Cấu hình tối ưu")]
    public float margin = 5f; // Tăng thêm lề để tránh hở khi bơi nhanh
    public bool autoResizeShape = true;
    public float predictionPower = 0.5f; // Độ "nhô" về phía trước khi bơi nhanh

    private float lastOrthoSize;
    private Vector3 lastCamPos;
    private Vector3 camVelocity;

    void Start()
    {
        mainCam = Camera.main;
        ps = GetComponent<ParticleSystem>();

        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        lastCamPos = mainCam.transform.position;
        UpdateShapeScale();
    }

    void LateUpdate()
    {
        if (mainCam == null) return;

        // 1. Tính toán vận tốc Camera
        camVelocity = (mainCam.transform.position - lastCamPos) / Time.deltaTime;
        lastCamPos = mainCam.transform.position;

        // 2. Vị trí Particle system: Có đón đầu hướng đi
        Vector3 targetPos = mainCam.transform.position + (camVelocity * predictionPower);
        targetPos.z = 0;
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
