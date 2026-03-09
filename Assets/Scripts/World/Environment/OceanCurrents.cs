using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class OceanCurrents : MonoBehaviour
{
    [Header("Cấu hình Dòng nước")]
    [SerializeField] private Vector2 pushDirection = Vector2.right;
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private bool showDebugArrow = true;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Sử dụng ForceMode2D.Force để đẩy mượt mà
                rb.AddForce(pushDirection.normalized * pushForce);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (showDebugArrow)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = transform.position;
            Vector3 direction = new Vector3(pushDirection.x, pushDirection.y, 0).normalized;
            Gizmos.DrawRay(center, direction * 2f);
            
            // Vẽ mũi tên nhỏ ở đầu
            Gizmos.DrawWireSphere(center + direction * 2f, 0.2f);
        }
    }
}
