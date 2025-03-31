using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button newGameButton; // 开始新游戏按钮
    public Button exitGameButton; // 退出游戏按钮

    void Start()
    {
        newGameButton.onClick.AddListener(StartNewGame);
        exitGameButton.onClick.AddListener(ExitGame);
    }

    void StartNewGame()
    {
        Debug.Log("开始新游戏");
        SceneManager.LoadScene("main");
    }


    void ExitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}