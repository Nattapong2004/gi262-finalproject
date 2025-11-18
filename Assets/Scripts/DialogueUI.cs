using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI npcText;
    public Transform choiceContainer;
    public Button choiceButtonPrefab; // ลาก Prefab ปุ่มตัวเลือกมาใส่
    public GameObject closeButtonDialogue;
    private DialogueSequen InteractNpcSequen;

    //เก็บปุ่มที่ถูกสร้างขึ้น เพื่อนำไปทำลาย/ซ่อนในภายหลัง
    private List<Button> activeButtons = new List<Button>();
    private float previousTimeScale = 1f;

    void Awake()
    {
        // ซ่อน UI เริ่มต้นไว้ เพื่อให้แสดงเฉพาะเมื่อมีการเรียก Setup() จาก NPC
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        if (closeButtonDialogue != null)
        {
            closeButtonDialogue.SetActive(false);
        }
    }

    public void Setup(DialogueSequen sequen)
    {
        //1. Set Dialogue Sequen
        InteractNpcSequen = sequen;
        DialogueNode currentNode = InteractNpcSequen.tree.root;
        // Show UI: activate panel first so layout/graphics are ready before creating buttons
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

        if (closeButtonDialogue != null)
        {
            closeButtonDialogue.SetActive(false);
        }

        // Ensure an EventSystem exists so UI buttons receive input
        if (EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem));
            // Try to add InputSystemUIInputModule if available (new Input System), otherwise use StandaloneInputModule
            var added = false;
            var asm = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var a in asm)
            {
                var t = a.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
                if (t != null)
                {
                    es.AddComponent(t);
                    added = true;
                    break;
                }
            }
            if (!added)
            {
                es.AddComponent<StandaloneInputModule>();
            }
            DontDestroyOnLoad(es);
        }

        ShowDialogue(currentNode);

        // ซ่อน Stamina UI ของผู้เล่นขณะแสดง Dialogue (ถ้ามี UIManager ในซีน)
        var uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.SetStaminaUIActive(false);
        }

        // หยุดเกม (pause) ขณะแสดงบทสนทนา
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    public void ShowDialogue(DialogueNode node)
    {
        // 2.set ให้เป็น โหนดปัจจุบัน
        InteractNpcSequen.currentNode = node;
        // 3.แสดงข้อควาาม NPC
        if (npcText != null)
        {
            npcText.text = node.text;
        }
        else
        {
            Debug.LogWarning("DialogueUI: ไม่มี `npcText` กำหนดไว้ใน Inspector");
        }
        // 4.ล้างปุ่มตัวเลือกเก่า
        ClearChoices();
        // 5.สร้างปุ่มตัวเลือกใหม่ตาม next
        var choies = new List<string>(node.nexts.Keys);
        for (int i = 0; i < choies.Count; i++)
        {
            string choiceText = choies[i];
            CreateChoiceButton(choiceText,i);
        }
   
    }

    private void CreateChoiceButton(string text, int index)
    {
        // Defensive checks to avoid NullReferenceException when references are missing
        if (choiceButtonPrefab == null)
        {
            Debug.LogError("DialogueUI: ไม่มี `choiceButtonPrefab` กำหนดไว้ (Inspector) — ไม่สามารถสร้างปุ่มตัวเลือก");
            return;
        }

        if (choiceContainer == null)
        {
            Debug.LogWarning("DialogueUI: `choiceContainer` ยังไม่ถูกกำหนด — ใช้ GameObject current เป็น parent แทน");
            choiceContainer = this.transform;
        }

        Button newButton = null;
        try
        {
            newButton = Instantiate(choiceButtonPrefab, choiceContainer);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"DialogueUI: ข้อผิดพลาดขณะ Instantiate ปุ่มตัวเลือก: {ex}");
            return;
        }

        // ตั้งค่าข้อความบนปุ่ม — หาก Prefab ไม่มี TextMeshProUGUI ให้สร้างสำรองไว้
        TextMeshProUGUI textComp = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp == null)
        {
            // พยายามเพิ่ม TextMeshProUGUI ลงบนปุ่มเป็น fallback
            Debug.LogWarning("DialogueUI: ปุ่มตัวเลือกไม่มี TextMeshProUGUI ภายใน Prefab — สร้าง component สำรอง");
            textComp = newButton.gameObject.GetComponent<TextMeshProUGUI>();
            if (textComp == null)
            {
                textComp = newButton.gameObject.AddComponent<TextMeshProUGUI>();
            }
        }

        if (textComp != null)
        {
            textComp.text = text;
            // ensure text can receive raycasts if needed (usually not necessary)
            textComp.raycastTarget = true;
        }

        // Ensure the button has a target graphic (Image) to receive raycasts
        UnityEngine.UI.Image img = newButton.GetComponent<UnityEngine.UI.Image>();
        if (img == null)
        {
            img = newButton.gameObject.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(1,1,1,0); // invisible but raycastable
        }
        img.raycastTarget = true;
        // assign targetGraphic if missing
        try
        {
            if (newButton.targetGraphic == null) newButton.targetGraphic = img;
        }
        catch { }

        // เพิ่ม Listener เพื่อกดปุ่ม
        // ใช้ Lambda Expression เพื่อส่ง index กลับไปให้ DialogueManager
        newButton.onClick.AddListener(() => OnChoiceSelected(index));

        // หากปุ่มมี component `DialogueChoice` ให้ตั้งค่า เพื่อให้สามารถใช้ Button.OnClick ใน Inspector ได้
        DialogueChoice dc = newButton.GetComponent<DialogueChoice>();
        if (dc == null)
        {
            dc = newButton.gameObject.AddComponent<DialogueChoice>();
        }
        dc.choiceIndex = index;
        dc.dialogueUI = this;

        // Ensure the button is interactable and raycast target is enabled
        try
        {
            newButton.interactable = true;
        }
        catch { }

        activeButtons.Add(newButton);
        // Ensure the instantiated button is active and layout is rebuilt so it appears/can receive clicks
        newButton.gameObject.SetActive(true);
        var rt = choiceContainer as RectTransform;
        if (rt != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }

    private void ClearChoices()
    {
        foreach (Button button in activeButtons)
        {
            Destroy(button.gameObject);
        }
        activeButtons.Clear();
    }

    private void OnChoiceSelected(int index)
    {
        // ส่ง index ตัวเลือกที่ผู้เล่นเลือกกลับไปให้ DialogueManager จัดการ
        InteractNpcSequen.SelectChoice(index);
    }

    /// <summary>
    /// Public wrapper so Button.OnClick(in Inspector) can call this directly.
    /// </summary>
    public void OnChoiceButtonClicked(int index)
    {
        OnChoiceSelected(index);
    }
    public void ShowCloseButtonDialog() {
        closeButtonDialogue.gameObject.SetActive(true);
    }
    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        ClearChoices();

        // คืนค่า Stamina UI ให้กลับมาแสดงอีกครั้ง
        var uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.SetStaminaUIActive(true);
        }

        // คืนค่าเวลา (unpause) เมื่อปิดบทสนทนา
        Time.timeScale = previousTimeScale;
    }
}
