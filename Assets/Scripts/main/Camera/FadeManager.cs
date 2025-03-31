using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }
    [SerializeField] private Image fadePanel;
    [SerializeField] private float defaultFadeDuration = 0.4f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        // 初始化为完全黑色
        if (fadePanel != null) fadePanel.color = new Color(0, 0, 0, 1); // 改为初始黑屏
        else Debug.LogError("FadePanel 未赋值！");
    }

    // 保持原有的淡入淡出方法
    public IEnumerator FadeToBlack(System.Action onFadeComplete = null, float? fadeDuration = null)
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("FadePanel 未设置，直接执行回调！");
            onFadeComplete?.Invoke();
            yield break;
        }

        float duration = fadeDuration ?? defaultFadeDuration;
        yield return StartCoroutine(Fade(0f, 1f, duration));
        onFadeComplete?.Invoke();
        yield return StartCoroutine(Fade(1f, 0f, duration));
    }

    // 添加单独的淡出方法
    public IEnumerator FadeOut(float? fadeDuration = null)
    {
        float duration = fadeDuration ?? defaultFadeDuration;
        yield return StartCoroutine(Fade(1f, 0f, duration));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color color = fadePanel.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            fadePanel.color = color;
            yield return null;
        }
        color.a = endAlpha;
        fadePanel.color = color;
    }
}