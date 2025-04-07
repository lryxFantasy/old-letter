using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task6 : TaskBase
{
    private bool letterDeliveredToMoShou = false; // �Ƿ��ʹ�¬�ϸ�ī�ص���
    private string[] currentDialogue;
    private int dialogueIndex = 0;
    private TMP_Text dialogueText;
    private GameObject dialoguePanel;
    private Button nextButton;
    private TaskManager taskManager;

    // ����ʼ������
    private GameObject taskCompletePanel;
    private TextMeshProUGUI taskCompleteText;

    public void SetupTask(TaskManager manager, GameObject panel, TMP_Text text, Button button)
    {
        taskManager = manager;
        dialoguePanel = panel;
        dialogueText = text;
        nextButton = button;
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextDialogue);
        dialoguePanel.SetActive(false);

        // ��ʼ������ʼ��岢��ʾ
        SetupTaskCompletePanel();
        StartCoroutine(ShowTaskStartPanel());
    }

    public override string GetTaskName() => "����֮Լ��";

    public override string GetTaskObjective() => $"�ʹ¬�ϡ�����ī�ء����ţ�{(letterDeliveredToMoShou ? "�����" : "δ���")}";

    public override bool IsTaskComplete() => letterDeliveredToMoShou;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "ī��" && !letterDeliveredToMoShou
            ? GetDialogueForMoShou()
            : new string[] { "���������㻹û���ſ��͸����ˡ�" };

        StartDialogue();
    }

    private void StartDialogue()
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = currentDialogue[dialogueIndex];
    }

    private void NextDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex < currentDialogue.Length)
        {
            dialogueText.text = currentDialogue[dialogueIndex];
        }
        else
        {
            dialoguePanel.SetActive(false);
            if (!letterDeliveredToMoShou && currentDialogue.Length > 1)
            {
                letterDeliveredToMoShou = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task6: �Ƴ�¬�ϵ��ţ��������ľ�ݵ�Կ��");
                    taskManager.inventoryManager.RemoveLetter("¬����ī��֮��");
                    Sprite icon = Resources.Load<Sprite>("key"); // �� Resources ����Կ��ͼ��
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "��ľ�ݵ�Կ��",
                        content = "����ī�ؼ��з���֮��ƿɿ����山��ľ�ݡ�����ս�ң�ī��Я¬��ĸ�ӹ�壬���ݻ�ؾ�ʱ֮�ء���������˽��ף���Ϭ�ȴ���10������̽���о�����",
                        icon = icon
                    });
                }
                else
                {
                    Debug.LogError("Task6: TaskManager �� InventoryManager δ��ȷ��");
                }
            }

            PlayerController playerController = Object.FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.IsInDialogue())
                playerController.EndDialogue();

            if (IsTaskComplete())
            {
                if (taskManager != null)
                {
                    Debug.Log("Task6: ������ɣ��л��� Task7");
                    Task7 newTask = gameObject.AddComponent<Task7>();
                    taskManager.SetTask(newTask);
                    newTask.SetupTask(taskManager, dialoguePanel, dialogueText, nextButton);
                    taskManager.UpdateTaskDisplay();
                }
                else
                {
                    Debug.LogError("Task6: TaskManager δ�ҵ����޷��л��� Task7");
                }
            }
        }
    }

    private string[] GetDialogueForMoShou()
    {
        return new string[]
        {
            "��ī�ء������㣬Сľͷ�������������ˣ�",
            "��ī�ء������ɣ�ī�ɵ��¶�����ˡ����Ÿ��ҡ�",
            "��������",
            "��ī�ء�¬�����š�������Թ�ң�����Ƿ��һ��̫�ࡣ",
            "��ī�ء�¬ƽ���ҵ����꽻������һͬ�Ͽ��ϸ���͢����",
            "��ī�ء����ᱲ�ջ�һǻ������ȴ�ܾ��˳�͢Ұ��С�˼Ǻޡ�",
            "��ī�ء���һ��������ʵ����ȴ��С�����ݣ��󱻱ᣬ�˺�һ�����𡭡�",
            "��ī�ء�ȥ��ǰ����ȥ�����ĸ�ۡ��",
            "��ī�ء���ץ���ң�����˵����������֪�������¬��ĸ�ӡ�",
            "��ī�ء��뵱����������һǻ��Ѫ��������ī��ǧ��氮�ǹ�˼������ˮ�����ĸﳯ����",
            "��ī�ء�����������Ҳ�Ļ����䣬�ش�ɽ�����ӡ�",
            "��ī�ء��Ҵ�¬��ĸ�ӻ���ī�Ҵ壬ÿ��С¬���Ҷ�����¬ƽ������Ц�����ӡ���",
            "��ī�ء������ҳ�ɽ�����������壬���ģ���û���ˡ�",
            "��ī�ء�������˵�öԣ���ī�ӡ����氮���������Ҹ�¬ƽ��Լ��û�ꡭ���ҵ����롣",
            "��ī�ء�����¬�ϣ��һ���ţ�����д��������̫�ۣ�С¬������",
            "��ī�ء����ˣ���Կ�����ҷ������ģ��山��ľ�ݵģ�������������ҵ���Ҫ�Ķ�������ȥ���ˣ���ȥ���ưɡ�"
        };
    }

    // ��ʼ������������
    private void SetupTaskCompletePanel()
    {
        taskCompletePanel = GameObject.Find("TaskCompletePanel");
        if (taskCompletePanel != null)
        {
            taskCompleteText = taskCompletePanel.GetComponentInChildren<TextMeshProUGUI>();
            if (taskCompleteText == null)
            {
                Debug.LogWarning("TaskCompletePanel ��û���ҵ� TextMeshProUGUI �����");
            }
            else
            {
                CanvasGroup canvasGroup = taskCompletePanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = taskCompletePanel.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f;
                taskCompletePanel.SetActive(false); // ȷ����ʼ����
            }
        }
        else
        {
            Debug.LogError("δ�ҵ� TaskCompletePanel����ȷ���������Ѵ��ڸ���壡");
        }
    }

    // ��ʾ����ʼ��ʾ
    private IEnumerator ShowTaskStartPanel()
    {
        if (taskCompletePanel != null && taskCompleteText != null)
        {
            taskCompleteText.text = "����6��������֮Լ��";
            taskCompletePanel.SetActive(true);

            CanvasGroup canvasGroup = taskCompletePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = taskCompletePanel.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0f;
            }

            // ����
            float fadeDuration = 1f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            // ��ʾ 2 ��
            yield return new WaitForSecondsRealtime(2f);

            // ����
            elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;

            Debug.Log("����6 ��ʼ�������ʾ������");
        }
        else
        {
            Debug.LogWarning("������������������δ��ȷ��ʼ�����޷���ʾ����6��ʼ��ʾ��");
        }
    }
}