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

    public override string GetTaskName() => "神秘友人";

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
            currentDialogue = new string[] { "【……】你还没有信" };
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
                        title = "罗婆的信",
                        content = "小友安好，今夜吾坐窗前，听外声，忆旧时。昔世未疫，天蓝如洗，星挂穹顶，灿若灯笼，暖可化心。晨，露珠缀草叶，似小珠，风吹草舞，沙沙如歌。河畔柳垂，枝如女发，轻拂水面，水清见鱼游。夏夜，蟋蟀草中鸣，萤火提灯飞，似星坠地嬉。大人院中摇扇，述故事。冬雪飘，轻如羽，积地成毯，吾裹巾堆雪人，笑声满村。\r\n" +
                                  "小友，此丽景，今唯存心，然吾欲告汝，长大非待世好，乃于暗中寻光。汝当勇，如汝母，以手撑生。汝当善，伸手助人，世方更美。汝当好奇，追问何故，终有一日，天因汝求索而复蓝。吾不知汝可解，唯愿汝长大，心有花园，纵外荒野，内亦花开。汝与母好生，似星空尚在时，带光，带热。——神秘友人\r\n",
                        icon = icon
                    });
                }
                else if (visitedLuoPo && !letterDeliveredToXiaoLu && currentDialogue.Length > 1)
                {
                    letterDeliveredToXiaoLu = true;
                    Debug.Log("Task4: 移除罗婆的信，添加小卢的信");
                    taskManager.inventoryManager.RemoveLetter("罗婆的信");
                    Sprite icon = Resources.Load<Sprite>("jane"); // 从 Resources 加载小卢的图标
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "小卢的信",
                        content = "娘亲安好，今日吾于屋中跑，欲听风可唱否，然窗闭，唯闻风叩窗，砰砰，如与吾戏！吾欲出门，娘言疫病坏，不可出。父于远方视吾否？吾思彼，欲知彼事。吾画许多画，有树，有屋，有娘笑，然不会画父，未见也，唯知娘言彼甚好，吾思彼。\r\n" +
                                  "娘常于屋中画，累否？昨吾偷观，画一俊面，线条多，色亦多，吾问何，娘言乃父之貌！吾长大亦如此否？娘画甚好，胜吾百万倍，吾画不出娘之景！吾爱娘，娘乃最好之娘，如故事中仙人，能以色变万物。可否画一星空予吾？欲知其是否如灯笼亮，纵吾唯能于屋中观！——小卢\r\n",
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
            "【罗婆】哎呀，又是你，送信的小木人，还是那么硬邦邦的。",
            "【罗婆】简工让你来的吧？她挺贴心，常惦记我这个老太婆。",
            "【罗婆】虽然我们是远亲，但自从我孙子没了，她是我在这世上唯一的亲人了。",
            "【罗婆】当年天象乱的时候，我的儿孙死的死，散的散，有的上了亡籍，有的没了音讯，我跟孙子相依为命……",
            "【罗婆】后来他也被疫病带走，只剩我守着这屋子。",
            "【罗婆】简工那时候来找我，她父亲是我侄子，战前就没了，留下她一个人投奔我。",
            "【罗婆】她从长安逃到这村子，才十九岁，背着个破袋子，瘦得像根柴……现在都好了，她在这村子里有了依靠。",
            "【罗婆】哎，我话多了，差点忘了正事。",
            "【罗婆】其实也没别的，就是想麻烦你送封信给小卢。",
            "【罗婆】那孩子不容易，战乱时生的，没见过父亲……唉，世事无常。",
            "【罗婆】他好奇心强，跟我孙子挺像……我想尽力让他知道些过去的事，他还没见过真正的星空……我又多说了。",
            "【罗婆】信上我没署名，就写“神秘友人”，别告诉他是我写的，怕他嫌我这个老太婆没意思。",
            "【罗婆】辛苦你了。"
        };
    }

    private string[] GetDialogueForXiaoLu()
    {
        return new string[]
        {
            "【小卢】哇，又是你！我的木头朋友！你送信真快，有没有我的信？",
            "【……】",
            "【小卢】神秘友人说了好多有趣的事！水里的“鱼”是什么？他不用呼吸吗？",
            "【小卢】我没见过星空，不过我画了好多星空想给神秘友人，可惜娘说星空已经没了……",
            "【小卢】真好奇神秘友人是谁，他知道好多东西，太厉害了，我有个猜想……",
            "【小卢】木头人，只告诉你哦……我猜他是我爹！他肯定是回不来，偷偷给我写信。",
            "【小卢】他说我得勇敢，像娘一样，我觉得娘很厉害，她常讲爹的故事，说他在远方看着我。",
            "【小卢】你知道吗，我跟爹同名！他叫大卢，我是小卢。",
            "【小卢】这封信给娘，麻烦你送过去！",
            "【小卢】你要问我既然住一起，为什么不直接给……我觉得这样更好玩！你送过去，肯定会有惊喜，我想让娘开心。",
            "【小卢】你下次还来吗？我画了个大大的你，木头身子亮亮的，真帅！",
            "【小卢】下次麻烦你问问神秘友人“鱼”长什么样！"
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
            taskCompleteText.text = "任务4——神秘友人";
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