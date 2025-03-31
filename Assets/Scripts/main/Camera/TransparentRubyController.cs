using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentRubyController : MonoBehaviour
{
    public float speed = 3.0f;

    public int maxHealth = 5;
    public float timeInvincible = 2.0f;

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

    private TransparentPlayerController transparentPlayerController; // 引用 TransparentPlayerController

    // 在第一次帧更新之前调用 Start
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;

        // 获取 TransparentPlayerController 组件
        transparentPlayerController = GetComponent<TransparentPlayerController>();
        if (transparentPlayerController == null)
        {
            Debug.LogError("TransparentPlayerController 组件未找到！请确保透明角色对象上有 TransparentPlayerController 脚本。");
        }
    }

    // 每帧调用一次 Update
    void Update()
    {
        // 如果在对话中，跳过输入处理
        if (transparentPlayerController != null && transparentPlayerController.IsInDialogue())
        {
            horizontal = 0f;
            vertical = 0f;
            animator.SetBool("IsRunning", false); // 停止运行动画
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

        // 根据是否有输入切换动画
        if (move.magnitude > 0.1f) // 阈值 0.1f 避免微小输入误判
        {
            animator.SetBool("IsRunning", true); // 运行状态
        }
        else
        {
            animator.SetBool("IsRunning", false); // 站立状态
        }

        // 根据水平输入翻转人物
        if (horizontal > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // 正常朝向
        }
        else if (horizontal < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // 翻转朝向
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
        // 如果在对话中，跳过移动
        if (transparentPlayerController != null && transparentPlayerController.IsInDialogue())
        {
            return;
        }

        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            animator.SetTrigger("Hit");
            if (isInvincible)
                return;

            isInvincible = true;
            invincibleTimer = timeInvincible;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log(currentHealth + "/" + maxHealth);
    }

    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        animator.SetTrigger("Launch");
    }
}
