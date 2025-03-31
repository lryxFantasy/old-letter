using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class HouseTrigger : MonoBehaviour
{
    public CameraController cameraController;
    public int houseIndex; // 房屋索引
    public bool isExitDoor = false; // 是否为出口
    private bool playerInTrigger = false;
    private PlayerController playerController;
    private TaskManager taskManager;
    private bool isShowingMessage = false;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("HouseTrigger: 未找到 PlayerController！");
        }

        taskManager = FindObjectOfType<TaskManager>();
        if (taskManager == null)
        {
            Debug.LogError("HouseTrigger: 未找到 TaskManager！");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            isShowingMessage = false;
        }
    }

    void Update()
    {
        // 处理玩家交互
        if (playerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            if (cameraController == null)
            {
                Debug.LogWarning("HouseTrigger: CameraController 未绑定！");
                return;
            }

            if (!cameraController.IsIndoors() && !isExitDoor)
            {
                if (houseIndex == 0) // 社区中心需要钥匙
                {
                    if (HasKey())
                    {
                        // 检查好感度并决定结局
                        CheckFavorabilityAndEndGame();
                    }
                    else
                    {
                        ShowNoKeyMessage();
                        Debug.Log("玩家没有钥匙，无法进入社区中心");
                    }
                }
                else
                {
                    cameraController.EnterHouse(houseIndex);
                    FindObjectOfType<RubyController>().UpdateLastHousePosition(cameraController.housePlayerPositions[houseIndex]);
                }
            }
            else if (cameraController.IsIndoors() && isExitDoor)
            {
                cameraController.ExitHouse();
            }
        }
    }

    // 检查是否有钥匙
    private bool HasKey()
    {
        if (playerController == null || taskManager == null || taskManager.inventoryManager == null)
        {
            Debug.LogWarning("HouseTrigger: PlayerController 或 InventoryManager 未正确初始化");
            return false;
        }

        foreach (var letter in taskManager.inventoryManager.letters)
        {
            if (letter.title == "废弃房屋的钥匙")
            {
                return true;
            }
        }
        return false;
    }

    // 检查所有NPC好感度并决定结局
    private void CheckFavorabilityAndEndGame()
    {
        var favorabilityData = playerController.GetFavorabilityData();
        bool allFavorabilityAbove5 = true;

        foreach (var favor in favorabilityData)
        {
            if (favor.favorability <= 10)
            {
                allFavorabilityAbove5 = false;
                break;
            }
        }

        if (allFavorabilityAbove5)
        {
            // 好感度全部大于10，进入Good Ending
            Debug.Log("所有NPC好感度大于10，进入Good Ending");
            cameraController.EnterHouse(houseIndex); // 进入废弃房屋
            FindObjectOfType<RubyController>().UpdateLastHousePosition(cameraController.housePlayerPositions[houseIndex]);
            TriggerGoodEnding();
        }
        else
        {
            // 存在好感度小于等于10，进入Bad Ending
            Debug.Log("存在NPC好感度小于等于10，进入Bad Ending");
            cameraController.EnterHouse(houseIndex); // 进入废弃房屋
            FindObjectOfType<RubyController>().UpdateLastHousePosition(cameraController.housePlayerPositions[houseIndex]);
            TriggerBadEnding();
        }
    }

    // 触发Good Ending
    private void TriggerGoodEnding()
    {
        // 跳转到Good Ending场景
        SceneManager.LoadScene("goodending");
    }

    // 触发Bad Ending
    private void TriggerBadEnding()
    {
        // 跳转到Bad Ending场景
        SceneManager.LoadScene("badending");
    }

    // 显示结局提示
    private IEnumerator ShowEnding(string title, string message)
    {
        if (playerController != null)
        {
            playerController.GetInteractionText().gameObject.SetActive(true);
            playerController.GetInteractionText().text = $"{title}\n{message}";
            playerController.GetBottomSprite().gameObject.SetActive(true);
            playerController.GetInteractionText().CrossFadeAlpha(1f, 0.1f, false);
            playerController.GetBottomSprite().CrossFadeAlpha(1f, 0.1f, false);

            yield return new WaitForSeconds(5f); // 显示5秒

            playerController.GetInteractionText().CrossFadeAlpha(0f, 0.1f, false);
            playerController.GetBottomSprite().CrossFadeAlpha(0f, 0.1f, false);
            yield return new WaitForSeconds(0.1f);
            playerController.GetInteractionText().gameObject.SetActive(false);
            playerController.GetBottomSprite().gameObject.SetActive(false);
        }
    }

    private void ShowNoKeyMessage()
    {
        if (playerController == null) return;

        if (!isShowingMessage)
        {
            isShowingMessage = true;
            playerController.GetInteractionText().gameObject.SetActive(true);
            playerController.GetInteractionText().text = "门上锁了";
            playerController.GetBottomSprite().gameObject.SetActive(true);
            playerController.GetInteractionText().CrossFadeAlpha(1f, 0.1f, false);
            playerController.GetBottomSprite().CrossFadeAlpha(1f, 0.1f, false);

            StartCoroutine(HideMessageAfterDelay(3f));
        }
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerController != null)
        {
            playerController.GetInteractionText().CrossFadeAlpha(0f, 0.1f, false);
            playerController.GetBottomSprite().CrossFadeAlpha(0f, 0.1f, false);
            yield return new WaitForSeconds(0.1f);
            playerController.GetInteractionText().gameObject.SetActive(false);
            playerController.GetBottomSprite().gameObject.SetActive(false);
        }
        isShowingMessage = false;
    }
}