using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task2 : TaskBase
{
    private bool letterDeliveredToMoShi = false; // 是否送达墨守给墨诗的信
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

    public override string GetTaskName() => "力学与诗光";

    public override string GetTaskObjective() => $"送达【墨守】给【墨诗】的信：{(letterDeliveredToMoShi ? "已完成" : "未完成")}";

    public override bool IsTaskComplete() => letterDeliveredToMoShi;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "墨诗" && !letterDeliveredToMoShi
            ? GetDialogueForMoShi()
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
            if (!letterDeliveredToMoShi && currentDialogue.Length > 1)
            {
                letterDeliveredToMoShi = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task2: 更新信件 - 移除墨守的信，添加墨诗的信");
                    Sprite icon = Resources.Load<Sprite>("jane"); // 从 Resources 加载墨诗的图标
                    taskManager.inventoryManager.RemoveLetter("墨守的信");
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "墨诗的信",
                        content = "简工，吾理性之光，吾魂中之焰，吾命中最炽之星。汝不解吾笔下之风，不明吾何以于彗星下低吟，然吾不以为意。简工，汝可忆初见之夜？天象初定，疫病未封村，天虽无日，残星如碎镜，洒于山坡。汝至，身着素裙，手提木匣，测星光之变，灯影摇曳，汝身影如光，照吾心。汝问吾何为，吾曰赋诗，汝颦笑，言“诗不可解天象”。彼时，吾见汝双目，清如未被疫染之水，吾心乱矣。\r\n" +
                                  "后，吾常往观汝。汝于小屋中埋首，专注之姿，胜吾诗中万景。彼刻，吾心沦矣。吾爱汝，简工。吾思汝，梦中皆汝影。疫病隔吾二人，然吾愿化飞蛾，扑汝窗前之灯，纵成灰，亦触汝温。汝不解吾热，简工，然此乃吾爱汝之证。纵汝永立理性之巅，冷观吾，吾亦以热为汝燃尽村野，为汝涂满此暗世之色。简工，此情不熄。——墨诗",
                        icon = icon
                    });
                }
                else
                {
                    Debug.LogError("Task2: TaskManager 或 InventoryManager 未正确绑定，无法更新信件");
                }
            }

            PlayerController playerController = Object.FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.IsInDialogue())
            {
                playerController.EndDialogue();
            }

            if (IsTaskComplete())
            {
                if (taskManager != null)
                {
                    Debug.Log("Task2: 任务完成，切换到 Task3");
                    Task3 newTask = gameObject.AddComponent<Task3>();
                    taskManager.SetTask(newTask);
                    newTask.SetupTask(taskManager, dialoguePanel, dialogueText, nextButton);
                    taskManager.UpdateTaskDisplay();
                }
                else
                {
                    Debug.LogError("Task2: TaskManager 未找到，无法切换到 Task3");
                }
            }
        }
    }

    private string[] GetDialogueForMoShi()
    {
        return new string[]
        {
            "【墨诗】哦，又是你，小木信使。谁的信？给我看看。",
            "【……】",
            "【墨诗】唉，父亲还是老样子，字里像带刀，不过这次……好像多了点温情，他以前的信可没这么软。也许书信里，人会更真实吧。",
            "【墨诗】他提到我娘，我还记得她读我第一首诗时的笑，说我可以当诗人……那时候星空还很美。",
            "【墨诗】后来彗星出现，天象乱了，父亲带我离开长安，来到墨科村。",
            "【墨诗】娘没跟来，多年后我才知道，她染了疫病去世了，父亲不想告诉我……他嫌我的诗没用，但这是我怀念她的方式，所以我跟他分开住。",
            "【墨诗】他居然问我过得怎么样……他竟然会问这个，他平时沉默得像块铁，战场上像座山，我还以为他早忘了什么是关心。",
            "【墨诗】你知道吗，他从没说过怕失去我。",
            "【墨诗】这封信给简工，带过去。告诉她我想她，想她研究时皱眉的样子，想她眼里的清光，哪怕她只会笑我的诗没道理。",
            "【墨诗】你要再去父亲那儿吗？替我说一句……我没忘他研究力学的志向，但我也不会扔下我的诗笔。我会给他回信。"
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
            taskCompleteText.text = "任务2——力学与诗光";
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

            Debug.Log("任务2 开始面板已显示并隐藏");
        }
        else
        {
            Debug.LogWarning("任务完成面板或文字组件未正确初始化，无法显示任务2开始提示！");
        }
    }
}