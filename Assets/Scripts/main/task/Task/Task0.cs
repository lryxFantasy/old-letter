using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Task0 : TaskBase
{
    private TMP_Text dialogueText;
    private GameObject dialoguePanel;
    private Button nextButton;

    private RubyController rubyController;
    private string[] currentDialogue;
    private int dialogueIndex = 0;
    private bool hasStarted = false;

    // 替换为简工的开场对话，基于墨科村背景
    private string[] janeDialogue = new string[]
    {
        "简工：你醒了？",
        "简工：我花了两天修好你，用的是《墨子》杠杆原理，村里的木料和滑轮拼成的，可能不太灵活，但应该能用。",
        "简工：你是墨信，知道吗？",
        "简工：我不知道你还记不记得自己的用途，不过从现在起，你有新任务了。",
        "简工：你木匣左边有任务刻板和信件竹筒。",
        "简工：任务刻板能记录你当前的任务。",
        "简工：信件竹筒里装着收到的信，别随便打开别人的信，太失礼了。",
        "简工：这里是墨科村，村子不大，但人跟人隔得挺远。",
        "简工：瘟疫闹得厉害，大家没法出门，只能靠书信联系。",
        "简工：可惜送信不能让人亲自跑——染病的风险太高了。",
        "简工：所以得靠你。",
        "简工：去村里每个人的住处走一趟，认认路吧。之前墨守好像说想寄封信给他儿子，你可以去看看……",
        "简工：别问我为什么不用驿站——瘟疫封了路，驿卒进不来。",
        "简工：你是唯一的办法。",
        "简工：这是我写的信，给墨守的，另外我给每个村民写了封简信。",
        "简工：快去，别磨蹭，任务完了回来找我。"
    };

    // 传送位置（可根据实际场景调整）
    private Vector3 teleportPosition = new Vector3(-7.3f, -2.5f, -6.1f);

    void Start()
    {
    }

    public void SetupDialogueUI(GameObject panel, TMP_Text text, Button button)
    {
        dialoguePanel = panel;
        dialogueText = text;
        nextButton = button;
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextDialogue);
        dialoguePanel.SetActive(false);
    }

    public void StartTaskDialogue()
    {
        if (!hasStarted)
        {
            rubyController = FindObjectOfType<RubyController>(); // 获取 RubyController
            rubyController.pauseHealthUpdate = true; // 暂停血量更新（这里假设墨信有类似机制，可改为其他状态暂停）
            hasStarted = true;
            currentDialogue = janeDialogue;
            dialogueIndex = 0;
            TaskManager taskManager = GetComponent<TaskManager>();
            if (taskManager != null && taskManager.normalDialoguePanel != null)
            {
                taskManager.normalDialoguePanel.SetActive(false);
            }
            StartCoroutine(StartDialogueWithFadeOut());
        }
    }

    private IEnumerator StartDialogueWithFadeOut()
    {
        // 直接显示对话面板
        dialoguePanel.SetActive(true);
        dialogueText.text = currentDialogue[dialogueIndex];
        // 从黑屏淡出
        yield return StartCoroutine(FadeManager.Instance.FadeOut(3f));
    }

    private void NextDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex < currentDialogue.Length)
        {
            dialogueText.text = currentDialogue[dialogueIndex];
        }
        else
        {
            StartCoroutine(TransitionAndTeleport());
        }
    }

    private IEnumerator TransitionAndTeleport()
    {
        yield return StartCoroutine(FadeManager.Instance.FadeToBlack(() =>
        {
            dialoguePanel.SetActive(false);
            TeleportPlayer();
            SetupNextTask();
        }, 1f));
    }

    private void TeleportPlayer()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        rubyController = FindObjectOfType<RubyController>(); // 获取 RubyController
        rubyController.pauseHealthUpdate = false; // 恢复血量更新（可改为墨信的耐久或其他机制）
        if (playerController != null)
        {
            playerController.transform.position = teleportPosition;
            if (playerController.IsInDialogue())
            {
                playerController.EndDialogue();
            }
        }
        else
        {
            Debug.LogWarning("未找到 PlayerController，无法传送玩家！");
        }
    }

    private void SetupNextTask()
    {
        TaskManager taskManager = GetComponent<TaskManager>();

        if (taskManager != null)
        {
            Task1 newTask = gameObject.AddComponent<Task1>();
            taskManager.SetTask(newTask);
            newTask.SetupDialogueUI(dialoguePanel, dialogueText, nextButton);
            taskManager.UpdateTaskDisplay();
        }
    }

    public override string GetTaskName()
    {
        return "初次送信"; // 改为更符合墨科村背景的任务名
    }

    public override string GetTaskObjective()
    {
        return "与简工对话"; // 保持简洁
    }

    public override bool IsTaskComplete()
    {
        return dialogueIndex >= currentDialogue.Length;
    }

    public override void DeliverLetter(string targetResident)
    {
        Debug.Log("Task0: 这只是初始对话任务，无法送信。");
        if (dialoguePanel != null && dialogueText != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = "简工：我已经给你任务了，快去送信吧。";
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => dialoguePanel.SetActive(false));
        }
    }
}