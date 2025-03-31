using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterCodex : MonoBehaviour
{
    [SerializeField] private GameObject codexPanel;          // 主图鉴面板
    [SerializeField] private Button openButton;             // 打开图鉴的按钮
    [SerializeField] private Button closeButton;            // 主面板关闭按钮
    [SerializeField] private GameObject characterInfoPanel; // 角色信息面板
    [SerializeField] private Button infoCloseButton;        // 角色信息面板关闭按钮

    // 人物按钮数组
    [SerializeField] private Button[] characterButtons = new Button[6];

    [SerializeField] private TextMeshProUGUI characterName; // 人物名称显示
    [SerializeField] private TextMeshProUGUI characterStory;// 人物背景故事显示

    private bool isMainPanelOpen = false;
    private bool isInfoPanelOpen = false;

    void Start()
    {
        // 初始化时隐藏所有面板
        codexPanel.SetActive(false);
        characterInfoPanel.SetActive(false);

        // 添加按钮监听
        openButton.onClick.AddListener(OpenCodex);
        closeButton.onClick.AddListener(CloseCodex);
        infoCloseButton.onClick.AddListener(CloseInfoPanel);

        // 为每个角色按钮添加监听
        characterButtons[0].onClick.AddListener(() => ShowCharacterInfo(0));
        characterButtons[1].onClick.AddListener(() => ShowCharacterInfo(1));
        characterButtons[2].onClick.AddListener(() => ShowCharacterInfo(2));
        characterButtons[3].onClick.AddListener(() => ShowCharacterInfo(3));
        characterButtons[4].onClick.AddListener(() => ShowCharacterInfo(4));
        characterButtons[5].onClick.AddListener(() => ShowCharacterInfo(5));
    }

    void OpenCodex()
    {
        codexPanel.SetActive(true);
        isMainPanelOpen = true;
        Time.timeScale = 0f; // 暂停时间
    }

    void CloseCodex()
    {
        codexPanel.SetActive(false);
        characterInfoPanel.SetActive(false); // 确保信息面板也关闭
        isMainPanelOpen = false;
        isInfoPanelOpen = false;
        Time.timeScale = 1f; // 恢复时间
    }

    void CloseInfoPanel()
    {
        characterInfoPanel.SetActive(false);
        isInfoPanelOpen = false;
    }

    void ShowCharacterInfo(int characterIndex)
    {
        characterInfoPanel.SetActive(true);
        isInfoPanelOpen = true;

        switch (characterIndex)
        {
            case 0: // 老兵 - 维克托·凯恩
                characterName.text = "维克托·凯恩";
                characterStory.text = "身份：退役军官，曾参与启示录战争，因战争腿部受伤严重。\n" +
                    "性格：严肃、沉默寡言，对未来持悲观态度，厌恶科技，但仍然关心自己的儿子。\n" +
                    "背景：\n      维克托·凯恩曾是“太阳战争”中的军官，参与过无数盛大的战役，在战争中，他眼睁睁看着战友倒在混乱的枪声中，妻子因为辐射病丧生。他的手下的一名牺牲士兵，是小卢克·伍德的父亲。在流弹击伤腿后，他从战场退役后，回到了信火村。\n" +
                    "      他沉默寡言，时常独自一人静坐在房间中，他有着坚定的信仰。孤独是他对抗痛苦的方式，但每当机器人送来消息，他都会颤抖着摸索信件，感受纸张的温度，他厌恶科技，咒骂那毁灭一切的导弹，却又离不开ai信使，它是他与儿子伊莱亚斯·凯恩唯一的纽带。他和儿子关系比较僵硬，他不喜欢儿子写诗歌这种毫无意义的事情，他和儿子目前分开居住，尽管如此他仍在心里牵挂着儿子。\n" +
                    "写信风格：\n      严肃、克制，几乎没有多余的修饰。用词简练，像军令一样直截了当，不喜欢拐弯抹角。";
                break;

            case 1: // 诗人 - 伊莱亚斯·凯恩
                characterName.text = "伊莱亚斯·凯恩";
                characterStory.text = "身份：年轻的诗人，老兵的儿子，崇尚和平，反对战争。\n" +
                    "性格：感性、富有想象力，追求艺术与思想自由。\n" +
                    "背景：\n      伊莱亚斯·凯恩是老兵维克托·凯恩的独子，生于战火纷飞的年代，从小听着父亲的战地故事长大，母亲因为辐射病去世后，那些血与铁的回忆在他心中种下对和平的渴望。\n" +
                    "      他拒绝继承父亲的军人传统，选择用诗歌对抗战争的阴影，他与女友简·怀特的爱情是他创作的灵感，她冷静的理性与他炽热的感性碰撞，他常说“她的公式算不出我的诗，但我需要她的光”，在残阳下，他用每一行诗句编织梦想，希望用文字为这个破碎的世界缝上一抹曙光。\n" +
                    "写信风格：\n      富有诗意，充满隐喻和象征。情感外露，热情洋溢，不介意在信中表达自己的内心世界。语言优美，每一封信都像首诗。在写给简·怀特的信件中，会更富有情感波动，甚至带点孩子气的抱怨。";
                break;

            case 2: // 工程师 - 简·怀特
                characterName.text = "简·怀特";
                characterStory.text = "身份：年轻的理工学生，诗人的女友。\n" +
                    "性格：冷静理性，不擅长表达情感，热衷于科技，擅长制作一些科技产品。\n" +
                    "背景：\n      简·怀特是一位年轻的理工学生，战争摧毁了她的校园，听取远房祖母萝丝的建议后，她带着一本烧焦的笔记本逃到了信火村。\n" +
                    "      她擅长用残存的零件制作实用的小科技产品，她冷静而理性，热衷于解决问题，坚信科技能让生活更好。由于辐射，她与恋人——诗人伊莱亚斯·凯恩无法时常见面，他的热情似火，而简却不擅长表达，她常笑着说“机器可比诗歌实在”。简对战争的浩劫无能为力，但她用一件件手工制品守护村子，她的目标简单而坚定：用科技的微光，温暖这个破碎的世界。\n" +
                    "写信风格：\n      逻辑清晰，偏向功能性，像是在做实验报告。语言简练，句子不带太多感情色彩，但会通过细节流露关心。经常在信中附带一些手绘草图、设备改造计划或小发明的进度报告。在写给伊莱亚斯的信里，会有一种“用理性克制感性”的微妙情绪。";
                break;

            case 3: // 老奶奶 - 萝丝
                characterName.text = "萝丝";
                characterStory.text = "身份：一位普通的老妇人，简的远房祖母，小卢克的神秘笔友。\n" +
                    "性格：温柔、神秘，拥有一颗悲悯的心。\n" +
                    "背景：\n      萝丝是一位普通的老妇人，住在信火村边缘的小屋里，外表平凡却藏着温柔而神秘的心，她的孙子死于辐射病，家人也在战火与混乱中分离，留下她独自面对残阳下的孤寂。每每想到自己逝去的孙子，她总会悲伤，但她从不沉溺，信中总带着乐观的语气。而小卢克·格雷——一位素未谋面的孩子成为了她的神秘笔友。\n" +
                    "      小卢克也失去了父亲，萝丝在信中写下温暖的话语，讲战前的花海、河流的歌声，和那些逝去的美好，她刻意隐瞒自己的身份，担心年老的自己与孩子有代沟，只以“神秘朋友”为名，用书信教他勇敢、善良，把他当作自己孙子的影子。萝丝虽失去亲人，却用智慧与爱支撑着这个孩子，在残阳的余晖中，她是卢克看不见的守护者，用每封信编织希望的微光。\n" +
                    "写信风格：\n      温暖而富有故事性，语言柔和，字里行间充满生活气息，带着一种旧时代的优雅和美好。避免表达悲伤，总是传递希望和温暖，时常提及过去的美好时光。在写给小卢克的信中，使用“神秘朋友”的语气。";
                break;

            case 4: // 小卢克·伍德
                characterName.text = "小卢克·伍德";
                characterStory.text = "身份：伊芙的儿子，聪明又富有好奇心。\n" +
                    "性格：天真、乐观，总是渴望探索世界。\n" +
                    "背景：\n      他的父亲在战争中牺牲，母亲为了纪念他，给儿子取名父亲的同名，小卢克从未见过父亲。他不知道自己的笔友是谁，只知道那个“神秘朋友”总能回信，给他讲许多过去的故事。他梦想着成为科学家，修复被破坏的世界，但。\n" +
                    "写信风格：\n      充满童趣，词句跳跃，有时语法混乱但天真可爱。会用夸张的比喻，信中充满对世界的好奇，经常有一连串的问号。在写给“神秘朋友”的信中，会毫无保留地表达自己的情绪。";
                break;

            case 5: // 画家 - 伊芙·伍德
                characterName.text = "伊芙·伍德";
                characterStory.text = "身份：艺术家，画家，小卢克的母亲，深爱色彩与创造。\n" +
                    "性格：感性、执着，对现实无奈但不愿屈服。\n" +
                    "背景：\n      伊芙·伍德是信火村的画家，一位深爱色彩与创造的单亲母亲，她的丈夫在“太阳战争”中丧生，留下她与幼子小卢克·格雷相依为命。她通过机器人从废墟中搜集泥土、草汁作画，她想为卢克画一个“没有战争的明天”。\n" +
                    "      伊芙感性而执着，常在儿子睡下后彻夜作画，在这破碎的世界里，她用每一抹色彩守护记忆与爱，是卢克生命中最坚韧的依靠。\n" +
                    "写信风格：\n      语言优美，充满色彩感，像是在描绘画面。善于用细腻的笔触描述日常，哪怕是一顿饭、一扇窗外的风景，都能写得富有生命力。经常附上一些速写。";
                break;
        }
    }

    void Update()
    {
        // 按ESC键关闭面板
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isInfoPanelOpen)
            {
                CloseInfoPanel();
            }
            else if (isMainPanelOpen)
            {
                CloseCodex();
            }
        }
    }
}