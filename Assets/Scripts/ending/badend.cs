using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // 用于场景跳转
using UnityEngine.Audio; // 用于音频控制

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
        "木童自墨守手中接过钥匙，步履蹒跚，前往村北废屋。",
        "废屋内，尘封的木箱空空如也，墨氏手稿无迹可寻。",
        "木童归来，简姝儿与墨成空望，改良水车之志受阻。",
        "洪水滔天，水车半成，试转之时，轮轴断裂，田地依旧汪洋。",
        "卢氏画下残破水车，墨迹模糊，泪水滴落画纸。",
        "小卢蹲于屋檐，喃喃自语：‘水车咋不转了？’",
        "墨成携简姝儿上书朝廷，献半成水车与信号装置。",
        "然朝廷忙于平叛，工部斥其‘乡野之术’，贬二人至山郡。",
        "墨守闭门不出，手握未示之手稿，腿疾缠身，终殒于书案。",
        "卢氏携小卢迁往他乡，画笔不再触及水车，仅绘小卢嬉戏。",
        "罗婆守空村，墨家故事随风散去，声渐喑哑。",
        "木童奔走途中，齿轮锈蚀，桑木朽烂，步履愈缓。",
        "村民四散，无人再需书信，木童被弃于村口泥泞。",
        "竹筒内，最后一封信无人开启，雨水浸透其身。",
        "山郡百姓感墨成与简姝儿之恩，称其‘墨氏双侠’，",
        "却无人知，二人曾险改天下水患之命。",
        "木童倒于泥中，凝望逝去的墨家村，",
        "直至风雨吹散其最后运作之声……",
        "彗星暗藏，洪水不息，木童无声，田野叹息。"
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