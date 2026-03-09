using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private bool triggerOnStart = true;
    [SerializeField] private bool destroyAfterUse = true;

    private void Start()
    {
        if (triggerOnStart)
        {
            Trigger();
        }
    }

    public void Trigger()
    {
        if (DialogueManager.Instance != null && dialogue != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue);
            if (destroyAfterUse) Destroy(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggerOnStart && other.CompareTag("Player"))
        {
            Trigger();
        }
    }
}
