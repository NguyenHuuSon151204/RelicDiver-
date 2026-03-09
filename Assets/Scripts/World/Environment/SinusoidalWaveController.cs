using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
public class SinusoidalWaveController : MonoBehaviour
{
    [Header("Cấu hình sóng Sin")]
    public float amplitude = 0.5f;   // Độ cao của sóng
    public float frequency = 1.0f;   // Độ dày của sóng (khoảng cách các đỉnh)
    public float speed = 2.0f;       // Tốc độ sóng chạy
    
    [Header("Độ chi tiết")]
    [Range(2, 50)]
    public int waveSegments = 20;    // Số lượng điểm nút để tạo sóng mượt

    private SpriteShapeController shapeController;
    private Spline spline;

    void Awake()
    {
        shapeController = GetComponent<SpriteShapeController>();
        spline = shapeController.spline;
        SetupSpline();
    }

    // Tạo các điểm nút ban đầu theo đường thẳng
    void SetupSpline()
    {
        spline.Clear();
        float width = 50f; // Độ rộng mặt biển (bạn có thể chỉnh lại)
        float step = width / (waveSegments - 1);

        for (int i = 0; i < waveSegments; i++)
        {
            float xPos = -width / 2 + (i * step);
            spline.InsertPointAt(i, new Vector3(xPos, 0, 0));
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }
    }

    void Update()
    {
        // Cập nhật vị trí Y của từng điểm nút theo hàm Sin
        for (int i = 0; i < waveSegments; i++)
        {
            Vector3 pos = spline.GetPosition(i);
            
            // Công thức sóng Sin: Y = Sin(Thời gian * Tốc độ + Vị trí X * Tần số) * Biên độ
            float newY = Mathf.Sin(Time.time * speed + pos.x * frequency) * amplitude;
            
            spline.SetPosition(i, new Vector3(pos.x, newY, 0));
            
            // Làm mượt tiếp tuyến (Tangents) để sóng không bị gãy khúc
            float tangentMag = 1f / frequency;
            spline.SetLeftTangent(i, new Vector3(-tangentMag, 0, 0));
            spline.SetRightTangent(i, new Vector3(tangentMag, 0, 0));
        }
    }

    // Hàm bổ trợ để thợ lặn/tàu lấy độ cao của sóng tại vị trí X của họ
    public float GetWaveHeightAtX(float xWorldPos)
    {
        float localX = xWorldPos - transform.position.x;
        return transform.position.y + Mathf.Sin(Time.time * speed + localX * frequency) * amplitude;
    }
}
