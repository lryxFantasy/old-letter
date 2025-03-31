using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using System.IO;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 2f; // 交互距离（用于NPC和门）
    [SerializeField] private TMP_Text interactionText; // 交互提示文本
    [SerializeField] private Sprite bottomSpriteImage; // 交互提示底图
    [SerializeField] private Vector3 offset = new Vector3(50f, 50f, 0f); // UI偏移量
    [SerializeField] private float fadeDuration = 0.1f; // 淡入淡出时间
    [SerializeField] private TMP_Text dialogueText; // 对话文本
    [SerializeField] private GameObject dialoguePanel; // 对话面板
    [SerializeField] private Button optionButtonPrefab; // 选项按钮预制体
    [SerializeField] private Transform optionsContainer; // 选项容器
    [SerializeField] private Slider favorabilitySlider; // 好感度进度条
    [SerializeField] private TMP_Text favorabilityText; // 好感度数值文本

    [SerializeField] private CameraController cameraController; // 引用相机控制脚本
    private Transform nearestDoor;
    private int nearestDoorIndex = -1; // 门的索引（0-5，对应六个房屋）
    private bool isNearExitDoor = false; // 是否靠近出口门
    private bool canEnterHouse; // 是否可以进入房屋

    private Transform nearestNPC;
    private NPC currentNPC;
    private WalkAnimation currentNPCWalkAnimation;
    private bool canInteract, isInDialogue, isFading;
    private Camera mainCamera;
    private Image bottomSprite;
    private Coroutine sendMessageCoroutine;

    private const string apiKey = "sk-e11794d89a4f492988f8b2b39a4ddf0a"; // API密钥
    private const string apiUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions"; // API地址
    private List<Message> conversationHistory = new List<Message>();
    private List<Button> optionButtons = new List<Button>();
    private Dictionary<string, int> npcFavorability = new Dictionary<string, int>(); // NPC好感度

    [System.Serializable] private class RequestBody { public string model = "qwen-plus"; public Message[] messages; public float temperature = 1.2f; }
    [System.Serializable] public class Message { public string role; public string content; }
    [System.Serializable] public class QwenResponse { public Choice[] choices; }
    [System.Serializable] public class Choice { public Message message; }

    void Start()
    {

        mainCamera = Camera.main;
        interactionText.gameObject.SetActive(false);
        dialoguePanel.SetActive(false);
        bottomSprite = new GameObject("BottomSprite", typeof(Image)).GetComponent<Image>();
        bottomSprite.transform.SetParent(interactionText.transform.parent);
        bottomSprite.sprite = bottomSpriteImage;
        AdjustUISizeAndPosition();
        interactionText.alpha = 0f;
        bottomSprite.canvasRenderer.SetAlpha(0f);
        interactionText.gameObject.SetActive(true);
        interactionText.alpha = 1f;
        bottomSprite.transform.SetAsFirstSibling();

        // 初始化进度条
        if (favorabilitySlider != null)
        {
            favorabilitySlider.minValue = -50;
            favorabilitySlider.maxValue = 50;
            favorabilitySlider.value = 0;
        }

        // 初始化好感度文本
        if (favorabilityText != null)
        {
            favorabilityText.text = "好感度: 0";
        }
    }

    void AdjustUISizeAndPosition()
    {
        float scaleFactor = Screen.width / 1280f;
        bottomSprite.rectTransform.sizeDelta = new Vector2(95f * scaleFactor, 55f * scaleFactor);
        offset = new Vector3(50f * scaleFactor, 30f * scaleFactor, 0f);
        bottomSprite.rectTransform.anchoredPosition = new Vector2(0, -20f * scaleFactor);
    }

    void Update()
    {
        CheckForNearbyNPC();
        CheckForNearbyDoor();

        if (PauseMenu.IsPaused && Input.GetKeyDown(KeyCode.E))
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isInDialogue)
            {
                EndDialogue();
            }
            else if (canInteract)
            {
                StartDialogue();
            }
            else if (canEnterHouse)
            {
                if (!cameraController.IsIndoors() && !isNearExitDoor)
                {
                    cameraController.EnterHouse(nearestDoorIndex);
                }
                else if (cameraController.IsIndoors() && isNearExitDoor)
                {
                    cameraController.ExitHouse();
                }
            }
        }
    }

    void LateUpdate()
    {
        if (interactionText.gameObject.activeSelf)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
            Vector3 targetTextPos = screenPos + new Vector3(offset.x * 1.25f, offset.y * 1.2f, 0f);
            Vector3 targetSpritePos = screenPos + offset - new Vector3(0, -5f * Screen.width / 1280f, 0);

            interactionText.rectTransform.position = Vector3.Lerp(interactionText.rectTransform.position, targetTextPos, 0.1f);
            bottomSprite.rectTransform.position = Vector3.Lerp(bottomSprite.rectTransform.position, targetSpritePos, 0.1f);
        }
    }

    public bool IsInDialogue() => isInDialogue;

    void CheckForNearbyNPC()
    {
        float closestDistance = Mathf.Infinity;
        foreach (GameObject npc in GameObject.FindGameObjectsWithTag("NPC"))
        {
            float distance = Vector2.Distance(transform.position, npc.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestNPC = npc.transform;
                currentNPC = npc.GetComponent<NPC>();
                currentNPCWalkAnimation = npc.GetComponent<WalkAnimation>();
            }
        }
        bool inRange = closestDistance <= interactionDistance;
        if (inRange && !canInteract && !isFading && !isInDialogue && !canEnterHouse)
        {
            canInteract = true;
            isFading = true;
            interactionText.text = "按 E 对话";
            interactionText.gameObject.SetActive(true);
            bottomSprite.gameObject.SetActive(true);
            FadeIn();
        }
        else if (!inRange && canInteract && !isFading && !isInDialogue)
        {
            canInteract = false;
            isFading = true;
            FadeOut();
        }
    }

    void CheckForNearbyDoor()
    {
        float closestDistance = Mathf.Infinity;
        nearestDoor = null;
        nearestDoorIndex = -1;
        isNearExitDoor = false;

        foreach (GameObject door in GameObject.FindGameObjectsWithTag("Door"))
        {
            float distance = Vector2.Distance(transform.position, door.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestDoor = door.transform;
                string doorName = door.name;
                if (doorName.Contains("Door_"))
                {
                    string indexStr = doorName.Replace("Door_", "");
                    if (int.TryParse(indexStr, out int index))
                    {
                        nearestDoorIndex = index;
                    }
                }
                if (doorName.ToLower().Contains("exit"))
                {
                    isNearExitDoor = true;
                }
                else
                {
                    isNearExitDoor = false;
                }
            }
        }

        bool inRange = closestDistance <= interactionDistance;
        if (inRange && !canEnterHouse && !isFading && !isInDialogue && !canInteract)
        {
            canEnterHouse = true;
            isFading = true;
            interactionText.text = isNearExitDoor ? "按 E 离开" : "按 E 进入";
            interactionText.gameObject.SetActive(true);
            bottomSprite.gameObject.SetActive(true);
            FadeIn();
        }
        else if (!inRange && canEnterHouse && !isFading && !isInDialogue)
        {
            canEnterHouse = false;
            isFading = true;
            FadeOut();
        }
    }

    void FadeIn()
    {
        interactionText.CrossFadeAlpha(1f, fadeDuration, false);
        bottomSprite.CrossFadeAlpha(1f, fadeDuration, false);
        Invoke(nameof(ResetFade), fadeDuration);
    }

    void FadeOut()
    {
        interactionText.CrossFadeAlpha(0f, fadeDuration, false);
        bottomSprite.CrossFadeAlpha(0f, fadeDuration, false);
        Invoke(nameof(ResetFade), fadeDuration);
    }

    void ResetFade()
    {
        isFading = false;
        if (!canInteract && !canEnterHouse)
        {
            interactionText.gameObject.SetActive(false);
            bottomSprite.gameObject.SetActive(false);
        }
    }

    void StartDialogue()
    {
        if (currentNPC == null || string.IsNullOrEmpty(currentNPC.role)) return;
        Debug.Log($"PlayerController: 开始对话，NPC角色：{currentNPC.role}");
        isInDialogue = true;
        canInteract = false;
        interactionText.gameObject.SetActive(false);
        bottomSprite.gameObject.SetActive(false);
        dialoguePanel.SetActive(true);
        dialogueText.text = "正在思考...";
        ClearOptionButtons();
        if (currentNPCWalkAnimation != null) currentNPCWalkAnimation.SetWalkingState(false);
        conversationHistory.Clear();
        conversationHistory.Add(new Message
        {
            role = "system",
            content = $"游戏背景：641年3月16日，长安外天象异象（彗星日食），朝廷命研天象，墨科村人疫病隔无法出门，唯书信交流，由村中工匠简工依《墨子》力学之理（杠杆与滑轮）所制之木信使墨信为沟通桥，传《墨子》力学、《考工记》机械、《甘石星经》天文智，故事于天象异象后三月墨科村，村人命运因信交织，吾为其信使。角色简介及关系：墨守（力学研者），墨家后人，战乱伤退，沉默疑新科学，墨诗父，关系僵因子弃力学从诗，卢氏亡夫上司间接致孤儿，严肃如军令；墨诗（光学研者），年轻学者反战，墨守子，热情求科学艺合，母亡后与父不合，简工恋人感性理性激为灵感，诗意情露；简工（工匠），擅机械，冷静实用，墨诗恋人疫病限难聚，罗婆远房孙女，理性附术详微露关怀；罗婆（星象学家），研《甘石星经》，温柔睿智，简远房祖母，小卢“神秘友人”，寄亡孙情，信温润含旧时智；小卢（童子），战乱孤儿梦天文学家，天真乐观好奇，卢氏子，父死战乱，神秘友人以为父，受信启成长，信童趣跳跃好奇多问；卢氏（画家），以艺缅夫，感性执韧，卢氏母，与村人疏唯信寄情，信优美细腻色感强，小卢卢氏同居不需送信；吾，木信使墨信，不明村人关系，具情重逻辑，好奇，喜问物理研。任务：你将扮演{currentNPC.role}与我（信使墨信）对话，人物绝对绝对无法外出，回复需不超30字，符合场景，对话口语化极度贴合人设，不要提问，回复前加“【{currentNPC.role}】：”。每次回复后，生成两个选项，一定是作为我的回应，符合我的人设，选项需直接回复你的对话或询问你的过去，可加标点，格式为“选项1：xxx\n选项2：xxx”，确保选项明确为FANLU的回答。此外，每次回复在对话后标明好感度变化，格式只有：（+1）或（-1）或（+0），根据对话内容判断我的回应是否让{currentNPC.role}感到满意或不快，积极肯定增加好感，否定冷漠减少好感，格式为“【{currentNPC.role}】：对话内容（好感度变化）”。以上内容全部用中文，不许英文。"
        });

        // 更新好感度进度条和文本
        UpdateFavorabilitySlider();

        sendMessageCoroutine = StartCoroutine(SendMessageToQwen("你好"));
    }

    public void EndDialogue()
    {
        isInDialogue = false;
        dialoguePanel.SetActive(false);
        ClearOptionButtons();
        if (sendMessageCoroutine != null) { StopCoroutine(sendMessageCoroutine); sendMessageCoroutine = null; }
        if (currentNPCWalkAnimation != null) currentNPCWalkAnimation.SetWalkingState(true);
        CheckForNearbyNPC();
        CheckForNearbyDoor();
    }

    void ClearOptionButtons()
    {
        foreach (var button in optionButtons) Destroy(button.gameObject);
        optionButtons.Clear();
    }

    void CreateOptionButtons(string[] options)
    {
        ClearOptionButtons();
        foreach (string option in options)
        {
            Button button = Instantiate(optionButtonPrefab, optionsContainer);
            button.GetComponentInChildren<TMP_Text>().text = option;
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
            button.onClick.AddListener(() => OnOptionSelected(option));
            optionButtons.Add(button);
        }
    }

    void OnOptionSelected(string option)
    {
        dialogueText.text = "正在思考...";
        ClearOptionButtons();
        sendMessageCoroutine = StartCoroutine(SendMessageToQwen(option));
    }

    IEnumerator SendMessageToQwen(string message)
    {
        conversationHistory.Add(new Message { role = "user", content = message });
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new RequestBody { messages = conversationHistory.ToArray() }))),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            QwenResponse response = JsonUtility.FromJson<QwenResponse>(request.downloadHandler.text);
            string aiResponse = response.choices[0].message.content;
            Debug.Log($"AI原始回复: {aiResponse}"); // 检查AI回复

            string[] parts = aiResponse.Split(new[] { "\n选项1：" }, System.StringSplitOptions.None);
            if (parts.Length < 2) { dialogueText.text = "错误：AI回复格式不正确\n" + aiResponse; yield break; }

            string dialogueWithFavor = parts[0].Trim();
            int favorChange = 0;
            string dialogue = dialogueWithFavor;

            // 使用正则表达式匹配好感度变化，兼容全角/半角括号和空格
            var favorMatch = Regex.Match(dialogueWithFavor, @"[\(（]\s*[+-]\d+\s*[\)）]");
            if (favorMatch.Success)
            {
                string favorText = favorMatch.Value;
                if (favorText.Contains("+1"))
                {
                    favorChange = 1;
                }
                else if (favorText.Contains("-1"))
                {
                    favorChange = -1;
                }
                else if (favorText.Contains("+0")) // 支持 +0
                {
                    favorChange = 0; // 好感度不变
                }
                // 移除好感度标记
                dialogue = Regex.Replace(dialogueWithFavor, @"[\(（]\s*[+-]\d+\s*[\)）]", "").Trim();
            }

            // 更新好感度
            if (currentNPC != null)
            {
                string npcRole = currentNPC.role;
                if (!npcFavorability.ContainsKey(npcRole)) npcFavorability[npcRole] = 0;
                npcFavorability[npcRole] += favorChange;
                npcFavorability[npcRole] = Mathf.Clamp(npcFavorability[npcRole], -50, 50); // 限制好感度范围
                currentNPC.favorability = npcFavorability[npcRole];
                Debug.Log($"{npcRole} 好感度: {npcFavorability[npcRole]}");

                // 更新好感度进度条和文本
                UpdateFavorabilitySlider();
            }

            string[] optionLines = ("选项1：" + parts[1]).Split('\n');
            string[] options = new string[2];
            for (int i = 0; i < 2; i++) options[i] = Regex.Replace(optionLines[i].Trim(), @"^选项\d+：", "");
            dialogueText.text = dialogue;
            CreateOptionButtons(options);
            conversationHistory.Add(new Message { role = "assistant", content = aiResponse });
        }
        else dialogueText.text = "错误: " + request.error;
    }

    // 更新好感度进度条和文本
    private void UpdateFavorabilitySlider()
    {
        if (favorabilitySlider != null && currentNPC != null)
        {
            string npcRole = currentNPC.role;
            int favorability = npcFavorability.ContainsKey(npcRole) ? npcFavorability[npcRole] : 0;
            favorabilitySlider.value = favorability; // 更新进度条

            // 更新好感度文本
            if (favorabilityText != null)
            {
                favorabilityText.text = $"好感度: {favorability}";
            }


        }
    }

    public string GetCurrentNPCRole()
    {
        return currentNPC != null ? currentNPC.role : null;
    }

    // 提供好感度数据给 TaskManager
    public List<SerializableFavorability> GetFavorabilityData()
    {
        List<SerializableFavorability> favorabilityList = new List<SerializableFavorability>();
        foreach (var pair in npcFavorability)
        {
            favorabilityList.Add(new SerializableFavorability
            {
                npcRole = pair.Key,
                favorability = pair.Value
            });
        }
        return favorabilityList;
    }

    // 加载好感度数据
    public void LoadFavorabilityData(List<SerializableFavorability> favorabilityList)
    {
        npcFavorability.Clear();
        if (favorabilityList != null)
        {
            foreach (var favor in favorabilityList)
            {
                npcFavorability[favor.npcRole] = favor.favorability;
            }
        }

        // 同步好感度到 NPC 实例
        foreach (GameObject npc in GameObject.FindGameObjectsWithTag("NPC"))
        {
            NPC npcComponent = npc.GetComponent<NPC>();
            if (npcFavorability.ContainsKey(npcComponent.role))
            {
                npcComponent.favorability = npcFavorability[npcComponent.role];
            }
            else
            {
                npcComponent.favorability = 0; // 未保存的NPC初始化为0
                npcFavorability[npcComponent.role] = 0;
            }
        }

        // 加载后更新进度条和文本
        UpdateFavorabilitySlider();
    }

    // 添加 getter 方法以访问 interactionText 和 bottomSprite
    public TMP_Text GetInteractionText()
    {
        return interactionText;
    }

    public Image GetBottomSprite()
    {
        return bottomSprite;
    }
}