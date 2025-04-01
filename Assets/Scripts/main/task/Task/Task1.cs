using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task1 : TaskBase
{
    [SerializeField] public int visitCount = 0; // 已拜访的居民数量
    [SerializeField] public bool letterDeliveredToMoShou = false; // 是否送达简姝儿给墨守的信
    [SerializeField] public bool returnedToJianShuEr = false; // 是否返回简姝儿处
    [SerializeField] public string[] residents = { "墨守", "墨成", "简姝儿", "罗婆", "小卢", "卢氏" }; // 墨家村居民列表
    [SerializeField] public bool[] visitedResidents; // 记录居民是否被拜访

    private TMP_Text dialogueText;
    private GameObject dialoguePanel;
    private Button nextButton;

    private GameObject normalDialoguePanel;
    private Button deliverButton;

    private string[] currentDialogue;
    private int dialogueIndex = 0;
    private string currentResident;

    private GameObject taskCompletePanel;
    private TextMeshProUGUI taskCompleteText;

    void Start()
    {
        visitedResidents = new bool[residents.Length];
        SetupTaskCompletePanel(); // 初始化任务完成面板
        StartCoroutine(ShowTaskStartPanel()); // 显示任务开始提示
    }

    public override string GetTaskName()
    {
        return "墨家村第一封信";
    }

    public override string GetTaskObjective()
    {
        return $"拜访墨家村的简姝儿以外每位居民（{visitCount}/5）\n\n" +
               $"送达【简姝儿】给【墨守】的信：{(letterDeliveredToMoShou ? "已完成" : "未完成")}\n\n" +
               $"回去找【简姝儿】：{(returnedToJianShuEr ? "已完成" : "未完成")}";
    }

    public override bool IsTaskComplete()
    {
        return visitCount >= 5 && letterDeliveredToMoShou && returnedToJianShuEr;
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

    public void SetupDeliverButton(GameObject normalPanel, Button deliverBtn)
    {
        normalDialoguePanel = normalPanel;
        deliverButton = deliverBtn;
        deliverButton.onClick.RemoveAllListeners();
        deliverButton.onClick.AddListener(() =>
        {
            normalDialoguePanel.SetActive(false);
            GetComponent<TaskManager>().TriggerDeliverLetter();
        });
    }

    public override void DeliverLetter(string targetResident)
    {
        currentResident = targetResident;
        dialogueIndex = 0;

        if (targetResident == "墨守" && !letterDeliveredToMoShou)
        {
            currentDialogue = GetDialogueForResident("墨守");
            letterDeliveredToMoShou = true;
            VisitResident(targetResident);
            TaskManager taskManager = GetComponent<TaskManager>();
            if (taskManager != null && taskManager.inventoryManager != null)
            {
                Debug.Log("获得新信件：墨守致墨成");
                Sprite icon = Resources.Load<Sprite>("jane"); // 从 Resources 加载墨守的图标
                taskManager.inventoryManager.RemoveLetter("简姝儿给墨守的信");
                taskManager.inventoryManager.AddLetter(new Letter
                {
                    title = "墨守致墨成之信",
                    content = "墨成，简姝儿弄了个木童送信，桑木壳子嵌齿轮，走路咔哒作响，看着碍眼。彗星乱天，洪水毁地，朝廷急召献策，吾知汝欲入世，可《墨子》云“力不足则止”，人力怎敌天势？昔卢平与吾约救苍生，他病逝田边，吾心已灰，汝还信这世可救乎？\r\n" +
                              "吾研杠杆，力臂可省力，昔在战场制投石机，一臂掷石百斤，然洪水滔天，何用？汝近况如何？简姝儿说汝常助她，她心思巧，滑轮引水或有成。吾非阻汝策，只是怕朝廷弃乡野声。腿疾又犯，天冷水大，吾只盼汝平安，勿令吾再失亲人。——墨守",
                    icon = icon
                });
            }
            else
            {
                Debug.LogError("TaskManager 或 InventoryManager 未正确绑定，无法更新背包");
            }
        }
        else if (targetResident == "简姝儿")
        {
            currentDialogue = GetDialogueForResident("简姝儿");
            if (visitCount >= 5 && letterDeliveredToMoShou && !returnedToJianShuEr)
            {
                returnedToJianShuEr = true;
            }
        }
        else if (System.Array.IndexOf(residents, targetResident) >= 0)
        {
            currentDialogue = GetDialogueForResident(targetResident);
            VisitResident(targetResident);
        }
        else
        {
            currentDialogue = new string[] { "【……】你还没有信可送给此人。" };
        }

        StartDialogue();
    }

    private void VisitResident(string residentName)
    {
        int index = System.Array.IndexOf(residents, residentName);
        if (index >= 0 && !visitedResidents[index])
        {
            visitedResidents[index] = true;
            visitCount++;
            Debug.Log($"拜访了 {residentName}，当前进度：{visitCount}/5");
            UpdateDisplay();
        }
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
            UpdateDisplay();
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.IsInDialogue())
            {
                playerController.EndDialogue();
            }
            if (IsTaskComplete())
            {
                TaskManager taskManager = GetComponent<TaskManager>();
                if (taskManager != null)
                {
                    Task2 newTask = gameObject.AddComponent<Task2>();
                    taskManager.SetTask(newTask);
                    newTask.SetupTask(taskManager, dialoguePanel, dialogueText, nextButton);
                    taskManager.UpdateTaskDisplay();
                }
            }
        }
    }

    private void UpdateDisplay()
    {
        TaskManager manager = GetComponent<TaskManager>();
        if (manager != null)
        {
            manager.UpdateTaskDisplay();
        }
    }

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
            Debug.LogError("未找到 TaskCompletePanel，请确保场景中已创建该面板！");
        }
    }

    private IEnumerator ShowTaskStartPanel()
    {
        if (taskCompletePanel != null && taskCompleteText != null)
        {
            taskCompleteText.text = "任务1——墨家村第一封信";
            taskCompletePanel.SetActive(true);

            CanvasGroup canvasGroup = taskCompletePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = taskCompletePanel.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0f;
            }

            float fadeDuration = 1f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(2f);

            elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;

            Debug.Log("任务1 开始面板已显示并隐藏");
        }
        else
        {
            Debug.LogWarning("任务完成面板或文字组件未正确初始化，无法显示任务1开始提示！");
        }
    }

    private string[] GetDialogueForResident(string residentName)
    {
        switch (residentName)
        {
            case "墨守":
                return new string[]
                {
                    "【墨守】这是什么怪东西？一堆木头也能跑来送信？",
                    "【墨守】简姝儿捣鼓出来的吧？",
                    "【墨守】哼，她还是这么不安分……信呢？快拿来，别在这儿晃悠！",
                    "【……】",
                    "【墨守】她让你当村里的信使？",
                    "【墨守】洪水都淹到门口了，还弄这些花样，谁知道你能撑几天？",
                    "【墨守】罢了，这封信给墨成，告诉他别老想着献策给朝廷，《墨子》有言‘力不足则止’，天要亡地，人何能为？",
                    "【墨守】你要是弄丢了信，我砸了你这木壳子！",
                    "【墨守】还不走？杵在这儿干嘛，看你这样就来气。"
                };
            case "墨成":
                return new string[]
                {
                    "【墨成】哟，你就是那个木信使？模样怪有趣，像从桑树里钻出来的。",
                    "【墨成】信给我瞧瞧。",
                    "【……】",
                    "【墨成】简姝儿的手艺真不赖，木头都能跑起来……可父亲还是老样子，不肯抬头。",
                    "【墨成】吾辈应当以天下为己任，救济苍生，可父亲依旧抱残守旧，消极避世……",
                    "【墨成】你常在村里跑吗？以后我若有抗洪的策子，靠你送出去，咱们得救田救人！"
                };
            case "简姝儿":
                if (visitCount >= 5 && letterDeliveredToMoShou)
                {
                    returnedToJianShuEr = true;
                    return new string[]
                    {
                        "【简姝儿】回来了？比我算的快半刻，你这木头挺顶用。",
                        "【简姝儿】信都送到了？干得不错。",
                        "【简姝儿】墨守没砸了你吧？他不喜新事物。",
                        "【简姝儿】还成，你比我想的靠谱。",
                        "【简姝儿】墨守有信让你送？好，我给你换任务刻板。",
                        "【简姝儿】右边木匣自己看，以后自己管吧。",
                        "【简姝儿】别等着我夸你，你是工具，送信是本分。"
                    };
                }
                else
                {
                    return new string[]
                    {
                        "【简姝儿】你送完信了吗？",
                        "【简姝儿】快去吧，别在这儿磨蹭，村里还有人等着呢。"
                    };
                }
            case "罗婆":
                return new string[]
                {
                    "【罗婆】哎呀，你是送信的小木人？",
                    "【罗婆】瞧着怪可爱的，木头身子硬邦邦。",
                    "【罗婆】简姝儿给我的信？谢了，小家伙。",
                    "【……】",
                    "【罗婆】原来是她修的你，真有能耐。",
                    "【罗婆】洪水来的时候，我还以为村子完了，她跟墨成忙着治水，总算有点盼头……",
                    "【罗婆】路上当心，洪水冲不倒你，可村里的泥泞路不好走，别卡住了。"
                };
            case "小卢":
                return new string[]
                {
                    "【小卢】哇！你是木头人？会搬水车吗？会堵洪水吗？还是能飞起来？",
                    "【小卢】你从哪儿来的？水底下吗？娘说水里藏着怪东西，你是怪东西吗？",
                    "【小卢】啊？有我的信！简姊姊的！快给我！",
                    "【……】",
                    "【小卢】太好了！以后有你送信，我就能知道水车啥时候修好！",
                    "【小卢】洪水来了，我老困在屋里，闷死了。",
                    "【小卢】你下次还来吗？我拿娘的墨给你画个像，画你扛着水车，可威风了！"
                };
            case "卢氏":
                return new string[]
                {
                    "【卢氏】你是送信的？稀奇了，你从哪儿冒出来的？",
                    "【卢氏】简姝儿派你来的？她能修木头，可惜我只会画图纸……信给我瞧瞧。",
                    "【……】",
                    "【卢氏】村里有了信使，兴许能想出治水的法子。",
                    "【卢氏】希望小卢能看到田地干的那天……多谢你跑腿。"
                };
            default:
                return new string[] { "【……】你还没有信可送给此人。" };
        }
    }
}