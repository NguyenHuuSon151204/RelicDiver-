using UnityEngine;

[ExecuteInEditMode]
public class WorldBoundary : MonoBehaviour
{
    [Header("Cấu hình giới hạn")]
    public Vector2 boundarySize = new Vector2(40f, 25f);
    public float wallThickness = 2f;

    [Header("Cinemachine Support")]
    public PolygonCollider2D polyCollider; 

    public static WorldBoundary Instance;

    void Awake()
    {
        Instance = this;
        SetupBoundaries();
    }

    void OnValidate()
    {
        if (!Application.isPlaying) {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += SetupBoundaries;
            #endif
        } else {
            SetupBoundaries();
        }
    }

    void SetupBoundaries()
    {
        if (this == null) return;

        // Đảm bảo Scale luôn là 1 để tránh sai lệch vật lý
        if (transform.localScale != Vector3.one && !Application.isPlaying)
        {
            Debug.LogWarning("WorldBoundary: Nên để Scale của đối tượng này là (1,1,1) để tính toán chính xác nhất.");
        }

        // 1. Setup Vùng Trigger cho Cinemachine
        if (polyCollider == null) polyCollider = GetComponent<PolygonCollider2D>();
        if (polyCollider == null) polyCollider = gameObject.AddComponent<PolygonCollider2D>();
        polyCollider.isTrigger = true;

        Vector2 halfSize = boundarySize / 2f;
        Vector2[] points = new Vector2[4];
        points[0] = new Vector2(-halfSize.x, halfSize.y);
        points[1] = new Vector2(halfSize.x, halfSize.y);
        points[2] = new Vector2(halfSize.x, -halfSize.y);
        points[3] = new Vector2(-halfSize.x, -halfSize.y);
        polyCollider.points = points;

        // 2. Setup Vùng Rắn
        UpdateWall("Wall_Top", new Vector2(0, boundarySize.y / 2 + wallThickness / 2), new Vector2(boundarySize.x + wallThickness * 2, wallThickness));
        UpdateWall("Wall_Bottom", new Vector2(0, -boundarySize.y / 2 - wallThickness / 2), new Vector2(boundarySize.x + wallThickness * 2, wallThickness));
        UpdateWall("Wall_Left", new Vector2(-boundarySize.x / 2 - wallThickness / 2, 0), new Vector2(wallThickness, boundarySize.y));
        UpdateWall("Wall_Right", new Vector2(boundarySize.x / 2 + wallThickness / 2, 0), new Vector2(wallThickness, boundarySize.y));
    }

    void UpdateWall(string wallName, Vector2 pos, Vector2 size)
    {
        Transform wallTrans = transform.Find(wallName);
        if (wallTrans == null)
        {
            GameObject wallObj = new GameObject(wallName);
            wallObj.transform.parent = transform;
            wallTrans = wallObj.transform;
        }

        wallTrans.localPosition = pos;
        BoxCollider2D col = wallTrans.GetComponent<BoxCollider2D>();
        if (col == null) col = wallTrans.gameObject.AddComponent<BoxCollider2D>();
        
        col.size = size;
        col.isTrigger = false;
    }

    private void OnDrawGizmos()
    {
        // Vẽ chính xác theo những gì Collider thực tế đang có
        Gizmos.matrix = transform.localToWorldMatrix;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(boundarySize.x, boundarySize.y, 0));
        
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Vector2 halfSize = boundarySize / 2f;
        Gizmos.DrawCube(new Vector3(0, halfSize.y + wallThickness / 2, 0), new Vector3(boundarySize.x + wallThickness * 2, wallThickness, 1));
        Gizmos.DrawCube(new Vector3(0, -halfSize.y - wallThickness / 2, 0), new Vector3(boundarySize.x + wallThickness * 2, wallThickness, 1));
        Gizmos.DrawCube(new Vector3(-halfSize.x - wallThickness / 2, 0, 0), new Vector3(wallThickness, boundarySize.y, 1));
        Gizmos.DrawCube(new Vector3(halfSize.x + wallThickness / 2, 0, 0), new Vector3(wallThickness, boundarySize.y, 1));
    }
}
