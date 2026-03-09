using UnityEngine;

public class FallingTrap : MonoBehaviour
{
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float damage = 40f;
    private Rigidbody2D rb;
    private bool isTriggered = false;
    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Ban đầu đứng im
        
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;
    }

    void Update()
    {
        if (isTriggered || player == null) return;

        // Kiểm tra xem người chơi có đang ở ngay bên dưới không
        float xDist = Mathf.Abs(transform.position.x - player.position.x);
        float yDist = transform.position.y - player.position.y;

        if (xDist < 1f && yDist > 0 && yDist < detectionRange)
        {
            TriggerFall();
        }
    }

    void TriggerFall()
    {
        isTriggered = true;
        rb.gravityScale = 2f; // Cho phép rơi
        Debug.Log("Bẫy đã kích hoạt! Rơi đá!!!");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isTriggered && collision.gameObject.CompareTag("Player"))
        {
            PlayerStatusManager status = collision.gameObject.GetComponent<PlayerStatusManager>();
            if (status != null) status.TakeDamage(damage);
            
            // Xóa đá sau khi đập trúng hoặc rơi xuống sàn
            Destroy(gameObject, 1f); 
        }
    }
}
