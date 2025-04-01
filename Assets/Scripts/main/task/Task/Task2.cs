using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task2 : TaskBase
{
    private bool letterDeliveredToMoCheng = false; // 是否送达墨守给墨成的信
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

    public override string GetTaskName() => "出世与入世";

    public override string GetTaskObjective() => $"送达【墨守】给【墨成】的信：{(letterDeliveredToMoCheng ? "已完成" : "未完成")}";

    public override bool IsTaskComplete() => letterDeliveredToMoCheng;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "墨成" && !letterDeliveredToMoCheng
            ? GetDialogueForMoCheng()
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
            if (!letterDeliveredToMoCheng && currentDialogue.Length > 1)
            {
                letterDeliveredToMoCheng = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task2: 更新信件 - 移除墨守的信，添加墨成的信");
                    Sprite icon = Resources.Load<Sprite>("jane"); // 从 Resources 加载墨成的图标
                    taskManager.inventoryManager.RemoveLetter("墨守致墨成之信");
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "墨成致简姝儿之信",
                        content = "简姝儿，吾志之友。记得彗星过天那夜吗？洪水未至，星稀天昏，汝于灯下敲水车，试滑轮力，影如杠杆，撑起吾志。汝笑吾“纸上谈水，怎救田”，眼清如溪，吾心动矣。\r\n" +
                                  "后常寻汝，见汝修车，专注如《墨子》“知行合一”，胜吾空言。洪水隔路，吾研小孔成像，欲以铜镜折光，制信号装置，求援朝廷，然缺汝滑轮调镜之术。汝不信吾志，然此志因汝而坚。昔父与卢伯父约济世，吾欲继之，以力学改水患之命，助汝引洪归渠，救田救人。简姝儿，可否共谋此策？——墨成",
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

    private string[] GetDialogueForMoCheng()
    {
        return new string[]
        {
            "【墨成】又是你，小木信使。谁的信？给我看看。",
            "【……】",
            "【墨成】父亲还是这德行，字里全是刺，可这次……多了点人味儿，兴许是信里藏了心吧。",
            "【墨成】他说起卢伯父，我还记得他跟父亲争论《墨子》“兼爱济世”时的样子，说咱们得救天下……",
            "【墨成】后来洪水来了，卢伯父病倒了，父亲带我来这村，他再也不提入世的事。",
            "【墨成】他问我过得咋样？他居然会问这个？他平时硬得像杠杆，我还以为他早忘了我是他儿子。",
            "【墨成】吾辈应当救世，以苍生为己任，不应抱残守旧，消极避世。",
            "【墨成】你要再去父亲那儿吗？替我说一句……《墨子》有言“志功为辩”，我没忘他跟卢伯父的志向，我得接着干，我会回信。"
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
            taskCompleteText.text = "任务2——力学与献策";
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