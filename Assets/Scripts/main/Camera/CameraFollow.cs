using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 目标物体（人物）的 Transform
    public Vector3 offset;   // 摄像机相对于人物的偏移量

    void LateUpdate()
    {
        if (target != null)
        {
            // 计算目标位置加上偏移量
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z) + offset;
            // 更新摄像机位置
            transform.position = targetPosition;
        }
    }
}