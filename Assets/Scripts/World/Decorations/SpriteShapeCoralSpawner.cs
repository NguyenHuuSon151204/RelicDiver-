using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteShapeController))]
public class SpriteShapeCoralSpawner : MonoBehaviour
{
    [Header("Mẫu vật thể (Prefab San hô)")]
    public GameObject[] coralPrefabs; 
    
    [Header("Cấu hình rải")]
    [Range(0, 100)]
    public float spawnChance = 40f; 
    public float spacing = 1.5f; // Khoảng cách thực tế giữa các cây
    public float yOffset = -0.1f; // Độ sâu cắm rễ vào đá
    public float minScale = 0.8f; // To thiểu
    public float maxScale = 1.5f; // To tối đa

    [Header("Lọc góc (Chống mọc vách/trần)")]
    public float maxSlopeAngle = 45f;

    [ContextMenu("🚀 Rải San Hô (Bám Đá Hoàn Hảo)")]
    public void SpawnCoralsPerfect()
    {
        ClearCorals();
        
        SpriteShapeController controller = GetComponent<SpriteShapeController>();
        Spline spline = controller.spline;
        
        // 1. Lấy dữ liệu độ dài Spline để rải đều
        // Chúng ta sử dụng cache của SpriteShape để lấy các điểm nội suy
        float totalLength = 0;
        int pointCount = spline.GetPointCount();
        
        // Ước tính số lượng mẫu (Sampling) dựa trên độ dài
        // Càng tăng số này (ví dụ 100-200) thì rải càng chính xác theo đường cong
        int samples = 200; 

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / (float)samples;
            
            // Lấy vị trí và tiếp tuyến chính xác trên đường cong Bezier
            Vector3 pos = GetPointOnSpline(spline, t, out Vector3 tangent);
            
            // 2. Kiểm tra khoảng cách để giữ mật độ mong muốn
            // (Đơn giản hóa: Chúng ta dùng tỉ lệ % ngẫu nhiên)
            if (Random.Range(0, 100) > spawnChance) continue;

            // 3. Tính góc quay Vuông Góc với mặt đá (Normal)
            Vector2 normal = new Vector2(-tangent.y, tangent.x).normalized;
            float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90f;

            // 4. Chỉ chọn mặt Sàn (Hướng lên trên)
            // Trong Sprite Shape, sàn có Normal là Vector hướng lên
            if (Vector2.Angle(normal, Vector3.up) < maxSlopeAngle)
            {
                SpawnSingleCoral(pos, angle, normal);
            }
        }
    }

    // Hàm nội suy vị trí và hướng trên đường cong Spline của Unity
    Vector3 GetPointOnSpline(Spline spline, float t, out Vector3 tangent)
    {
        int pointCount = spline.GetPointCount();
        float index = t * (pointCount - (spline.isOpenEnded ? 1 : 0));
        int i = Mathf.FloorToInt(index);
        float localT = index - i;

        int nextI = (i + 1) % pointCount;

        Vector3 p0 = spline.GetPosition(i);
        Vector3 p1 = spline.GetPosition(nextI);
        Vector3 rt = p0 + spline.GetRightTangent(i);
        Vector3 lt = p1 + spline.GetLeftTangent(nextI);

        // Tính vị trí Bezier
        Vector3 pos = BezierPoint(p0, rt, lt, p1, localT);
        // Tính tiếp tuyến (Tangent)
        tangent = BezierTangent(p0, rt, lt, p1, localT);

        return pos;
    }

    Vector3 BezierPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) {
        return Vector3.Lerp(Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t), Vector3.Lerp(Vector3.Lerp(b, c, t), Vector3.Lerp(c, d, t), t), t);
    }

    Vector3 BezierTangent(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) {
        return 3 * Mathf.Pow(1 - t, 2) * (b - a) + 6 * (1 - t) * t * (c - b) + 3 * Mathf.Pow(t, 2) * (d - c);
    }

    void SpawnSingleCoral(Vector3 localPos, float angle, Vector3 normal)
    {
        if (coralPrefabs.Length == 0) return;

        GameObject prefab = coralPrefabs[Random.Range(0, coralPrefabs.Length)];
        GameObject coral = Instantiate(prefab, transform);
        
        // Đặt vị trí chính xác tại bề mặt đá (vẫn dùng Normal để đẩy lún rễ vào đất)
        coral.transform.localPosition = localPos + (normal * yOffset);
        
        // LUÔN QUAY THẲNG ĐỨNG HƯỚNG LÊN TRỜI (Z = 0)
        // Bạn có thể thêm Random.Range(-10f, 10f) nếu muốn san hô hơi nghiêng ngả tự nhiên
        coral.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
        
        // Ngẫu nhiên To ròng / Nhỏ xinh dựa trên cấu hình
        float randScale = Random.Range(minScale, maxScale);
        coral.transform.localScale = new Vector3(
            Random.value > 0.5f ? randScale : -randScale,
            randScale, 
            1
        );
        
        coral.name = "Auto_Coral_" + coral.name;
    }

    public void ClearCorals()
    {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Auto_Coral_")) {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}
