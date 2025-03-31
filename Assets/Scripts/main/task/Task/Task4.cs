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

    public override string GetTaskName() => "��������";

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
            currentDialogue = new string[] { "���������㻹û����" };
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
                        title = "���ŵ���",
                        content = "С�Ѱ��ã���ҹ������ǰ�������������ʱ������δ�ߣ�������ϴ���ǹ�񷶥������������ů�ɻ��ġ�����¶��׺��Ҷ����С�飬�紵���裬ɳɳ��衣����������֦��Ů�������ˮ�棬ˮ������Ρ���ҹ�����������ө����Ʒɣ�����׹���ҡ�����Ժ��ҡ�ȣ������¡���ѩƮ�������𣬻��س�̺��������ѩ�ˣ�Ц�����塣\r\n" +
                                  "С�ѣ�����������Ψ���ģ�Ȼ�������꣬����Ǵ����ã����ڰ���Ѱ�⡣�굱�£�����ĸ�����ֳ������굱�ƣ��������ˣ������������굱���棬׷�ʺιʣ�����һ�գ��������������������᲻֪��ɽ⣬ΨԸ�곤�����л�԰�������Ұ�����໨��������ĸ���������ǿ�����ʱ�����⣬���ȡ�������������\r\n",
                        icon = icon
                    });
                }
                else if (visitedLuoPo && !letterDeliveredToXiaoLu && currentDialogue.Length > 1)
                {
                    letterDeliveredToXiaoLu = true;
                    Debug.Log("Task4: �Ƴ����ŵ��ţ����С¬����");
                    taskManager.inventoryManager.RemoveLetter("���ŵ���");
                    Sprite icon = Resources.Load<Sprite>("jane"); // �� Resources ����С¬��ͼ��
                    taskManager.inventoryManager.AddLetter(new Letter
                    {
                        title = "С¬����",
                        content = "���װ��ã��������������ܣ�������ɳ���Ȼ���գ�Ψ�ŷ�ߵ�������飬������Ϸ���������ţ������߲��������ɳ�������Զ���������˼�ˣ���֪���¡��ử��໭�����������ݣ�����Ц��Ȼ���ử����δ��Ҳ��Ψ֪���Ա����ã���˼�ˡ�\r\n" +
                                  "�ﳣ�����л����۷�����͵�ۣ���һ���棬�����࣬ɫ��࣬���ʺΣ������˸�֮ò���᳤������˷��ﻭ���ã�ʤ����򱶣��ử������֮�����ᰮ��������֮�����������ˣ�����ɫ������ɷ�һ�ǿ����᣿��֪���Ƿ��������������Ψ�������йۣ�����С¬\r\n",
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
            "�����š���ѽ�������㣬���ŵ�Сľ�ˣ�������ôӲ���ġ�",
            "�����š����������İɣ���ͦ���ģ�������������̫�š�",
            "�����š���Ȼ������Զ�ף����Դ�������û�ˣ���������������Ψһ�������ˡ�",
            "�����š����������ҵ�ʱ���ҵĶ�����������ɢ��ɢ���е������������е�û����Ѷ���Ҹ���������Ϊ������",
            "�����š�������Ҳ���߲����ߣ�ֻʣ�����������ӡ�",
            "�����š�����ʱ�������ң�����������ֶ�ӣ�սǰ��û�ˣ�������һ����Ͷ���ҡ�",
            "�����š����ӳ����ӵ�����ӣ���ʮ���꣬���Ÿ��ƴ��ӣ��ݵ�����񡭡����ڶ����ˣ����������������������",
            "�����š������һ����ˣ�����������¡�",
            "�����š���ʵҲû��ģ��������鷳���ͷ��Ÿ�С¬��",
            "�����š��Ǻ��Ӳ����ף�ս��ʱ���ģ�û�������ס������������޳���",
            "�����š���������ǿ����������ͦ�񡭡����뾡������֪��Щ��ȥ���£�����û�����������ǿա������ֶ�˵�ˡ�",
            "�����š�������û��������д���������ˡ��������������д�ģ��������������̫��û��˼��",
            "�����š��������ˡ�"
        };
    }

    private string[] GetDialogueForXiaoLu()
    {
        return new string[]
        {
            "��С¬���ۣ������㣡�ҵ�ľͷ���ѣ���������죬��û���ҵ��ţ�",
            "��������",
            "��С¬����������˵�˺ö���Ȥ���£�ˮ��ġ��㡱��ʲô�������ú�����",
            "��С¬����û�����ǿգ������һ��˺ö��ǿ�����������ˣ���ϧ��˵�ǿ��Ѿ�û�ˡ���",
            "��С¬�����������������˭����֪���öණ����̫�����ˣ����и����롭��",
            "��С¬��ľͷ�ˣ�ֻ������Ŷ�����Ҳ������ҵ������϶��ǻز�����͵͵����д�š�",
            "��С¬����˵�ҵ��¸ң�����һ�����Ҿ���������������������Ĺ��£�˵����Զ�������ҡ�",
            "��С¬����֪�����Ҹ���ͬ�������д�¬������С¬��",
            "��С¬������Ÿ���鷳���͹�ȥ��",
            "��С¬����Ҫ���Ҽ�Ȼסһ��Ϊʲô��ֱ�Ӹ������Ҿ������������棡���͹�ȥ���϶����о�ϲ���������￪�ġ�",
            "��С¬�����´λ������һ��˸������㣬ľͷ���������ģ���˧��",
            "��С¬���´��鷳�������������ˡ��㡱��ʲô����"
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
            taskCompleteText.text = "����4������������";
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