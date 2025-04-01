using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // ���ڳ�����ת
using UnityEngine.Audio; // ������Ƶ����

public class GoodEnd : MonoBehaviour
{
    public TMP_Text displayText; // TextMeshPro ���ı����
    public float fadeDuration = 1f; // ���뵭���ĳ���ʱ��
    public float displayDuration = 3f; // ÿ�仰��ʾ�ĳ���ʱ��

    public RectTransform imageTransform; // �ƶ���ͼƬ
    public float moveDuration = 100f; // ͼƬ���µ����ƶ�����ʱ��
    private float startY; // ͼƬ��ʼ Y λ��
    private float endY;   // ͼƬ���� Y λ��

    public AudioSource backgroundMusic; // �������ֵ� AudioSource ���
    public float musicFadeOutDuration = 2f; // ���ֵ����ĳ���ʱ��

    private string[] sentences = new string[]
    {
        "ľͯð���꣬���山���ݣ��ٵá�ī����ѧ�ָ塷��",
        "�ָ������ī��̾¬ƽ���ڣ���ϲ�������Ľᡣ",
        "ī��Я��歶����ָ壬�ϸܸ��뻬�֣��ɡ�ī��ˮ������",
        "ˮ�����ȣ������������ظ��̣�ī�Ҵ��ػ�������",
        "¬�ϻ�ˮ��ͼֽ��������Ұ���գ�С¬�Ƴ����ܡ�",
        "��������ڣ�Ц�ԣ���ī�Ҳ��������Ĳ�������",
        "ī�����歶�Яˮ�����ź�װ�ã����鳯͢��",
        "��۶���������̾��ߪī��Ϊ���ɣ����歶�Ϊ��ī������",
        "�����볤���������ѿ���ճ���������ī����ѧ��",
        "ľͯ�ü�歶�������ɣľ����ľ�����־��ɣ�������硣",
        "������뾩��ľͯΪ������ʹ����������Ϊ��ľͯ�񡯡�",
        "ī�����壬��С¬��ѧ�����갲�ꡣ",
        "¬�ϻ���Ұ�Ҽ��У���ο����¬ƽ��",
        "���ű�ī�Ҹ�ҥ����������������ˮ��������",
        "ľͯ������䣬���Ų�ꡣ�ÿ���Գ���ϣ����",
        "ī�����歶����ڳ������������ˮ����ƽ��",
        "ľͯ����������Ұ����֤��ī����������",
        "����ɢ�����չ⸴����ľͯ���գ���Ұ���ࡣ",
        "ī����ѧ������ǧ�ꡣ"
    };

    void Start()
    {
        // ȷ������Ѹ�ֵ
        if (displayText == null || imageTransform == null)
        {
            Debug.LogError("���� Inspector �и�ֵ displayText �� imageTransform��");
            return;
        }
        if (backgroundMusic == null)
        {
            Debug.LogError("���� Inspector �и�ֵ backgroundMusic��");
            return;
        }

        // ����ͼƬ����ʼ�ͽ���λ��
        float imageHeight = imageTransform.rect.height; // ͼƬ�߶�
        startY = -Screen.height / 2f + imageHeight / 2.8f; // ����Ļ�ײ���ʼ
        endY = Screen.height / 2f - imageHeight / 2.9f;    // �ƶ�����Ļ����
        imageTransform.anchoredPosition = new Vector2(imageTransform.anchoredPosition.x, startY);

        // ���ű�������
        backgroundMusic.Play();

        // ��ʼ�ı����뵭����ͼƬ�ƶ�
        StartCoroutine(DisplaySentences());
        StartCoroutine(MoveImage());
    }

    IEnumerator DisplaySentences()
    {
        foreach (string sentence in sentences)
        {
            displayText.text = sentence;

            // ����
            yield return StartCoroutine(FadeIn());

            // ��ʾһ��ʱ��
            yield return new WaitForSeconds(displayDuration);

            // ����
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

        // ȷ������λ�þ�ȷ
        imageTransform.anchoredPosition = new Vector2(imageTransform.anchoredPosition.x, endY);

        // ͼƬ�ƶ���ɺ󵭳����ֲ���ת����
        yield return StartCoroutine(FadeOutMusic());
        SceneManager.LoadScene("start"); // ��ת�� Start ����
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

        // ȷ����������Ϊ 0 ��ֹͣ����
        backgroundMusic.volume = 0f;
        backgroundMusic.Stop();
    }
}