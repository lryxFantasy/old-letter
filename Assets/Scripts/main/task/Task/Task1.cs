using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task1 : TaskBase
{
    [SerializeField] public int visitCount = 0; // �Ѱݷõľ�������
    [SerializeField] public bool letterDeliveredToMoShou = false; // �Ƿ��ʹ��歶���ī�ص���
    [SerializeField] public bool returnedToJianShuEr = false; // �Ƿ񷵻ؼ�歶���
    [SerializeField] public string[] residents = { "ī��", "ī��", "��歶�", "����", "С¬", "¬��" }; // ī�Ҵ�����б�
    [SerializeField] public bool[] visitedResidents; // ��¼�����Ƿ񱻰ݷ�

    private TMP_Text dialogueText;
    private GameObject dialoguePanel;
    private Button nextButton;

    private GameObject normalDialoguePanel;
    private Button deliverButton;

    private string[] currentDialogue;
    private int dialogueIndex = 0;
    private string currentResident;

    private GameObject taskCompletePanel;
    private TextMeshProUGUI taskCompleteText;

    void Start()
    {
        visitedResidents = new bool[residents.Length];
        SetupTaskCompletePanel(); // ��ʼ������������
        StartCoroutine(ShowTaskStartPanel()); // ��ʾ����ʼ��ʾ
    }

    public override string GetTaskName()
    {
        return "ī�Ҵ��һ����";
    }

    public override string GetTaskObjective()
    {
        return $"�ݷ�ī�Ҵ�ļ�歶�����ÿλ����{visitCount}/5��\n\n" +
               $"�ʹ��歶�������ī�ء����ţ�{(letterDeliveredToMoShou ? "�����" : "δ���")}\n\n" +
               $"��ȥ�ҡ���歶�����{(returnedToJianShuEr ? "�����" : "δ���")}";
    }

    public override bool IsTaskComplete()
    {
        return visitCount >= 5 && letterDeliveredToMoShou && returnedToJianShuEr;
    }

    public void SetupDialogueUI(GameObject panel, TMP_Text text, Button button)
    {
        dialoguePanel = panel;
        dialogueText = text;
        nextButton = button;
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextDialogue);
        dialoguePanel.SetActive(false);
    }

    public void SetupDeliverButton(GameObject normalPanel, Button deliverBtn)
    {
        normalDialoguePanel = normalPanel;
        deliverButton = deliverBtn;
        deliverButton.onClick.RemoveAllListeners();
        deliverButton.onClick.AddListener(() =>
        {
            normalDialoguePanel.SetActive(false);
            GetComponent<TaskManager>().TriggerDeliverLetter();
        });
    }

    public override void DeliverLetter(string targetResident)
    {
        currentResident = targetResident;
        dialogueIndex = 0;

        if (targetResident == "ī��" && !letterDeliveredToMoShou)
        {
            currentDialogue = GetDialogueForResident("ī��");
            letterDeliveredToMoShou = true;
            VisitResident(targetResident);
            TaskManager taskManager = GetComponent<TaskManager>();
            if (taskManager != null && taskManager.inventoryManager != null)
            {
                Debug.Log("������ż���ī����ī��");
                Sprite icon = Resources.Load<Sprite>("jane"); // �� Resources ����ī�ص�ͼ��
                taskManager.inventoryManager.RemoveLetter("��歶���ī�ص���");
                taskManager.inventoryManager.AddLetter(new Letter
                {
                    title = "ī����ī��֮��",
                    content = "ī�ɣ���歶�Ū�˸�ľͯ���ţ�ɣľ����Ƕ���֣���·�������죬���Ű��ۡ��������죬��ˮ�ٵأ���͢�����ײߣ���֪�����������ɡ�ī�ӡ��ơ���������ֹ���������������ƣ���¬ƽ����Լ�Ȳ�������������ߣ������ѻң��껹�������ɾȺ���\r\n" +
                              "���иܸˣ����ۿ�ʡ��������ս����Ͷʯ����һ����ʯ�ٽȻ��ˮ���죬���ã��������Σ���歶�˵�곣����������˼�ɣ�������ˮ���гɡ��������ߣ�ֻ���³�͢����Ұ�����ȼ��ַ�������ˮ����ֻ����ƽ������������ʧ���ˡ�����ī��",
                    icon = icon
                });
            }
            else
            {
                Debug.LogError("TaskManager �� InventoryManager δ��ȷ�󶨣��޷����±���");
            }
        }
        else if (targetResident == "��歶�")
        {
            currentDialogue = GetDialogueForResident("��歶�");
            if (visitCount >= 5 && letterDeliveredToMoShou && !returnedToJianShuEr)
            {
                returnedToJianShuEr = true;
            }
        }
        else if (System.Array.IndexOf(residents, targetResident) >= 0)
        {
            currentDialogue = GetDialogueForResident(targetResident);
            VisitResident(targetResident);
        }
        else
        {
            currentDialogue = new string[] { "���������㻹û���ſ��͸����ˡ�" };
        }

        StartDialogue();
    }

    private void VisitResident(string residentName)
    {
        int index = System.Array.IndexOf(residents, residentName);
        if (index >= 0 && !visitedResidents[index])
        {
            visitedResidents[index] = true;
            visitCount++;
            Debug.Log($"�ݷ��� {residentName}����ǰ���ȣ�{visitCount}/5");
            UpdateDisplay();
        }
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
            UpdateDisplay();
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null && playerController.IsInDialogue())
            {
                playerController.EndDialogue();
            }
            if (IsTaskComplete())
            {
                TaskManager taskManager = GetComponent<TaskManager>();
                if (taskManager != null)
                {
                    Task2 newTask = gameObject.AddComponent<Task2>();
                    taskManager.SetTask(newTask);
                    newTask.SetupTask(taskManager, dialoguePanel, dialogueText, nextButton);
                    taskManager.UpdateTaskDisplay();
                }
            }
        }
    }

    private void UpdateDisplay()
    {
        TaskManager manager = GetComponent<TaskManager>();
        if (manager != null)
        {
            manager.UpdateTaskDisplay();
        }
    }

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
            Debug.LogError("δ�ҵ� TaskCompletePanel����ȷ���������Ѵ�������壡");
        }
    }

    private IEnumerator ShowTaskStartPanel()
    {
        if (taskCompletePanel != null && taskCompleteText != null)
        {
            taskCompleteText.text = "����1����ī�Ҵ��һ����";
            taskCompletePanel.SetActive(true);

            CanvasGroup canvasGroup = taskCompletePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = taskCompletePanel.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0f;
            }

            float fadeDuration = 1f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(2f);

            elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;

            Debug.Log("����1 ��ʼ�������ʾ������");
        }
        else
        {
            Debug.LogWarning("������������������δ��ȷ��ʼ�����޷���ʾ����1��ʼ��ʾ��");
        }
    }

    private string[] GetDialogueForResident(string residentName)
    {
        switch (residentName)
        {
            case "ī��":
                return new string[]
                {
                    "��ī�ء�����ʲô�ֶ�����һ��ľͷҲ���������ţ�",
                    "��ī�ء���歶����ĳ����İɣ�",
                    "��ī�ء��ߣ���������ô�����֡������أ�������������������ƣ�",
                    "��������",
                    "��ī�ء������㵱�������ʹ��",
                    "��ī�ء���ˮ���͵��ſ��ˣ���Ū��Щ������˭֪�����ܳż��죿",
                    "��ī�ء����ˣ�����Ÿ�ī�ɣ����������������ײ߸���͢����ī�ӡ����ԡ���������ֹ������Ҫ���أ��˺���Ϊ��",
                    "��ī�ء���Ҫ��Ū�����ţ�����������ľ���ӣ�",
                    "��ī�ء������ߣ���������������������������"
                };
            case "ī��":
                return new string[]
                {
                    "��ī�ɡ�Ӵ��������Ǹ�ľ��ʹ��ģ������Ȥ�����ɣ����������ġ�",
                    "��ī�ɡ��Ÿ������ơ�",
                    "��������",
                    "��ī�ɡ���歶��������治����ľͷ���������������ɸ��׻��������ӣ�����̧ͷ��",
                    "��ī�ɡ��ᱲӦ��������Ϊ���Σ��ȼò������ɸ������ɱ����ؾɣ�������������",
                    "��ī�ɡ��㳣�ڴ��������Ժ������п���Ĳ��ӣ������ͳ�ȥ�����ǵþ�����ˣ�"
                };
            case "��歶�":
                if (visitCount >= 5 && letterDeliveredToMoShou)
                {
                    returnedToJianShuEr = true;
                    return new string[]
                    {
                        "����歶��������ˣ�������Ŀ��̣�����ľͷͦ���á�",
                        "����歶����Ŷ��͵��ˣ��ɵò���",
                        "����歶���ī��û������ɣ�����ϲ�����",
                        "����歶������ɣ��������Ŀ��ס�",
                        "����歶���ī�����������ͣ��ã��Ҹ��㻻����̰塣",
                        "����歶����ұ�ľϻ�Լ������Ժ��Լ��ܰɡ�",
                        "����歶���������ҿ��㣬���ǹ��ߣ������Ǳ��֡�"
                    };
                }
                else
                {
                    return new string[]
                    {
                        "����歶���������������",
                        "����歶�����ȥ�ɣ��������ĥ�䣬���ﻹ���˵����ء�"
                    };
                }
            case "����":
                return new string[]
                {
                    "�����š���ѽ���������ŵ�Сľ�ˣ�",
                    "�����š����Źֿɰ��ģ�ľͷ����Ӳ��",
                    "�����š���歶����ҵ��ţ�л�ˣ�С�һ",
                    "��������",
                    "�����š�ԭ�������޵��㣬�������͡�",
                    "�����š���ˮ����ʱ���һ���Ϊ�������ˣ�����ī��æ����ˮ�������е���ͷ����",
                    "�����š�·�ϵ��ģ���ˮ�岻���㣬�ɴ������Ţ·�����ߣ���ס�ˡ�"
                };
            case "С¬":
                return new string[]
                {
                    "��С¬���ۣ�����ľͷ�ˣ����ˮ���𣿻�º�ˮ�𣿻����ܷ�������",
                    "��С¬������Ķ����ģ�ˮ��������˵ˮ����Źֶ��������ǹֶ�����",
                    "��С¬���������ҵ��ţ����梵ģ�����ң�",
                    "��������",
                    "��С¬��̫���ˣ��Ժ��������ţ��Ҿ���֪��ˮ��ɶʱ���޺ã�",
                    "��С¬����ˮ���ˣ�����������������ˡ�",
                    "��С¬�����´λ������������ī���㻭���񣬻��㿸��ˮ�����������ˣ�"
                };
            case "¬��":
                return new string[]
                {
                    "��¬�ϡ��������ŵģ�ϡ���ˣ�����Ķ�ð�����ģ�",
                    "��¬�ϡ���歶��������ģ�������ľͷ����ϧ��ֻ�ửͼֽ�����Ÿ������ơ�",
                    "��������",
                    "��¬�ϡ�����������ʹ�������������ˮ�ķ��ӡ�",
                    "��¬�ϡ�ϣ��С¬�ܿ�����ظɵ����졭����л�����ȡ�"
                };
            default:
                return new string[] { "���������㻹û���ſ��͸����ˡ�" };
        }
    }
}