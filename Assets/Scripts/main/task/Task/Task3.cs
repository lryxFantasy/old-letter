using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task3 : TaskBase
{
    private bool letterDeliveredToJianShuEr = false; // �Ƿ��ʹ�ī�ɸ���歶�����
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

    public override string GetTaskName() => "��ѧ���ѧ";

    public override string GetTaskObjective() => $"�ʹī�ɡ�������歶������ţ�{(letterDeliveredToJianShuEr ? "�����" : "δ���")}";

    public override bool IsTaskComplete() => letterDeliveredToJianShuEr;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "��歶�" && !letterDeliveredToJianShuEr
            ? GetDialogueForJianShuEr()
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
            if (!letterDeliveredToJianShuEr && currentDialogue.Length > 1)
            {
                letterDeliveredToJianShuEr = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task3: �Ƴ�ī�ɵ���");
                    taskManager.inventoryManager.RemoveLetter("ī���¼�歶�֮��");
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

    private string[] GetDialogueForJianShuEr()
    {
        return new string[]
        {
            "����歶��������ˣ����ҵ��ţ����������ơ�",
            "��������",
            "����歶���ī�ɵ��ţ���һﻰ������ô�࣬��һ���𿪿���",
            "����歶���������������Ҽǵã���ʱ���ڵ�ˮ���Ļ��֣������ó���ҧ�ϸ��ȣ����п�����Ϲ����",
            "����歶�����д�˲��ٹ�ѧ���£�С�׳���ͭ���۹⣬�����ź�װ�ã��⵹�е���˼��",
            "����歶����������㣬����ͭ�����û�������Ƕȣ���Ȼ��Ͷ��Զ������ͷ��ɣľ��ϸ�����ܴ�����֧�ܣ����ϳ���ת�ᣬ�����ܳɡ�",
            "����歶������춫����ʵ��ʵ������ʡ�������ִ������ա�ī�ӡ���֪�к�һ���ķ�����������˵�ײ߾����£����������֡�",
            "����歶��������ѧװ��ȷʵ�����洦�������о����ɹ������ܼӿ���Ϣ���ݡ�",
            "����歶�����������ת�ÿ죬�ҵó��ϣ�������ĥ�ܸ�ʱ��ռ����䡣",
            "����歶������´μ���������˵һ�䣬���Ĺ�ѧ���������ԣ����ֺ�֧������Ū�������Ѿ��Ӱ�Ū׼�ˣ��һ���š�",
            "����歶����һ�����˵��ī�أ��������������˽�ī�Ҵ���£�����������ſɷ������ߡ�"
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
                taskCompletePanel.SetActive(false);

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
            taskCompleteText.text = "����3������ѧ���ѧ";
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

            Debug.Log("����3 ��ʼ�������ʾ������");
        }
        else
        {
            Debug.LogWarning("������������������δ��ȷ��ʼ�����޷���ʾ����3��ʼ��ʾ��");
        }
    }
}