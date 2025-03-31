using UnityEngine;

public class WalkAnimation : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float timer;
    private int state;
    private bool facingRight;
    private bool canWalk = true; // 新增：控制是否允许移动

    // 可配置的参数
    [Header("Animation Settings")]
    [SerializeField] private string walkParameter = "isWalking"; // Animator 的行走参数
    [SerializeField] private float standDuration = 2f; // 站立持续时间
    [SerializeField] private float walkDuration = 3f;  // 行走持续时间

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1.5f;     // 移动速度
    [SerializeField] private float moveDelay = 0.2f;   // 位移延迟时间

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        state = 0;
        timer = 0f;
        facingRight = false; // 默认朝左
    }

    void Update()
    {
        // 如果不允许移动，强制进入站立状态
        if (!canWalk)
        {
            animator.SetBool(walkParameter, false);
            spriteRenderer.flipX = facingRight; // 保持当前朝向
            return;
        }

        timer += Time.deltaTime;

        switch (state)
        {
            case 0: // 初始站立
                animator.SetBool(walkParameter, false);
                spriteRenderer.flipX = facingRight; // 保持上一次的朝向
                if (timer >= standDuration)
                {
                    state = 1;
                    timer = 0f;
                }
                break;

            case 1: // 左走
                animator.SetBool(walkParameter, true);
                spriteRenderer.flipX = false; // 朝左
                facingRight = false;
                if (timer >= moveDelay)
                {
                    transform.position += Vector3.left * moveSpeed * Time.deltaTime;
                }
                if (timer >= walkDuration)
                {
                    state = 2;
                    timer = 0f;
                }
                break;

            case 2: // 中间站立
                animator.SetBool(walkParameter, false);
                spriteRenderer.flipX = facingRight; // 保持上一次的朝向
                if (timer >= standDuration)
                {
                    state = 3;
                    timer = 0f;
                }
                break;

            case 3: // 右走
                animator.SetBool(walkParameter, true);
                spriteRenderer.flipX = true; // 朝右
                facingRight = true;
                if (timer >= moveDelay)
                {
                    transform.position += Vector3.right * moveSpeed * Time.deltaTime;
                }
                if (timer >= walkDuration)
                {
                    state = 0;
                    timer = 0f;
                }
                break;
        }
    }

    // 公共方法：控制是否允许移动和行走动画
    public void SetWalkingState(bool allowWalking)
    {
        canWalk = allowWalking;
        if (!canWalk)
        {
            timer = 0f; // 重置计时器，避免状态切换
            state = 0;  // 强制进入站立状态
        }
    }
}