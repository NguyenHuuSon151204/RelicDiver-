using UnityEngine;

public class HealthItem : Interactable
{
    [SerializeField] private float healthAmount = 20f;
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
        HealthSystem health = interactor.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.Heal(healthAmount);
            
            if (collectionEffect != null)
            {
                Instantiate(collectionEffect, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
            Debug.Log($"Đã hồi {healthAmount} máu!");
        }
    }
}
