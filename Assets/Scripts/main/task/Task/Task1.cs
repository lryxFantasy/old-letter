using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class Task1 : TaskBase
{
    [SerializeField] public int visitCount = 0; // �Ѱݷõľ�������
    [SerializeField] public bool letterDeliveredToMoShou = false; // �Ƿ��ʹ�򹤸�ī�ص���
    [SerializeField] public bool returnedToJianGong = false; // �Ƿ񷵻ؼ򹤴�
    [SerializeField] public string[] residents = { "ī��", "īʫ", "��", "����", "С¬", "¬��" }; // ī�ƴ�����б�
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
        return "ī�ƴ��һ����";
    }

    public override string GetTaskObjective()
    {
        return $"�ݷ�ī�ƴ�ļ�����ÿһλ����{visitCount}/5��\n\n" +
               $"�ʹ�򹤡�����ī�ء����ţ�{(letterDeliveredToMoShou ? "�����" : "δ���")}\n\n" +
               $"��ȥ�ҡ��򹤡���{(returnedToJianGong ? "�����" : "δ���")}";
    }

    public override bool IsTaskComplete()
    {
        return visitCount >= 5 && letterDeliveredToMoShou && returnedToJianGong;
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
                Debug.Log("������ż�");
                Sprite icon = Resources.Load<Sprite>("moshou"); // �� Resources ����ī�ص�ͼ��
                taskManager.inventoryManager.RemoveLetter("�򹤸�ī�ص���");
                taskManager.inventoryManager.AddLetter(new Letter
                {
                    title = "ī�ص���",
                    content = "īʫ������һľ��ʹ����ī�ţ�ľ���ª����֮���ã��᲻ϲ�����Ȼ���Դ���Ψһ;���߲����ˣ������ã�Ψ��ľ���ɴ����������޴˺����գ�����ľ��Ϊ֮����������ܡ�������Ū��ʫҮ��������ϲֽ�ʣ��᲻�����ã���ĸ��ʱ���������вţ�ν��ʫ��˲���ȴϲ֮�������춯����͢�������иܸ�֮�ã����۶���һ��ʡ������Ȼ��ī�ӡ��ơ������س����ȡ������ѧ�ν�չ����ս�ң���ʸ�����ȣ�ʹ���ղ��ߣ����ͬ�ۚ��أ�ѪȾɳ������С¬֮������������ĸ�ӹ¿࡭��������֮�գ����ھ��У�����ɫ��䣬��ĸ���಻����ȥ���᲻֪�����������գ�Ȼ������Ҳ��\r\n�괦��Σ��������������ã���ͷ������������֮������������ѧһ������ν��ʫ���ã�Ψ������֪�갲����������Ҳ��ʫ���ѽ�����Ȼ�᲻����ʧ�ꡣ���Դ�ľ��ʹ�ɿ��������ԣ���飬��ʹ��ú���Ͼ��գ�����ǿ������ī��\r\n",
                    icon = icon
                });
            }
            else
            {
                Debug.LogError("TaskManager �� InventoryManager δ��ȷ�󶨣��޷����±���");
            }
        }
        else if (targetResident == "��")
        {
            currentDialogue = GetDialogueForResident("��");
            if (visitCount >= 5 && letterDeliveredToMoShou && !returnedToJianGong)
            {
                returnedToJianGong = true;
            }
        }
        else if (System.Array.IndexOf(residents, targetResident) >= 0)
        {
            currentDialogue = GetDialogueForResident(targetResident);
            VisitResident(targetResident);
        }
        else
        {
            currentDialogue = new string[] { "���������㻹û����" };
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
            Debug.LogError("δ�ҵ� TaskCompletePanel����ȷ�� Task0 �Ѵ�������壡");
        }
    }

    private IEnumerator ShowTaskStartPanel()
    {
        if (taskCompletePanel != null && taskCompleteText != null)
        {
            taskCompleteText.text = "����1����ī�ƴ��һ����";
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
                    "��ī�ء�����ʲô��������ľͷ������Ķ����ģ�",
                    "��ī�ء��������������ģ�",
                    "��ī�ء��ߣ���Ѿͷ�ܰ�ŪЩϡ��ŹֵĶ�������",
                    "��ī�ء����أ����ó�������ĥ�䣡",
                    "��������",
                    "��ī�ء��������㵱ī�ƴ����ʹ��",
                    "��ī�ء��ߣ����ڵ��˰��������Ŷ���ľͷ���ģ�˭֪����᲻�ỵ����",
                    "��ī�ء����ˣ�����Ÿ�īʫ������������д��Щû�õ�ʫ�����˾͸�ѧ��ʵ�õĶ�����",
                    "��ī�ء���Ҫ�ǸҲ��Ż���Ū���ˣ��ҷǲ����㲻�ɣ�",
                    "��ī�ء����������ߣ�����������ۣ���������ľͷ���Ӿͷ���"
                };
            case "īʫ":
                return new string[]
                {
                    "��īʫ��Ŷ�������µ���ʹ��������ģ�������������ĳ����ġ�",
                    "��īʫ���Ÿ��ҿ�����",
                    "��������",
                    "��īʫ����Ȼ�Ǽ��޵��㣿��������������Ȼ����̫���ҵ�ʫ��",
                    "��īʫ����������о�ʱ���������ǹ���������ʫ��ľ�ɫ������",
                    "��īʫ����ϧ�߲������Ǹ����ˣ����Ѽ��档",
                    "��īʫ��������������������רע�����ӣ�������ֻ����ҽ����ֺͻ��ֵĵ�����",
                    "��īʫ�����һֱ�ڴ���������Ҳ�����죬�Ҹ���д��ʫ����������ľͷ���Ӻ����ĺ�г��"
                };
            case "��":
                if (visitCount >= 5)
                {
                    returnedToJianGong = true;
                    return new string[]
                    {
                        "���򹤡������ˣ�����Ԥ�Ƶ����̣�����ľͷ��ͦ��ʵ��",
                        "���򹤡��Ŷ��͵��ˣ��ܺã���ɵò���",
                        "���򹤡�ī��û����ϳ�ȥ�ɣ�����ϲ����е������������ս�����µģ�������Ҳ��ս��ȥ���ˡ���",
                        "���򹤡��������У����������Ŀ��ס�",
                        "���򹤡�ī�����������ˣ��ã��Ҹ����������̰塣",
                        "���򹤡���ľϻ�ұ߿����Լ������Ժ��Լ����¡�",
                        "���򹤡���ָ���ҿ��㣬����Ǹ����ߣ���������ı�ְ��"
                    };
                }
                else
                {
                    return new string[]
                    {
                        "���򹤡�������������",
                        "���򹤡���ȥ�ɣ���������˷�ʱ�䣬���ﻹ�������˵����������ء�"
                    };
                }
            case "����":
                return new string[]
                {
                    "�����š���ѽ��������Ǹ����ŵ�Сľ�ˣ�",
                    "�����š�ͦ�ɰ��ģ���Ȼ��ľͷ���ģ��Դ��϶�Ӳ����ʯͷ��",
                    "�����š��򹤸��ҵ������������ˣ�Сľ�ˡ�",
                    "��������",
                    "�����š�ԭ�����������򹤰����޺��ˣ�����Ȼ�б��¡�",
                    "�����š����������Ҵӳ����ӵ�����ӱ��ߣ��һ���������������Ƶûţ����ڿ���������īʫ���ò����Ǻǡ���",
                    "�����š�·��С�ģ��߲��˲����㣬�������·�����ߣ���ˤ���ˡ�"
                };
            case "С¬":
                return new string[]
                {
                    "��С¬���ۣ��������ľͷ�ˣ�����𣿻�˵���𣿻����ܱ���",
                    "��С¬������Ķ����ģ�ɽ������˵ɽ���йֶ��������ǹֶ�����",
                    "��С¬���������ҵ��ţ�����ģ�����ҿ���",
                    "��������",
                    "��С¬��̫���ˣ��Ժ���������ʹ�ˡ�",
                    "��С¬����ǰһ���²��и����������Ǵ塣",
                    "��С¬�����´λ������ҿ��������ī���㻭һ���������������㣡"
                };
            case "¬��":
                return new string[]
                {
                    "��¬�ϡ��������ŵģ���ϡ�棬����Ķ����ģ�",
                    "��¬�ϡ����������İɣ��������޳����ֶ�������ֻ����ī��Щ���յĻ��䡭���Ÿ��ҿ�����",
                    "��������",
                    "��¬�ϡ��Ժ��������ʹ�ˣ�����Ҳ����������",
                    "��¬�ϡ�ϣ��С¬�ܿ�����Щ����лл������һ�ˡ�",
                    "��¬�ϡ������������ϵ�ľ�ƣ�����ͦ�ã���Ż����������µĺۼ���"
                };
            default:
                return new string[] { "���������㻹û����" };
        }
    }
}