using UnityEngine;
using System.Collections.Generic;

public class DeepSeaWorldManager : MonoBehaviour
{
    [Header("--- Cấu hình Căn cứ (Home Base) ---")]
    public Transform spawnPoint;
    public GameObject player;
    public float dockingRange = 5f;
    public float oxygenRefillRate = 20f;
    public SpriteRenderer baseHighlight;

    [Header("--- Cấu hình Sinh vật (Fish Spawning) ---")]
    public Vector2 spawnZoneSize = new Vector2(40, 25); 
    public int maxFishCount = 50; 
    public GameObject[] soloFishPrefabs;
    public GameObject[] schoolFishPrefabs;

    [Header("--- Chế độ ---")]
    public bool followPlayer = true; 
    [Range(0, 1)] public float schoolSpawnChance = 0.2f;

    private List<GameObject> activeFish = new List<GameObject>();
    private PlayerStatusManager playerStatus;
    private bool isInBaseRange = false;

    void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            playerStatus = player.GetComponent<PlayerStatusManager>();
            // Chỉ dịch chuyển player nếu Manager này được đặt làm CĂN CỨ (có SpawnPoint)
            if (spawnPoint != null) player.transform.position = spawnPoint.position;
        }

        InitialFishSpawn();
    }

    void Update()
    {
        if (spawnPoint != null) HandleHomeBaseLogic();
        HandleFishSpawning();
    }

    private void HandleHomeBaseLogic()
    {
        if (player == null || playerStatus == null || spawnPoint == null) return;

        float dist = Vector2.Distance(spawnPoint.position, player.transform.position);

        if (dist < dockingRange)
        {
            if (!isInBaseRange) OnEnterBase();
            isInBaseRange = true;
            playerStatus.Heal(oxygenRefillRate * Time.deltaTime); 
            playerStatus.RestoreOxygen(oxygenRefillRate * Time.deltaTime);
        }
        else
        {
            if (isInBaseRange) OnExitBase();
            isInBaseRange = false;
        }
    }

    private void OnEnterBase() {
        Debug.Log("🏠 Đã về Căn cứ!");
        if (baseHighlight) baseHighlight.color = Color.green;
        if (LevelManager.Instance != null) LevelManager.Instance.SetPlayerAtHomeBase(true);
    }

    private void OnExitBase() {
        if (baseHighlight) baseHighlight.color = Color.yellow;
        if (LevelManager.Instance != null) LevelManager.Instance.SetPlayerAtHomeBase(false);
    }

    // --- LOGIC SINH CÁ ---
    private void HandleFishSpawning()
    {
        if (activeFish.Count < maxFishCount)
        {
            SpawnFishFromSide();
        }
        CleanupOffscreenFish();
    }

    private void InitialFishSpawn()
    {
        if (soloFishPrefabs == null || soloFishPrefabs.Length == 0) return;

        Vector3 center = GetCenter();
        for (int i = 0; i < maxFishCount / 2; i++)
        {
            Vector3 randomPos = center + new Vector3(
                Random.Range(-spawnZoneSize.x / 2.1f, spawnZoneSize.x / 2.1f),
                Random.Range(-spawnZoneSize.y / 2.1f, spawnZoneSize.y / 2.1f), 0);
            SpawnFish(randomPos);
        }
    }

    private void SpawnFishFromSide()
    {
        if (soloFishPrefabs == null || soloFishPrefabs.Length == 0) return;

        Vector3 center = GetCenter();
        bool fromLeft = Random.value > 0.5f;
        float spawnX = fromLeft ? -spawnZoneSize.x / 2 - 2f : spawnZoneSize.x / 2 + 2f;
        Vector3 spawnPos = center + new Vector3(spawnX, Random.Range(-spawnZoneSize.y / 2.5f, spawnZoneSize.y / 2.5f), 0);
        
        SpawnFish(spawnPos, fromLeft ? Vector2.right : Vector2.left);
    }

    private void SpawnFish(Vector3 pos, Vector2? forcedDir = null)
    {
        Vector2 dir = forcedDir ?? (Random.value > 0.5f ? Vector2.right : Vector2.left);
        
        // Quyết định sinh cá lẻ hay theo đàn
        bool spawnSchool = (schoolFishPrefabs != null && schoolFishPrefabs.Length > 0 && Random.value < schoolSpawnChance);
        
        if (spawnSchool)
        {
            GameObject prefab = schoolFishPrefabs[Random.Range(0, schoolFishPrefabs.Length)];
            SpawnIndividual(prefab, pos, dir);
        }
        else
        {
            GameObject prefab = soloFishPrefabs[Random.Range(0, soloFishPrefabs.Length)];
            SpawnIndividual(prefab, pos, dir);
        }
    }

    private void SpawnIndividual(GameObject prefab, Vector3 pos, Vector2 dir)
    {
        GameObject fish = Instantiate(prefab, pos, Quaternion.identity);
        SpriteRenderer sr = fish.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) { sr.sortingLayerName = "Player"; sr.sortingOrder = -1; }

        FishAI ai = fish.GetComponent<FishAI>();
        if (ai == null) ai = fish.AddComponent<FishAI>();
        ai.SetVelocity(dir, Random.Range(1.2f, 2.5f));
        
        activeFish.Add(fish);
    }

    private void CleanupOffscreenFish()
    {
        Vector3 center = GetCenter();
        float bX = spawnZoneSize.x / 2 + 10f;
        float bY = spawnZoneSize.y / 2 + 10f;

        for (int i = activeFish.Count - 1; i >= 0; i--)
        {
            if (activeFish[i] == null) { activeFish.RemoveAt(i); continue; }
            Vector3 relPos = activeFish[i].transform.position - center;
            if (Mathf.Abs(relPos.x) > bX || Mathf.Abs(relPos.y) > bY)
            {
                Destroy(activeFish[i]);
                activeFish.RemoveAt(i);
            }
        }
    }

    private Vector3 GetCenter() => (followPlayer && player != null) ? player.transform.position : transform.position;

    private void OnDrawGizmos()
    {
        Vector3 center = Application.isPlaying ? GetCenter() : transform.position;
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireCube(center, new Vector3(spawnZoneSize.x, spawnZoneSize.y, 1));
        
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, dockingRange);
        }
    }

}
