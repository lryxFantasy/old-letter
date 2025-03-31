using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task1 : TaskBase
{
    [SerializeField] public int visitCount = 0; // 已拜访的居民数量
    [SerializeField] public bool letterDeliveredToMoShou = false; // 是否送达简工给墨守的信
    [SerializeField] public bool returnedToJianGong = false; // 是否返回简工处
    [SerializeField] public string[] residents = { "墨守", "墨诗", "简工", "罗婆", "小卢", "卢氏" }; // 墨科村居民列表
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
        return "墨科村第一封信";
    }

    public override string GetTaskObjective()
    {
        return $"拜访墨科村的简工以外每一位居民（{visitCount}/5）\n\n" +
               $"送达【简工】给【墨守】的信：{(letterDeliveredToMoShou ? "已完成" : "未完成")}\n\n" +
               $"回去找【简工】：{(returnedToJianGong ? "已完成" : "未完成")}";
    }

    public override bool IsTaskComplete()
    {
        return visitCount >= 5 && letterDeliveredToMoShou && returnedToJianGong;
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
                Debug.Log("获得新信件");
                Sprite icon = Resources.Load<Sprite>("moshou"); // 从 Resources 加载墨守的图标
                taskManager.inventoryManager.RemoveLetter("简工给墨守的信");
                taskManager.inventoryManager.AddLetter(new Letter
                {
                    title = "墨守的信",
                    content = "墨诗，简工制一木信使，名墨信，木身粗陋，观之不悦，吾不喜此新物，然彼言此乃唯一途。疫病困人，出不得，唯此木疙瘩可代步。彼言修此耗两日，村中木料为之，亦算彼有能……汝尚弄那诗耶？自幼汝喜纸笔，吾不解其用，汝母在时，常言汝有才，谓汝诗虽彼不解却喜之。天象异动，朝廷急，吾研杠杆之用，力臂二比一，省力甚，然《墨子》云“力与重成正比”，汝光学何进展？昔战乱，流矢伤吾腿，痛三日不眠，吾见同袍殞地，血染沙场，含小卢之父，唉……彼母子孤苦……彗星现之日，吾在军中，见天色骤变，汝母后亦不敌疫去。吾不知此生可有明日，然生当续也。\r\n汝处如何？村人言汝与简工相好，彼头脑清明，修器之术精，汝可随彼学一二。非谓汝诗无用，唯……欲知汝安否，汝乃吾子也。诗或难解天象，然吾不欲再失汝。简工言此木信使可靠，若有言，书归，勿使吾久候。无暇则罢，勿勉强。——墨守\r\n",
                    icon = icon
                });
            }
            else
            {
                Debug.LogError("TaskManager 或 InventoryManager 未正确绑定，无法更新背包");
            }
        }
        else if (targetResident == "简工")
        {
            currentDialogue = GetDialogueForResident("简工");
            if (visitCount >= 5 && letterDeliveredToMoShou && !returnedToJianGong)
            {
                returnedToJianGong = true;
            }
        }
        else if (System.Array.IndexOf(residents, targetResident) >= 0)
        {
            currentDialogue = GetDialogueForResident(targetResident);
            VisitResident(targetResident);
        }
        else
        {
            currentDialogue = new string[] { "【……】你还没有信" };
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
            Debug.LogError("未找到 TaskCompletePanel，请确保 Task0 已创建该面板！");
        }
    }

    private IEnumerator ShowTaskStartPanel()
    {
        if (taskCompletePanel != null && taskCompleteText != null)
        {
            taskCompleteText.text = "任务1——墨科村第一封信";
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
                    "【墨守】这是什么东西？这木头玩意儿哪儿来的？",
                    "【墨守】简工派你送信来的？",
                    "【墨守】哼，这丫头总爱弄些稀奇古怪的东西……",
                    "【墨守】信呢？快拿出来，别磨蹭！",
                    "【……】",
                    "【墨守】简工想让你当墨科村的信使？",
                    "【墨守】哼，现在的人啊，连送信都用木头做的，谁知道你会不会坏掉？",
                    "【墨守】算了，这封信给墨诗，告诉他别再写那些没用的诗，男人就该学点实用的东西。",
                    "【墨守】你要是敢拆信或者弄丢了，我非拆了你不可！",
                    "【墨守】……还不走？别在这儿碍眼，看见你这木头身子就烦。"
                };
            case "墨诗":
                return new string[]
                {
                    "【墨诗】哦？你是新的信使？看你这模样，像从老树里蹦出来的。",
                    "【墨诗】信给我看看。",
                    "【……】",
                    "【墨诗】果然是简工修的你？她真厉害……虽然她不太懂我的诗。",
                    "【墨诗】你见过她研究时的眼神吗？那光亮，比我诗里的景色还美。",
                    "【墨诗】可惜疫病把我们隔开了，很难见面。",
                    "【墨诗】告诉她我想她，想她专注的样子，哪怕她只会跟我讲齿轮和滑轮的道理……",
                    "【墨诗】你会一直在村里送信吗？也许哪天，我给你写首诗，歌颂你这木头身子和灵魂的和谐。"
                };
            case "简工":
                if (visitCount >= 5)
                {
                    returnedToJianGong = true;
                    return new string[]
                    {
                        "【简工】回来了？比我预计的早半刻，你这木头还挺结实。",
                        "【简工】信都送到了？很好，你干得不错。",
                        "【简工】墨守没把你赶出去吧？他不喜欢机械，当年腿伤是战乱留下的，他妻子也在战中去世了……",
                        "【简工】……还行，你比我想象的靠谱。",
                        "【简工】墨守让你送信了？好，我给你更新任务刻板。",
                        "【简工】你木匣右边可以自己看，以后自己更新。",
                        "【简工】别指望我夸你，你就是个工具，送信是你的本职。"
                    };
                }
                else
                {
                    return new string[]
                    {
                        "【简工】你送完信了吗？",
                        "【简工】快去吧，别在这儿浪费时间，村里还有其他人等着你送信呢。"
                    };
                }
            case "罗婆":
                return new string[]
                {
                    "【罗婆】哎呀，你就是那个送信的小木人？",
                    "【罗婆】挺可爱的，虽然是木头做的，脑袋肯定硬得像石头。",
                    "【罗婆】简工给我的信吗？辛苦你了，小木人。",
                    "【……】",
                    "【罗婆】原来是这样，简工把你修好了，她果然有本事。",
                    "【罗婆】当年她跟我从长安逃到这村子避疫，我还担心她觉得这儿闷得慌，现在看来，她跟墨诗处得不错，呵呵……",
                    "【罗婆】路上小心，疫病伤不了你，但村里的路不好走，别摔着了。"
                };
            case "小卢":
                return new string[]
                {
                    "【小卢】哇！你真的是木头人？会飞吗？会说话吗？还是能变身？",
                    "【小卢】你从哪儿来的？山里吗？娘说山里有怪东西，你是怪东西吗？",
                    "【小卢】啊？有我的信！简姐姐的！快给我看！",
                    "【……】",
                    "【小卢】太好了！以后村里就有信使了。",
                    "【小卢】以前一个月才有个驿卒来我们村。",
                    "【小卢】你下次还来吗？我可以用娘的墨给你画一幅画，画个大大的你！"
                };
            case "卢氏":
                return new string[]
                {
                    "【卢氏】你是送信的？真稀奇，你从哪儿来的？",
                    "【卢氏】简工派你来的吧？她总能修出这种东西，我只会用墨画些旧日的回忆……信给我看看。",
                    "【……】",
                    "【卢氏】以后村里有信使了，事情也许会好起来。",
                    "【卢氏】希望小卢能看到这些……谢谢你跑这一趟。",
                    "【卢氏】别在意你身上的木纹，这样挺好，像古画，带着岁月的痕迹。"
                };
            default:
                return new string[] { "【……】你还没有信" };
        }
    }
}