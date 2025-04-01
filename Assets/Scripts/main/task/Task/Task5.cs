using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task5 : TaskBase
{
    private bool letterDeliveredToLuShi = false; // 是否送达小卢给卢氏的信
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

    public override string GetTaskName() => "画中的父亲";

    public override string GetTaskObjective() => $"送达【小卢】给【卢氏】的信：{(letterDeliveredToLuShi ? "已完成" : "未完成")}";

    public override bool IsTaskComplete() => letterDeliveredToLuShi;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "卢氏" && !letterDeliveredToLuShi
            ? GetDialogueForLuShi()
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
            if (!letterDeliveredToLuShi && currentDialogue.Length > 1)
            {
                letterDeliveredToLuShi = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task5: 移除小卢的信，添加卢氏的信");
                    taskManager.inventoryManager.RemoveLetter("小卢致卢氏之信");
                    Sprite icon = Resources.Load<Sprite>("jane"); // 从 Resources 加载卢氏的图标
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "卢氏致墨守之信",
                        content = "墨公，久未通言，公近况可安？彗星乱天，洪水吞田，村人困守，墨科危矣。妾知公避世，心冷如冬，然《墨子》云“兼爱济世”，昔日公与卢君平立约，欲以力学救苍生，公岂忘之？妾画水车，研重心平衡，欲稳轮引水，复田灌溉，然力不足，缺杠杆之妙。公精于此道，昔在战场制投石机，一臂掷石百斤，今洪水虽猛，何不能挡？简姝儿以滑轮制木童，墨成以镜折光，皆为抗灾奔走，唯公手握墨家真传，却闭门不出。\r\n" +
                                  "卢君平若在，必劝公出山。彼病逝田边，犹念农事，妾母子得公照拂至今，感铭于心。公若不出手，田尽毁，村何存？吾子小卢日日问水车可转，妾无言以对。公腿疾缠身，妾知其苦，然墨家传承非一人之物，乃济世之器。若公有意，付木童一策，杠杆之术与妾水车相合，或可引洪归渠，救田救人。昔约未完，今日可续，盼公莫辞。——卢氏",
                        icon = icon
                    });
                }
                else
                {
                    Debug.LogError("Task5: TaskManager 或 InventoryManager 未正确绑定");
                }
            }

            PlayerController playerController = Object.FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.IsInDialogue())
                playerController.EndDialogue();

            if (IsTaskComplete())
            {
                if (taskManager != null)
                {
                    Debug.Log("Task5: 任务完成，切换到 Task6");
                    Task6 newTask = gameObject.AddComponent<Task6>();
                    taskManager.SetTask(newTask);
                    newTask.SetupTask(taskManager, dialoguePanel, dialogueText, nextButton);
                    taskManager.UpdateTaskDisplay();
                }
                else
                {
                    Debug.LogError("Task5: TaskManager 未找到，无法切换到 Task6");
                }
            }
        }
    }

    private string[] GetDialogueForLuShi()
    {
        return new string[]
        {
            "【卢氏】你又来了，小木人。咦？小卢的信，这孩子真是，住一块儿还让你送……",
            "【……】",
            "【卢氏】唉……他想知道爹的事，还让我画没水的田……这孩子，天真得让我心酸。",
            "【卢氏】卢平三年前病死了，跟墨守是兄弟，约好用《墨子》‘兼爱济世’救人，我没见他最后一面，只知道他走前惦记着天下黎民百姓。",
            "【卢氏】小卢老问爹在哪儿，我只能说他爹在天上看着，等他长大回来……不知道这么说对不对，我只能画。",
            "【卢氏】墨守受尽了朝廷打压，之后心灰意冷回到村子避世。",
            "【卢氏】简姝儿和墨成说想济世，需要他的技术是吗。",
            "【卢氏】或许我能劝得动他。",
            "【卢氏】这信给墨守，麻烦你。墨成说墨守老觉得对不起卢平……我不怪他，谢他这些年帮我。",
            "【卢氏】谢谢你跑这趟，小木人，你是村里最忙的。"
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
            taskCompleteText.text = "任务5——画中的父亲";
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

            Debug.Log("任务5 开始面板已显示并隐藏");
        }
        else
        {
            Debug.LogWarning("任务完成面板或文字组件未正确初始化，无法显示任务5开始提示！");
        }
    }
}