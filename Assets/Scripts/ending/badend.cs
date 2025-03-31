using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // 用于场景跳转
using UnityEngine.Audio; // 用于音频控制（可选）

public class BadEnd : MonoBehaviour
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
        "墨信自墨守手中接过那柄古钥，怀好奇，启村北废屋，内堆战时遗物。",
        "墨信觅得一箱残旧防疫药材，虽不全，然对简工而言，宛若至宝。",
        "墨信携药材归简工，彼目骤亮，似燃星火。",
        "彼埋首研数日，终成一防疫药方之初稿。",
        "消息传开，村人沸腾。",
        "简工以村中药材改良药方，制出更多药剂。",
        "疫病不再困人，村人首出家门，呼吸清新之气，彼此对面交谈。",
        "墨守倚简工特制之木杖，首挺身；",
        "墨诗与简工于山坡重逢，彼诗终有听者；",
        "罗婆与小卢于村口相认，老者卸‘神秘友人’之名；",
        "卢画则于星空之想中成‘无战之明日’画。",
        "然村人出门，书信之需渐消。",
        "墨信任务刻板不再更，信件竹筒空空。",
        "简工忙于制药，村人沉于重获自由之喜，墨信被置于村中堂前，木身渐朽。",
        "无人再需墨信送信，无人再提此‘木疙瘩’。",
        "墨信木匣暗淡，微光渐熄。",
        "防疫药方使人重获自由，书信之世终结。",
        "墨信被弃于村中堂前，凝望村之新生，",
        "直至风吹散其最后微响……"
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
        startY = -Screen.height / 2f + imageHeight / 3f; // 从屏幕底部开始
        endY = Screen.height / 2f - imageHeight / 4f;    // 移动到屏幕顶部
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