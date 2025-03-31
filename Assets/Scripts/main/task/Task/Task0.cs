using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Task0 : TaskBase
{
    private TMP_Text dialogueText;
    private GameObject dialoguePanel;
    private Button nextButton;

    private RubyController rubyController;
    private string[] currentDialogue;
    private int dialogueIndex = 0;
    private bool hasStarted = false;

    // �滻Ϊ�򹤵Ŀ����Ի�������ī�ƴ屳��
    private string[] janeDialogue = new string[]
    {
        "�򹤣������ˣ�",
        "�򹤣��һ��������޺��㣬�õ��ǡ�ī�ӡ��ܸ�ԭ�������ľ�Ϻͻ���ƴ�ɵģ����ܲ�̫����Ӧ�����á�",
        "�򹤣�����ī�ţ�֪����",
        "�򹤣��Ҳ�֪���㻹�ǲ��ǵ��Լ�����;�������������������������ˡ�",
        "�򹤣���ľϻ���������̰���ż���Ͳ��",
        "�򹤣�����̰��ܼ�¼�㵱ǰ������",
        "�򹤣��ż���Ͳ��װ���յ����ţ������򿪱��˵��ţ�̫ʧ���ˡ�",
        "�򹤣�������ī�ƴ壬���Ӳ��󣬵��˸��˸���ͦԶ��",
        "�򹤣������ֵ����������û�����ţ�ֻ�ܿ�������ϵ��",
        "�򹤣���ϧ���Ų������������ܡ���Ⱦ���ķ���̫���ˡ�",
        "�򹤣����Եÿ��㡣",
        "�򹤣�ȥ����ÿ���˵�ס����һ�ˣ�����·�ɡ�֮ǰī�غ���˵��ķ��Ÿ������ӣ������ȥ��������",
        "�򹤣�������Ϊʲô������վ�������߷���·�������������",
        "�򹤣�����Ψһ�İ취��",
        "�򹤣�������д���ţ���ī�صģ������Ҹ�ÿ������д�˷���š�",
        "�򹤣���ȥ����ĥ�䣬�������˻������ҡ�"
    };

    // ����λ�ã��ɸ���ʵ�ʳ���������
    private Vector3 teleportPosition = new Vector3(-7.3f, -2.5f, -6.1f);

    void Start()
    {
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
            rubyController = FindObjectOfType<RubyController>(); // ��ȡ RubyController
            rubyController.pauseHealthUpdate = true; // ��ͣѪ�����£��������ī�������ƻ��ƣ��ɸ�Ϊ����״̬��ͣ��
            hasStarted = true;
            currentDialogue = janeDialogue;
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
        // ֱ����ʾ�Ի����
        dialoguePanel.SetActive(true);
        dialogueText.text = currentDialogue[dialogueIndex];
        // �Ӻ�������
        yield return StartCoroutine(FadeManager.Instance.FadeOut(3f));
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
        rubyController = FindObjectOfType<RubyController>(); // ��ȡ RubyController
        rubyController.pauseHealthUpdate = false; // �ָ�Ѫ�����£��ɸ�Ϊī�ŵ��;û��������ƣ�
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
    }

    public override string GetTaskName()
    {
        return "��������"; // ��Ϊ������ī�ƴ屳����������
    }

    public override string GetTaskObjective()
    {
        return "��򹤶Ի�"; // ���ּ��
    }

    public override bool IsTaskComplete()
    {
        return dialogueIndex >= currentDialogue.Length;
    }

    public override void DeliverLetter(string targetResident)
    {
        Debug.Log("Task0: ��ֻ�ǳ�ʼ�Ի������޷����š�");
        if (dialoguePanel != null && dialogueText != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = "�򹤣����Ѿ����������ˣ���ȥ���Űɡ�";
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => dialoguePanel.SetActive(false));
        }
    }
}