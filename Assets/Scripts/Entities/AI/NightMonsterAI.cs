using UnityEngine;
using System.Collections;

public class NightMonsterAI : MonoBehaviour
{
    [Header("Cấu hình Săn đuổi")]
    [SerializeField] private float appearDistance = 15f;
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float catchDistance = 1.5f;
    [SerializeField] private float smoothTime = 1f;

    [Header("Hiệu ứng Visual")]
    [SerializeField] private SpriteRenderer monsterSprite;
    [SerializeField] private float ghostAlpha = 0.4f;

    private Transform player;
    private bool isHunting = false;
    private Vector2 currentVelocity;
    private TimeCycleManager timeManager;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        timeManager = FindObjectOfType<TimeCycleManager>();
        if (timeManager)
        {
            timeManager.OnNightFall += StartHunt;
        }

        // Ban đầu quái vật ẩn đi
        if (monsterSprite) monsterSprite.color = new Color(1, 1, 1, 0);
        gameObject.SetActive(false);
    }

    public void StartHunt()
    {
        if (isHunting) return;
        
        gameObject.SetActive(true);
        isHunting = true;
        
        // Xuất hiện ngẫu nhiên phía sau player ngoài tầm nhìn
        if (player)
        {
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            transform.position = (Vector2)player.position + (randomDir * appearDistance);
        }

        StartCoroutine(AppearFadeEffect());
    }

    private IEnumerator AppearFadeEffect()
    {
        float timer = 0;
        float duration = 3f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, ghostAlpha, timer / duration);
            if (monsterSprite) monsterSprite.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }

    private void Update()
    {
        if (!isHunting || !player) return;

        // Di chuyển mượt mà về phía player
        Vector2 targetPos = player.position;
        transform.position = Vector2.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime, followSpeed);

        // Xoay quái vật nhìn về phía player
        Vector2 direction = (Vector2)player.position - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Kiểm tra bắt được player
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < catchDistance)
        {
            CatchPlayer();
        }
    }

    private void CatchPlayer()
    {
        isHunting = false;
        Debug.Log("QUÁI VẬT ĐÃ BẮT ĐƯỢC BẠN!");
        
        // Gọi Game Over thông qua HUD
        HUDController hud = FindObjectOfType<HUDController>();
        if (hud) hud.ShowGameOver();

        // Khởi động lại màn chơi (Quy tắc GDD mới)
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartLevel();
        }
    }
}
