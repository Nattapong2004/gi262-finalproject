# วิธีการตั้งค่า UI Stamina และ Game Over

## ขั้นตอนที่ 1: สร้าง Canvas

1. ใน Hierarchy ให้ Right-click
2. เลือก **UI → Canvas** (หรือ **Create → UI → Canvas** ถ้า UI ยังไม่มี)
3. Canvas จะถูกสร้างพร้อม EventSystem

---

## ขั้นตอนที่ 2: สร้าง Stamina Display (Text)

### 2.1 สร้าง Text สำหรับแสดง Stamina
1. ใน Canvas ให้ Right-click
2. เลือก **UI → Text - TextMeshPro**
3. ตั้งชื่อเป็น `StaminaText`
4. ใน Inspector ตั้งค่า:
   - **Text**: `Stamina: 100/100`
   - **Font Size**: 36 หรือตามที่ต้องการ
   - **Color**: ขาว (White)
5. ใน RectTransform ตั้ง Anchor และ Position:
   - **Anchor Presets**: ซ้ายบน (Top-Left)
   - **Pos X**: 20, **Pos Y**: -20
   - **Width**: 300, **Height**: 50

---

## ขั้นตอนที่ 3: สร้าง Stamina Bar (Progress Bar)

### 3.1 สร้าง Background Panel สำหรับ Bar
1. ใน Canvas ให้ Right-click → **UI → Panel**
2. ตั้งชื่อเป็น `StaminaBarBackground`
3. ใน Inspector:
   - **Image → Source Image**: ปล่อยว่าง (ให้เป็นพื้นเทาหรือดำ)
   - **Image → Color**: เทาเข้ม (เช่น RGB: 80, 80, 80)
4. ใน RectTransform:
   - **Anchor Presets**: ซ้ายบน (Top-Left)
   - **Pos X**: 20, **Pos Y**: -80
   - **Width**: 300, **Height**: 30

### 3.2 สร้าง Fill Image สำหรับ Progress
1. ใน `StaminaBarBackground` ให้ Right-click → **UI → Image**
2. ตั้งชื่อเป็น `StaminaBarFill`
3. ใน Inspector:
   - **Image → Image Type**: **Filled** (สำคัญ!)
   - **Image → Fill Method**: Horizontal
   - **Image → Fill Origin**: Left
   - **Image → Color**: สีเขียว (RGB: 0, 255, 0)
4. ใน RectTransform:
   - **Stretch** แบบเต็ม (ตั้ง Left, Right, Top, Bottom เป็น 0)
   - หรือตั้ง **Width**: 300, **Height**: 30

---

## ขั้นตอนที่ 4: สร้าง Game Over Screen

### 4.1 สร้าง Game Over Panel
1. ใน Canvas ให้ Right-click → **UI → Panel**
2. ตั้งชื่อเป็น `GameOverPanel`
3. ใน Inspector:
   - **Image → Color**: ดำโปร่งแสง (RGB: 0, 0, 0, Alpha: 200)
4. ใน RectTransform:
   - **Anchor Presets**: **Stretch (All)**
   - Left, Right, Top, Bottom: 0 (เต็มจอ)

### 4.2 สร้าง Text สำหรับข้อความ Game Over
1. ใน `GameOverPanel` ให้ Right-click → **UI → Text - TextMeshPro**
2. ตั้งชื่อเป็น `GameOverText`
3. ใน Inspector:
   - **Text**: `GAME OVER!\nYou ran out of stamina!`
   - **Font Size**: 60
   - **Color**: แดง (RGB: 255, 0, 0)
   - **Alignment**: Center
4. ใน RectTransform:
   - **Anchor Presets**: Center
   - **Pos X**: 0, **Pos Y**: 100
   - **Width**: 800, **Height**: 200

### 4.3 สร้าง Restart Button
1. ใน `GameOverPanel` ให้ Right-click → **UI → Button - TextMeshPro**
2. ตั้งชื่อเป็น `RestartButton`
3. ใน Button:
   - ปรับข้อความภายใน (Text child) เป็น `Restart Game`
4. ใน RectTransform:
   - **Anchor Presets**: Center
   - **Pos X**: 0, **Pos Y**: -100
   - **Width**: 200, **Height**: 80

---

## ขั้นตอนที่ 5: Attach UIManager Script

1. เลือก **Canvas** GameObject
2. ใน Inspector คลิก **Add Component**
3. ค้นหา `UIManager` แล้วเพิ่มเข้ามา

---

## ขั้นตอนที่ 6: เชื่อมอ้างอิง UI Elements

### 6.1 ใน Canvas → UIManager component:

1. **Stamina Text**: 
   - ลาก `StaminaText` ไปใส่ field `Stamina Text`

2. **Stamina Bar**:
   - ลาก `StaminaBarFill` ไปใส่ field `Stamina Bar`

3. **Game Over Panel**:
   - ลาก `GameOverPanel` ไปใส่ field `Game Over Panel`

4. **Game Over Text**:
   - ลาก `GameOverText` ไปใส่ field `Game Over Text`

5. **Restart Button**:
   - ลาก `RestartButton` ไปใส่ field `Restart Button`

---

## ขั้นตอนที่ 7: ทดสอบ

1. เข้า Play Mode
2. ผู้เล่นเดิน (กด WASD)
3. ตรวจสอบว่า:
   - Text stamina อัปเดตเมื่อเดิน ✓
   - Progress bar ลดลง ✓
   - สีเปลี่ยน: เขียว → เหลือง → แดง ✓
   - เมื่อ stamina = 0 → Game Over screen ปรากฏ ✓
   - คลิก Restart → เกมรีสตาร์ท ✓

---

## เทคนิคเพิ่มเติม

### แสดง Stamina ที่ใหญ่ขึ้น
- ปรับ Font Size ใน TextMeshPro ขึ้นไป

### เปลี่ยนตำแหน่ง UI
- ตั้งค่า RectTransform เปลี่ยน Anchor และ Position

### ปรับสีของ Progress Bar
- ใน `UIManager.cs` เปลี่ยนค่า Color ใน `UpdateStaminaDisplay()`:
  ```csharp
  if (fillAmount > 0.5f)
      staminaBar.color = Color.green;  // เขียว
  else if (fillAmount > 0.25f)
      staminaBar.color = Color.yellow; // เหลือง
  else
      staminaBar.color = Color.red;    // แดง
  ```

### ตั้งค่า maxStamina
- ใน Player component ใน Inspector เปลี่ยน `Max Stamina` ค่าเริ่มต้น

### Fruit Recovery
- ใน Fruit prefab ปรับ `Stamina Recovery` เพื่อเปลี่ยนจำนวน stamina ที่ได้รับ

### Monster Damage
- ใน Monster prefab ปรับ:
  - `Stamina Damage`: ความเสียหายต่อครั้ง
  - `Attacks Remaining`: จำนวนครั้งที่โจมตี (0 = ไม่จำกัด)

---

## หากไม่เห็นการเปลี่ยนแปลง

1. ตรวจสอบว่า **Canvas** เลือก Scale Mode เป็น **Scale with Screen Size**
2. ตรวจสอบว่า RectTransform ตั้ง Anchor ถูกต้อง
3. ดูใน Console (Window → General → Console) เพื่อหา error messages
4. ตรวจสอบว่า **EventSystem** มีอยู่ใน Hierarchy

---

## เพิ่มเติม: ชอบให้ Restart Button ทำงานต้องทำอย่างไร?

1. เลือก `RestartButton`
2. ใน Inspector → Button Component → On Click ()
3. คลิก **+** เพื่อเพิ่ม event
4. ลาก **Canvas** (หรือ GameObject ที่มี UIManager) เข้าไป
5. ใน Dropdown เลือก **UIManager → RestartGame()**

หรือสามารถปล่อยให้ UIManager เชื่อม button โดยอัตโนมัติผ่าน code ได้

---

## เสร็จสิ้น! 🎮

ตอนนี้เกมของคุณมี:
✅ Stamina display text
✅ Progress bar ที่เปลี่ยนสี
✅ Game Over screen
✅ Restart functionality

สนุกกับการเล่น! 🎉
