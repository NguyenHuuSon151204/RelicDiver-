using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Cấu hình tương tác 2D")]
    [SerializeField] protected string interactionPrompt = "Nhấn E để tương tác";
    [SerializeField] protected float interactionDistance = 2f;

    public string GetPrompt() => interactionPrompt;
    public float GetInteractionDistance() => interactionDistance;

    public abstract void Interact(GameObject interactor);

    // Sử dụng OnTriggerEnter2D thay vì OnTriggerEnter
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AutoInteract(other.gameObject);
        }
    }

    protected virtual void AutoInteract(GameObject interactor)
    {
        // Mặc định không tự động, ghi đè ở lớp con nếu cần
    }
}
