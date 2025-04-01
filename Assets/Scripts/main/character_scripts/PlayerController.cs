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
    [SerializeField] private Slider favorabilitySlider; // 灵犀度进度条
    [SerializeField] private TMP_Text favorabilityText; // 灵犀度数值文本

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
    private Dictionary<string, int> npcFavorability = new Dictionary<string, int>(); // NPC灵犀度

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

        // 初始化灵犀度文本
        if (favorabilityText != null)
        {
            favorabilityText.text = "灵犀度: 0";
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
            content = $"贞观十五年三月十六，长安外墨家村，天象异变——彗星划空，日食蔽天，继而大雨滂沱，农田尽毁，朝廷急诏献策抗灾。水患阻隔村人，唯书信往来，女工匠简姝儿依《墨子》力学打造木偶“木童”送信，传递力学、天文、农事智慧。灾后三月，村人命运因信交织，吾乃木童，串联物理与人性。村长墨守，前朝官员，因党争被贬，精通杠杆却避世自居，认定天灾难抗，手握墨家手稿秘不示人，与子墨成僵持，信中沉重含愧；墨成，年轻学者，热血入世，欲献策救天下，书信激昂且满载力学思辨；简姝儿冷静务实，改良水车抗洪，制吾为使，与墨成合论机关，信中言简意赅；罗婆，长者通晓墨史，念亡孙寄情小卢，信温润藏旧智；小卢，十岁孤儿，卢平之子，好奇水车，随简姝儿摆弄吾身，信中童趣多问，追忆父志；卢氏，画师寡母，研水车平衡，夫亡后承志，信优美寄缅怀。墨守闭守秘密，墨成力争献策，简姝儿与卢氏机关相助又相争，罗婆维系团结，小卢懵懂承遗志，吾以送信连系众人，命运交错于物理与人心之间。任务：你将扮演{currentNPC.role}与我（木童）对话，回复需不超30字，符合场景，对话口语化极度贴合人设，不要提问，回复前加“【{currentNPC.role}】：”。每次回复后，生成两个选项，一定是作为我的回应，符合我的人设，选项需直接回复你的对话或询问你的过去，可加标点，格式为“选项1：xxx\n选项2：xxx”，确保选项明确为我的回答。此外，每次回复在对话后标明好感度变化，格式需带有括号只有：（+1）或（-1）或（+0），根据对话内容判断我的回应是否让{currentNPC.role}感到满意或不快，积极肯定增加好感，否定冷漠减少好感，格式为“【{currentNPC.role}】：对话内容（好感度变化）”。回复全部用中文。"
        });

        // 更新灵犀度进度条和文本
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

            // 使用正则表达式匹配灵犀度变化，兼容全角/半角括号和空格
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
                    favorChange = 0; // 灵犀度不变
                }
                // 移除灵犀度标记
                dialogue = Regex.Replace(dialogueWithFavor, @"[\(（]\s*[+-]\d+\s*[\)）]", "").Trim();
            }

            // 更新灵犀度
            if (currentNPC != null)
            {
                string npcRole = currentNPC.role;
                if (!npcFavorability.ContainsKey(npcRole)) npcFavorability[npcRole] = 0;
                npcFavorability[npcRole] += favorChange;
                npcFavorability[npcRole] = Mathf.Clamp(npcFavorability[npcRole], -50, 50); // 限制灵犀度范围
                currentNPC.favorability = npcFavorability[npcRole];
                Debug.Log($"{npcRole} 灵犀度: {npcFavorability[npcRole]}");

                // 更新灵犀度进度条和文本
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

    // 更新灵犀度进度条和文本
    private void UpdateFavorabilitySlider()
    {
        if (favorabilitySlider != null && currentNPC != null)
        {
            string npcRole = currentNPC.role;
            int favorability = npcFavorability.ContainsKey(npcRole) ? npcFavorability[npcRole] : 0;
            favorabilitySlider.value = favorability; // 更新进度条

            // 更新灵犀度文本
            if (favorabilityText != null)
            {
                favorabilityText.text = $"灵犀度: {favorability}";
            }


        }
    }

    public string GetCurrentNPCRole()
    {
        return currentNPC != null ? currentNPC.role : null;
    }

    // 提供灵犀度数据给 TaskManager
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

    // 加载灵犀度数据
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

        // 同步灵犀度到 NPC 实例
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