using UnityEngine;

public class UnderwaterLifeAI : MonoBehaviour
{
    [Header("Cấu hình Di chuyển")]
    public float moveSpeed = 2f;
    public float changeTargetDistance = 1f;
    public Vector2 roamArea = new Vector2(5f, 3f); // Vùng bơi lượn xung quanh vị trí gốc

    [Header("Hiệu ứng")]
    public bool automaticFlip = true;
    public float rotationSmooth = 5f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    void Start()
    {
        startPosition = transform.position;
        SetNewTarget();
    }

    void Update()
    {
        // 1. Di chuyển tới mục tiêu
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 2. Quay mặt theo hướng bơi
        if (automaticFlip)
        {
            float direction = targetPosition.x - transform.position.x;
            if (Mathf.Abs(direction) > 0.1f)
            {
                // Lật Scale để xoay mặt
                Vector3 scale = transform.localScale;
                scale.x = direction > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x); // Giả sử sprite gốc nhìn trái
                transform.localScale = scale;
            }
        }

        // 3. Nếu gần đến nơi thì tìm chỗ mới để bơi
        if (Vector3.Distance(transform.position, targetPosition) < changeTargetDistance)
        {
            SetNewTarget();
        }
    }

    void SetNewTarget()
    {
        float randomX = Random.Range(-roamArea.x, roamArea.x);
        float randomY = Random.Range(-roamArea.y, roamArea.y);
        targetPosition = startPosition + new Vector3(randomX, randomY, 0);
    }

    // Vẽ vùng bơi lượn trong Editor để dễ quan sát
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(startPosition == Vector3.zero ? transform.position : startPosition, (Vector3)roamArea * 2f);
    }
}
