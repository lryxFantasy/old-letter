using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task3 : TaskBase
{
    private bool letterDeliveredToJianGong = false; // �Ƿ��ʹ�īʫ���򹤵���
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

    public override string GetTaskName() => "����֮����";

    public override string GetTaskObjective() => $"�ʹīʫ�������򹤡����ţ�{(letterDeliveredToJianGong ? "�����" : "δ���")}";

    public override bool IsTaskComplete() => letterDeliveredToJianGong;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "��" && !letterDeliveredToJianGong
            ? GetDialogueForJianGong()
            : new string[] { "���������㻹û����" };

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
            if (!letterDeliveredToJianGong && currentDialogue.Length > 1)
            {
                letterDeliveredToJianGong = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task3: �Ƴ�īʫ����");
                    taskManager.inventoryManager.RemoveLetter("īʫ����");
                }
                else
                {
                    Debug.LogError("Task3: TaskManager �� InventoryManager δ��ȷ��");
                }
            }

            PlayerController playerController = Object.FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.IsInDialogue())
                playerController.EndDialogue();

            if (IsTaskComplete())
            {
                if (taskManager != null)
                {
                    Debug.Log("Task3: ������ɣ��л��� Task4");
                    Task4 newTask = gameObject.AddComponent<Task4>();
                    taskManager.SetTask(newTask);
                    newTask.SetupTask(taskManager, dialoguePanel, dialogueText, nextButton);
                    taskManager.UpdateTaskDisplay();
                }
                else
                {
                    Debug.LogError("Task3: TaskManager δ�ҵ����޷��л��� Task4");
                }
            }
        }
    }

    private string[] GetDialogueForJianGong()
    {
        return new string[]
        {
            "���򹤡������ˣ��и��ҵ��ţ����ҿ�����",
            "��������",
            "���򹤡�īʫ���ţ�����д��Щ������ƪ���ڵĻ����ҵ���������ܿ����ס�",
            "���򹤡���˵�����ǳ���ʱ���ǿգ��Ҽǵ���ʱ�򣬵���ʱ��ֻ����ǹ�ı仯��������˼������",
            "���򹤡�������д����ôֱ�ף������е㡭����֪����ô�أ������ˡ�",
            "���򹤡�������ʱ�������������������е��мܲ�ס��",
            "���򹤡���ʶ��֮ǰ���ҵ�����ֻ�г��ֺͻ��֣�һ�ж����ü��������",
            "���򹤡��������������Ż𣬰��ҵ��Ľ����ˣ������ⶫ������Ӧ��������",
            "���򹤡���ϲ��������ϲ�����޵Ķ�������ȴ��˵ʫ��˵�Ķ���˵��Щץ��ס�Ķ�������",
            "���򹤡�Ҳ�����ܶ�������Щ����˵�����ڣ����ܡ����е㺦�߰ɡ�",
            "���򹤡����ɵ�ӣ�������ô�죿",
            "���򹤡��������ȥ����������˵һ�䣬�ţ��Ҳ��ó����ڵĻ�����Ҳ����ȷʵͦϲ�����ġ�",
            "���򹤡��һ�������š�",
            "���򹤡�Ŷ������˵�������æ���ţ���ס�ڴ嶫����С�ݣ�лл�㡣"
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
            taskCompleteText.text = "����3��������֮����";
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

            Debug.Log("����3 ��ʼ�������ʾ������");
        }
        else
        {
            Debug.LogWarning("������������������δ��ȷ��ʼ�����޷���ʾ����3��ʼ��ʾ��");
        }
    }
}