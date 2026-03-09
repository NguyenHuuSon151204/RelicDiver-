using UnityEngine;

public class SeaweedInteraction : MonoBehaviour
{
    [Header("Cố định góc nghiêng")]
    [SerializeField] private float tiltAmount = 15f; // Độ nghiêng tối đa
    [SerializeField] private float returnSpeed = 2f; // Tốc độ trở lại vị trí cũ

    private float currentTilt = 0f;
    private float targetTilt = 0f;

    private void Update()
    {
        // Luôn mượt mà quay về targetTilt (mặc định là 0 nếu không có va chạm)
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * returnSpeed);
        
        // Áp dụng độ nghiêng vào Transform.Rotation
        // Chú ý: Nghiêng quanh trục Z
        transform.rotation = Quaternion.Euler(0, 0, currentTilt);
        
        // Reset targetTilt mỗi Frame để nếu thợ lặn đi xa, nó sẽ tự về 0
        targetTilt = Mathf.Lerp(targetTilt, 0, Time.deltaTime);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tính toán hướng nghiêng dựa trên vị trí của Player
            float direction = transform.position.x - other.transform.position.x;
            
            // Nếu Player ở bên trái, rặng san hô sẽ nghiêng sang bên phải (số dương)
            targetTilt = (direction > 0) ? tiltAmount : -tiltAmount;
        }
    }
}
