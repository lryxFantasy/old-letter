using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // 用于场景跳转
using UnityEngine.Audio; // 用于音频控制

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
        "木童冒风雨，启村北废屋，觅得《墨氏力学手稿》。",
        "手稿归来，墨守叹卢平若在，必喜，终释心结。",
        "墨成携简姝儿依手稿，合杠杆与滑轮，成‘墨氏水车’。",
        "水车轻稳，引洪归渠，田地复绿，墨家村重焕生机。",
        "卢氏绘水车图纸，笔下田野复苏，小卢绕车欢跑。",
        "罗婆坐村口，笑言：‘墨家不灭，因人心不死。’",
        "墨成与简姝儿携水车与信号装置，上书朝廷。",
        "贞观冬，工部惊叹，擢墨成为侍郎，封简姝儿为‘墨匠’。",
        "二人入长安，情愫萌芽，终成连理，共研墨家力学。",
        "木童得简姝儿改良，桑木换榆木，齿轮精巧，步履如风。",
        "随二人入京，木童为工部信使，百姓颂其为‘木童神’。",
        "墨守留村，教小卢力学，晚年安详。",
        "卢氏画田野挂家中，告慰亡夫卢平。",
        "罗婆编墨家歌谣，传唱后世，村因水车闻名。",
        "木童穿梭田间，送信不辍，每步皆承载希望。",
        "墨成与简姝儿立于长安，俯瞰天下水患渐平。",
        "木童凝望新生田野，见证了墨家新世代。",
        "彗星散尽，日光复明，木童咔哒，田野长青。",
        "墨氏力学，济世千年。"
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