using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Task0 : TaskBase
{
    private TMP_Text dialogueText;
    private GameObject dialoguePanel;
    private Button nextButton;

    private RubyController rubyController; // ����ľͯ�����ƿ����������滻Ϊ��������
    private string[] currentDialogue;
    private int dialogueIndex = 0;
    private bool hasStarted = false;

    // ��歶��Ŀ����Ի�������ī�Ҵ屳��
    private string[] jianShuErDialogue = new string[]
    {
        "��歶��������ˣ�",
        "��歶����һ��������޺��㣬�ô��������ɣľ�ͻ���ƴ�ɣ�ģ����ª�����ǲ�̫�ȣ�����������û���⡣",
        "��歶������ţ�����ľͯ�����ײ���",
        "��歶����Ҳ�֪�㻹�ǲ��ǵþ��յ��ô����ɴӽ����������»���ˡ�",
        "��歶�������ǰ��Ͳ��ߣ�������̰���š�",
        "��歶��������ī�Ҵ壬�ط�����",
        "��歶������װ�ÿ��ܻ��в��ȶ�����·���;ö���Ĺ��࣬���ܻᵹ�¡�",
        "��歶��������̫��������������У�ǰȥ��������Ѱ�������",
        "��歶�����ȥ���������һ�ˣ�����·��ǰЩ����ī��������ī���ͷ��ţ���ȥ���ưɡ���",
        "��歶���������Ϊɶû�Եķ��ӣ���ˮ������վ��·����ī�ӡ����ԡ�֪�к�һ����Ҫ����͵ÿ�����ľͷ���ܡ�",
        "��歶���������д���ţ���ī�صģ������Ҹ������˸����˷���š�",
        "��歶�����ȥ���𵢸飬����������ҡ�"
    };

    // ����λ�ã��ɸ���ʵ�ʳ���������
    private Vector3 teleportPosition = new Vector3(-7.3f, -2.5f, -6.1f);

    void Start()
    {
        // ��ʼ��ʱ����������������
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

    public void StartTaskDialogue()
    {
        if (!hasStarted)
        {
            rubyController = FindObjectOfType<RubyController>(); // ��ȡľͯ�����������滻Ϊ��������
            if (rubyController != null)
            {
                rubyController.pauseHealthUpdate = true; // ��ͣ�;öȸ��£�����������ƻ��ƣ�
            }
            hasStarted = true;
            currentDialogue = jianShuErDialogue;
            dialogueIndex = 0;

            TaskManager taskManager = GetComponent<TaskManager>();
            if (taskManager != null && taskManager.normalDialoguePanel != null)
            {
                taskManager.normalDialoguePanel.SetActive(false);
            }
            StartCoroutine(StartDialogueWithFadeOut());
        }
    }

    private IEnumerator StartDialogueWithFadeOut()
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = currentDialogue[dialogueIndex];
        yield return StartCoroutine(FadeManager.Instance.FadeOut(3f)); // �Ӻ�������
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
            StartCoroutine(TransitionAndTeleport());
        }
    }

    private IEnumerator TransitionAndTeleport()
    {
        yield return StartCoroutine(FadeManager.Instance.FadeToBlack(() =>
        {
            dialoguePanel.SetActive(false);
            TeleportPlayer();
            SetupNextTask();
        }, 1f));
    }

    private void TeleportPlayer()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (rubyController != null)
        {
            rubyController.pauseHealthUpdate = false; // �ָ��;öȸ���
        }
        if (playerController != null)
        {
            playerController.transform.position = teleportPosition;
            if (playerController.IsInDialogue())
            {
                playerController.EndDialogue();
            }
        }
        else
        {
            Debug.LogWarning("δ�ҵ� PlayerController���޷�������ң�");
        }
    }

    private void SetupNextTask()
    {
        TaskManager taskManager = GetComponent<TaskManager>();
        if (taskManager != null)
        {
            Task1 newTask = gameObject.AddComponent<Task1>();
            taskManager.SetTask(newTask);
            newTask.SetupDialogueUI(dialoguePanel, dialogueText, nextButton);
            taskManager.UpdateTaskDisplay();
        }
        else
        {
            Debug.LogError("TaskManager δ�ҵ����޷��л��� Task1��");
        }
    }

    public override string GetTaskName()
    {
        return "ī�Ҵ����ų���"; // ������ī�Ҵ屳����������
    }

    public override string GetTaskObjective()
    {
        return "���歶��Ի�"; // ������˵�Ŀ��
    }

    public override bool IsTaskComplete()
    {
        return dialogueIndex >= currentDialogue.Length; // �Ի��������������
    }

    public override void DeliverLetter(string targetResident)
    {
        Debug.Log("Task0: ��ֻ�ǳ�ʼ�Ի������޷����š�");
        if (dialoguePanel != null && dialogueText != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = "��歶������Ѿ����������ˣ���ȥ���Űɡ�";
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => dialoguePanel.SetActive(false));
        }
    }
}