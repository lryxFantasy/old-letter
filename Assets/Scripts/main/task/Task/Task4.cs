using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task4 : TaskBase
{
    private bool visitedLuoPo = false; // 是否拜访罗婆
    private bool letterDeliveredToXiaoLu = false; // 是否送达罗婆给小卢的信
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

    public override string GetTaskName() => "墨家旧事";

    public override string GetTaskObjective() => $"拜访【罗婆】，\n" +
                                                 $"送达【罗婆】给【小卢】的信：{(letterDeliveredToXiaoLu ? "已完成" : "未完成")}";

    public override bool IsTaskComplete() => visitedLuoPo && letterDeliveredToXiaoLu;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        if (targetResident == "罗婆" && !visitedLuoPo)
        {
            currentDialogue = GetDialogueForLuoPo();
        }
        else if (targetResident == "小卢" && visitedLuoPo && !letterDeliveredToXiaoLu)
        {
            currentDialogue = GetDialogueForXiaoLu();
        }
        else
        {
            currentDialogue = new string[] { "【……】你还没有信可送给此人。" };
        }

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
            if (taskManager != null && taskManager.inventoryManager != null)
            {
                if (!visitedLuoPo && currentDialogue.Length > 1)
                {
                    visitedLuoPo = true;
                    Debug.Log("Task4: 添加罗婆的信");
                    Sprite icon = Resources.Load<Sprite>("jane"); // 从 Resources 加载罗婆的图标
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "神秘友人致小卢之信",
                        content = "小友平安，今夜水声如叹，吾忆旧日。洪水未至，天蓝星亮，晨露如珠，风拂草歌。水边柳柔，鱼跃清波，夏萤如星，冬雪如毯，笑声绕村。今成梦矣，然《墨子》云“力不足则止”，长大非等天晴，乃水中觅路。\r\n" +
                                  "昔墨科先祖用水车抗洪，以重心稳轮，引水归渠，田复绿，民安乐。汝父卢平亦如此，欲以力学济世，病逝未成。汝要勇，如汝娘，用手撑家；要问，如水车为何转，总有一天，天因汝问而清。愿汝心有田野，纵水淹外，内仍绿。汝与娘好生过，如旧日有光。——神秘友人",
                        icon = icon
                    });
                }
                else if (visitedLuoPo && !letterDeliveredToXiaoLu && currentDialogue.Length > 1)
                {
                    letterDeliveredToXiaoLu = true;
                    Debug.Log("Task4: 移除罗婆的信，添加小卢的信");
                    taskManager.inventoryManager.RemoveLetter("神秘友人致小卢之信");
                    Sprite icon = Resources.Load<Sprite>("jane"); // 从 Resources 加载小卢的图标
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "小卢致卢氏之信",
                        content = "娘好，今天我在屋里跑来跑去，想听木童走路会不会响，可它不在，只有风吹窗，咚咚的，像敲木头。爹在天上看我吗？我老想他，想知道他长啥样，干啥事。娘说他是好人，我想见他！\r\n" +
                                  "娘老画水车，手都不歇，累不累呀？我偷看娘画了个好看的人，线多颜色多，娘说是爹！我长大也能画这么好吗？我试着画了，有树有水车，还有娘笑，可画不了爹，没见过。娘画得像仙人，能把东西变出来，太厉害了！娘能给我画个爹吗？或者画个大水车，带齿轮那种，转起来可有劲！我想知道水车咋动，娘教我好不好？——小卢",
                        icon = icon
                    });
                }
            }
            else
            {
                Debug.LogError("Task4: TaskManager 或 InventoryManager 未正确绑定");
            }

            PlayerController playerController = Object.FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.IsInDialogue())
                playerController.EndDialogue();

            if (IsTaskComplete())
            {
                if (taskManager != null)
                {
                    Debug.Log("Task4: 任务完成，切换到 Task5");
                    Task5 newTask = gameObject.AddComponent<Task5>();
                    taskManager.SetTask(newTask);
                    newTask.SetupTask(taskManager, dialoguePanel, dialogueText, nextButton);
                    taskManager.UpdateTaskDisplay();
                }
                else
                {
                    Debug.LogError("Task4: TaskManager 未找到，无法切换到 Task5");
                }
            }
        }
    }

    private string[] GetDialogueForLuoPo()
    {
        return new string[]
        {
            "【罗婆】哎呀，又是你，小木信使，还是硬邦邦的。",
            "【罗婆】简姝儿让你来的吧？她心细，总惦记我这把老骨头。她算我半个亲人了。",
            "【罗婆】当年卢平跟墨守约好救天下，卢平走了，墨守躲了，我家也散了，只剩我守着这村。",
            "【罗婆】简姝儿来的时候，瘦得像根柴，从长安跑来投奔我……现在她跟墨成忙治水，我看着欣慰。",
            "【罗婆】哎，话多了，差点忘了事。没啥大事，就是麻烦你送封信给小卢。那孩子没爹，卢平走得早，可怜……",
            "【罗婆】他好奇，跟卢平小时候一个样。我想让他知道点旧事，他没见过没水的日子……又唠叨了。",
            "【罗婆】信上没写我名，就叫‘神秘友人’，别告诉他是我，怕他嫌我老太婆烦。谢了你。"
        };
    }

    private string[] GetDialogueForXiaoLu()
    {
        return new string[]
        {
            "【小卢】哇！又是你！我的木头朋友！你送信真快，有我的信吗？",
            "【……】",
            "【小卢】我没见过没水的田，画了好多水车想给神秘友人，可娘说田都泡水里了……",
            "【小卢】真想知道神秘友人是谁，他懂好多，太厉害了，我猜是……木头人，只告诉你啊……我猜他是我爹！他肯定被水挡住了，偷偷写信给我。",
            "【小卢】他说我得勇敢，像娘，我觉得娘可厉害了，她老讲爹，说他在天上看我，你知道吗，我跟爹同名！他叫卢平，我是小卢。",
            "【小卢】这信给娘，麻烦你送！你问我为啥不自己给……这样好玩！你送过去，娘肯定高兴。",
            "【小卢】你下次还来吗？我画了个你扛水车，可帅了！"
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
            taskCompleteText.text = "任务4——墨家旧事";
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

            Debug.Log("任务4 开始面板已显示并隐藏");
        }
        else
        {
            Debug.LogWarning("任务完成面板或文字组件未正确初始化，无法显示任务4开始提示！");
        }
    }
}