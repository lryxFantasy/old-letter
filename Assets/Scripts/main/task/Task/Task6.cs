using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task6 : TaskBase
{
    private bool letterDeliveredToMoShou = false; // 是否送达卢氏给墨守的信
    private string[] currentDialogue;
    private int dialogueIndex = 0;
    private TMP_Text dialogueText;
    private GameObject dialoguePanel;
    private Button nextButton;
    private TaskManager taskManager;

    // 任务开始面板相关
    private GameObject taskCompletePanel;
    private TextMeshProUGUI taskCompleteText;

    public void SetupTask(TaskManager manager, GameObject panel, TMP_Text text, Button button)
    {
        taskManager = manager;
        dialoguePanel = panel;
        dialogueText = text;
        nextButton = button;
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextDialogue);
        dialoguePanel.SetActive(false);

        // 初始化任务开始面板并显示
        SetupTaskCompletePanel();
        StartCoroutine(ShowTaskStartPanel());
    }

    public override string GetTaskName() => "苍生之约定";

    public override string GetTaskObjective() => $"送达【卢氏】给【墨守】的信：{(letterDeliveredToMoShou ? "已完成" : "未完成")}";

    public override bool IsTaskComplete() => letterDeliveredToMoShou;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "墨守" && !letterDeliveredToMoShou
            ? GetDialogueForMoShou()
            : new string[] { "【……】你还没有信可送给此人。" };

        StartDialogue();
    }

    private void StartDialogue()
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = currentDialogue[dialogueIndex];
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
            dialoguePanel.SetActive(false);
            if (!letterDeliveredToMoShou && currentDialogue.Length > 1)
            {
                letterDeliveredToMoShou = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task6: 移除卢氏的信，添加神秘木屋的钥匙");
                    taskManager.inventoryManager.RemoveLetter("卢氏致墨守之信");
                    Sprite icon = Resources.Load<Sprite>("key"); // 从 Resources 加载钥匙图标
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "废木屋的钥匙",
                        content = "此乃墨守家中翻出之物，似可开启村北废木屋。昔日战乱，墨守携卢氏母子归村，此屋或藏旧时之秘。若汝与村人皆亲（灵犀度大于10），可探其中究竟。",
                        icon = icon
                    });
                }
                else
                {
                    Debug.LogError("Task6: TaskManager 或 InventoryManager 未正确绑定");
                }
            }

            PlayerController playerController = Object.FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.IsInDialogue())
                playerController.EndDialogue();

            if (IsTaskComplete())
            {
                if (taskManager != null)
                {
                    Debug.Log("Task6: 任务完成，切换到 Task7");
                    Task7 newTask = gameObject.AddComponent<Task7>();
                    taskManager.SetTask(newTask);
                    newTask.SetupTask(taskManager, dialoguePanel, dialogueText, nextButton);
                    taskManager.UpdateTaskDisplay();
                }
                else
                {
                    Debug.LogError("Task6: TaskManager 未找到，无法切换到 Task7");
                }
            }
        }
    }

    private string[] GetDialogueForMoShou()
    {
        return new string[]
        {
            "【墨守】又是你，小木头……又送信来了？",
            "【墨守】进来吧，墨成的事多亏你了……信给我。",
            "【……】",
            "【墨守】卢氏这信……她不怨我，可我欠她一家太多。",
            "【墨守】卢平是我的忘年交，我们一同赶考赶赴朝廷……",
            "【墨守】可吾辈空怀一腔抱负，却受尽了朝廷野党小人记恨。",
            "【墨守】他一心想做出实绩，却被小人诬陷，后被贬，此后一病不起……",
            "【墨守】去世前，我去了他的府邸。",
            "【墨守】他抓着我，喘着说不出话，我知道他惦记卢氏母子。",
            "【墨守】想当初，他和我一腔热血，妄想用墨家千年兼爱非攻思想兴修水利，改革朝政。",
            "【墨守】可他死后，我也心灰意冷，回此山村隐居。",
            "【墨守】我带卢氏母子回了墨家村，每见小卢，我都想起卢平，想他笑的样子……",
            "【墨守】她让我出山，可我这身体，这心，早没用了。",
            "【墨守】不过她说得对，《墨子》‘兼爱济世’，我跟卢平的约还没完……我得想想。",
            "【墨守】告诉卢氏，我会回信，慢慢写。让她别太累，小卢靠她。",
            "【墨守】对了，这钥匙是我翻出来的，村北废木屋的，这里面或许能找到你要的东西，我去不了，你去瞧瞧吧。"
        };
    }

    // 初始化任务完成面板
    private void SetupTaskCompletePanel()
    {
        taskCompletePanel = GameObject.Find("TaskCompletePanel");
        if (taskCompletePanel != null)
        {
            taskCompleteText = taskCompletePanel.GetComponentInChildren<TextMeshProUGUI>();
            if (taskCompleteText == null)
            {
                Debug.LogWarning("TaskCompletePanel 中没有找到 TextMeshProUGUI 组件！");
            }
            else
            {
                CanvasGroup canvasGroup = taskCompletePanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = taskCompletePanel.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f;
                taskCompletePanel.SetActive(false); // 确保初始隐藏
            }
        }
        else
        {
            Debug.LogError("未找到 TaskCompletePanel，请确保场景中已存在该面板！");
        }
    }

    // 显示任务开始提示
    private IEnumerator ShowTaskStartPanel()
    {
        if (taskCompletePanel != null && taskCompleteText != null)
        {
            taskCompleteText.text = "任务6——苍生之约定";
            taskCompletePanel.SetActive(true);

            CanvasGroup canvasGroup = taskCompletePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = taskCompletePanel.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0f;
            }

            // 淡入
            float fadeDuration = 1f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            // 显示 2 秒
            yield return new WaitForSecondsRealtime(2f);

            // 淡出
            elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;

            Debug.Log("任务6 开始面板已显示并隐藏");
        }
        else
        {
            Debug.LogWarning("任务完成面板或文字组件未正确初始化，无法显示任务6开始提示！");
        }
    }
}