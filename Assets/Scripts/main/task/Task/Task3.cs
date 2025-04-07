using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task3 : TaskBase
{
    private bool letterDeliveredToJianShuEr = false; // 是否送达墨成给简姝儿的信
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

    public override string GetTaskName() => "力学与光学";

    public override string GetTaskObjective() => $"送达【墨成】给【简姝儿】的信：{(letterDeliveredToJianShuEr ? "已完成" : "未完成")}";

    public override bool IsTaskComplete() => letterDeliveredToJianShuEr;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "简姝儿" && !letterDeliveredToJianShuEr
            ? GetDialogueForJianShuEr()
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
            if (!letterDeliveredToJianShuEr && currentDialogue.Length > 1)
            {
                letterDeliveredToJianShuEr = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task3: 移除墨成的信");
                    taskManager.inventoryManager.RemoveLetter("墨成致简姝儿之信");
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

    private string[] GetDialogueForJianShuEr()
    {
        return new string[]
        {
            "【简姝儿】回来了？有我的信？快拿来瞧瞧。",
            "【……】",
            "【简姝儿】墨成的信？这家伙话还是这么多，得一句句拆开看。",
            "【简姝儿】他提初见那晚，我记得，当时我在调水车的滑轮，试着让齿轮咬合更稳，哪有空听他瞎扯。",
            "【简姝儿】他写了不少光学的事，小孔成像、铜镜折光，想做信号装置？这倒有点意思。",
            "【简姝儿】我算了算，他那铜镜得用滑轮组调角度，不然光投不远。我手头有桑木和细绳，能搭个轻便支架，配上齿轮转轴，兴许能成。",
            "【简姝儿】我造东西讲实打实，滑轮省力，齿轮传动，照《墨子》‘知行合一’的法子来，他老说献策救天下，空想多过动手。",
            "【简姝儿】但这光学装置确实大有益处，若能研究出成果，或能加快信息传递。",
            "【简姝儿】他那脑子转得快，我得承认，比我琢磨杠杆时多拐几个弯。",
            "【简姝儿】你下次见他，替我说一句，他的光学设想我试试，滑轮和支架我来弄，让他把镜子摆弄准了，我会回信。",
            "【简姝儿】我还是想说服墨守，听闻罗婆甚是了解墨家村旧事，或可问问罗婆可否有良策。"
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
                taskCompletePanel.SetActive(false);

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
            taskCompleteText.text = "任务3——力学与光学";
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

            Debug.Log("任务3 开始面板已显示并隐藏");
        }
        else
        {
            Debug.LogWarning("任务完成面板或文字组件未正确初始化，无法显示任务3开始提示！");
        }
    }
}