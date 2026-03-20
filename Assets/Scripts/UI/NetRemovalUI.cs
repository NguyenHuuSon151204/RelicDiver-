using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class NetRemovalUI : MonoBehaviour
{
    [Header("Cấu hình Mini-game")]
    [SerializeField] private GameObject netIconPrefab;
    [SerializeField] private RectTransform spawnArea; // Vùng để random vị trí ban đầu
    [SerializeField] private float dragDistanceThreshold = 300f; // Khoảng cách kéo ra xa để gỡ
    [SerializeField] private GameObject completionText;
    [SerializeField] private GameObject submarineImage; // Hình con tàu ở giữa

    private SubmarineStation targetSub;
    private List<NetDraggable> activeNets = new List<NetDraggable>();

    public void Setup(int count, SubmarineStation sub)
    {
        targetSub = sub;
        if (completionText) completionText.SetActive(false);

        // Xóa các lưới cũ (nếu có)
        foreach (Transform child in spawnArea)
        {
            if (child.gameObject != submarineImage) Destroy(child.gameObject);
        }
        activeNets.Clear();

        // Tạo lưới mới tại vị trí ngẫu nhiên xung quanh tàu
        for (int i = 0; i < count; i++)
        {
            GameObject netObj = Instantiate(netIconPrefab, spawnArea);
            
            // Random vị trí trong vùng spawn
            float rx = Random.Range(-spawnArea.rect.width/3, spawnArea.rect.width/3);
            float ry = Random.Range(-spawnArea.rect.height/3, spawnArea.rect.height/3);
            netObj.transform.localPosition = new Vector3(rx, ry, 0);

            // Thêm logic kéo thả
            NetDraggable draggable = netObj.AddComponent<NetDraggable>();
            draggable.Init(this, dragDistanceThreshold);
            activeNets.Add(draggable);
        }
    }

    public void OnNetRemoved(NetDraggable net)
    {
        activeNets.Remove(net);
        Destroy(net.gameObject);

        if (activeNets.Count <= 0)
        {
            OnAllCleared();
        }
    }

    private void OnAllCleared()
    {
        if (completionText) completionText.SetActive(true);
        if (targetSub) targetSub.ClearNets();
        Invoke("Close", 1.5f);
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}

// Helper class để xử lý kéo thả
public class NetDraggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private NetRemovalUI manager;
    private Vector3 startPos;
    private float threshold;
    private CanvasGroup canvasGroup;

    public void Init(NetRemovalUI m, float t)
    {
        manager = m;
        threshold = t;
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = transform.localPosition;
        if (canvasGroup) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Di chuyển lưới theo chuột
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup) canvasGroup.blocksRaycasts = true;

        // Tính khoảng cách từ vị trí bắt đầu (tâm tàu) đến vị trí hiện tại
        float dist = Vector3.Distance(transform.localPosition, Vector3.zero);

        if (dist > threshold)
        {
            // Nếu đã kéo ra đủ xa -> Gỡ lưới thành công
            manager.OnNetRemoved(this);
        }
        else
        {
            // Trở về vị trí cũ nếu chưa đủ xa
            transform.localPosition = startPos;
        }
    }
}
