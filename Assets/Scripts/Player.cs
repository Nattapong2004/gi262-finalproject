using UnityEngine;

/// <summary>
/// Player - สคริปต์สำหรับควบคุมตัวละคร
/// 
/// ฟีเจอร์:
/// - ผู้เล่นสามารถเดินด้วยปุ่ม WASD
/// - ตรวจสอบการชนกับสิ่งกีดขวางและขอบเขต
/// - อัปเดตตำแหน่ง positionX, positionY และ GameObject ตำแหน่ง
/// </summary>
public class Player : Identity
{
    [Tooltip("ความเร็วในการเดิน (ระยะทางต่อเฟรม)")]
    public float moveSpeed = 0.1f;

    [Tooltip("สเตมิน่าสูงสุดของผู้เล่น")]
    public int maxStamina = 100;
    
    /// <summary>สเตมิน่าปัจจุบันของผู้เล่น (ลด 1 ทุกการเคลื่อนที่)</summary>
    public int currentStamina = 100;

    private float moveTimer = 0f;
    // Record player's start tile to detect when player moved > 2 tiles
    private int startPosX;
    private int startPosY;
    private bool barriersActivated = false;

    new void Start()
    {
        // เรียก Identity.Start() แล้ว
        base.Start();
        // ตั้งค่าสเตมิน่าเริ่มต้น
        // หากมีการเรียก RestartGame ที่ต้องการให้ผู้เล่นเริ่มต้นด้วย stamina = 0,
        // เราอ่านค่านี้จาก PlayerPrefs (ตั้งโดย UIManager.RestartGame หรืออื่น ๆ)
        int startZero = PlayerPrefs.GetInt("StartWithZeroStamina", 0);
        if (startZero == 1)
        {
            currentStamina = 0;
            PlayerPrefs.DeleteKey("StartWithZeroStamina");
        }
        else
        {
            currentStamina = maxStamina;
        }

        // store start position (SetUpPlayer in MapGenerator sets positionX/Y before Start())
        startPosX = positionX;
        startPosY = positionY;
    }

    void Update()
    {
        HandleMovement();
    }

    /// <summary>
    /// HandleMovement - จัดการการเดิน
    /// 
    /// - ตรวจจับการกดปุ่ม WASD
    /// - คำนวณตำแหน่งต่อไป
    /// - ตรวจสอบการชน
    /// - ถ้าสามารถเดินได้ ให้อัปเดตตำแหน่ง
    /// </summary>
    private void HandleMovement()
    {
        Vector2Int nextPos = new Vector2Int(positionX, positionY);
        bool canMove = false;

        // ตรวจจับการกดปุ่มและคำนวณตำแหน่งต่อไป
        if (Input.GetKeyDown(KeyCode.W))
        {
            nextPos.y += 1; // ขึ้น
            canMove = true;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            nextPos.y -= 1; // ลง
            canMove = true;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            nextPos.x -= 1; // ซ้าย
            canMove = true;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            nextPos.x += 1; // ขวา
            canMove = true;
        }

        if (!canMove) return;

        // ตรวจสอบสเตมิน่า
        if (currentStamina <= 0)
        {
            Debug.LogWarning("Player out of stamina! Cannot move.");
            return;
        }

        // ตรวจสอบขอบเขตแผนที่
        if (nextPos.x < 0 || nextPos.x >= mapGenerator.X || nextPos.y < 0 || nextPos.y >= mapGenerator.Y)
        {
            Debug.LogWarning($"Player tried to move out of bounds: ({nextPos.x}, {nextPos.y})");
            return;
        }

        // ตรวจสอบการชนกับสิ่งกีดขวาง
        Identity target = mapGenerator.GetMapData(nextPos.x, nextPos.y);
        if (target != null)
        {
            // มีสิ่งกีดขวาง - เรียก Hit() เพื่อจัดการ
            bool hitResult = target.Hit();
            if (!hitResult)
            {
                // ถ้า Hit() ส่งคืน false แสดงว่าไม่สามารถเดินได้
                Debug.LogWarning($"Player collision at ({nextPos.x}, {nextPos.y}) with {target.Name}");
                return;
            }
            // ถ้า Hit() ส่งคืน true ให้ดำเนินการเดินต่อ
        }

        // ลบผู้เล่นจากตำแหน่งเดิม
        mapGenerator.mapdata[positionX, positionY] = null;

        // อัปเดตตำแหน่ง
        positionX = nextPos.x;
        positionY = nextPos.y;

        // อัปเดตตำแหน่ง GameObject
        transform.position = new Vector3(positionX, positionY, -0.1f);

        // ลงทะเบียนผู้เล่นที่ตำแหน่งใหม่
        mapGenerator.mapdata[positionX, positionY] = this;

        // ใช้สเตมิน่า 1 ต่อการเคลื่อนที่
        currentStamina -= 1;

        Debug.Log($"Player moved to ({positionX}, {positionY}). Stamina: {currentStamina}/{maxStamina}");

        // Check distance from start; if moved more than 2 tiles activate barriers
        if (!barriersActivated)
        {
            int dx = Mathf.Abs(positionX - startPosX);
            int dy = Mathf.Abs(positionY - startPosY);
            int chebyshev = Mathf.Max(dx, dy);
            if (chebyshev > 2)
            {
                // Activate all barriers in the scene
                Barrier[] barriers = UnityEngine.Object.FindObjectsByType<Barrier>(FindObjectsSortMode.None);
                foreach (var b in barriers)
                {
                    b.ActivateBarrier();
                }
                barriersActivated = true;
                Debug.Log("Player moved away from start >2 tiles — activated barriers.");
            }
        }
    }

    /// <summary>
    /// OnTriggerEnter2D - ตรวจจับการชนของ Collider 2D
    /// เรียก Hit() ของวัตถุที่ชน
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        // หากชน Barrier ให้ Player จัดการสเตมิน่าเป็น 0
        Barrier barrier = collision.GetComponent<Barrier>();
        if (barrier != null)
        {
            Debug.Log($"Player hit Barrier: {barrier.Name}");
            // แจ้งให้ Barrier อัปเดตสถานะ internal ของมัน
            barrier.Hit();
            // Player จัดการสเตมิน่าเอง
            currentStamina = 0;
            Debug.Log($"Player stamina set to 0 by Barrier collision");
            return;
        }

        // ถ้าไม่ใช่ Barrier ให้ดึง Identity ทั่วไปและเรียก Hit()
        Identity hitObject = collision.GetComponent<Identity>();
        
        if (hitObject != null)
        {
            Debug.Log($"Player trigger with: {hitObject.Name}");
            
            // เรียก Hit() ของวัตถุ
            bool hitResult = hitObject.Hit();
            
            if (!hitResult)
            {
                Debug.LogWarning($"Player collision blocked by {hitObject.Name}");
            }
        }
    }
}
