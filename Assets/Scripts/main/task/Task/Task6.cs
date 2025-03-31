using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task6 : TaskBase
{
    private bool letterDeliveredToVictor = false;
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

    public override string GetTaskName() => "硝烟中的尘埃";

    public override string GetTaskObjective() => $"送达【伊芙·伍德】给【维克托·凯恩】的信：{(letterDeliveredToVictor ? "已完成" : "未完成")}";

    public override bool IsTaskComplete() => letterDeliveredToVictor;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "维克托·凯恩" && !letterDeliveredToVictor
            ? GetDialogueForVictor()
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
            if (!letterDeliveredToVictor && currentDialogue.Length > 1)
            {
                letterDeliveredToVictor = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task6: 移除伊芙的信，添加神秘房子的钥匙");
                    Sprite icon = Resources.Load<Sprite>("key"); // 从 Resources 加载图标
                    taskManager.inventoryManager.RemoveLetter("伊芙的信");
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "废弃房屋的钥匙",
                        content = "老兵在家里翻出来的钥匙，似乎可以打开村子上边那座房子，如果你已经做完了所有想做的事情，和每个人都好感度大于10，也许可以看看里面有些什么。",
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

    private string[] GetDialogueForVictor()
    {
        return new string[]
        {
            "【维克托·凯恩】是你啊，铁皮罐头……又跑来送信了？",
            "【维克托·凯恩】进来吧，关于我儿子的事，还得谢谢你了……信呢？拿给我。",
            "【……】",
            "【维克托·凯恩】唉，伊芙这女人……她说她不怪我。可我对不起他们一家……",
            "【维克托·凯恩】小卢克的父亲，卢克，他死在我面前，我没保住他……",
            "【维克托·凯恩】那天战场上很混乱，我们溃败了，敌军打到了指挥部。",
            "【维克托·凯恩】子弹在我耳边呼啸着掠过，空气中硝烟味刺鼻到让人窒息，天红得像血……",
            "【维克托·凯恩】我喊他趴下，可他没听见，流弹穿过他的胸口，血溅了我一身……",
            "【维克托·凯恩】我拖着这条烂腿爬过去时，他已经只剩一口气了，他伸手握住我，另一只手里还攥着枪，",
            "【维克托·凯恩】他想说些什么，可已经没声了，但我知道，他在乎他们娘俩。",
            "【维克托·凯恩】我没做到……没让他活着回去见他的儿子。",
            "【维克托·凯恩】这条腿是那时候废的，疼得我睡不着，可我活着回来了，他没回。",
            "【维克托·凯恩】伊芙说她不怪我，可我过不了自己这关……战争结束后，我把伊芙他们接到了信火村避难。",
            "【维克托·凯恩】每次看到小卢克那孩子，我就想起卢克，想起他每次提到儿子脸上的笑容……",
            "【维克托·凯恩】我欠他们的，太多了。",
            "【维克托·凯恩】也许是时候释怀了，我会给他们回信的，我想把记忆里的卢克写下来，可能……可能需要点时间吧……",
            "【维克托·凯恩】告诉伊芙我会回信的。替我跟她说……让她别太累，小卢克还得靠她。",
            "【维克托·凯恩】哦对了，这是我在家里翻出来的钥匙，似乎可以打开村子上边那座房子，听说是战时留下的。",
            "【维克托·凯恩】我是没机会出门了，也许你可以看看里面有些什么。"
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
            taskCompleteText.text = "任务6——硝烟中的尘埃";
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