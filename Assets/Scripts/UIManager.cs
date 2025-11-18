using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// UIManager - จัดการ UI สำหรับแสดง stamina และ game over
/// 
/// ฟีเจอร์:
/// - แสดง stamina เป็นข้อความ และ/หรือ progress bar
/// - ตรวจสอบสถานะเกมจบ เมื่อ stamina <= 0
/// - แสดงหน้าจอ game over
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Stamina Display")]
    [Tooltip("Text (TextMeshProUGUI) component สำหรับแสดง stamina (เช่น '100/100')")]
    public TextMeshProUGUI staminaText;

    [Tooltip("Image component สำหรับแสดง stamina bar (ตั้ง Image Type เป็น Filled)")]
    public Image staminaBar;

    [Header("Game Over")]
    [Tooltip("Panel สำหรับแสดงหน้าจอ game over")]
    public GameObject gameOverPanel;

    [Tooltip("Text (TextMeshProUGUI) สำหรับแสดงข้อความ game over")]
    public TextMeshProUGUI gameOverText;

    [Tooltip("ปุ่มสำหรับรีเซ็ตเกม")]
    public Button restartButton;

    private Player player;
    private bool isGameOver = false;

    void Start()
    {
        // หาผู้เล่น
        player = FindAnyObjectByType<Player>();
        
        if (player == null)
        {
            Debug.LogError("UIManager: ไม่พบ Player ในซีน!");
            return;
        }

        // ซ่อนหน้าจอ game over เมื่อเริ่ม
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // เชื่อม restart button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    void Update()
    {
        if (player == null || isGameOver) return;

        // อัปเดต UI stamina
        UpdateStaminaDisplay();

        // ตรวจสอบ game over
        if (player.currentStamina <= 0)
        {
            GameOver();
        }
    }

    /// <summary>
    /// UpdateStaminaDisplay - อัปเดตการแสดง stamina
    /// </summary>
    private void UpdateStaminaDisplay()
    {
        // อัปเดต text
        if (staminaText != null)
        {
            staminaText.text = $"Stamina: {player.currentStamina}/{player.maxStamina}";
        }

        // อัปเดต progress bar
        if (staminaBar != null)
        {
            float fillAmount = (float)player.currentStamina / player.maxStamina;
            staminaBar.fillAmount = fillAmount;

            // เปลี่ยนสีตามระดับ stamina (สีเหลืองเมื่อต่ำ สีแดงเมื่อวิกฤต)
            if (fillAmount > 0.5f)
            {
                staminaBar.color = Color.green;
            }
            else if (fillAmount > 0.25f)
            {
                staminaBar.color = Color.yellow;
            }
            else
            {
                staminaBar.color = Color.red;
            }
        }
    }

    /// <summary>
    /// HideStaminaUI - ซ่อน UI stamina (เมื่อเกมจบ) (public เพื่อให้เรียกจาก Dialogue/อื่น ๆ ได้)
    /// </summary>
    public void HideStaminaUI()
    {
        if (staminaText != null)
        {
            staminaText.gameObject.SetActive(false);
        }

        if (staminaBar != null)
        {
            staminaBar.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ShowStaminaUI - แสดง UI stamina (เรียกเมื่อปิดบทสนทนา)
    /// </summary>
    public void ShowStaminaUI()
    {
        if (staminaText != null)
        {
            staminaText.gameObject.SetActive(true);
        }

        if (staminaBar != null)
        {
            staminaBar.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// GameOver - เมื่อเกมจบ (stamina หมด)
    /// </summary>
    private void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f; // หยุดเกม

        Debug.Log("GAME OVER! Player out of stamina.");

        // ซ่อน stamina UI
        HideStaminaUI();

        // แสดงหน้าจอ game over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER!\nYou ran out of stamina!";
        }
    }

    /// <summary>
    /// ShowWinScreen - แสดงหน้าจอชนะเกม
    /// </summary>
    public void ShowWinScreen()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        Debug.Log("YOU WIN! Player reached the exit!");

        // ซ่อน stamina UI
        HideStaminaUI();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.text = "YOU WIN!\nYou reached the exit!";
            // เปลี่ยนสีฟอนต์เป็นสีเหลืองอ่อน
            gameOverText.color = new Color(1f, 1f, 0.5f, 1f); // RGB: 255, 255, 128 (light yellow)
        }
    }

    /// <summary>
    /// RestartGame - รีเซ็ตเกม
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f; // คืนค่าเวลา

        // If you want the restarted scene to begin with the player's stamina = 0,
        // set this PlayerPrefs flag before reloading. Player.Start() will read
        // the flag and clear it on first Start.
        // By default we do not set it here; uncomment the next line if you want
        // restarting via this button to begin with 0 stamina.
        // PlayerPrefs.SetInt("StartWithZeroStamina", 1);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
