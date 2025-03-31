using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public List<Letter> letters = new List<Letter>(); // 信件列表
    public GameObject itemSlotPrefab;                 // 信件槽预制体
    public Transform gridTransform;                   // 网格的 Transform
    public GameObject letterDetailPanel;              // 信件详情面板
    public TMP_Text letterContentText;                // 使用 TMP_Text 

    void Start()
    {
        // 初始化时不添加测试信件，留空等待任务添加
        UpdateInventoryUI();
    }

    // 添加信件到背包
    public void AddLetter(Letter letter)
    {
        letters.Add(letter);
        UpdateInventoryUI();
    }

    // 根据标题移除信件
    public void RemoveLetter(string title)
    {
        Letter letterToRemove = letters.Find(l => l.title == title);
        if (letterToRemove != null)
        {
            letters.Remove(letterToRemove);
            UpdateInventoryUI();
        }
    }

    public void UpdateInventoryUI()
    {
        // 清空现有槽
        foreach (Transform child in gridTransform)
        {
            Destroy(child.gameObject);
        }

        // 为每封信生成一个槽
        foreach (Letter letter in letters)
        {
            GameObject slot = Instantiate(itemSlotPrefab, gridTransform);

            // 设置标题
            TMP_Text titleText = slot.GetComponentInChildren<TMP_Text>();
            if (titleText != null)
            {
                titleText.text = letter.title;
            }

            // 设置图标
            UnityEngine.UI.Image iconImage = slot.transform.Find("Icon")?.GetComponent<UnityEngine.UI.Image>();
            if (iconImage != null)
            {
                iconImage.sprite = letter.icon;
                iconImage.gameObject.SetActive(letter.icon != null); // 如果没有图标，隐藏 Image
            }
            else
            {
                Debug.LogWarning("ItemSlotPrefab 中未找到名为 'Icon' 的 Image 组件！");
            }

            // 设置按钮点击事件
            UnityEngine.UI.Button button = slot.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => ShowLetterDetail(letter));
            }
        }
    }

    public void ShowLetterDetail(Letter letter)
    {
        letterDetailPanel.SetActive(true);
        letterContentText.text = letter.content;
    }
}