using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task2 : TaskBase
{
    private bool letterDeliveredToMoShi = false; // �Ƿ��ʹ�ī�ظ�īʫ����
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

    public override string GetTaskName() => "��ѧ��ʫ��";

    public override string GetTaskObjective() => $"�ʹī�ء�����īʫ�����ţ�{(letterDeliveredToMoShi ? "�����" : "δ���")}";

    public override bool IsTaskComplete() => letterDeliveredToMoShi;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        currentDialogue = targetResident == "īʫ" && !letterDeliveredToMoShi
            ? GetDialogueForMoShi()
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
            if (!letterDeliveredToMoShi && currentDialogue.Length > 1)
            {
                letterDeliveredToMoShi = true;
                if (taskManager != null && taskManager.inventoryManager != null)
                {
                    Debug.Log("Task2: �����ż� - �Ƴ�ī�ص��ţ����īʫ����");
                    Sprite icon = Resources.Load<Sprite>("jane"); // �� Resources ����īʫ��ͼ��
                    taskManager.inventoryManager.RemoveLetter("ī�ص���");
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "īʫ����",
                        content = "�򹤣�������֮�⣬�����֮�棬���������֮�ǡ��겻�������֮�磬����������������µ�����Ȼ�᲻��Ϊ�⡣�򹤣���������֮ҹ������������߲�δ��壬�������գ��������龵������ɽ�¡�������������ȹ������ľϻ�����ǹ�֮�䣬��Ӱҡҷ������Ӱ��⣬�����ġ��������Ϊ����Ի��ʫ�����Ц���ԡ�ʫ���ɽ����󡱡���ʱ�������˫Ŀ������δ����Ⱦ֮ˮ���������ӡ�\r\n" +
                                  "���᳣�����ꡣ����С�������ף�רע֮�ˣ�ʤ��ʫ���򾰡��˿̣��������ӡ��ᰮ�꣬�򹤡���˼�꣬���н���Ӱ���߲�������ˣ�Ȼ��Ը���ɶ꣬���괰ǰ֮�ƣ��ݳɻң��ഥ���¡��겻�����ȣ��򹤣�Ȼ�����ᰮ��֤֮��������������֮�ۣ�����ᣬ��������Ϊ��ȼ����Ұ��Ϊ��Ϳ���˰���֮ɫ���򹤣����鲻Ϩ������īʫ",
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

    private string[] GetDialogueForMoShi()
    {
        return new string[]
        {
            "��īʫ��Ŷ�������㣬Сľ��ʹ��˭���ţ����ҿ�����",
            "��������",
            "��īʫ���������׻��������ӣ������������������Ρ���������˵����飬����ǰ���ſ�û��ô��Ҳ��������˻����ʵ�ɡ�",
            "��īʫ�����ᵽ����һ��ǵ������ҵ�һ��ʫʱ��Ц��˵�ҿ��Ե�ʫ�ˡ�����ʱ���ǿջ�������",
            "��īʫ���������ǳ��֣��������ˣ����״����뿪����������ī�ƴ塣",
            "��īʫ����û������������Ҳ�֪������Ⱦ���߲�ȥ���ˣ����ײ�������ҡ��������ҵ�ʫû�ã��������һ������ķ�ʽ�������Ҹ����ֿ�ס��",
            "��īʫ������Ȼ���ҹ�����ô����������Ȼ�����������ƽʱ��Ĭ���������ս��������ɽ���һ���Ϊ��������ʲô�ǹ��ġ�",
            "��īʫ����֪��������û˵����ʧȥ�ҡ�",
            "��īʫ������Ÿ��򹤣�����ȥ���������������������о�ʱ��ü�����ӣ������������⣬������ֻ��Ц�ҵ�ʫû����",
            "��īʫ����Ҫ��ȥ�����Ƕ�������˵һ�䡭����û�����о���ѧ��־�򣬵���Ҳ���������ҵ�ʫ�ʡ��һ�������š�"
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
            taskCompleteText.text = "����2������ѧ��ʫ��";
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