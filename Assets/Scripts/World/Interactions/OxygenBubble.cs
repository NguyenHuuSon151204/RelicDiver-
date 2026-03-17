using UnityEngine;

public class OxygenBubble : Interactable
{
    [SerializeField] private float oxygenAmount = 20f;
    [SerializeField] private GameObject collectionEffect;
    [SerializeField] private AudioClip collectSound;

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
                GameObject fx = Instantiate(collectionEffect, transform.position, Quaternion.identity);
                ParticleSystem ps = fx.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
            }

            if (collectSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(collectSound);
            }
            
            Destroy(gameObject);
            Debug.Log($"Đã hồi {oxygenAmount} oxy!");
        }
    }
}
