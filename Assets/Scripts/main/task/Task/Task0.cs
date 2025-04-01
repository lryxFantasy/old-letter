using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Task0 : TaskBase
{
    private TMP_Text dialogueText;
    private GameObject dialoguePanel;
    private Button nextButton;

    private RubyController rubyController; // 假设木童有类似控制器，可替换为其他机制
    private string[] currentDialogue;
    private int dialogueIndex = 0;
    private bool hasStarted = false;

    // 简姝儿的开场对话，基于墨家村背景
    private string[] jianShuErDialogue = new string[]
    {
        "简姝儿：你醒了？",
        "简姝儿：我花了两日修好你，用村里捡来的桑木和滑轮拼成，模样简陋，怕是不太稳，但走起来该没问题。",
        "简姝儿：听着，你是木童，明白不？",
        "简姝儿：我不知你还记不记得旧日的用处，可从今往后，你有新活计了。",
        "简姝儿：你胸前竹筒左边，有任务刻板和信。",
        "简姝儿：这儿是墨家村，地方不大。",
        "简姝儿：你的装置可能还尚不稳定，若路上耐久度损耗过多，可能会倒下。",
        "简姝儿：若损耗太大，请务必量力而行，前去村民屋中寻求帮助。",
        "简姝儿：先去村里各家走一趟，认认路。前些日子墨守提过想给墨成送封信，你去瞧瞧吧……",
        "简姝儿：别问我为啥没旁的法子，洪水断了驿站的路，《墨子》有言‘知行合一’，要救田，就得靠你这木头腿跑。",
        "简姝儿：这是我写的信，给墨守的，另外我给村里人各备了封简信。",
        "简姝儿：快去，别耽搁，干完回来找我。"
    };

    // 传送位置（可根据实际场景调整）
    private Vector3 teleportPosition = new Vector3(-7.3f, -2.5f, -6.1f);

    void Start()
    {
        // 初始化时无需额外操作，留空
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
            rubyController = FindObjectOfType<RubyController>(); // 获取木童控制器，可替换为其他机制
            if (rubyController != null)
            {
                rubyController.pauseHealthUpdate = true; // 暂停耐久度更新（假设存在类似机制）
            }
            hasStarted = true;
            currentDialogue = jianShuErDialogue;
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
        dialoguePanel.SetActive(true);
        dialogueText.text = currentDialogue[dialogueIndex];
        yield return StartCoroutine(FadeManager.Instance.FadeOut(3f)); // 从黑屏淡出
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
        if (rubyController != null)
        {
            rubyController.pauseHealthUpdate = false; // 恢复耐久度更新
        }
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
        else
        {
            Debug.LogError("TaskManager 未找到，无法切换到 Task1！");
        }
    }

    public override string GetTaskName()
    {
        return "墨家村送信初启"; // 更贴合墨家村背景的任务名
    }

    public override string GetTaskObjective()
    {
        return "与简姝儿对话"; // 简洁明了的目标
    }

    public override bool IsTaskComplete()
    {
        return dialogueIndex >= currentDialogue.Length; // 对话结束即任务完成
    }

    public override void DeliverLetter(string targetResident)
    {
        Debug.Log("Task0: 这只是初始对话任务，无法送信。");
        if (dialoguePanel != null && dialogueText != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = "简姝儿：我已经给你任务了，快去送信吧。";
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => dialoguePanel.SetActive(false));
        }
    }
}