using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // 用于场景跳转
using UnityEngine.Audio; // 用于音频控制（可选）

public class GoodEnd : MonoBehaviour
{
    public TMP_Text displayText; // TextMeshPro 的文本组件
    public float fadeDuration = 1f; // 淡入淡出的持续时间
    public float displayDuration = 3f; // 每句话显示的持续时间

    public RectTransform imageTransform; // 移动的图片
    public float moveDuration = 100f; // 图片从下到上移动的总时间
    private float startY; // 图片起始 Y 位置
    private float endY;   // 图片结束 Y 位置

    public AudioSource backgroundMusic; // 背景音乐的 AudioSource 组件
    public float musicFadeOutDuration = 2f; // 音乐淡出的持续时间

    private string[] sentences = new string[]
    {
        "墨信启废屋，觅防疫药材，携归简工。",
        "简工投研，成防疫药方，村人出家门，感久违自由。",
        "笑声村中回荡，此番，彼未忘墨信。",
        "送信日中，墨信不唯传纸，更传其情。",
        "墨守拍墨信木身，言：汝此木疙瘩，较吾所料可靠。",
        "墨诗为墨信赋《木身与魂之和鸣》，称墨信为‘村野信使’；",
        "罗婆笑抚墨信头，言墨信似彼孙之影；",
        "小卢为墨信画一图，木身闪光；",
        "卢画书言，墨信乃村中最忙之色。",
        "简工不善言，修墨信时低语：此皆谢汝。",
        "此点滴使墨信不唯器，乃墨科村之一部。",
        "防疫药方虽使人可面见，村人却觉书信之温不可替。",
        "墨守仍书与墨诗，言‘面争不如纸上辩’；",
        "墨诗与简工约以书诉心；",
        "罗婆与小卢续以‘神秘友人’通信，存默契；",
        "卢画以书记画灵，寄各人。",
        "村人决留墨信为信使，不因疫解而弃。",
        "简工为墨信更木料，任务刻板仍闪新目标。",
        "墨信穿村送信，步履不辍，每封信后，皆彼牵挂。",
        "村野之上，墨信觅己义——不唯器，亦希望之传者。",
        "终章"
    };

    void Start()
    {
        // 确保组件已赋值
        if (displayText == null || imageTransform == null)
        {
            Debug.LogError("请在 Inspector 中赋值 displayText 和 imageTransform！");
            return;
        }
        if (backgroundMusic == null)
        {
            Debug.LogError("请在 Inspector 中赋值 backgroundMusic！");
            return;
        }

        // 设置图片的起始和结束位置
        float imageHeight = imageTransform.rect.height; // 图片高度
        startY = -Screen.height / 2f + imageHeight / 2.8f; // 从屏幕底部开始
        endY = Screen.height / 2f - imageHeight / 2.9f;    // 移动到屏幕顶部
        imageTransform.anchoredPosition = new Vector2(imageTransform.anchoredPosition.x, startY);

        // 播放背景音乐
        backgroundMusic.Play();

        // 开始文本淡入淡出和图片移动
        StartCoroutine(DisplaySentences());
        StartCoroutine(MoveImage());
    }

    IEnumerator DisplaySentences()
    {
        foreach (string sentence in sentences)
        {
            displayText.text = sentence;

            // 淡入
            yield return StartCoroutine(FadeIn());

            // 显示一段时间
            yield return new WaitForSeconds(displayDuration);

            // 淡出
            yield return StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = displayText.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            displayText.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = displayText.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            displayText.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
    }

    IEnumerator MoveImage()
    {
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            float newY = Mathf.Lerp(startY, endY, t);
            imageTransform.anchoredPosition = new Vector2(imageTransform.anchoredPosition.x, newY);
            yield return null;
        }

        // 确保最终位置精确
        imageTransform.anchoredPosition = new Vector2(imageTransform.anchoredPosition.x, endY);

        // 图片移动完成后淡出音乐并跳转场景
        yield return StartCoroutine(FadeOutMusic());
        SceneManager.LoadScene("start"); // 跳转到 Start 场景
    }

    IEnumerator FadeOutMusic()
    {
        float elapsedTime = 0f;
        float startVolume = backgroundMusic.volume;

        while (elapsedTime < musicFadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / musicFadeOutDuration;
            backgroundMusic.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        // 确保音量最终为 0 并停止播放
        backgroundMusic.volume = 0f;
        backgroundMusic.Stop();
    }
}