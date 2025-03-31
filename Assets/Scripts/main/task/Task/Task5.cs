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

    public override string GetTaskName() => "画中之星";

    public override string GetTaskObjective() => $"送达【小卢】给【卢氏】的信：{(letterDeliveredToLuShi ? "已完成" : "未完成")}";

    public override bool IsTaskComplete() => letterDeliveredToLuShi;

    public override void DeliverLetter(string targetResident) // 修正为 targetResident，去掉空格
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "卢氏" && !letterDeliveredToLuShi
            ? GetDialogueForLuShi()
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
            if (!letterDeliveredToLuShi && currentDialogue.Length > 1)
            {
                letterDeliveredToLuShi = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task5: 移除小卢的信，添加卢氏的信");
                    taskManager.inventoryManager.RemoveLetter("小卢的信");
                    Sprite icon = Resources.Load<Sprite>("lushi"); // 从 Resources 加载卢氏的图标
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "卢氏的信",
                        content = "墨守先生，久未与君言，不知近况如何。简工言欲制一木架，君腿可痛否？君曾言此伤常扰，疫病天寒，君多留意，莫硬撑。墨诗提及君，言君依旧，口硬如石，然管他诗事渐少，可是父子情好些许？吾为彼喜。彼常言君不解诗，然吾见彼甚念君。\r\n" +
                                  "近日小卢常问其父。彼渐长，好奇甚，问彼父何在，欲知其事。吾知彼殞于战，君部下也，流矢夺命，吾未尝责君，战乱无情，君生归已奇迹。多年来，君助甚多，衣物、粮草，常有君份，吾铭记此恩。墨诗言君心难安，憾未保彼，吾不欲君负此担。彼去时，吾未见最后一面，君必忆彼乎？彼笑、彼性，或彼日言何……吾欲闻，纵数语，吾好告小卢，使知父何人也。\r\n" +
                                  "小卢欲吾画星空，吾不能，唯画彼面，愈画愈模糊。君若忆何，书归，不急，吾待。谢君，墨守，多年矣，君仍彼兄弟也。——卢氏\r\n",
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
            "【卢氏】你又来了，小木人。咦？小卢给我的信，这孩子真会玩，住一起还让你送……",
            "【……】",
            "【卢氏】唉……他说想知道爹的事，还让我画星空……这孩子，天真得让我心疼。",
            "【卢氏】他爹死在战场上，是墨守的手下，你应该认识墨守。",
            "【卢氏】我……没能见他最后一面，只知道流箭要了他的命。",
            "【卢氏】小卢老问爹在哪儿，我只能说他爹在远方看着他，等他长大了回来……",
            "【卢氏】不知道这么说对不对，我只能画画了。",
            "【卢氏】我想画从前的东西，田野、溪流，他爹的笑脸，可每次下笔，手都抖，画出来的只有模糊的影子。",
            "【卢氏】我想为他画一个没有战争的明天，星空很美，孩子在草地上跑，可现在画不出来，也许以后能吧……",
            "【卢氏】这封信给墨守，麻烦你送过去。",
            "【卢氏】墨诗说墨守常自责，说战场上没保住我丈夫……我不怪他，谢谢他这些年照顾我们。",
            "【卢氏】谢谢你跑这一趟，小木人，你是村里最忙的。"
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
            taskCompleteText.text = "任务5——画中之星";
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