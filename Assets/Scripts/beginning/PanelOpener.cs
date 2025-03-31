using UnityEngine;
using UnityEngine.UI;

public class PanelOpener : MonoBehaviour
{
    public Button openButton;  // 打开面板的按钮
    public Button closeButton; // 关闭面板的按钮
    public GameObject panel;   // 要打开或关闭的面板

    void Start()
    {

        // 初始化时关闭面板
        panel.SetActive(false);

        // 绑定按钮点击事件
        openButton.onClick.AddListener(OpenPanel);
        closeButton.onClick.AddListener(ClosePanel);
    }

    // 打开面板
    public void OpenPanel()
    {
        panel.SetActive(!panel.activeSelf); 
    }

    // 关闭面板
    public void ClosePanel()
    {
        panel.SetActive(false);
    }
}