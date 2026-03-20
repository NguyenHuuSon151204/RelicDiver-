using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SegmentedStatBar : MonoBehaviour
{
    [Header("--- Cấu hình Vạch ---")]
    public List<Image> segments; // Kéo các cục pin xanh vào đây (thứ tự từ dưới lên)
    public float flashWindow = 5f; // Khoảng cách % bắt đầu nháy (VD: còn 5% cuối của vạch đó sẽ nháy)
    public float flashSpeed = 15f;

    public void UpdateValue(float current, float max)
    {
        if (segments == null || segments.Count == 0 || max <= 0) return;

        float percentage = (current / max) * 100f;
        float percentPerSegment = 100f / segments.Count;

        for (int i = 0; i < segments.Count; i++)
        {
            float segmentTargetPercent = (i + 1) * percentPerSegment;
            float previousThreshold = i * percentPerSegment;

            // 1. Phía dưới ngưỡng hoàn toàn (Pin đã mất)
            if (percentage <= previousThreshold)
            {
                segments[i].gameObject.SetActive(false);
            }
            // 2. Đang trong giai đoạn nhấp nháy (Sắp mất vạch này)
            else if (percentage < previousThreshold + flashWindow)
            {
                segments[i].gameObject.SetActive(true);
                float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f;
                Color c = Color.white;
                c.a = alpha;
                segments[i].color = c;
            }
            // 3. Pin đang đầy ở vạch này
            else
            {
                segments[i].gameObject.SetActive(true);
                segments[i].color = Color.white;
            }
        }
    }
}
