using UnityEngine;
using UnityEngine.UI; // 用于Image组件

public class NPCImageLoader : MonoBehaviour
{
    [SerializeField] private Image npcImage; // 用于显示NPC图片的UI Image组件
    [SerializeField] private PlayerController playerController; // 引用PlayerController以获取当前NPC

    private string currentNPCRole; // 当前NPC的角色名称

    void Start()
    {
        // 如果未在Inspector中赋值，尝试自动查找组件
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("NPCImageLoader: 未找到 PlayerController！");
            }
        }

        if (npcImage == null)
        {
            Debug.LogError("NPCImageLoader: 请在Inspector中赋值 npcImage！");
        }

        // 初始化时隐藏图片
        if (npcImage != null)
        {
            npcImage.enabled = false;
        }
    }

    void Update()
    {
        // 获取当前NPC角色
        string newNPCRole = playerController?.GetCurrentNPCRole();

        // 如果在对话中，确保加载图片
        if (playerController.IsInDialogue() && !string.IsNullOrEmpty(newNPCRole))
        {
            if (newNPCRole != currentNPCRole || !npcImage.enabled) // NPC变化或图片未显示时加载
            {
                currentNPCRole = newNPCRole;
                LoadNPCImage(currentNPCRole);
            }
        }
        else if (!playerController.IsInDialogue() && npcImage.enabled)
        {
            // 如果不在对话中，隐藏图片
            npcImage.enabled = false;
            currentNPCRole = null; // 重置当前NPC角色
        }
    }

    // 根据NPC名称加载图片
    private void LoadNPCImage(string npcRole)
    {
        if (string.IsNullOrEmpty(npcRole) || npcImage == null)
        {
            Debug.LogWarning("NPCImageLoader: NPC角色名称为空或Image组件未赋值");
            return;
        }

        // 从Resources文件夹加载Sprite，假设文件名与npcRole一致
        Sprite npcSprite = Resources.Load<Sprite>($"NPCImages/{npcRole}");
        if (npcSprite != null)
        {
            npcImage.sprite = npcSprite;
            npcImage.enabled = true; // 显示图片
            Debug.Log($"NPCImageLoader: 已加载 {npcRole} 的图片");
        }
        else
        {
            npcImage.enabled = false; // 隐藏图片
            Debug.LogWarning($"NPCImageLoader: 未找到 {npcRole} 的图片，请检查Resources/NPCImages文件夹");
        }
    }
}