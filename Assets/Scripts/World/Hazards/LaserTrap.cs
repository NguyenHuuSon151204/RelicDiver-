using UnityEngine;
using System.Collections;

public class LaserTrap : MonoBehaviour
{
    [Header("Cấu hình chu kỳ")]
    public float onDuration = 2f;  // Thời gian bật
    public float offDuration = 2f; // Thời gian tắt
    public float damagePerSecond = 20f;

    [Header("Hiệu ứng Visual")]
    [SerializeField] private GameObject laserEffect; // Cái Sprite/Hạt của tia laser
    [SerializeField] private Collider2D laserCollider;

    private bool isActive = true;

    void Start()
    {
        StartCoroutine(LaserCycle());
    }

    IEnumerator LaserCycle()
    {
        while (true)
        {
            // BẬT Laze
            isActive = true;
            if (laserEffect) laserEffect.SetActive(true);
            if (laserCollider) laserCollider.enabled = true;
            yield return new WaitForSeconds(onDuration);

            // TẮT Laze
            isActive = false;
            if (laserEffect) laserEffect.SetActive(false);
            if (laserCollider) laserCollider.enabled = false;
            yield return new WaitForSeconds(offDuration);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isActive && collision.CompareTag("Player"))
        {
            PlayerStatusManager status = collision.GetComponent<PlayerStatusManager>();
            if (status != null)
            {
                status.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }
}
