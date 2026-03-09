using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SineWaveMesh : MonoBehaviour
{
    [Header("Cấu hình Sóng Sin")]
    public float width = 50f;        // Độ rộng mặt biển
    public float height = 5f;       // Độ sâu dải nước (để tô màu)
    public int segments = 50;       // Độ chi tiết (càng cao càng mượt)
    
    [Header("Toán học đồ thị Sin")]
    public float amplitude = 0.5f;  // Biên độ (Độ cao ngọn sóng)
    public float frequency = 0.5f;  // Tần số (Số lượng ngọn sóng)
    public float speed = 2.0f;      // Tốc độ sóng chạy

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateMesh();
    }

    void Update()
    {
        UpdateWaves();
    }

    void CreateMesh()
    {
        int vCount = (segments + 1) * 2;
        vertices = new Vector3[vCount];
        triangles = new int[segments * 6];
        uvs = new Vector2[vCount];

        float xStep = width / segments;

        for (int i = 0; i <= segments; i++)
        {
            float x = i * xStep - (width / 2);
            // Vertices
            vertices[i * 2] = new Vector3(x, 0, 0); // Đỉnh trên
            vertices[i * 2 + 1] = new Vector3(x, -height, 0); // Đáy dưới

            // UVs (để gán tấm ảnh sóng của bạn vào)
            uvs[i * 2] = new Vector2((float)i / segments, 1);
            uvs[i * 2 + 1] = new Vector2((float)i / segments, 0);

            // Triangles
            if (i < segments)
            {
                int startIdx = i * 2;
                int triIdx = i * 6;
                triangles[triIdx] = startIdx;
                triangles[triIdx + 1] = startIdx + 1;
                triangles[triIdx + 2] = startIdx + 2;
                triangles[triIdx + 3] = startIdx + 2;
                triangles[triIdx + 4] = startIdx + 1;
                triangles[triIdx + 5] = startIdx + 3;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
    }

    void UpdateWaves()
    {
        for (int i = 0; i <= segments; i++)
        {
            float x = vertices[i * 2].x;
            // Công thức đồ thị Sin chuẩn
            float y = Mathf.Sin(Time.time * speed + x * frequency) * amplitude;
            
            // Chỉ gán Y cho đỉnh phía trên
            vertices[i * 2].y = y;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

    // Hàm public giúp Player/Tàu "xin" tọa độ Y để nổi
    public float GetHeightAtX(float xWorldPos)
    {
        float localX = xWorldPos - transform.position.x;
        return transform.position.y + Mathf.Sin(Time.time * speed + localX * frequency) * amplitude;
    }
}
