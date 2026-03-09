using UnityEngine;

public class SpikeHazard : MonoBehaviour
{
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damageCooldown = 1f;

    private float lastDamageTime;

    // Sử dụng OnCollisionStay2D cho hệ thống vật lý 2D
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time > lastDamageTime + damageCooldown)
            {
                PlayerStatusManager status = collision.gameObject.GetComponent<PlayerStatusManager>();
                if (status != null)
                {
                    status.TakeDamage(damageAmount);
                    lastDamageTime = Time.time;
                    Debug.Log("Người chơi 2D bị trúng gai!");
                }
            }
        }
    }
}
