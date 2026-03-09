using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SineWaveVisual : MonoBehaviour
{
    [Header("Cấu hình dải sóng")]
    public float waveWidth = 50f;     // Độ rộng dải sóng
    public float waveHeight = 10f;    // Độ sâu từ đỉnh xuống đáy
    public int resolution = 40;       // Độ mịn (càng cao càng mượt)
    
    [Header("Thông số đồ thị Sin")]
    public float amplitude = 0.5f;    // Độ cao của sóng
    public float frequency = 0.8f;    // Độ dày (số ngọn sóng)
    public float speed = 1.5f;        // Tốc độ nhấp nhô

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

    void CreateMesh()
    {
        int vCount = (resolution + 1) * 2;
        vertices = new Vector3[vCount];
        triangles = new int[resolution * 6];
        uvs = new Vector2[vCount];

        float step = waveWidth / resolution;
        for (int i = 0; i <= resolution; i++)
        {
            float x = i * step - (waveWidth / 2f);
            vertices[i * 2] = new Vector3(x, 0, 0);       // Đỉnh trên
            vertices[i * 2 + 1] = new Vector3(x, -waveHeight, 0); // Đáy dưới

            uvs[i * 2] = new Vector2((float)i / resolution, 1);
            uvs[i * 2 + 1] = new Vector2((float)i / resolution, 0);

            if (i < resolution) {
                int start = i * 2;
                int tri = i * 6;
                triangles[tri] = start; triangles[tri+1] = start+1; triangles[tri+2] = start+2;
                triangles[tri+3] = start+2; triangles[tri+4] = start+1; triangles[tri+5] = start+3;
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
    }

    void Update()
    {
        for (int i = 0; i <= resolution; i++)
        {
            float x = vertices[i * 2].x;
            // Công thức đồ thị Sin: Y = sin(thời gian + vị trí x)
            float y = Mathf.Sin(Time.time * speed + x * frequency) * amplitude;
            vertices[i * 2].y = y; // Chỉ nhấp nhô đỉnh trên
        }
        mesh.vertices = vertices;
    }
}
