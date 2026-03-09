using UnityEngine;

public class FishAI : MonoBehaviour
{
    public enum MovementMode { Solo, Couple, Schooling }
    
    [Header("Cấu hình di chuyển")]
    public MovementMode mode = MovementMode.Solo;
    [Tooltip("Tích nếu hình gốc cá nhìn sang PHẢI, bỏ nếu nhìn sang TRÁI")]
    public bool spriteFacesRight = true; 
    public float moveSpeed = 2f;
    public float waveFrequency = 2f;
    public float waveAmplitude = 0.5f;
    
    [Header("Trạng thái")]
    public Vector2 direction = Vector2.right;
    public Transform leader; 
    
    private float startTime;
    private SpriteRenderer sr;
    private FishAI leaderAI;
    private Vector3 targetOffset; // Vị trí riêng của mỗi con trong bầy

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        
        if (leader != null) leaderAI = leader.GetComponent<FishAI>();

        // TẠO VỊ TRÍ RIÊNG TRONG ĐỘI HÌNH (Để bầy tản ra, không bị đè lên nhau)
        targetOffset = new Vector3(Random.Range(-2.5f, -1.5f), Random.Range(-1.5f, 1.5f), 0);

        startTime = Random.Range(0f, 10f);
        UpdateFacing();
    }

    void Update()
    {
        if (mode == MovementMode.Solo) Wander();
        else if (mode == MovementMode.Schooling) FollowLeader();
        
        UpdateFacing();
    }

    private void Wander()
    {
        startTime += Time.deltaTime;
        float verticalWave = Mathf.Sin(startTime * waveFrequency) * waveAmplitude;
        Vector3 movement = (Vector3)direction * moveSpeed + transform.up * verticalWave;
        transform.position += movement * Time.deltaTime;

        float tilt = verticalWave * 15f;
        transform.rotation = Quaternion.Euler(0, 0, (direction.x > 0 ? tilt : -tilt));
    }

    private void FollowLeader()
    {
        // NẾU MẤT LEADER: Chuyển sang bơi tự do để không bị đứng im một chỗ
        if (leader == null)
        {
            mode = MovementMode.Solo;
            return;
        }

        if (leaderAI == null) leaderAI = leader.GetComponent<FishAI>();

        // Ưu tiên hướng của leader 
        direction = leaderAI != null ? leaderAI.direction : direction;

        // VỊ TRÍ ĐỘI HÌNH: Dãn cách rộng hơn để nhìn rõ từng con
        Vector3 finalOffset = new Vector3(targetOffset.x * direction.x, targetOffset.y, 0);
        Vector3 target = leader.position + finalOffset;

        // Bơi dẻo theo nhịp sóng
        float verticalWave = Mathf.Sin(startTime * waveFrequency) * (waveAmplitude * 0.5f);
        transform.position = Vector3.Lerp(transform.position, target + transform.up * verticalWave, Time.deltaTime * moveSpeed);
        
        transform.rotation = Quaternion.Lerp(transform.rotation, leader.rotation, Time.deltaTime * 3f);
    }

    private void UpdateFacing()
    {
        if (sr == null) return;

        // QUAN TRỌNG: Lật mặt bằng FlipX của SpriteRenderer (An toàn hơn lật Scale)
        if (direction.x > 0) // Di chuyển sang PHẢI
        {
            sr.flipX = !spriteFacesRight;
        }
        else if (direction.x < 0) // Di chuyển sang TRÁI
        {
            sr.flipX = spriteFacesRight;
        }
    }

    public void SetVelocity(Vector2 dir, float speed)
    {
        direction = dir;
        moveSpeed = speed;
    }
}
