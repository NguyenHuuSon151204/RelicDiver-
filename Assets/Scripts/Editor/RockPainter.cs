using UnityEngine;
using UnityEditor;

public class RockPainter : EditorWindow
{
    [MenuItem("Tools/Relic Diver/Rock Painter")]
    public static void ShowWindow()
    {
        GetWindow<RockPainter>("Rock Painter");
    }

    public GameObject rockPrefab;
    public float brushRadius = 2f;
    public float density = 0.5f;
    public float minScale = 0.5f;
    public float maxScale = 1.5f;
    
    private bool isPainting = false;
    private Vector3 lastPaintPos;

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        GUILayout.Label("Cấu hình cọ vẽ đá", EditorStyles.boldLabel);
        rockPrefab = (GameObject)EditorGUILayout.ObjectField("Mẫu đá (Prefab)", rockPrefab, typeof(GameObject), false);
        brushRadius = EditorGUILayout.Slider("Bán kính cọ", brushRadius, 0.5f, 10f);
        density = EditorGUILayout.Slider("Độ dày (Khoảng cách)", density, 0.1f, 5f);
        
        EditorGUILayout.Space();
        GUILayout.Label("Ngẫu nhiên hóa", EditorStyles.boldLabel);
        minScale = EditorGUILayout.Slider("Tỉ lệ nhỏ nhất", minScale, 0.1f, 1f);
        maxScale = EditorGUILayout.Slider("Tỉ lệ lớn nhất", maxScale, 1f, 5f);

        EditorGUILayout.HelpBox("CÁCH DÙNG:\n1. Chọn Prefab đá.\n2. Giữ phím [ B ] trong cửa sổ Scene và di chuyển chuột để VẼ.\n3. Giữ phím [ N ] để XÓA lân cận.", MessageType.Info);
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // 1. Nhận phím B để bắt đầu vẽ
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.B) isPainting = true;
        if (e.type == EventType.KeyUp && e.keyCode == KeyCode.B) isPainting = false;

        if (isPainting && rockPrefab != null)
        {
            // Vô hiệu hóa việc chọn vật thể trong Scene để tập trung vẽ
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3 spawnPos = ray.origin;
            spawnPos.z = 0; // Đảm bảo vẽ trên mặt phẳng 2D

            // Vẽ vòng tròn cọ để người dùng thấy
            Handles.color = new Color(0, 1, 1, 0.2f);
            Handles.DrawSolidDisc(spawnPos, Vector3.forward, brushRadius);

            // Chỉ vẽ khi chuột di chuyển đủ xa (tránh đè lên nhau)
            if (Vector3.Distance(spawnPos, lastPaintPos) > density)
            {
                PaintRock(spawnPos);
                lastPaintPos = spawnPos;
            }
            
            sceneView.Repaint();
        }
    }

    void PaintRock(Vector3 pos)
    {
        // Tạo đá ngẫu nhiên trong bán kính cọ
        Vector3 randomOffset = Random.insideUnitCircle * brushRadius;
        Vector3 finalPos = pos + randomOffset;

        GameObject newRock = (GameObject)PrefabUtility.InstantiatePrefab(rockPrefab);
        newRock.transform.position = finalPos;
        
        // Ngẫu nhiên xoay (360 độ)
        newRock.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        
        // Ngẫu nhiên kích thước
        float scale = Random.Range(minScale, maxScale);
        newRock.transform.localScale = new Vector3(scale, scale, 1);

        // Gán vào một cha để gọn Hierarchy
        GameObject holder = GameObject.Find("Map_Holder");
        if (holder == null) holder = new GameObject("Map_Holder");
        newRock.transform.parent = holder.transform;

        Undo.RegisterCreatedObjectUndo(newRock, "Paint Rock");
    }
}
