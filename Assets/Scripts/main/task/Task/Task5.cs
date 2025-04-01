using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task5 : TaskBase
{
    private bool letterDeliveredToLuShi = false; // �Ƿ��ʹ�С¬��¬�ϵ���
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

    public override string GetTaskName() => "���еĸ���";

    public override string GetTaskObjective() => $"�ʹС¬������¬�ϡ����ţ�{(letterDeliveredToLuShi ? "�����" : "δ���")}";

    public override bool IsTaskComplete() => letterDeliveredToLuShi;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "¬��" && !letterDeliveredToLuShi
            ? GetDialogueForLuShi()
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
            if (!letterDeliveredToLuShi && currentDialogue.Length > 1)
            {
                letterDeliveredToLuShi = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task5: �Ƴ�С¬���ţ����¬�ϵ���");
                    taskManager.inventoryManager.RemoveLetter("С¬��¬��֮��");
                    Sprite icon = Resources.Load<Sprite>("jane"); // �� Resources ����¬�ϵ�ͼ��
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "¬����ī��֮��",
                        content = "ī������δͨ�ԣ��������ɰ����������죬��ˮ����������أ�ī��Σ�ӡ��֪�������������綬��Ȼ��ī�ӡ��ơ��氮�����������չ���¬��ƽ��Լ��������ѧ�Ȳ�����������֮��檻�ˮ����������ƽ�⣬��������ˮ�������ȣ�Ȼ�����㣬ȱ�ܸ�֮������ڴ˵�������ս����Ͷʯ����һ����ʯ�ٽ���ˮ���ͣ��β��ܵ�����歶��Ի�����ľͯ��ī���Ծ��۹⣬��Ϊ���ֱ��ߣ�Ψ������ī���洫��ȴ���Ų�����\r\n" +
                                  "¬��ƽ���ڣ���Ȱ����ɽ���˲�����ߣ�����ũ�£��ĸ�ӵù��շ����񣬸������ġ����������֣��ﾡ�٣���δ棿����С¬������ˮ����ת��������Զԡ����ȼ������֪��࣬Ȼī�Ҵ��з�һ��֮��˼���֮�����������⣬��ľͯһ�ߣ��ܸ�֮�����ˮ����ϣ�������������������ˡ���Լδ�꣬���տ������ι�Ī�ǡ�����¬��",
                        icon = icon
                    });
                }
                else
                {
                    Debug.LogError("Task5: TaskManager �� InventoryManager δ��ȷ��");
                }
            }

            PlayerController playerController = Object.FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.IsInDialogue())
                playerController.EndDialogue();

            if (IsTaskComplete())
            {
                if (taskManager != null)
                {
                    Debug.Log("Task5: ������ɣ��л��� Task6");
                    Task6 newTask = gameObject.AddComponent<Task6>();
                    taskManager.SetTask(newTask);
                    newTask.SetupTask(taskManager, dialoguePanel, dialogueText, nextButton);
                    taskManager.UpdateTaskDisplay();
                }
                else
                {
                    Debug.LogError("Task5: TaskManager δ�ҵ����޷��л��� Task6");
                }
            }
        }
    }

    private string[] GetDialogueForLuShi()
    {
        return new string[]
        {
            "��¬�ϡ��������ˣ�Сľ�ˡ��ף�С¬���ţ��⺢�����ǣ�סһ����������͡���",
            "��������",
            "��¬�ϡ�����������֪�������£������һ�ûˮ������⺢�ӣ�������������ᡣ",
            "��¬�ϡ�¬ƽ����ǰ�����ˣ���ī�����ֵܣ�Լ���á�ī�ӡ����氮���������ˣ���û�������һ�棬ֻ֪������ǰ���������������ա�",
            "��¬�ϡ�С¬���ʵ����Ķ�����ֻ��˵���������Ͽ��ţ������������������֪����ô˵�Բ��ԣ���ֻ�ܻ���",
            "��¬�ϡ�ī���ܾ��˳�͢��ѹ��֮���Ļ�����ص����ӱ�����",
            "��¬�ϡ���歶���ī��˵���������Ҫ���ļ�������",
            "��¬�ϡ���������Ȱ�ö�����",
            "��¬�ϡ����Ÿ�ī�أ��鷳�㡣ī��˵ī���Ͼ��öԲ���¬ƽ�����Ҳ�������л����Щ����ҡ�",
            "��¬�ϡ�лл�������ˣ�Сľ�ˣ����Ǵ�����æ�ġ�"
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
            taskCompleteText.text = "����5�������еĸ���";
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

            Debug.Log("����5 ��ʼ�������ʾ������");
        }
        else
        {
            Debug.LogWarning("������������������δ��ȷ��ʼ�����޷���ʾ����5��ʼ��ʾ��");
        }
    }
}