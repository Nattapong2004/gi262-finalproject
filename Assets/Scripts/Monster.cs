using UnityEngine;

/// <summary>
/// Monster - สัตว์ประสาดที่โจมตีผู้เล่น
/// 
/// เมื่อผู้เล่นชนสัตว์ประสาด:
/// - ลดสเตมิน่าของผู้เล่นตามค่า stamina Damage
/// - อาจทำให้ผู้เล่นตายถ้าสเตมิน่าหมด
/// </summary>
public class Monster : Identity
{
    [Tooltip("ความเสียหายต่อสเตมิน่าของผู้เล่น")]
    public int staminaDamage = 5;

    [Tooltip("จำนวนครั้งที่โจมตีผู้เล่นได้ (0 = ไม่จำกัด)")]
    public int attacksRemaining = 0; // 0 = unlimited
public GameObject staminaBar;

    new void Start()
    {
        
    }

    void Update()
    {
        staminaBar.transform.localScale = new Vector3(attacksRemaining * 0.2f, 0.2f, 0.2f);
    }

    /// <summary>
    /// Hit - เรียกเมื่อผู้เล่นชนสัตว์ประสาด
    /// 
    /// - ลด stamina ของผู้เล่น
    /// - แสดง debug log
    /// - ส่งคืน false = ผู้เล่นไม่สามารถเดินผ่านได้
    /// </summary>
    public override bool Hit()
    {
        Player player = mapGenerator.player;
        if (player != null)
        {
            // ลด stamina ของผู้เล่น
            player.currentStamina -= staminaDamage;
            player.currentStamina = Mathf.Max(player.currentStamina, 0); // ไม่ให้ติดลบ

            Debug.Log($"Monster attacked! Player stamina reduced: -{staminaDamage}. Current: {player.currentStamina}/{player.maxStamina}");
        }

        // ลดจำนวนการโจมตี (ถ้ามีการจำกัด)
        if (attacksRemaining > 0)
        {
            attacksRemaining--;
            
            // ถ้าใช้การโจมตีหมดแล้ว ให้ลบสัตว์ประสาดออก
            if (attacksRemaining <= 0)
            {
                Debug.Log("Monster defeated!");
                mapGenerator.mapdata[positionX, positionY] = null;
                mapGenerator.monstersOnMap.Remove(this);
                Destroy(gameObject);
            }
        }

        return false; // ผู้เล่นไม่สามารถเดินผ่านสัตว์ประสาดได้
    }
}
