using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogues/DialogueData")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;    // Tên người nói
        [TextArea(3, 10)]
        public string content;          // Nội dung câu nói
        public Sprite characterIcon;    // Ảnh đại diện
        public bool isRightSide;        // Xuất hiện bên Phải (nếu false là bên Trái)
    }

    public DialogueLine[] lines;        // Danh sách các câu thoại
}
