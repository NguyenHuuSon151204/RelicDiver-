using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Cấu hình màn chơi")]
    [SerializeField] private int requiredArtifacts = 3;
    [SerializeField] private string nextSceneName;

    private int collectedArtifactCount = 0;
    private bool levelComplete = false;

    public event Action<int, int> OnArtifactCollected; // (collected, total)
    public event Action OnLevelComplete;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddArtifact()
    {
        if (levelComplete) return;

        collectedArtifactCount++;
        OnArtifactCollected?.Invoke(collectedArtifactCount, requiredArtifacts);

        if (collectedArtifactCount >= requiredArtifacts)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        levelComplete = true;
        OnLevelComplete?.Invoke();
        Debug.Log("Màn chơi hoàn thành! Đã thu thập đủ cổ vật.");
    }

    public void LoadNextLevel()
    {
        if (levelComplete && !string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
