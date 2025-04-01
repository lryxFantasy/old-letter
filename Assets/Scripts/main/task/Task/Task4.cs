using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task4 : TaskBase
{
    private bool visitedLuoPo = false; // �Ƿ�ݷ�����
    private bool letterDeliveredToXiaoLu = false; // �Ƿ��ʹ����Ÿ�С¬����
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

    public override string GetTaskName() => "ī�Ҿ���";

    public override string GetTaskObjective() => $"�ݷá����š���\n" +
                                                 $"�ʹ���š�����С¬�����ţ�{(letterDeliveredToXiaoLu ? "�����" : "δ���")}";

    public override bool IsTaskComplete() => visitedLuoPo && letterDeliveredToXiaoLu;

    public override void DeliverLetter(string targetResident)
    {
        dialogueIndex = 0;
        if (targetResident == "����" && !visitedLuoPo)
        {
            currentDialogue = GetDialogueForLuoPo();
        }
        else if (targetResident == "С¬" && visitedLuoPo && !letterDeliveredToXiaoLu)
        {
            currentDialogue = GetDialogueForXiaoLu();
        }
        else
        {
            currentDialogue = new string[] { "���������㻹û���ſ��͸����ˡ�" };
        }

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
            if (taskManager != null && taskManager.inventoryManager != null)
            {
                if (!visitedLuoPo && currentDialogue.Length > 1)
                {
                    visitedLuoPo = true;
                    Debug.Log("Task4: ������ŵ���");
                    Sprite icon = Resources.Load<Sprite>("jane"); // �� Resources �������ŵ�ͼ��
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "����������С¬֮��",
                        content = "С��ƽ������ҹˮ����̾��������ա���ˮδ����������������¶���飬����ݸ衣ˮ�����ᣬ��Ծ�岨����ө���ǣ���ѩ��̺��Ц���ƴ塣������ӣ�Ȼ��ī�ӡ��ơ���������ֹ��������ǵ����磬��ˮ����·��\r\n" +
                                  "��ī��������ˮ�����飬���������֣���ˮ�������︴�̣����֡��길¬ƽ����ˣ�������ѧ����������δ�ɡ���Ҫ�£���������ֳżң�Ҫ�ʣ���ˮ��Ϊ��ת������һ�죬�������ʶ��塣Ը��������Ұ����ˮ���⣬�����̡��������������������й⡣������������",
                        icon = icon
                    });
                }
                else if (visitedLuoPo && !letterDeliveredToXiaoLu && currentDialogue.Length > 1)
                {
                    letterDeliveredToXiaoLu = true;
                    Debug.Log("Task4: �Ƴ����ŵ��ţ����С¬����");
                    taskManager.inventoryManager.RemoveLetter("����������С¬֮��");
                    Sprite icon = Resources.Load<Sprite>("jane"); // �� Resources ����С¬��ͼ��
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "С¬��¬��֮��",
                        content = "��ã�������������������ȥ������ľͯ��·�᲻���죬�������ڣ�ֻ�з紵�������˵ģ�����ľͷ���������Ͽ�����������������֪������ɶ������ɶ�¡���˵���Ǻ��ˣ����������\r\n" +
                                  "���ϻ�ˮ�����ֶ���Ъ���۲���ѽ����͵���ﻭ�˸��ÿ����ˣ��߶���ɫ�࣬��˵�ǵ����ҳ���Ҳ�ܻ���ô���������Ż��ˣ�������ˮ����������Ц���ɻ����˵���û�������ﻭ�������ˣ��ܰѶ����������̫�����ˣ����ܸ��һ������𣿻��߻�����ˮ�������������֣�ת�������о�������֪��ˮ��զ��������Һò��ã�����С¬",
                        icon = icon
                    });
                }
            }
            else
            {
                Debug.LogError("Task4: TaskManager �� InventoryManager δ��ȷ��");
            }

            PlayerController playerController = Object.FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.IsInDialogue())
                playerController.EndDialogue();

            if (IsTaskComplete())
            {
                if (taskManager != null)
                {
                    Debug.Log("Task4: ������ɣ��л��� Task5");
                    Task5 newTask = gameObject.AddComponent<Task5>();
                    taskManager.SetTask(newTask);
                    newTask.SetupTask(taskManager, dialoguePanel, dialogueText, nextButton);
                    taskManager.UpdateTaskDisplay();
                }
                else
                {
                    Debug.LogError("Task4: TaskManager δ�ҵ����޷��л��� Task5");
                }
            }
        }
    }

    private string[] GetDialogueForLuoPo()
    {
        return new string[]
        {
            "�����š���ѽ�������㣬Сľ��ʹ������Ӳ���ġ�",
            "�����š���歶��������İɣ�����ϸ���ܵ��������Ϲ�ͷ�������Ұ�������ˡ�",
            "�����š�����¬ƽ��ī��Լ�þ����£�¬ƽ���ˣ�ī�ض��ˣ��Ҽ�Ҳɢ�ˣ�ֻʣ��������塣",
            "�����š���歶�����ʱ���ݵ�����񣬴ӳ�������Ͷ���ҡ�����������ī��æ��ˮ���ҿ�����ο��",
            "�����š����������ˣ���������¡�ûɶ���£������鷳���ͷ��Ÿ�С¬���Ǻ���û����¬ƽ�ߵ��磬��������",
            "�����š������棬��¬ƽСʱ��һ��������������֪������£���û����ûˮ�����ӡ�������߶�ˡ�",
            "�����š�����ûд�������ͽС��������ˡ�������������ң�����������̫�ŷ���л���㡣"
        };
    }

    private string[] GetDialogueForXiaoLu()
    {
        return new string[]
        {
            "��С¬���ۣ������㣡�ҵ�ľͷ���ѣ���������죬���ҵ�����",
            "��������",
            "��С¬����û����ûˮ������˺ö�ˮ������������ˣ�����˵�ﶼ��ˮ���ˡ���",
            "��С¬������֪������������˭�������ö̫࣬�����ˣ��Ҳ��ǡ���ľͷ�ˣ�ֻ�����㰡�����Ҳ������ҵ������϶���ˮ��ס�ˣ�͵͵д�Ÿ��ҡ�",
            "��С¬����˵�ҵ��¸ң�����Ҿ�����������ˣ����Ͻ�����˵�������Ͽ��ң���֪�����Ҹ���ͬ��������¬ƽ������С¬��",
            "��С¬�����Ÿ���鷳���ͣ�������Ϊɶ���Լ��������������棡���͹�ȥ����϶����ˡ�",
            "��С¬�����´λ������һ��˸��㿸ˮ������˧�ˣ�"
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
            taskCompleteText.text = "����4����ī�Ҿ���";
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

            Debug.Log("����4 ��ʼ�������ʾ������");
        }
        else
        {
            Debug.LogWarning("������������������δ��ȷ��ʼ�����޷���ʾ����4��ʼ��ʾ��");
        }
    }
}