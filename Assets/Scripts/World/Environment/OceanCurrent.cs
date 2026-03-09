using UnityEngine;

public class OceanCurrent : MonoBehaviour
{
    public Vector2 pushDirection = Vector2.right; // Hướng đẩy (ví dụ: sang phải)
    public float force = 5f;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Áp dụng lực đẩy liên tục như dòng nước
                rb.AddForce(pushDirection.normalized * force);
            }
        }
    }

    // Vẽ mũi tên hướng nước trong Editor để dễ chỉnh
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, (Vector3)pushDirection * 2f);
    }
}
