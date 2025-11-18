# วิธีการตั้งค่า Win Screen UI - แบบละเอียด

## ภาพรวม
ในระบบปัจจุบัน Win Screen และ Game Over Screen ใช้ **UI Panel เดียวกัน** — เพียงแค่เปลี่ยนข้อความจาก "GAME OVER!" เป็น "YOU WIN!" เท่านั้น

---

## ขั้นตอนที่ 1: ตรวจสอบ Game Over UI ที่มีอยู่

### 1.1 ตรวจสอบโครงสร้าง Hierarchy ปัจจุบัน
ถ้าคุณทำตาม `UI_SETUP_GUIDE.md` แล้ว คุณควรมี:

```
Canvas
├── StaminaText (TextMeshProUGUI)
├── StaminaBarBackground (Panel)
│   └── StaminaBarFill (Image - Filled)
└── GameOverPanel (Panel - ครอบ Game Over UI)
    ├── GameOverText (TextMeshProUGUI)
    └── RestartButton (Button - TextMeshPro)
```

### 1.2 ตรวจสอบ Canvas ที่มี UIManager
- Select Canvas GameObject
- ใน Inspector ดู UIManager component
- ตรวจสอบ field เหล่านี้ได้รับการกำหนดหรือไม่:
  - ✅ **Stamina Text** → StaminaText
  - ✅ **Stamina Bar** → StaminaBarFill
  - ✅ **Game Over Panel** → GameOverPanel
  - ✅ **Game Over Text** → GameOverText
  - ✅ **Restart Button** → RestartButton

---

## ขั้นตอนที่ 2: ข้อมูลโดยละเอียด - Win Screen ทำงานอย่างไร

### 2.1 เมื่อผู้เล่นชนะ (Player Reaches Exit)

**ในโค้ด C# (Exit.cs)**:
```csharp
public override bool Hit()
{
    Debug.Log("Player reached EXIT! YOU WIN!");
    
    // ... เพิ่ม stamina bonus
    
    // เรียก OnExitReached() → ต้องการให้ UIManager แสดง win screen
    OnExitReached();
    
    return true;
}

private void OnExitReached()
{
    UIManager uiManager = FindAnyObjectByType<UIManager>();
    if (uiManager != null)
    {
        // ✅ เรียกฟังก์ชัน ShowWinScreen() 
        // ฟังก์ชันนี้จะแสดง "YOU WIN!" บน panel เดียวกับ game over
        uiManager.ShowWinScreen();
    }
}
```

### 2.2 UIManager.ShowWinScreen() ทำงาน

**ในโค้ด C# (UIManager.cs)**:
```csharp
public void ShowWinScreen()
{
    // 1. ตั้งค่า flag
    isGameOver = true;  // ⚠️ เอาไว้บังคับให้ Update() ไม่ทำงาน
    
    // 2. หยุดเวลา
    Time.timeScale = 0f;  // ⏸️ เกมจะหยุดทั้งหมด

    Debug.Log("YOU WIN! Player reached the exit!");

    // 3. เปิด Panel (ใช้ panel เดียวกับ game over)
    if (gameOverPanel != null)
    {
        gameOverPanel.SetActive(true);
    }

    // 4. เปลี่ยนข้อความเป็น "YOU WIN!"
    if (gameOverText != null)
    {
        gameOverText.text = "YOU WIN!\nYou reached the exit!";
    }
    
    // ✅ Restart Button ยังเชื่อมต่อกับ RestartGame() เหมือนเดิม
    // ผู้เล่นคลิก → โหลดซีนใหม่ → เกมเริ่มใหม่
}
```

---

## ขั้นตอนที่ 3: ตั้งค่า UI Elements (ละเอียด)

### สิ่งที่ต้องทำ:
✅ **ไม่ต้องสร้าง UI ใหม่** — ใช้ `GameOverPanel` เดิมได้เลย  
✅ **ข้อความจะเปลี่ยนโดยอัตโนมัติ** — Exit.cs เรียก `ShowWinScreen()` ซึ่งเปลี่ยนข้อความ

---

## ขั้นตอนที่ 4: ตัวอย่างการไหลของข้อความ UI

### 4.1 ตัวอย่างที่ 1: ผู้เล่นเล่นปกติจนจบ
```
🎮 เกมเริ่มต้น
  ↓
📊 StaminaText แสดง "Stamina: 100/100"
📊 StaminaBar แสดงเขียว
  ↓
👾 ผู้เล่นเดินไปเก็บผลไม้ → stamina เพิ่มขึ้น
👾 ผู้เล่นชนมอนสเตอร์ → stamina ลดลง
  ↓
🏁 ผู้เล่นเดินไปถึง Exit
  ↓
⏸️ Time.timeScale = 0 (เกมหยุด)
📋 GameOverPanel เปิด
📝 GameOverText เปลี่ยนเป็น: "YOU WIN!\nYou reached the exit!"
🎵 Colors หรือ effects อาจมีการเปลี่ยนแปลง (ถ้าต้องการ)
  ↓
🔘 ผู้เล่นคลิก "Restart" button
  ↓
↩️ RestartGame() → โหลดซีนใหม่
```

### 4.2 ตัวอย่างที่ 2: ผู้เล่นหมด stamina (ตายก่อน)
```
🎮 เกมเริ่มต้น
  ↓
📊 StaminaText แสดง "Stamina: 50/100" (หลังจากเดินหลายครั้ง)
📊 StaminaBar แสดงเหลือง/แดง
  ↓
💀 Stamina กลายเป็น 0
  ↓
⏸️ Time.timeScale = 0 (เกมหยุด)
📋 GameOverPanel เปิด
📝 GameOverText เปลี่ยนเป็น: "GAME OVER!\nYou ran out of stamina!"
  ↓
🔘 ผู้เล่นคลิก "Restart" button
  ↓
↩️ RestartGame() → โหลดซีนใหม่
```

---

## ขั้นตอนที่ 5: ตัวเลือก - เพิ่มตกแต่ง Win Screen (Optional)

### 5.1 เปลี่ยนสีพื้นหลัง
ถ้าต้องการให้ Win Screen มีสีต่างจาก Game Over:

**ตั้งค่าใน Exit.cs:**
```csharp
private void OnExitReached()
{
    UIManager uiManager = FindAnyObjectByType<UIManager>();
    if (uiManager != null)
    {
        // ❌ ตัวเลือก A (ถ้าต้องการสีต่างจาก game over)
        // uiManager.SetWinScreenColor(new Color(0, 1, 0, 200)); // เขียว
        
        // ✅ ตัวเลือก B (ใช้เดิม game over panel)
        uiManager.ShowWinScreen();
    }
}
```

### 5.2 เพิ่มเอฟเฟกต์ (Animation)
**ตัวเลือก: ทำให้ Win Text ขึ้นมาค่อยๆ**

แก้ไข `UIManager.ShowWinScreen()`:
```csharp
public void ShowWinScreen()
{
    isGameOver = true;
    Time.timeScale = 0f;

    if (gameOverPanel != null)
    {
        gameOverPanel.SetActive(true);
        
        // ❌ ตัวเลือก: เพิ่ม animation
        // CanvasGroup cg = gameOverPanel.GetComponent<CanvasGroup>();
        // if (cg != null) cg.alpha = 0; // ซ่อน
        // StartCoroutine(FadeInPanel(cg)); // ค่อยๆแสดง
    }

    if (gameOverText != null)
    {
        gameOverText.text = "YOU WIN!\nYou reached the exit!";
    }
}

// ❌ ตัวเลือก: Fade-in effect
// IEnumerator FadeInPanel(CanvasGroup cg)
// {
//     float elapsed = 0f;
//     float duration = 1f;
//     while (elapsed < duration)
//     {
//         elapsed += Time.deltaTime;
//         cg.alpha = Mathf.Clamp01(elapsed / duration);
//         yield return null;
//     }
//     cg.alpha = 1f;
// }
```

---

## ขั้นตอนที่ 6: ทดสอบ Win Screen

### 6.1 ทดสอบในเกม (Play Mode)
1. **Start Play Mode** (กด Play)
2. **เดินไปหาทางออก** (Exit อยู่มุมบน X-1, Y-1 หรือตำแหน่งที่ตั้ง `exitStartPos`)
3. **ตรวจสอบ**:
   - ✅ เกมหยุด (Time.timeScale = 0)
   - ✅ Panel ปรากฏ (สีดำโปร่งแสง)
   - ✅ ข้อความเป็น "YOU WIN!"
   - ✅ Restart Button โผล่มา
   - ✅ คลิก Restart → เกมเริ่มใหม่

### 6.2 Debug ในโค้ด (ถ้าไม่ทำงาน)
**Check Console ว่าลอกอะไร**:
- ✅ "Player reached EXIT! YOU WIN!" ปรากฏ
- ✅ "YOU WIN! Player reached the exit!" ปรากฏ
- ❌ Error: `'UIManager' does not contain a definition for ShowWinScreen`
  → ต้องแน่ใจว่า `ShowWinScreen()` เพิ่มเข้า `UIManager.cs` แล้ว

---

## ขั้นตอนที่ 7: Checklist ก่อนถือว่า "เสร็จสิ้น"

### ไฟล์โค้ด
- ✅ `Exit.cs` มี `Hit()` method
- ✅ `Exit.cs` มี `OnExitReached()` method
- ✅ `UIManager.cs` มี `ShowWinScreen()` method
- ✅ ไม่มี compile errors

### ตั้งค่า Inspector
- ✅ Canvas มี UIManager component
- ✅ UIManager ได้รับ GameOverPanel, GameOverText, RestartButton
- ✅ Exit Prefab ได้รับการวาง (MapGenerator วาง exit)

### ทดสอบการเล่น
- ✅ ผู้เล่นเดินไปได้ (WASD)
- ✅ Stamina ลดลงเมื่อเดิน
- ✅ ผลไม้ฟื้นฟูสมดุล stamina
- ✅ มอนสเตอร์ลดลง stamina
- ✅ ไปถึง exit → "YOU WIN!" ปรากฏ
- ✅ Restart button ทำงาน → เกมเริ่มใหม่

---

## Q&A

**Q: ทำไม Win Screen และ Game Over Screen เหมือนกัน?**  
A: เพื่อประหยัด UI — ทั้งสองใช้ Panel เดียวกัน เพียงแค่เปลี่ยนข้อความ

**Q: สามารถเพิ่มปุ่ม "Main Menu" ใน Win Screen ได้ไหม?**  
A: ได้ — เพิ่มปุ่มใหม่ใน GameOverPanel แล้วเชื่อมกับฟังก์ชัน `LoadMainMenu()`

**Q: Stamina Bonus ทำงานตอนชนะไหม?**  
A: ใช่ — ถ้าตั้ง `staminaBonus > 0` ใน Exit component

**Q: ถ้าผู้เล่นชนะแล้วคลิก Restart หลายครั้ง จะเป็นไรไหม?**  
A: ปลอดภัย — โค้ด `Time.timeScale = 0` ทำให้ผู้เล่นคลิก Restart ได้ครั้งเดียวเท่านั้น

---

## โค้ดหลัก (สำหรับอ้างอิง)

### Exit.cs - ส่วนสำคัญ
```csharp
public override bool Hit()
{
    Debug.Log("Player reached EXIT! YOU WIN!");
    
    if (mapGenerator != null && mapGenerator.player != null)
    {
        Player player = mapGenerator.player;
        if (staminaBonus > 0)
        {
            player.currentStamina = Mathf.Min(
                player.currentStamina + staminaBonus,
                player.maxStamina
            );
            Debug.Log($"Stamina bonus: +{staminaBonus}");
        }
    }

    OnExitReached();
    return true;
}

private void OnExitReached()
{
    UIManager uiManager = FindAnyObjectByType<UIManager>();
    if (uiManager != null)
    {
        uiManager.ShowWinScreen();
    }
    else
    {
        Time.timeScale = 0f;
        Debug.Log("GAME WON! Player reached the exit!");
    }
}
```

### UIManager.cs - ส่วนสำคัญ
```csharp
public void ShowWinScreen()
{
    isGameOver = true;
    Time.timeScale = 0f;

    Debug.Log("YOU WIN! Player reached the exit!");

    if (gameOverPanel != null)
    {
        gameOverPanel.SetActive(true);
    }

    if (gameOverText != null)
    {
        gameOverText.text = "YOU WIN!\nYou reached the exit!";
    }
}
```

---

## ภาพรวมการไหลข้อมูล (Sequence Diagram)

```
Player           Exit.cs          UIManager         GameOverPanel
  │                 │                 │                   │
  │─ walks to ────→ │                 │                   │
  │  exit tile      │                 │                   │
  │                 │                 │                   │
  │ ← Hit() ────────│                 │                   │
  │  returns true   │                 │                   │
  │                 │                 │                   │
  │                 │─ FindUIManager→│                   │
  │                 │                 │                   │
  │                 │─ ShowWinScreen()│                   │
  │                 │                 │                   │
  │                 │                 │─ SetActive(true)─→│
  │                 │                 │                   │ (appears)
  │                 │                 │                   │
  │                 │                 │─ Change Text ────→│
  │                 │                 │  "YOU WIN!"        │
  │                 │                 │                   │
  │← sees "YOU WIN!" panel ─────────────────────────────→│
  │                 │                 │                   │
  │─ clicks ───────────────────────────────────────────→│
  │  Restart                                    Restart
  │                 │                 │        button
  │                 │                 │         │
  │                 │                 │← OnClick──
  │                 │                 │
  │                 │                 │─ RestartGame()
  │                 │                 │   - Time.scale=1
  │                 │                 │   - LoadScene()
  │                 │                 │
  │← Scene Reloads ─────────────────────────────────────→
  │                 │                 │                   │
  V                 V                 V                   V
  (new game)    (new instance)    (fresh)           (hidden)
```

---

## เสร็จสิ้น! 🎮✨

Win Screen ตั้งค่าเสร็จแล้ว — ผู้เล่นสามารถชนะเกมและเล่นต่อได้!
