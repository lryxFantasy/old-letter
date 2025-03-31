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

    public override string GetTaskName() => "����֮��";

    public override string GetTaskObjective() => $"�ʹС¬������¬�ϡ����ţ�{(letterDeliveredToLuShi ? "�����" : "δ���")}";

    public override bool IsTaskComplete() => letterDeliveredToLuShi;

    public override void DeliverLetter(string targetResident) // ����Ϊ targetResident��ȥ���ո�
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "¬��" && !letterDeliveredToLuShi
            ? GetDialogueForLuShi()
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
            if (!letterDeliveredToLuShi && currentDialogue.Length > 1)
            {
                letterDeliveredToLuShi = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task5: �Ƴ�С¬���ţ����¬�ϵ���");
                    taskManager.inventoryManager.RemoveLetter("С¬����");
                    Sprite icon = Resources.Load<Sprite>("lushi"); // �� Resources ����¬�ϵ�ͼ��
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "¬�ϵ���",
                        content = "ī����������δ����ԣ���֪������Ρ���������һľ�ܣ����ȿ�ʹ�񣿾����Դ��˳��ţ��߲��캮���������⣬ĪӲ�š�īʫ�ἰ�����Ծ����ɣ���Ӳ��ʯ��Ȼ����ʫ�½��٣����Ǹ������Щ����Ϊ��ϲ���˳��Ծ�����ʫ��Ȼ������������\r\n" +
                                  "����С¬�����丸���˽��������������ʱ˸����ڣ���֪���¡���֪�˚���ս��������Ҳ����ʸ��������δ�������ս�����飬���������漣�����������������࣬������ݣ����о��ݣ������Ǵ˶���īʫ�Ծ����Ѱ�����δ���ˣ��᲻�������˵�����ȥʱ����δ�����һ�棬������˺�����Ц�����ԣ�������ԺΡ��������ţ��������ø�С¬��ʹ֪������Ҳ��\r\n" +
                                  "С¬���ử�ǿգ��᲻�ܣ�Ψ�����棬������ģ����������Σ���飬�����������л����ī�أ������ӣ����Ա��ֵ�Ҳ������¬��\r\n",
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
            "��¬�ϡ��������ˣ�Сľ�ˡ��ף�С¬���ҵ��ţ��⺢������棬סһ�������͡���",
            "��������",
            "��¬�ϡ���������˵��֪�������£������һ��ǿա����⺢�ӣ�������������ۡ�",
            "��¬�ϡ���������ս���ϣ���ī�ص����£���Ӧ����ʶī�ء�",
            "��¬�ϡ��ҡ���û�ܼ������һ�棬ֻ֪������Ҫ����������",
            "��¬�ϡ�С¬���ʵ����Ķ�����ֻ��˵������Զ�������������������˻�������",
            "��¬�ϡ���֪����ô˵�Բ��ԣ���ֻ�ܻ����ˡ�",
            "��¬�ϡ����뻭��ǰ�Ķ�������Ұ��Ϫ����������Ц������ÿ���±ʣ��ֶ�������������ֻ��ģ����Ӱ�ӡ�",
            "��¬�ϡ�����Ϊ����һ��û��ս�������죬�ǿպ����������ڲݵ����ܣ������ڻ���������Ҳ���Ժ��ܰɡ���",
            "��¬�ϡ�����Ÿ�ī�أ��鷳���͹�ȥ��",
            "��¬�ϡ�īʫ˵ī�س�����˵ս����û��ס���ɷ򡭡��Ҳ�������лл����Щ���չ����ǡ�",
            "��¬�ϡ�лл������һ�ˣ�Сľ�ˣ����Ǵ�����æ�ġ�"
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
            taskCompleteText.text = "����5��������֮��";
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