using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // ��� SceneManager ֧��

public class OpeningAnimation : MonoBehaviour
{
    public TextMeshProUGUI textDisplay; // ������ʾ�������ֵĵ�һ TextMeshPro ����
    public SpriteRenderer[] backgroundImages; // ����ͼ���飬��˳���Ӧÿ������
    public AudioSource backgroundMusic; // �����������
    public float fadeDuration = 2f; // ���뵭��ʱ�䣨���ֺ����ֹ��ã�
    public float typingSpeed = 0.05f; // ���ֻ��ٶ�
    public float displayDuration = 3f; // ÿ�����ֺͱ�����ʾ�ĳ���ʱ��
    public float moveDistance = 5f; // �����������ƶ��ľ���
    public float musicMaxVolume = 0.5f; // �����������������0 �� 1��

    private Vector2 defaultTextPosition; // �洢����Ĭ��λ��
    private string[] storySegments = new string[]
    {
        "�������顤����־���أ�\"���ʮ�������±��������ǳ�̫΢�����Ϲ�����ʮ�������������¸���˷������ʳ֮��\"�������顤����־�����ǣ�\"���꣬���´�ˮ�����ϡ��ӱ�������Ưû®�ᣬ�����ա�\"",
        "���ʮ���꣬����ʮ����ʢ��֮���µĵ�ʮ�����ͷ������������ʮ�ī�Ҵ�ƻ�΢ã������������ʯС����ˮ����ѽ����������ľ��֮������������Χ�����ۡ�ī�ӡ����¡�\"�������ӣ�˵��ʤ\"֮��",
        "�����ϼ�������䡣���ǻ��գ���ʳ�������̶�������꣬���ӷ��ģ�ũ�ﾡ�١���͢�𶯣�گ�������ײ߿��֡�Ȼī�Ҵ�ش�ƫԶ������Ψ�������Ŵ��ݡ������ǡ�֮������������ʯ�Ǿ���֮�������ݡ�������Ҫ����֮ũ�²��ȣ���Ϊ��͢���ǡ�",
        "ľͯ�����ɴ��й�����歶�����ī�ӡ���ѧ֮������֮ľ��ʹ�����˴������顣һ���������ľͯ����ī�Ҵ��и�����",
        "�����ǵĹ��£�ʼ���������������ꡣ",
        "ƫԶɽ�塪��ī�Ҵ塣"
    };

    void Start()
    {
        // ��ʼ�����������б���ͼ������͸����Ϊ 0
        foreach (SpriteRenderer bg in backgroundImages)
        {
            bg.enabled = true;
            Color color = bg.color;
            color.a = 0f;
            bg.color = color;
            bg.transform.localPosition = new Vector3(-moveDistance, 0, 0);
        }
        // ��ʼ������͸����Ϊ 0 ����¼Ĭ��λ��
        Color textColor = textDisplay.color;
        textColor.a = 0f;
        textDisplay.color = textColor;
        defaultTextPosition = textDisplay.rectTransform.anchoredPosition;

        // ��ʼ����������
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = musicMaxVolume; // ��ʼ����Ϊ���ֵ
            backgroundMusic.Play(); // ��ʼ����
        }

        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        // �� Esc ��������������ת����
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
            // �������벢�ƶ�������
            yield return StartCoroutine(FadeInBackground(backgroundImages[i]));

            // ���ݶ����������λ�ò�ѡ��Ч��
            if (i == 0 || i >= storySegments.Length - 2) // ��һ�κ��������
            {
                textDisplay.rectTransform.anchoredPosition = Vector2.zero; // ����
                textDisplay.text = storySegments[i];
                yield return StartCoroutine(FadeInText(textDisplay));
                yield return new WaitForSeconds(displayDuration);
                yield return StartCoroutine(FadeOutText(textDisplay));
            }
            else // ��������ʹ�ô��ֻ�Ч��
            {
                textDisplay.rectTransform.anchoredPosition = defaultTextPosition; // �ָ�Ĭ��λ��
                textDisplay.text = ""; // �������
                yield return StartCoroutine(TypeText(textDisplay, storySegments[i]));
                yield return new WaitForSeconds(displayDuration);
                yield return StartCoroutine(FadeOutText(textDisplay));
            }

            // �������������������ƶ�
            yield return StartCoroutine(FadeOutBackground(backgroundImages[i]));

            // ����ͣ�٣�׼����һ��
            yield return new WaitForSeconds(1f);
        }

        // ���ֵ���������������
        if (backgroundMusic != null)
        {
            yield return StartCoroutine(FadeOutMusic());
            backgroundMusic.Stop(); // ֹͣ����
        }

        // ����������������ת�� "start" ����
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
        color.a = 1f; // ���ֻ�Ч��ʱֱ�ӿɼ�
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
        Vector3 startPos = new Vector3(-moveDistance, 0, 0); // ����࿪ʼ
        Vector3 endPos = new Vector3(0, 0, 0); // �ƶ�������
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            color.a = Mathf.Lerp(0f, 1f, t); // ����
            bg.color = color;
            bg.transform.localPosition = Vector3.Lerp(startPos, endPos, t); // ��������
            yield return null;
        }
        color.a = 1f;
        bg.color = color;
        bg.transform.localPosition = endPos;
    }

    IEnumerator FadeOutBackground(SpriteRenderer bg)
    {
        Color color = bg.color;
        Vector3 startPos = new Vector3(0, 0, 0); // �����Ŀ�ʼ
        Vector3 endPos = new Vector3(moveDistance, 0, 0); // �ƶ����Ҳ�
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            color.a = Mathf.Lerp(1f, 0f, t); // ����
            bg.color = color;
            bg.transform.localPosition = Vector3.Lerp(startPos, endPos, t); // �����ĵ���
            yield return null;
        }
        color.a = 0f;
        bg.color = color;
        bg.transform.localPosition = new Vector3(-moveDistance, 0, 0); // ���õ����
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