using UnityEngine;

/// <summary>
/// Barrier - สิ่งกีดขวางที่เมื่อผู้เล่นชนจะทำให้ stamina เหลือ 0 ทันที
/// ใช้ OnTriggerEnter2D เพื่อตรวจจับการชน
/// </summary>
public class Barrier : Identity
{
    [Header("Barrier Movement")]
    public float startSpeed = 0.5f;
    public float speedIncreasePerSecond = 0.1f;
    private float currentSpeed;
    private bool hasTriggered = false;

    [Header("Trigger Options")]
    [Tooltip("Allow the barrier to trigger multiple times. If false, it will only trigger once.")]
    public bool allowRepeat = true;

    [Tooltip("Minimum seconds between repeated triggers when `allowRepeat` is true.")]
    public float repeatCooldown = 1f;

    // Tracks last trigger time for cooldown-based repeating
    private float lastTriggerTime = -Mathf.Infinity;
    [Header("Activation")]
    [Tooltip("When false the barrier will not move until activated.")]
    public bool isActive = false;

    new void Start()
    {
        currentSpeed = startSpeed;
    }

    void Update()
    {
        if (!isActive) return; // do nothing until activated
        // เพิ่มความเร็วตามเวลา
        currentSpeed += speedIncreasePerSecond * Time.deltaTime;
        // เลื่อนไปทางขวา
        transform.position += Vector3.right * currentSpeed * Time.deltaTime;
    }

    /// <summary>
    /// OnTriggerEnter2D - ตรวจจับการชนของ Collider 2D
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        // ตรวจสอบว่าชนกับ Player ไหม
        Player player = collision.GetComponent<Player>();
        
        if (player != null)
        {
            // Notify the barrier that it has been hit (updates internal state/cooldown)
            Hit();
            // Do NOT modify player's stamina here any more — Player handles its own stamina on collision.
        }
    }

    public override bool Hit()
    {
        // ถ้ามีการใช้ grid-based system ด้วย
        if (!allowRepeat)
        {
            if (hasTriggered) return true;
            hasTriggered = true;
            Debug.Log("Barrier: Grid Hit! (marked)");
            return true;
        }

        // allowRepeat == true: use cooldown to allow repeated hits
        if (Time.time - lastTriggerTime < repeatCooldown) return true;
        lastTriggerTime = Time.time;
        Debug.Log("Barrier: Grid Hit (repeat) (marked)");
        return true;
    }

    /// <summary>
    /// ResetBarrier - รีเซ็ตสถานะ internal ของ Barrier (ใช้เมื่อ reuse หรือ pool)
    /// </summary>
    public void ResetBarrier()
    {
        hasTriggered = false;
        lastTriggerTime = -Mathf.Infinity;
        currentSpeed = startSpeed;
        isActive = false;
    }

    /// <summary>
    /// ActivateBarrier - เปิดการทำงานของ barrier (เริ่มเลื่อน)
    /// </summary>
    public void ActivateBarrier()
    {
        isActive = true;
    }

    /// <summary>
    /// DeactivateBarrier - หยุดการทำงานของ barrier
    /// </summary>
    public void DeactivateBarrier()
    {
        isActive = false;
    }
}
