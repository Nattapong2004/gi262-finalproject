using UnityEngine;

/// <summary>
/// Exit - ทางออกของเกม
/// เมื่อผู้เล่นไปถึงทางออก เกมจะแสดง "YOU WIN!" และให้เล่นใหม่
/// </summary>
public class Exit : Identity
{
    [Tooltip("Stamina recovery when reaching exit (0 = no recovery)")]
    public int staminaBonus = 10;

    public override bool Hit()
    {
        Debug.Log("Player reached EXIT! YOU WIN!");
        
        // ดึง Player จาก mapGenerator
        if (mapGenerator != null && mapGenerator.player != null)
        {
            Player player = mapGenerator.player;
            
            // เพิ่ม stamina bonus ถ้ามี
            if (staminaBonus > 0)
            {
                player.currentStamina = Mathf.Min(
                    player.currentStamina + staminaBonus,
                    player.maxStamina
                );
                Debug.Log($"Stamina bonus: +{staminaBonus}");
            }
        }

        // เรียก win condition
        OnExitReached();
        
        // ผู้เล่นสามารถเดินเข้าทางออกได้
        return true;
    }

    /// <summary>
    /// OnExitReached - เมื่อผู้เล่นถึงทางออก
    /// </summary>
    private void OnExitReached()
    {
        // ถ้ามี UIManager ให้แสดงหน้าจอชนะ
        UIManager uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowWinScreen();
        }
        else
        {
            // Fallback: หยุดเกมและแสดง log
            Time.timeScale = 0f;
            Debug.Log("GAME WON! Player reached the exit!");
        }
    }
}
