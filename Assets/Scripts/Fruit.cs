using UnityEngine;

/// <summary>
/// Fruit - ไอเท็มผลไม้ที่ผู้เล่นสามารถเก็บได้
/// 
/// เมื่อผู้เล่นไปเก็บผลไม้:
/// - ฟื้นตัว stamina ตามค่า staminaRecovery
/// - ลบตัวเองออกจากแผนที่
/// </summary>
public class Fruit : Identity
{
    [Tooltip("จำนวน stamina ที่ฟื้นตัวเมื่อเก็บ")]
    public int staminaRecovery = 20;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Hit - เรียกเมื่อผู้เล่นไปเก็บผลไม้
    /// </summary>
    public override bool Hit()
    {
        // หาผู้เล่นจาก mapGenerator
        Player player = mapGenerator.player;
        if (player != null)
        {
            // เพิ่ม stamina (ไม่เกิน maxStamina)
            player.currentStamina = Mathf.Min(player.currentStamina + staminaRecovery, player.maxStamina);
            Debug.Log($"Player collected fruit! Stamina recovered: +{staminaRecovery}. Current: {player.currentStamina}/{player.maxStamina}");
        }

        // ลบผลไม้ออกจากแผนที่
        mapGenerator.mapdata[positionX, positionY] = null;
        Destroy(gameObject);

        return true; // ผู้เล่นสามารถเดินผ่านได้
    }
}
