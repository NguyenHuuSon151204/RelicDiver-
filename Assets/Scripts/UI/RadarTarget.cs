using UnityEngine;

public enum TargetType { Treasure, Hazard, Resource }

public class RadarTarget : MonoBehaviour
{
    public TargetType type;
    public Sprite radarIcon; // Icon hiển thị trên Radar (vòng tròn, dấu X...)
    public Color iconColor = Color.green;

    // Khi vật thể bị phá hủy hoặc thu thập, nó sẽ tự báo cho Radar (nếu cần)
}
