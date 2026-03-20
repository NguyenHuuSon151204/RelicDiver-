using UnityEngine;

public class FishingNet : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    [SerializeField] private float floatSpeed = 0.5f;
    [SerializeField] private float floatAmount = 0.2f;
    [SerializeField] private float driftSpeed = 1f;
    [SerializeField] private Vector2 driftDirection = Vector2.left;

    [Header("Hiệu ứng")]
    [SerializeField] private GameObject collectEffect;

    private float startY;
    private float lifeTime;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        lifeTime += Time.deltaTime;

        // Trôi dạt tự nhiên (Sine Wave)
        Vector3 newPos = transform.position;
        newPos += (Vector3)(driftDirection * driftSpeed * Time.deltaTime);
        newPos.y = startY + Mathf.Sin(lifeTime * floatSpeed) * floatAmount;
        transform.position = newPos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi chạm vào tàu ngầm
        if (collision.CompareTag("Submarine") || collision.GetComponent<SubmarineStation>() != null)
        {
            SubmarineStation sub = collision.GetComponent<SubmarineStation>();
            if (sub == null) sub = collision.GetComponentInParent<SubmarineStation>();

            if (sub != null)
            {
                sub.AddTangledNet();
                
                if (collectEffect) Instantiate(collectEffect, transform.position, Quaternion.identity);
                
                // Tự hủy sau khi dính vào tàu
                Destroy(gameObject);
            }
        }
    }
}
