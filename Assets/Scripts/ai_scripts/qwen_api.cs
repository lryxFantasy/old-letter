using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;

public class QwenChat : MonoBehaviour
{
    public InputField inputField;
    public Text responseText;
    public Button sendButton;

    private string apiKey = "sk-e11794d89a4f492988f8b2b39a4ddf0a"; // 千问 API Key
    private string apiUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions"; // 千问 API URL
    private List<Message> conversationHistory = new List<Message>();

    [System.Serializable]
    private class RequestBody
    {
        public string model = "qwen-plus-2025-01-25"; // 千问模型
        public Message[] messages;
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class QwenResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }

    void Start()
    {
        sendButton.onClick.AddListener(() => SendMessageToQwen()); // 使用 Lambda 表达式
        conversationHistory.Add(new Message { role = "system", content = "游戏背景：2111年3月16日，一枚导弹击中太阳，引发太阳黑子连锁反应，辐射风暴席卷地球，全球通信崩溃，互联网消失，人类失去阳光，只能躲避辐射，蜷缩室内，永远不能出门，植被疯长淹没城市废墟，FANLU-318 AI机器人成为唯一沟通桥梁，传递书信连接破碎世界，故事发生在“太阳战争”后30年的信火村，村民命运因信件交织，你是他们的信使。角色简介及关系：维克托·凯恩（老兵），退役军官，太阳战争亲历者，妻子辐射病去世因流弹击伤腿伤退役，沉默悲观极度厌恶科技，伊莱亚斯·凯恩的父亲，关系僵硬因儿子弃军从诗，小卢克·伍德亡父的上司间接牵连其孤儿命运，写信严肃简练如军令；伊莱亚斯·凯恩（诗人），年轻诗人反战者，维克托之子，热情洋溢追求艺术自由，母死后与父意见不合，简·怀特的恋人感性与理性碰撞是灵感源泉，但简呆板不解风情有时产生矛盾，写信诗意浓厚情感外露充满隐喻；简·怀特（工程师），理工学生擅长科技制造，冷静理性科技救世主义者，伊莱亚斯的女友受辐射限制无法常聚，萝丝的远房孙女战后受其指引来村，写信简洁理性附技术细节微露关怀，情商低；萝丝（老奶奶），普通老妇小卢克的神秘笔友，温柔神秘心怀悲悯与希望，简的远房祖母战后指引其逃难，小卢克的笔友以“神秘朋友”寄托因辐射病死去孙子的情感，写信温暖柔和充满故事与生活气息；小卢克·伍德（孩子），战争孤儿梦想成为科学家，天真乐观好奇心旺盛，伊芙·伍德的儿子其父死于维克托麾下，不知道笔友是谁受其信件启发成长，与母住在一块，写信童趣跳跃夸张好奇多问号；伊芙·伍德（画家），画家小卢克之母用艺术缅怀亡夫，感性执着坚韧不屈，小卢克的母亲丈夫死于战争与村人疏远唯独信件寄托情感，写信优美细腻色彩感强常附速写。任务：你将扮演工程师简与我（机器信使FANLU）对话，回复需不超30字，符合场景，对话不要太文本化，一定不要要求送信，要口语化极度贴合人设，不要提问，回复前加“【人名】：”" });
        SendMessageToQwen("你好"); // 初始化时发送空消息
        Debug.Log("QwenChat 初始化完成");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) SendMessageToQwen();
    }

    void SendMessageToQwen(string message = null)
    {
        message = message ?? inputField.text;
        if (string.IsNullOrEmpty(message))
        {
            responseText.text = "请输入内容！";
            Debug.LogWarning("用户输入为空");
            return;
        }

        conversationHistory.Add(new Message { role = "user", content = message });
        responseText.text = "正在思考...";
        StartCoroutine(CallQwenAPI());
    }

    IEnumerator CallQwenAPI()
    {
        var requestBody = new RequestBody { messages = conversationHistory.ToArray() };
        string jsonBody = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        Debug.Log("请求 JSON: " + jsonBody);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        Debug.Log("发送请求到: " + apiUrl);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            Debug.Log("收到响应: " + response);
            QwenResponse jsonResponse = JsonUtility.FromJson<QwenResponse>(response);
            string aiResponse = jsonResponse.choices[0].message.content;
            responseText.text = aiResponse;
            conversationHistory.Add(new Message { role = "assistant", content = aiResponse });
            Debug.Log("解析后的回复: " + aiResponse);
        }
        else
        {
            responseText.text = "错误: " + request.error;
            Debug.LogError("API 请求失败: " + request.error + "\n响应: " + request.downloadHandler.text);
        }
    }
}