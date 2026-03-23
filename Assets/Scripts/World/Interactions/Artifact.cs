using UnityEngine;

public class Artifact : Interactable
{
    public enum ItemType { Research, BlackMarket }

    [Header("Cấu hình Cổ vật")]
    [SerializeField] private ItemType type = ItemType.Research;
    [SerializeField] private int creditValue = 150;
    [SerializeField] private string artifactName = "Cổ vật cổ đại";
    [SerializeField] private GameObject collectionEffect;
    [SerializeField] private AudioClip collectSound;

    public override void Interact(GameObject interactor)
    {
        Collect();
    }

    protected override void AutoInteract(GameObject interactor)
    {
        Collect();
    }

    private void Collect()
    {
        // Thêm vào quản lý màn chơi nếu là Đồ nghiên cứu
        if (LevelManager.Instance != null && type == ItemType.Research)
        {
            LevelManager.Instance.AddArtifact();
        }

        // Cộng tiền mặt đã bị gỡ bỏ

        // Tạo hiệu ứng nếu có
        if (collectionEffect != null)
        {
            Instantiate(collectionEffect, transform.position, Quaternion.identity);
        }

        if (collectSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(collectSound);
        }

        // Huỷ vật thể
        Destroy(gameObject);
        Debug.Log("Đã thu thập cổ vật!");
    }
}
