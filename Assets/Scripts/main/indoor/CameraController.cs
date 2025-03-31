using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform target; // 相机跟随的目标
    public Vector3 offset; // 相机偏移
    public Vector3[] housePositions; // 房屋内的相机位置
    public Vector3[] housePlayerPositions; // 房屋内的玩家位置
    public bool isIndoors = false; // 是否在室内
    public int currentHouseIndex = -1; // 当前房屋索引
    private Vector3 mapPosition;
    public Vector3 lastPlayerMapPosition; // 玩家最后在地图上的位置

    void Start()
    {
        // 初始化房屋位置数组
        if (housePositions == null || housePositions.Length == 0)
        {
            housePositions = new Vector3[6];
            housePositions[0] = new Vector3(100, 0, -10);
            housePositions[1] = new Vector3(200, 0, -10);
            housePositions[2] = new Vector3(300, 0, -10);
            housePositions[3] = new Vector3(400, 0, -10);
            housePositions[4] = new Vector3(500, 0, -10);
            housePositions[5] = new Vector3(600, 0, -10);
        }

        // 初始化玩家在房屋中的位置数组
        if (housePlayerPositions == null || housePlayerPositions.Length == 0)
        {
            housePlayerPositions = new Vector3[6];
            housePlayerPositions[0] = new Vector3(100, 0, 0);
            housePlayerPositions[1] = new Vector3(200, 0, 0);
            housePlayerPositions[2] = new Vector3(300, 0, 0);
            housePlayerPositions[3] = new Vector3(400, 0, 0);
            housePlayerPositions[4] = new Vector3(500, 0, 0);
            housePlayerPositions[5] = new Vector3(600, 0, 0);
        }

        mapPosition = transform.position;
    }

    void LateUpdate()
    {
        // 室外时跟随玩家
        if (target != null && !isIndoors)
        {
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z) + offset;
            transform.position = targetPosition;
            lastPlayerMapPosition = target.position;
        }
    }

    // 进入房屋
    public void EnterHouse(int houseIndex)
    {
        if (houseIndex >= 0 && houseIndex < housePositions.Length)
        {
            StartCoroutine(FadeToHouse(houseIndex));
        }
    }

    // 离开房屋
    public void ExitHouse()
    {
        if (isIndoors)
        {
            StartCoroutine(FadeToMap());
        }
    }

    private IEnumerator FadeToHouse(int houseIndex)
    {
        yield return StartCoroutine(FadeManager.Instance.FadeToBlack(() =>
        {
            transform.position = housePositions[houseIndex];
            target.position = housePlayerPositions[houseIndex];
            isIndoors = true;
            currentHouseIndex = houseIndex;
        }));
    }

    private IEnumerator FadeToMap()
    {
        yield return StartCoroutine(FadeManager.Instance.FadeToBlack(() =>
        {
            isIndoors = false;
            currentHouseIndex = -1;
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z) + offset;
            target.position = lastPlayerMapPosition;
            target.position = new Vector3(target.position.x, target.position.y - 1f, target.position.z);
        }));
    }

    public bool IsIndoors()
    {
        return isIndoors;
    }
}