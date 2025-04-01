using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // 添加 SceneManager 支持

public class OpeningAnimation : MonoBehaviour
{
    public TextMeshProUGUI textDisplay; // 用于显示所有文字的单一 TextMeshPro 对象
    public SpriteRenderer[] backgroundImages; // 背景图数组，按顺序对应每段文字
    public AudioSource backgroundMusic; // 背景音乐组件
    public float fadeDuration = 2f; // 淡入淡出时间（文字和音乐共用）
    public float typingSpeed = 0.05f; // 打字机速度
    public float displayDuration = 3f; // 每段文字和背景显示的持续时间
    public float moveDistance = 5f; // 背景从左到右移动的距离
    public float musicMaxVolume = 0.5f; // 背景音乐最大音量（0 到 1）

    private Vector2 defaultTextPosition; // 存储文字默认位置
    private string[] storySegments = new string[]
    {
        "《新唐书·天文志》载：\"贞观十五年三月丙辰，彗星出太微，贯紫宫，二十余日乃灭。夏四月庚寅朔，日有食之。\"《旧唐书·五行志》复记：\"是岁，天下大水，河南、河北尤甚，漂没庐舍，稼穑绝收。\"",
        "贞观十五年，三月十六，盛唐之治下的第十五个年头。长安城外三十里，墨家村灯火微茫，薄雾漫过青石小径。水车咿呀，工匠敲凿木料之声不绝，村民围坐，论《墨子·经下》\"负而不挠，说在胜\"之理。",
        "无人料及天象骤变。彗星划空，日食随至，继而连年大雨，江河泛滥，农田尽毁。朝廷震动，诏令天下献策抗灾。然墨家村地处偏远，村民唯有以书信传递《考工记》之制器术、《甘石星经》之天象推演、《齐民要术》之农事补救，欲为朝廷分忧。",
        "木童——由村中工匠简姝儿依《墨子》力学之理所制之木信使，代人传递手书。一封封书信由木童送至墨家村中各处。",
        "而我们的故事，始于天象异象后第五年。",
        "偏远山村——墨家村。"
    };

    void Start()
    {
        // 初始化：隐藏所有背景图并设置透明度为 0
        foreach (SpriteRenderer bg in backgroundImages)
        {
            bg.enabled = true;
            Color color = bg.color;
            color.a = 0f;
            bg.color = color;
            bg.transform.localPosition = new Vector3(-moveDistance, 0, 0);
        }
        // 初始化文字透明度为 0 并记录默认位置
        Color textColor = textDisplay.color;
        textColor.a = 0f;
        textDisplay.color = textColor;
        defaultTextPosition = textDisplay.rectTransform.anchoredPosition;

        // 初始化背景音乐
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = musicMaxVolume; // 初始音量为最大值
            backgroundMusic.Play(); // 开始播放
        }

        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        // 按 Esc 键跳过动画并跳转场景
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopAllCoroutines();
            if (backgroundMusic != null) backgroundMusic.Stop();
            SceneManager.LoadScene("start");
        }
    }

    IEnumerator PlaySequence()
    {
        for (int i = 0; i < storySegments.Length; i++)
        {
            // 背景淡入并移动到中心
            yield return StartCoroutine(FadeInBackground(backgroundImages[i]));

            // 根据段落调整文字位置并选择效果
            if (i == 0 || i >= storySegments.Length - 2) // 第一段和最后两段
            {
                textDisplay.rectTransform.anchoredPosition = Vector2.zero; // 居中
                textDisplay.text = storySegments[i];
                yield return StartCoroutine(FadeInText(textDisplay));
                yield return new WaitForSeconds(displayDuration);
                yield return StartCoroutine(FadeOutText(textDisplay));
            }
            else // 其他段落使用打字机效果
            {
                textDisplay.rectTransform.anchoredPosition = defaultTextPosition; // 恢复默认位置
                textDisplay.text = ""; // 清空文字
                yield return StartCoroutine(TypeText(textDisplay, storySegments[i]));
                yield return new WaitForSeconds(displayDuration);
                yield return StartCoroutine(FadeOutText(textDisplay));
            }

            // 背景淡出并继续向右移动
            yield return StartCoroutine(FadeOutBackground(backgroundImages[i]));

            // 短暂停顿，准备下一段
            yield return new WaitForSeconds(1f);
        }

        // 音乐淡出（动画结束后）
        if (backgroundMusic != null)
        {
            yield return StartCoroutine(FadeOutMusic());
            backgroundMusic.Stop(); // 停止播放
        }

        // 动画正常结束后跳转到 "start" 场景
        SceneManager.LoadScene("start");
    }

    IEnumerator FadeInText(TextMeshProUGUI textObj)
    {
        Color color = textObj.color;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime * 2;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            textObj.color = color;
            yield return null;
        }
        color.a = 1f;
        textObj.color = color;
    }

    IEnumerator FadeOutText(TextMeshProUGUI textObj)
    {
        Color color = textObj.color;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            textObj.color = color;
            yield return null;
        }
        color.a = 0f;
        textObj.color = color;
    }

    IEnumerator TypeText(TextMeshProUGUI textObj, string fullText)
    {
        Color color = textObj.color;
        color.a = 1f; // 打字机效果时直接可见
        textObj.color = color;

        foreach (char letter in fullText.ToCharArray())
        {
            textObj.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    IEnumerator FadeInBackground(SpriteRenderer bg)
    {
        Color color = bg.color;
        Vector3 startPos = new Vector3(-moveDistance, 0, 0); // 从左侧开始
        Vector3 endPos = new Vector3(0, 0, 0); // 移动到中心
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            color.a = Mathf.Lerp(0f, 1f, t); // 淡入
            bg.color = color;
            bg.transform.localPosition = Vector3.Lerp(startPos, endPos, t); // 从左到中心
            yield return null;
        }
        color.a = 1f;
        bg.color = color;
        bg.transform.localPosition = endPos;
    }

    IEnumerator FadeOutBackground(SpriteRenderer bg)
    {
        Color color = bg.color;
        Vector3 startPos = new Vector3(0, 0, 0); // 从中心开始
        Vector3 endPos = new Vector3(moveDistance, 0, 0); // 移动到右侧
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            color.a = Mathf.Lerp(1f, 0f, t); // 淡出
            bg.color = color;
            bg.transform.localPosition = Vector3.Lerp(startPos, endPos, t); // 从中心到右
            yield return null;
        }
        color.a = 0f;
        bg.color = color;
        bg.transform.localPosition = new Vector3(-moveDistance, 0, 0); // 重置到左侧
    }

    IEnumerator FadeOutMusic()
    {
        float elapsedTime = 0f;
        float startVolume = backgroundMusic.volume;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            backgroundMusic.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        backgroundMusic.volume = 0f;
    }
}