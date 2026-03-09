using UnityEngine;

public class OxygenBubble : Interactable
{
    [SerializeField] private float oxygenAmount = 20f;
    [SerializeField] private GameObject collectionEffect;

    public override void Interact(GameObject interactor)
    {
        Restore(interactor);
    }

    protected override void AutoInteract(GameObject interactor)
    {
        Restore(interactor);
    }

    private void Restore(GameObject interactor)
    {
        PlayerStatusManager status = interactor.GetComponent<PlayerStatusManager>();
        if (status != null)
        {
            status.RestoreOxygen(oxygenAmount);
            
            if (collectionEffect != null)
            {
                Instantiate(collectionEffect, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
            Debug.Log($"Đã hồi {oxygenAmount} oxy!");
        }
    }
}
