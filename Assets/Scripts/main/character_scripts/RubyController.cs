using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RubyController : MonoBehaviour
{
    public float speed = 3.0f;
    public int maxHealth = 100; // 最大血量设为100
    public float timeInvincible = 2.0f;
    public Slider healthBar; // 血量条UI组件
    public TextMeshProUGUI healthText;
    public bool pauseHealthUpdate = false; // 新增变量，用于暂停血量更新

    public int health { get { return currentHealth; } }
    int currentHealth;

    bool isInvincible;
    float invincibleTimer;

    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;

    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);

    public GameObject projectilePrefab;
    private PlayerController playerController;
    private CameraController cameraController; // 用于判断室内外状态
    private Vector3 lastHousePosition; // 记录最后进入的房子位置
    private Vector3 teleportPosition = new Vector3(-7.3f, -2.5f, -6.1f);
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        playerController = GetComponent<PlayerController>();
        cameraController = FindObjectOfType<CameraController>();
        if (playerController == null) Debug.LogError("PlayerController组件未找到！");
        if (cameraController == null) Debug.LogError("CameraController组件未找到！");

        // 初始化血量条
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        // 初始化血量文字
        UpdateHealthText();

        StartCoroutine(HealthUpdateRoutine()); // 开始血量更新协程
    }

    void Update()
    {
        // 对话中禁用移动
        if (playerController != null && playerController.IsInDialogue())
        {
            horizontal = 0f;
            vertical = 0f;
            animator.SetBool("IsRunning", false);
            return;
        }

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        // 控制角色动画和朝向
        if (move.magnitude > 0.1f)
        {
            animator.SetBool("IsRunning", true);
        }
        else
        {
            animator.SetBool("IsRunning", false);
        }

        if (horizontal > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (horizontal < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Launch();
        }
    }

    void FixedUpdate()
    {
        if (playerController != null && playerController.IsInDialogue())
        {
            return;
        }

        // 处理物理移动
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }

    // 血量更新核心逻辑
    private IEnumerator HealthUpdateRoutine()
    {
        while (true)
        {
            if (!pauseHealthUpdate) // 只有在未暂停时才更新血量
            {
                if (cameraController != null)
                {
                    if (!cameraController.IsIndoors() && currentHealth > 0) // 室外每秒-4血
                    {
                        ChangeHealth(-6);
                    }
                    else if (cameraController.IsIndoors() && currentHealth < maxHealth) // 室内每秒+5血
                    {
                        ChangeHealth(12);
                    }
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    // 修改血量方法
    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible) return;
            isInvincible = true;
            invincibleTimer = timeInvincible;
            //animator.SetTrigger("Hit");
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        // 更新血量条UI
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        // 更新血量文字
        UpdateHealthText();

        // 检查是否死亡
        if (currentHealth <= 0)
        {
            StartCoroutine(DieAndRespawn());
        }

        Debug.Log(currentHealth + "/" + maxHealth);
    }

    // 更新血量文字的方法
    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}"; // 显示 "当前血量/最大血量"
        }
    }

    // 死亡和重生逻辑
    private IEnumerator DieAndRespawn()
    {
        yield return StartCoroutine(FadeManager.Instance.FadeToBlack(() =>
        {
            // 始终复活在房屋1（索引1）
            if (cameraController != null && cameraController.housePlayerPositions.Length > 1)
            {
                transform.position = cameraController.housePlayerPositions[1]; // 玩家复活在房屋1的玩家位置
                cameraController.transform.position = cameraController.housePositions[1]; // 相机移动到房屋1的相机位置
                cameraController.isIndoors = true; // 设置为室内状态
                cameraController.currentHouseIndex = 1; // 更新当前房屋索引为1
            }
            else
            {
                transform.position = Vector3.zero; // 如果没有房屋1，默认初始位置
            }

            // 重置血量
            currentHealth = maxHealth;
            if (healthBar != null)
            {
                healthBar.value = currentHealth;
            }
            UpdateHealthText(); // 更新文字显示
        }));
    }

    public void UpdateLastHousePosition(Vector3 position)
    {
        lastHousePosition = position;
    }

    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        animator.SetTrigger("Launch");
    }
}