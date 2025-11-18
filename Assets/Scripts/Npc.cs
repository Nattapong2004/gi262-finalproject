using UnityEngine;

/// <summary>
/// NPC - ตัวละคร NPC ที่สามารถคุยกับผู้เล่นได้
/// เมื่อผู้เล่นชน NPC จะเรียก `Hit()` เพื่อเริ่มบทสนทนา (ถ้าอนุญาต)
/// </summary>
public class NPC : Identity
{
    [Tooltip("UI สำหรับแสดงบทสนทนา")]
    public DialogueUI dialogueUI;

    [Tooltip("ลำดับบทสนทนา (Dialogue sequence)")]
    public DialogueSequen sequen;

    [Tooltip("กำหนดว่า NPC นี้สามารถคุยกับผู้เล่นได้หรือไม่")]
    public bool canTalk = true;

    /// <summary>
    /// Hit - เรียกเมื่อผู้เล่นชน NPC
    /// ถ้า `canTalk` เป็นจริง จะตั้งค่าบทสนทนาใน UI และคืนค่า false
    /// (คืนค่า false เพื่ออนุญาตให้ผู้เล่นยังคงย้ายเข้าตำแหน่งได้ถ้าต้องการ)
    /// </summary>
    public override bool Hit()
    {
        if (!canTalk)
        {
            Debug.Log("NPC: ไม่จำเป็นต้องคุย");
            return false;
        }

        // If dialogueUI not assigned in inspector, try to find one in the scene
        if (dialogueUI == null)
        {
            dialogueUI = FindAnyObjectByType<DialogueUI>();
            if (dialogueUI == null)
            {
                Debug.LogWarning($"NPC ({Name}): ไม่มี DialogueUI กำหนดไว้ และไม่พบ DialogueUI ในซีน");
                return false;
            }
            else
            {
                Debug.Log($"NPC ({Name}): หา DialogueUI ในซีนเจอและใช้โดยอัตโนมัติ");
            }
        }

        // If sequen not assigned, try to find a DialogueSequen on this NPC GameObject
        if (sequen == null)
        {
            sequen = GetComponent<DialogueSequen>();
            if (sequen == null)
            {
                // as a last resort try to find any DialogueSequen in the scene
                sequen = FindAnyObjectByType<DialogueSequen>();
            }

            if (sequen == null)
            {
                Debug.LogWarning($"NPC ({Name}): ไม่มี DialogueSequen กำหนดไว้ และไม่พบ DialogueSequen ในซีน");
                return false;
            }
            else
            {
                Debug.Log($"NPC ({Name}): หา DialogueSequen เจอและใช้โดยอัตโนมัติ");
            }
        }

        // ปลอดภัย: ทั้ง dialogueUI และ sequen ถูกกำหนดแล้ว
        try
        {
            dialogueUI.Setup(sequen);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"NPC ({Name}): เกิดข้อผิดพลาดขณะตั้งค่าบทสนทนา: {ex}");
        }

        return false;
    }
}