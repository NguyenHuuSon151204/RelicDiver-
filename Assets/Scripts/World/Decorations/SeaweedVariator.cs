using UnityEngine;

public class SeaweedVariator : MonoBehaviour
{
    private void Start()
    {
        // Tạo một bản sao Material riêng cho cây này để không ảnh hưởng cây khác
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material; 
            
            // Chỉnh ngẫu nhiên tốc độ và độ uốn
            mat.SetFloat("_Speed", Random.Range(1.5f, 3f));
            mat.SetFloat("_Frequency", Random.Range(3f, 7f));
            mat.SetFloat("_Amount", Random.Range(0.05f, 0.15f));
        }
    }
}
