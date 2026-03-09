using UnityEngine;

public class GrabberController : MonoBehaviour
{
    [Header("Cấu hình Cánh tay")]
    [SerializeField] private Transform clawTransform;
    [SerializeField] private float extendSpeed = 5f;
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private LayerMask collectibleLayer;

    private Vector3 initialLocalPos;
    private bool isExtending = false;
    private GameObject grabbedObject = null;

    private void Start()
    {
        if (clawTransform != null)
            initialLocalPos = clawTransform.localPosition;
    }

    private void Update()
    {
        // Giữ Space để thò tay ra
        if (Input.GetKey(KeyCode.Space))
        {
            ExtendClaw();
        }
        else
        {
            RetractClaw();
        }

        // Nếu đã gắp được đồ, giữ nó đi theo đầu gắp
        if (grabbedObject != null)
        {
            grabbedObject.transform.position = clawTransform.position;
        }
    }

    private void ExtendClaw()
    {
        float dist = Vector3.Distance(clawTransform.localPosition, initialLocalPos);
        if (dist < maxDistance)
        {
            clawTransform.Translate(Vector3.right * extendSpeed * Time.deltaTime);
            CheckForGrab();
        }
    }

    private void RetractClaw()
    {
        float dist = Vector3.Distance(clawTransform.localPosition, initialLocalPos);
        if (dist > 0.1f)
        {
            clawTransform.Translate(Vector3.left * extendSpeed * Time.deltaTime);
        }
        else
        {
            clawTransform.localPosition = initialLocalPos;
            // Khi thu về sát tàu, coi như đã thu hoạch xong
            if (grabbedObject != null)
            {
                DeliverObject();
            }
        }
    }

    private void CheckForGrab()
    {
        if (grabbedObject != null) return;

        Collider2D hit = Physics2D.OverlapCircle(clawTransform.position, 0.5f, collectibleLayer);
        if (hit != null)
        {
            grabbedObject = hit.gameObject;
            Debug.Log("Đã gắp được: " + grabbedObject.name);
        }
    }

    private void DeliverObject()
    {
        // Gọi hàm Interact trên vật phẩm để cộng tiền/điểm
        Interactable interactable = grabbedObject.GetComponent<Interactable>();
        if (interactable != null)
        {
            interactable.Interact(gameObject); 
        }
        
        grabbedObject = null;
    }

    private void OnDrawGizmos()
    {
        if (clawTransform)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(clawTransform.position, 0.5f);
        }
    }
}
