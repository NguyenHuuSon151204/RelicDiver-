using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class VignetteGenerator : MonoBehaviour
{
    [Header("Cấu hình Vignette")]
    [Range(0.1f, 1f)]
    [SerializeField] private float smoothness = 0.5f; // Độ mờ của viền
    [SerializeField] private Color vignetteColor = Color.red;

    private void Awake()
    {
        GenerateVignette();
    }

    [ContextMenu("Generate Vignette")]
    public void GenerateVignette()
    {
        Image image = GetComponent<Image>();
        
        // Tạo một Texture mới (256x256 là đủ mượt và nhẹ)
        int size = 256;
        Texture2D texture = new Texture2D(size, size);
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxDistance = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01((dist / maxDistance - (1f - smoothness)) / smoothness);
                
                // Màu sắc gốc của Vignette với Alpha tính toán được
                Color pixelColor = vignetteColor;
                pixelColor.a = alpha;
                
                texture.SetPixel(x, y, pixelColor);
            }
        }

        texture.Apply();

        // Chuyển Texture thành Sprite và gắn cho Image
        Sprite vignetteSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        image.sprite = vignetteSprite;
        image.color = Color.white; // Đảm bảo màu gốc của Image là trắng để không đè màu của Sprite
    }
}
