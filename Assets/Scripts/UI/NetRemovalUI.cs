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
    [SerializeField] private GameObject tutorialHand;  // Icon hướng dẫn (Bàn tay/Mũi tên)

    private SubmarineStation targetSub;
    private List<NetDraggable> activeNets = new List<NetDraggable>();
    private Coroutine tutorialCoroutine;

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
            
            // Ép lưới văng xa khỏi tâm tàu
            float spreadW = spawnArea.rect.width * 0.7f;
            float spreadH = spawnArea.rect.height * 0.7f;
            
            float rx = Random.Range(-spreadW, spreadW);
            float ry = Random.Range(-spreadH, spreadH);

            // Nếu lỡ may cả x và y đều quá nhỏ (gần tâm), ta ép nó ra rìa
            if (Mathf.Abs(rx) < 50 && Mathf.Abs(ry) < 50) {
                rx += (rx > 0 ? 100 : -100);
                ry += (ry > 0 ? 100 : -100);
            }

            netObj.transform.localPosition = new Vector3(rx, ry, 0);

            // Thêm logic kéo thả
            NetDraggable draggable = netObj.AddComponent<NetDraggable>();
            draggable.Init(this, dragDistanceThreshold);
            activeNets.Add(draggable);
        }

        // --- Logic Hướng Dẫn Lần Đầu ---
        if (tutorialHand != null)
        {
            // Nếu chưa từng gỡ lưới bao giờ (giá trị 1 là lần đầu)
            bool isFirstTime = PlayerPrefs.GetInt("TutorialNets", 1) == 1;
            bool shouldShow = isFirstTime && count > 0;
            tutorialHand.SetActive(shouldShow);
            
            if (shouldShow)
            {
                if (tutorialCoroutine != null) StopCoroutine(tutorialCoroutine);
                tutorialCoroutine = StartCoroutine(AnimateTutorialHand());
            }
        }
    }

    private System.Collections.IEnumerator AnimateTutorialHand()
    {
        if (tutorialHand == null) yield break;

        Vector3 startPos = submarineImage != null ? submarineImage.transform.localPosition : Vector3.zero;
        Vector3 endPos = startPos + new Vector3(dragDistanceThreshold + 100f, 0, 0); // Kéo sang phải

        while (true)
        {
            float elapsed = 0f;
            float duration = 1.2f;

            // Di chuyển từ tâm ra ngoài
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Sử dụng SmoothStep để mượt hơn
                tutorialHand.transform.localPosition = Vector3.Lerp(startPos, endPos, t * t * (3f - 2f * t));
                yield return null;
            }

            // Nghỉ một chút rồi lặp lại
            yield return new WaitForSeconds(0.5f);
            tutorialHand.transform.localPosition = startPos;
        }
    }

    public void OnNetRemoved(NetDraggable net)
    {
        activeNets.Remove(net);
        Destroy(net.gameObject);

        // Đã gỡ được 1 cái -> Ẩn luôn hướng dẫn cho đỡ vướng
        if (tutorialHand != null && tutorialHand.activeSelf)
        {
            if (tutorialCoroutine != null) StopCoroutine(tutorialCoroutine);
            tutorialHand.SetActive(false);
            PlayerPrefs.SetInt("TutorialNets", 0); // Lưu lại là đã qua hướng dẫn
            PlayerPrefs.Save();
        }

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
        gameObject.SetActive(false);
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
