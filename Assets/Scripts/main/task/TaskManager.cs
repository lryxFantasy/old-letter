using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class TaskManager : MonoBehaviour
{
    // UI 相关字段
    public GameObject taskPanel, taskMask, dialoguePanel, normalDialoguePanel;
    public Button taskButton, closeButton, nextButton, deliverButton, saveButton, loadButton;
    public TMP_Text taskTitle, taskObjective, dialogueText;

    // 核心组件引用
    public PlayerController playerController;
    public InventoryManager inventoryManager;
    public CameraController cameraController;
    private RubyController rubyController;

    // 状态变量
    private bool isPanelOpen;
    private float previousTimeScale;
    public TaskBase currentTask;
    private string currentNPCName;
    private string savePath;

    void Start()
    {
        // 初始化 UI
        taskPanel.SetActive(false);
        taskMask.SetActive(false);
        dialoguePanel.SetActive(false);

        // 绑定按钮事件
        taskButton.onClick.AddListener(ToggleTaskPanel);
        closeButton.onClick.AddListener(ToggleTaskPanel);
        InitializeDeliverButton();
        saveButton.onClick.AddListener(SaveGame);
        loadButton.onClick.AddListener(LoadGame);

        // 启动初始任务
        currentTask = gameObject.AddComponent<Task0>();
        (currentTask as Task0)?.SetupDialogueUI(dialoguePanel, dialogueText, nextButton);
        (currentTask as Task0)?.StartTaskDialogue();
        UpdateTaskDisplay();

        previousTimeScale = Time.timeScale;
        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) ToggleTaskPanel();

        // 更新对话状态
        currentNPCName = playerController.IsInDialogue() ? playerController.GetCurrentNPCRole() : null;
        if (!string.IsNullOrEmpty(currentNPCName)) Debug.Log($"当前NPC: {currentNPCName}");
    }

    // 切换任务面板
    public void ToggleTaskPanel()
    {
        isPanelOpen = !isPanelOpen;
        Time.timeScale = isPanelOpen ? 0f : previousTimeScale;
        taskPanel.SetActive(isPanelOpen);
        taskMask.SetActive(isPanelOpen);
        dialoguePanel.SetActive(!isPanelOpen && dialoguePanel.activeSelf);
    }

    // 初始化送信按钮
    private void InitializeDeliverButton()
    {
        if (deliverButton == null) Debug.LogError("deliverButton 未绑定");
        else deliverButton.onClick.AddListener(TriggerDeliverLetter);
    }

    // 触发送信逻辑
    public void TriggerDeliverLetter()
    {
        if (currentTask == null || string.IsNullOrEmpty(currentNPCName))
        {
            Debug.LogWarning("送信失败: 任务或NPC未设置");
            return;
        }
        if (normalDialoguePanel != null) normalDialoguePanel.SetActive(false);
        currentTask.DeliverLetter(currentNPCName);
    }

    // 更新任务显示
    public void UpdateTaskDisplay()
    {
        if (currentTask != null)
        {
            taskTitle.text = currentTask.GetTaskName();
            taskObjective.text = currentTask.GetTaskObjective();
        }
    }

    // 设置新任务
    public void SetTask(TaskBase newTask)
    {
        if (currentTask != null) Destroy(currentTask);
        currentTask = newTask;
        UpdateTaskDisplay();
        Sprite icon = Resources.Load<Sprite>("jane"); // 从 Resources 加载图标

        if (inventoryManager != null && !(newTask is Task0) && newTask is Task1)
            inventoryManager.AddLetter(new Letter 
            { 
                title = "简姝儿给墨守的信", 
                content = "墨守兄，天象乱，彗星贯紫宫，日食蔽日，洪水滔天，田毁人困，墨科村危在旦夕。吾依《墨子》“杠杆胜重”之理，以桑木嵌滑轮、齿轮，制木童，虽简陋，行走可信。洪患阻路，驿站尽断，唯书信通智，木童代步，乃现唯一途。\r\n\r\n兄避世久矣，然《墨子》云“兼爱济世”，昔与卢平约救苍生，兄心未冷乎？吾研水车引洪之术，欲以滑轮提升效率，疏洪救田，然力不足，缺兄杠杆之妙。若兄有意，付木童一策，助吾成水闸，护农田，报朝廷，墨科村或可生还。盼兄回言，勿辞。——简姝儿\r\n",
                icon = icon
            });
    }

    // 保存游戏状态
    public void SaveGame()
    {
        SaveData data = new SaveData
        {
            taskNumber = GetTaskNumber(),
            playerPosition = playerController.transform.position,
            letters = inventoryManager.letters,
            taskStateJson = JsonUtility.ToJson(currentTask),
            isIndoors = cameraController?.IsIndoors() ?? false,
            currentHouseIndex = cameraController?.currentHouseIndex ?? -1,
            lastPlayerMapPosition = cameraController?.lastPlayerMapPosition ?? Vector3.zero,
            npcFavorabilityList = playerController.GetFavorabilityData() // 获取灵犀度数据
        };

        File.WriteAllText(savePath, JsonUtility.ToJson(data));
        Debug.Log("已保存到: " + savePath);
    }

    // 加载游戏状态
    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("存档文件不存在");
            return;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));
        SetTaskFromNumber(data.taskNumber);
        JsonUtility.FromJsonOverwrite(data.taskStateJson, currentTask);

        // 重新初始化任务
        if (currentTask is Task0 t0) t0.SetupDialogueUI(dialoguePanel, dialogueText, nextButton);
        else if (currentTask is Task1 t1)
        {
            t1.SetupDialogueUI(dialoguePanel, dialogueText, nextButton);
            t1.SetupDeliverButton(normalDialoguePanel, deliverButton);
        }
        else if (currentTask is Task2 t2) t2.SetupTask(this, dialoguePanel, dialogueText, nextButton);
        else if (currentTask is Task3 t3) t3.SetupTask(this, dialoguePanel, dialogueText, nextButton);
        else if (currentTask is Task4 t4) t4.SetupTask(this, dialoguePanel, dialogueText, nextButton);
        else if (currentTask is Task5 t5) t5.SetupTask(this, dialoguePanel, dialogueText, nextButton);
        else if (currentTask is Task6 t6) t6.SetupTask(this, dialoguePanel, dialogueText, nextButton);
        else if (currentTask is Task7 t7) t7.SetupTask(this, dialoguePanel, dialogueText, nextButton);

        // 恢复玩家和相机
        if (cameraController != null)
        {
            playerController.transform.position = data.playerPosition;
            cameraController.isIndoors = data.isIndoors;
            cameraController.currentHouseIndex = data.currentHouseIndex;
            cameraController.lastPlayerMapPosition = data.lastPlayerMapPosition;
            cameraController.transform.position = data.isIndoors
                ? cameraController.housePositions[data.currentHouseIndex]
                : new Vector3(data.playerPosition.x, data.playerPosition.y, cameraController.transform.position.z) + cameraController.offset;
        }

        // 加载灵犀度数据
        playerController.LoadFavorabilityData(data.npcFavorabilityList);

        inventoryManager.letters = data.letters;
        inventoryManager.UpdateInventoryUI();
        InitializeDeliverButton();
        UpdateTaskDisplay();

        rubyController = FindObjectOfType<RubyController>(); // 获取 RubyController
        rubyController.pauseHealthUpdate = false; // 恢复血量更新

        Debug.Log("游戏已加载");
    }

    // 获取任务编号
    private int GetTaskNumber() => currentTask switch
    {
        Task0 => 0,
        Task1 => 1,
        Task2 => 2,
        Task3 => 3,
        Task4 => 4,
        Task5 => 5,
        Task6 => 6,
        Task7 => 7,
        _ => -1
    };

    // 根据编号设置任务
    private void SetTaskFromNumber(int taskNumber)
    {
        if (currentTask != null) Destroy(currentTask);
        currentTask = taskNumber switch
        {
            0 => gameObject.AddComponent<Task0>(),
            1 => gameObject.AddComponent<Task1>(),
            2 => gameObject.AddComponent<Task2>(),
            3 => gameObject.AddComponent<Task3>(),
            4 => gameObject.AddComponent<Task4>(),
            5 => gameObject.AddComponent<Task5>(),
            6 => gameObject.AddComponent<Task6>(),
            7 => gameObject.AddComponent<Task7>(),
            _ => null
        };
        if (currentTask == null) Debug.LogError("未知任务编号: " + taskNumber);
    }
}