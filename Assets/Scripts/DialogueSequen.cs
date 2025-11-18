using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSequen : MonoBehaviour
{
    public DialogueTree tree;
    public DialogueNode currentNode;
    DialogueUI dialogueUI; 

    public void Start()
    {
        // พยายามหา DialogueUI: ถ้า DialogueSequen อยู่บนตัว NPC อาจไม่มี DialogueUI บน GameObject เดียวกัน
        dialogueUI = GetComponent<DialogueUI>(); // หรือกำหนดผ่าน Inspector
        if (dialogueUI == null)
        {
            dialogueUI = FindAnyObjectByType<DialogueUI>();
            if (dialogueUI != null)
            {
                Debug.Log("DialogueSequen: พบ DialogueUI ในซีน และจะใช้โดยอัตโนมัติ");
            }
        }

        LoadConversations();
    }

    private void LoadConversations()
    {
        // NPC: โอ้! สวัสดีเพื่อนยาก นายดูสับสนนะ มีอะไรจะถามฉันไหม?
        //     |
        //     +-- [1] ผมอยากไปหาแม่ฮะ? พี่รู้ทางไหม?
        //     |       |
        //     |       +-- NPC: ง่ายมาก นายก็แค่วิ่งไปทางขวาเรื่อย ๆ แม่นายอาจจะรออยู่ปลายทางด้านบนก็ได้
        //     |
        //     +-- [2] แล้วถ้าผมหลงทางละฮะ?
        //     |       |
        //     |       +-- NPC: นายเหลือเวลาไม่มากแล้ว อาจจะไม่ได้เจอแม่นายอีกก็ได้ ทุกครั้งที่เดินนายจะหมดแรง(Stamina:-1)คิดให้ดีทุกครั้งก่อนจะเดิน แต่อย่าชักช้าละ
        //     |
        //     +-- [3] คนชุดดำนั่นคืออะไรฮะ?
        //     |       |
        //     |       +-- NPC: พวกนั้นจะพรากนายไปจากแม่ตลอดกาล ถ้ายังไม่อยากแยกจากครอบครัวก็หลบพวกนั้นให้ดี แต่ถ้าไม่มีทางเลือกจะจู่โจมก็ได้นะ ซึ่งนายจะหมดแรงเร็วขึ้นมาก
        //     |
        //     +-- [4] หมอกฝั่งซ้ายคืออะไรหรอฮะ?
        //     |       |
        //     |       +-- NPC: มันคือหมอกแห่งความตาย นายไม่รู้หรอกว่าความตายคืออะไร แต่มันจะพรากนายไปจากแม่ของนาย มันจะไล่ตามนายมาเรื่อย ๆ วิ่งหนีเร็ว ๆ ละ    
        //     |
        //     +-- [5] ขอบคุณมากฮะ
        //             |
        //             +-- NPC: ขอให้ได้เจอแม่นะเจ้าหนู

        // 3. สร้าง Dialogue Node (โหนดของบทสนทนา)
        DialogueNode greeting = new DialogueNode("โอ้! สวัสดีเพื่อนยาก นายดูสับสนนะ มีอะไรจะถามฉันไหม?");
        DialogueNode askForWin = new DialogueNode("ง่ายมาก นายก็แค่วิ่งไปทางขวาเรื่อย ๆ แม่นายอาจจะรออยู่ปลายทางด้านบนก็ได้");
        DialogueNode askForLost = new DialogueNode("นายเหลือเวลาไม่มากแล้ว อาจจะไม่ได้เจอแม่นายอีกก็ได้ ทุกครั้งที่เดินนายจะหมดแรง(Stamina:-1)คิดให้ดีทุกครั้งก่อนจะเดิน แต่อย่าชักช้าละ");
        DialogueNode askForMonster = new DialogueNode("พวกนั้นจะพรากนายไปจากแม่ตลอดกาล ถ้ายังไม่อยากแยกจากครอบครัวก็หลบพวกนั้นให้ดี แต่ถ้าไม่มีทางเลือกจะจู่โจมก็ได้นะ ซึ่งนายจะหมดแรงเร็วขึ้นมาก");
        DialogueNode askForBarrier = new DialogueNode("มันคือหมอกแห่งความตาย นายไม่รู้หรอกว่าความตายคืออะไร แต่มันจะพรากนายไปจากแม่ของนาย มันจะไล่ตามนายมาเรื่อย ๆ วิ่งหนีเร็ว ๆ ละ");
        DialogueNode goodbye = new DialogueNode("ขอให้ได้เจอแม่นะเจ้าหนู");

        // 4. สร้างโครงสร้างต้นไม้บทสนทนา และเพิ่มคำตอบ/เส้นทางต่าง ๆ

        greeting.AddNext(askForWin, "ผมอยากไปหาแม่ฮะ? พี่รู้ทางไหม?");
        greeting.AddNext(askForLost, "แล้วถ้าผมหลงทางละฮะ?");
        greeting.AddNext(askForMonster, "คนชุดดำนั่นคืออะไรฮะ?");
        greeting.AddNext(askForBarrier, "หมอกฝั่งซ้ายคืออะไรหรอฮะ?");
        greeting.AddNext(goodbye, "ขอบคุณมากฮะ");

        // 5. ตั้งค่า root ของต้นไม้บทสนทนา
        tree = new DialogueTree(greeting);
    }

    // **เมธอดใหม่สำหรับรับการเลือกจากปุ่ม UI**
    public void SelectChoice(int index)
    {
        var choiceTextKeys = new List<string>(currentNode.nexts.Keys);

        if (index >= 0 && index < choiceTextKeys.Count)
        {
            string choiceKey = choiceTextKeys[index];

            // 1. เลื่อนไปยัง Dialogue Node ถัดไป
            currentNode = currentNode.nexts[choiceKey];

            // 2. ตรวจสอบว่ามีตัวเลือกถัดไปหรือไม่ (จบการสนทนา)
            // หาตัว DialogueUI ที่จะใช้แสดง หากฟิลด์ยังเป็น null ให้ลองหาในซีน
            DialogueUI targetUI = dialogueUI ?? FindAnyObjectByType<DialogueUI>();
            if (targetUI == null)
            {
                Debug.LogWarning("DialogueSequen: ไม่พบ DialogueUI สำหรับอัปเดตบทสนทนา");
                return;
            }

            if (currentNode.nexts.Count > 0)
            {
                targetUI.ShowDialogue(currentNode); // แสดง Node ถัดไป
            }
            else
            {
                // ถ้าไม่มีตัวเลือกถัดไป ถือว่าจบบทสนทนา
                targetUI.ShowDialogue(currentNode);   // แสดงข้อความสุดท้าย
                targetUI.ShowCloseButtonDialog();    // แสดงปุ่มปิดบทสนทนา
            }
        }
    }
}

