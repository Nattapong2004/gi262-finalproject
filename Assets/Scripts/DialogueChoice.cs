using UnityEngine;

/// <summary>
/// DialogueChoice - helper component to attach to choice button prefabs.
/// Allows designers to wire the button's OnClick() in the Inspector to call
/// this component, which forwards the click to the active DialogueUI.
/// </summary>
public class DialogueChoice : MonoBehaviour
{
    [Tooltip("Index ของตัวเลือกตามที่ DialogueSequen คาดหวัง")]
    public int choiceIndex = 0;

    [Tooltip("DialogueUI ที่จะรับการคลิก (กำหนดโดย DialogueUI เมื่อ Instantiate ปุ่ม)")]
    public DialogueUI dialogueUI;

    /// <summary>
    /// OnClick() - เรียกจาก Button.OnClick ใน Inspector
    /// </summary>
    public void OnClick()
    {
        if (dialogueUI != null)
        {
            dialogueUI.OnChoiceButtonClicked(choiceIndex);
        }
        else
        {
            Debug.LogWarning($"DialogueChoice: ไม่มี DialogueUI กำหนดไว้ (choice={choiceIndex})");
        }
    }
}
