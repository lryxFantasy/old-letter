using UnityEngine;

public class TransparentPlayerController : MonoBehaviour
{
    public GameObject mainPlayer; // 主角对象，同步位置用
    private PlayerController mainPlayerController; // 主角的 PlayerController，用于检测对话状态

    void Start()
    {
        // 获取主角的 PlayerController 组件
        if (mainPlayer != null)
        {
            mainPlayerController = mainPlayer.GetComponent<PlayerController>();
            if (mainPlayerController == null)
            {
                Debug.LogError("主角色上未找到 PlayerController 组件！");
            }
        }
        else
        {
            Debug.LogError("请在 Inspector 中指定 Main Player！");
        }
    }

    void Update()
    {
        // 同步位置到主角
        if (mainPlayer != null)
        {
            transform.position = mainPlayer.transform.position;
        }

        // 如果主角在对话中，暂停其他逻辑（这里可以扩展，比如暂停动画）
        if (mainPlayerController != null && mainPlayerController.IsInDialogue())
        {
            return; // 暂停后续逻辑
        }
    }

    // 提供给外部检测对话状态的接口（与主角一致）
    public bool IsInDialogue()
    {
        return mainPlayerController != null && mainPlayerController.IsInDialogue();
    }
}