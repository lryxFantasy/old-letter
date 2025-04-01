using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task2 : TaskBase
{
    private bool letterDeliveredToMoCheng = false; // �Ƿ��ʹ�ī�ظ�ī�ɵ���
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

    public override string GetTaskName() => "����������";

    public override string GetTaskObjective() => $"�ʹī�ء�����ī�ɡ����ţ�{(letterDeliveredToMoCheng ? "�����" : "δ���")}";

    public override bool IsTaskComplete() => letterDeliveredToMoCheng;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "ī��" && !letterDeliveredToMoCheng
            ? GetDialogueForMoCheng()
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
            if (!letterDeliveredToMoCheng && currentDialogue.Length > 1)
            {
                letterDeliveredToMoCheng = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task2: �����ż� - �Ƴ�ī�ص��ţ����ī�ɵ���");
                    Sprite icon = Resources.Load<Sprite>("jane"); // �� Resources ����ī�ɵ�ͼ��
                    taskManager.inventoryManager.RemoveLetter("ī����ī��֮��");
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "ī���¼�歶�֮��",
                        content = "��歶�����־֮�ѡ��ǵ����ǹ�����ҹ�𣿺�ˮδ������ϡ��裬���ڵ�����ˮ�����Ի�������Ӱ��ܸˣ�������־����Ц�ᡰֽ��̸ˮ���������������Ϫ�����Ķ��ӡ�\r\n" +
                                  "��Ѱ�꣬�����޳���רע�硶ī�ӡ���֪�к�һ����ʤ����ԡ���ˮ��·������С�׳�������ͭ���۹⣬���ź�װ�ã���Ԯ��͢��Ȼȱ�껬�ֵ���֮�����겻����־��Ȼ��־������ᡣ������¬����Լ������������֮������ѧ��ˮ��֮�����������������������ˡ���歶����ɷ�ı�˲ߣ�����ī��",
                        icon = icon
                    });
                }
                else
                {
                    Debug.LogError("Task2: TaskManager �� InventoryManager δ��ȷ�󶨣��޷������ż�");
                }
            }

            PlayerController playerController = Object.FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.IsInDialogue())
            {
                playerController.EndDialogue();
            }

            if (IsTaskComplete())
            {
                if (taskManager != null)
                {
                    Debug.Log("Task2: ������ɣ��л��� Task3");
                    Task3 newTask = gameObject.AddComponent<Task3>();
                    taskManager.SetTask(newTask);
                    newTask.SetupTask(taskManager, dialoguePanel, dialogueText, nextButton);
                    taskManager.UpdateTaskDisplay();
                }
                else
                {
                    Debug.LogError("Task2: TaskManager δ�ҵ����޷��л��� Task3");
                }
            }
        }
    }

    private string[] GetDialogueForMoCheng()
    {
        return new string[]
        {
            "��ī�ɡ������㣬Сľ��ʹ��˭���ţ����ҿ�����",
            "��������",
            "��ī�ɡ����׻�������У�����ȫ�Ǵ̣�����Ρ������˵���ζ������������������İɡ�",
            "��ī�ɡ���˵��¬�������һ��ǵ������������ۡ�ī�ӡ����氮������ʱ�����ӣ�˵���ǵþ����¡���",
            "��ī�ɡ�������ˮ���ˣ�¬���������ˣ����״�������壬����Ҳ�����������¡�",
            "��ī�ɡ������ҹ���զ��������Ȼ�����������ƽʱӲ����ܸˣ��һ���Ϊ�����������������ӡ�",
            "��ī�ɡ��ᱲӦ���������Բ���Ϊ���Σ���Ӧ�����ؾɣ�����������",
            "��ī�ɡ���Ҫ��ȥ�����Ƕ�������˵һ�䡭����ī�ӡ����ԡ�־��Ϊ�硱����û������¬������־���ҵý��Ÿɣ��һ���š�"
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
            taskCompleteText.text = "����2������ѧ���ײ�";
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

            Debug.Log("����2 ��ʼ�������ʾ������");
        }
        else
        {
            Debug.LogWarning("������������������δ��ȷ��ʼ�����޷���ʾ����2��ʼ��ʾ��");
        }
    }
}