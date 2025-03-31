using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task3 : TaskBase
{
    private bool letterDeliveredToJianGong = false; // 是否送达墨诗给简工的信
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

    public override string GetTaskName() => "理性之恋歌";

    public override string GetTaskObjective() => $"送达【墨诗】给【简工】的信：{(letterDeliveredToJianGong ? "已完成" : "未完成")}";

    public override bool IsTaskComplete() => letterDeliveredToJianGong;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "简工" && !letterDeliveredToJianGong
            ? GetDialogueForJianGong()
            : new string[] { "【……】你还没有信" };

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
            if (!letterDeliveredToJianGong && currentDialogue.Length > 1)
            {
                letterDeliveredToJianGong = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task3: 移除墨诗的信");
                    taskManager.inventoryManager.RemoveLetter("墨诗的信");
                }
                else
                {
                    Debug.LogError("Task3: TaskManager 或 InventoryManager 未正确绑定");
                }
            }

            PlayerController playerController = Object.FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.IsInDialogue())
                playerController.EndDialogue();

            if (IsTaskComplete())
            {
                if (taskManager != null)
                {
                    Debug.Log("Task3: 任务完成，切换到 Task4");
                    Task4 newTask = gameObject.AddComponent<Task4>();
                    taskManager.SetTask(newTask);
                    newTask.SetupTask(taskManager, dialoguePanel, dialogueText, nextButton);
                    taskManager.UpdateTaskDisplay();
                }
                else
                {
                    Debug.LogError("Task3: TaskManager 未找到，无法切换到 Task4");
                }
            }
        }
    }

    private string[] GetDialogueForJianGong()
    {
        return new string[]
        {
            "【简工】回来了？有给我的信？让我看看。",
            "【……】",
            "【简工】墨诗的信？他又写这些……满篇花哨的话，我得理清楚才能看明白。",
            "【简工】他说起我们初见时的星空，我记得那时候，但当时我只想测星光的变化，哪有心思看他。",
            "【简工】他……写得这么直白，我真有点……不知道怎么回，心乱了。",
            "【简工】他恋爱时就这样，热情得像火，我有点招架不住。",
            "【简工】认识他之前，我的日子只有齿轮和滑轮，一切都能用技术解决，",
            "【简工】可他的热情像团火，把我的心搅乱了，感情这东西，我应付不来。",
            "【简工】我喜欢技术，喜欢能修的东西，他却老说诗，说心动，说那些抓不住的东西……",
            "【简工】也许我能懂，可有些话我说不出口，可能……有点害羞吧。",
            "【简工】这个傻子，让我怎么办？",
            "【简工】如果你再去见他，替我说一句，嗯，我不擅长花哨的话，但也许，我确实挺喜欢他的。",
            "【简工】我会给他回信。",
            "【简工】哦，罗婆说想让你帮忙送信，她住在村东北的小屋，谢谢你。"
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
            taskCompleteText.text = "任务3——理性之恋歌";
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

            Debug.Log("任务3 开始面板已显示并隐藏");
        }
        else
        {
            Debug.LogWarning("任务完成面板或文字组件未正确初始化，无法显示任务3开始提示！");
        }
    }
}