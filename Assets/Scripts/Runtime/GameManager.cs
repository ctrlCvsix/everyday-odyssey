using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EverydayOdyssey
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private Text objectiveText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text focusText;
        [SerializeField] private Text promptText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text tutorialBodyText;
        [SerializeField] private Text languageLabelText;
        [SerializeField] private Text startButtonText;
        [SerializeField] private Text chineseButtonText;
        [SerializeField] private Text koreanButtonText;
        [SerializeField] private Image tutorialPanel;
        [SerializeField] private Button startButton;
        [SerializeField] private Button chineseButton;
        [SerializeField] private Button koreanButton;
        [SerializeField] private Button floatingChineseButton;
        [SerializeField] private Button floatingKoreanButton;
        [SerializeField] private Text floatingChineseButtonText;
        [SerializeField] private Text floatingKoreanButtonText;
        [SerializeField] private Text gameOverTitleText;
        [SerializeField] private Text gameOverBodyText;
        [SerializeField] private Text restartButtonText;
        [SerializeField] private Image gameOverPanel;
        [SerializeField] private Button restartButton;
        [SerializeField] private float totalTimeSeconds = 420f;
        [SerializeField] private int totalTerminals = 5;
        [SerializeField] private int maxCaughtCount = 3;
        [SerializeField] private PrototypeAudioBank audioBank;
        [SerializeField] private PlayerController player;

        private float timeRemaining;
        private int hackedTerminals;
        private float statusTimer;
        private string pendingPrompt;
        private int caughtCount;

        public bool AllTerminalsHacked => hackedTerminals >= totalTerminals;
        public bool IsGameOver { get; private set; }
        public bool GameplayActive { get; private set; }
        public LanguageOption CurrentLanguage { get; private set; } = LanguageOption.Chinese;
        public PrototypeAudioBank AudioBank => audioBank;

        private void Awake()
        {
            Instance = this;
            timeRemaining = totalTimeSeconds;
        }

        private void Start()
        {
            SetLanguage(LanguageOption.Chinese, false);
            OpenTutorial();
            HookButtons();
            RefreshUI();
        }

        private void Update()
        {
            if (IsGameOver || !GameplayActive)
            {
                return;
            }

            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                Lose(Localize(
                    "\u622a\u6b62\u65f6\u95f4\u5230\u4e86\uff0c\u4f60\u6ca1\u80fd\u63d0\u4ea4\u9879\u76ee\u3002",
                    "\ub9c8\uac10 \uc2dc\uac04\uc774 \uc9c0\ub0ac\uc2b5\ub2c8\ub2e4. \ud504\ub85c\uc81d\ud2b8\ub97c \uc81c\ucd9c\ud558\uc9c0 \ubabb\ud588\uc2b5\ub2c8\ub2e4."));
            }

            if (statusTimer > 0f)
            {
                statusTimer -= Time.deltaTime;
                if (statusTimer <= 0f && statusText != null)
                {
                    statusText.text = string.Empty;
                }
            }

            RefreshUI();
        }

        public void RegisterTerminalHack(int index, string terminalName)
        {
            hackedTerminals++;
            Announce(Localize(
                $"\u5df2\u83b7\u53d6\u8bc4\u5ba1\u788e\u7247 {hackedTerminals}/{totalTerminals}\u3002",
                $"\ud3c9\uac00 \uc870\uac01 \ud655\ubcf4 {hackedTerminals}/{totalTerminals}."));
            RefreshUI();
        }

        public void HandleTeacherCatch(string teacherNameZh, string teacherNameKo)
        {
            if (IsGameOver)
            {
                return;
            }

            caughtCount++;
            Announce(Localize(
                $"{teacherNameZh}\u6293\u5230\u4e86\u4f60\uff0c\u5df2\u88ab\u6293 {caughtCount}/{maxCaughtCount}\u6b21\u3002",
                $"{teacherNameKo}\uc5d0\uac8c \uc7a1\ud614\uc2b5\ub2c8\ub2e4. \uc7a1\ud78c \ud69f\uc218 {caughtCount}/{maxCaughtCount}."));

            if (caughtCount >= maxCaughtCount)
            {
                Lose(Localize(
                    "\u9003\u79bb\u5931\u8d25",
                    "\ud0c8\ucd9c \uc2e4\ud328"));
                return;
            }

            player?.RespawnToSafePoint(Localize(
                "\u4f60\u88ab\u6559\u5e08\u903c\u9000\u5230\u5b89\u5168\u70b9\u3002",
                "\uad50\uc218\uc5d0\uac8c \uc7a1\ud600 \uc548\uc804 \uc9c0\uc810\uc73c\ub85c \ub3cc\uc544\uac14\uc2b5\ub2c8\ub2e4."));
            RefreshUI();
        }

        public void CompleteUpload()
        {
            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;
            if (objectiveText != null)
            {
                objectiveText.text = Localize(
                    "\u80dc\u5229\uff1a\u4f60\u5728\u53cc\u9ed1\u67f1\u4e4b\u95f4\u4e0a\u4f20\u4e86\u6700\u7ec8\u7b54\u8fa9\u7a0b\u5e8f\u3002",
                    "\uc2b9\ub9ac: \uac80\uc740 \uc30d\uae30\ub465\uc5d0\uc11c \ucd5c\uc885 \ub2f5\ubcc0 \ube4c\ub4dc\ub97c \uc5c5\ub85c\ub4dc\ud588\uc2b5\ub2c8\ub2e4.");
            }

            if (promptText != null)
            {
                promptText.text = string.Empty;
            }

            if (statusText != null)
            {
                statusText.text = Localize(
                    "\u4f60\u6574\u5408\u4e86\u4e94\u4f4d\u5bfc\u5e08\u7684\u8bc4\u5ba1\u7ed3\u679c\uff0c\u5b8c\u6210\u4e86\u6700\u7ec8\u5c55\u793a\u7248\u672c\u3002",
                    "\ub2e4\uc12f \uba85\uc758 \uba58\ud1a0 \ud3c9\uac00\ub97c \ud1b5\ud569\ud574 \ucd5c\uc885 \uc2dc\uc5f0 \ubc84\uc804\uc744 \uc644\uc131\ud588\uc2b5\ub2c8\ub2e4.");
            }

            player?.SetCursorState(true);
            AudioBank?.PlayUiConfirm();
        }

        public void SetPrompt(string text)
        {
            pendingPrompt = text;
            if (promptText != null)
            {
                promptText.text = text;
            }
        }

        public void SetUploadProgress(float normalized)
        {
            Announce(Localize(
                $"\u6b63\u5728\u4e0a\u4f20\u6700\u7ec8\u7b54\u8fa9\u7a0b\u5e8f... {(normalized * 100f):0}%",
                $"\ucd5c\uc885 \ube4c\ub4dc \uc5c5\ub85c\ub4dc \uc911... {(normalized * 100f):0}%"));
        }

        public void Announce(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }

            statusTimer = 2.2f;
        }

        public string GetControlsPrompt()
        {
            return Localize(
                "\u9f20\u6807\u5de6\u952e\u4ee3\u7801\u653b\u51fb  |  Shift \u75be\u8dd1  |  V \u5207\u6362\u89c6\u89d2  |  WASD \u79fb\u52a8  |  Space \u8df3\u8dc3",
                "\ub9c8\uc6b0\uc2a4 \uc88c\ud074\ub9ad \ucf54\ub4dc \uacf5\uaca9  |  Shift \uc9c8\uc8fc  |  V \uc2dc\uc810 \uc804\ud658  |  WASD \uc774\ub3d9  |  Space \uc810\ud504");
        }

        public void SetLanguage(LanguageOption language, bool playSound = true)
        {
            CurrentLanguage = language;
            if (playSound)
            {
                AudioBank?.PlayUiClick();
            }

            RefreshTutorialText();
            RefreshUI();

            if (!string.IsNullOrEmpty(pendingPrompt) && promptText != null)
            {
                promptText.text = pendingPrompt;
            }
        }

        public void StartGameplay()
        {
            GameplayActive = true;
            if (tutorialPanel != null)
            {
                tutorialPanel.transform.parent.gameObject.SetActive(false);
            }
            if (gameOverPanel != null)
            {
                gameOverPanel.transform.parent.gameObject.SetActive(false);
            }

            player?.SetCursorState(false);
            AudioBank?.PlayGameStart();
            Announce(Localize(
                "\u6e38\u620f\u5f00\u59cb\u3002\u5148\u53bb\u9ed1\u5165\u4e94\u4e2a\u8bc4\u5ba1\u7ec8\u7aef\u3002",
                "\uac8c\uc784 \uc2dc\uc791. \ub2e4\uc12f \uac1c\uc758 \ud3c9\uac00 \ud130\ubbf8\ub110\uc744 \ud574\ud0b9\ud558\uc138\uc694."));
        }

        public void BindUI(
            Text objective,
            Text timer,
            Text focus,
            Text prompt,
            Text status,
            Text title,
            Text tutorialBody,
            Text languageLabel,
            Text startLabel,
            Text chineseLabel,
            Text koreanLabel,
            Text floatingChineseLabel,
            Text floatingKoreanLabel,
            Text loseTitle,
            Text loseBody,
            Text restartLabel,
            Image tutorialImage,
            Image loseImage,
            Button start,
            Button chinese,
            Button korean,
            Button floatingChinese,
            Button floatingKorean,
            Button restart,
            PrototypeAudioBank bank,
            PlayerController boundPlayer)
        {
            objectiveText = objective;
            timerText = timer;
            focusText = focus;
            promptText = prompt;
            statusText = status;
            titleText = title;
            tutorialBodyText = tutorialBody;
            languageLabelText = languageLabel;
            startButtonText = startLabel;
            chineseButtonText = chineseLabel;
            koreanButtonText = koreanLabel;
            floatingChineseButtonText = floatingChineseLabel;
            floatingKoreanButtonText = floatingKoreanLabel;
            gameOverTitleText = loseTitle;
            gameOverBodyText = loseBody;
            restartButtonText = restartLabel;
            tutorialPanel = tutorialImage;
            gameOverPanel = loseImage;
            startButton = start;
            chineseButton = chinese;
            koreanButton = korean;
            floatingChineseButton = floatingChinese;
            floatingKoreanButton = floatingKorean;
            restartButton = restart;
            audioBank = bank;
            player = boundPlayer;
        }

        private void OpenTutorial()
        {
            GameplayActive = false;
            player?.SetCursorState(true);
            if (tutorialPanel != null)
            {
                tutorialPanel.transform.parent.gameObject.SetActive(true);
            }
        }

        private void HookButtons()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(StartGameplay);
            }

            if (chineseButton != null)
            {
                chineseButton.onClick.RemoveAllListeners();
                chineseButton.onClick.AddListener(() => SetLanguage(LanguageOption.Chinese));
            }

            if (koreanButton != null)
            {
                koreanButton.onClick.RemoveAllListeners();
                koreanButton.onClick.AddListener(() => SetLanguage(LanguageOption.Korean));
            }

            if (floatingChineseButton != null)
            {
                floatingChineseButton.onClick.RemoveAllListeners();
                floatingChineseButton.onClick.AddListener(() => SetLanguage(LanguageOption.Chinese));
            }

            if (floatingKoreanButton != null)
            {
                floatingKoreanButton.onClick.RemoveAllListeners();
                floatingKoreanButton.onClick.AddListener(() => SetLanguage(LanguageOption.Korean));
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(RestartScene);
            }
        }

        private void Lose(string reason)
        {
            IsGameOver = true;
            if (objectiveText != null)
            {
                objectiveText.text = Localize("\u5931\u8d25", "\ud328\ubc30");
            }

            if (promptText != null)
            {
                promptText.text = string.Empty;
            }

            if (statusText != null)
            {
                statusText.text = reason;
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.transform.parent.gameObject.SetActive(true);
            }

            if (gameOverTitleText != null)
            {
                gameOverTitleText.text = Localize("\u9003\u79bb\u5931\u8d25", "\ud0c8\ucd9c \uc2e4\ud328");
            }

            if (gameOverBodyText != null)
            {
                gameOverBodyText.text = reason;
            }

            if (restartButtonText != null)
            {
                restartButtonText.text = Localize("\u91cd\u65b0\u5f00\u59cb", "\ub2e4\uc2dc \uc2dc\uc791");
            }

            player?.SetCursorState(true);
            AudioBank?.PlayUiError();
        }

        private void RefreshUI()
        {
            if (objectiveText != null)
            {
                objectiveText.text = AllTerminalsHacked
                    ? Localize(
                        "\u76ee\u6807\uff1a\u56de\u5230\u53cc\u9ed1\u67f1\u4e4b\u95f4\uff0c\u4e0a\u4f20\u6700\u7ec8\u7b54\u8fa9\u7a0b\u5e8f\u3002",
                        "\ubaa9\ud45c: \uac80\uc740 \uc30d\uae30\ub465\uc73c\ub85c \ub3cc\uc544\uac00 \ucd5c\uc885 \ub2f5\ubcc0 \ube4c\ub4dc\ub97c \uc5c5\ub85c\ub4dc\ud558\uc138\uc694.")
                    : Localize(
                        $"\u76ee\u6807\uff1a\u7528\u7b14\u8bb0\u672c\u4ee3\u7801\u653b\u51fb\u9ed1\u5165 5 \u4e2a\u5bfc\u5e08\u8bc4\u5ba1\u7ec8\u7aef\u3002({hackedTerminals}/{totalTerminals})",
                        $"\ubaa9\ud45c: \ub178\ud2b8\ubd81 \ucf54\ub4dc \uacf5\uaca9\uc73c\ub85c 5\uac1c\uc758 \uba58\ud1a0 \ud3c9\uac00 \ud130\ubbf8\ub110\uc744 \ud574\ud0b9\ud558\uc138\uc694. ({hackedTerminals}/{totalTerminals})");
            }

            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                timerText.text = Localize(
                    $"\u622a\u6b62  {minutes:00}:{seconds:00}",
                    $"\ub9c8\uac10  {minutes:00}:{seconds:00}");
            }

            if (focusText != null)
            {
                focusText.text = Localize(
                    $"\u88ab\u6293  {caughtCount}/{maxCaughtCount}\n\u4f53\u529b  {(player != null ? player.StaminaNormalized * 100f : 0f):0}%",
                    $"\uc7a1\ud798  {caughtCount}/{maxCaughtCount}\n\uc2a4\ud0dc\ubbf8\ub098  {(player != null ? player.StaminaNormalized * 100f : 0f):0}%");
            }

            if (floatingChineseButtonText != null)
            {
                floatingChineseButtonText.text = "\u4e2d\u6587";
            }

            if (floatingKoreanButtonText != null)
            {
                floatingKoreanButtonText.text = "\ud55c\uad6d\uc5b4";
            }
        }

        private void RefreshTutorialText()
        {
            if (titleText != null)
            {
                titleText.text = "The Everyday Odyssey";
            }

            if (languageLabelText != null)
            {
                languageLabelText.text = Localize("\u9009\u62e9\u8bed\u8a00", "\uc5b8\uc5b4 \uc120\ud0dd");
            }

            if (startButtonText != null)
            {
                startButtonText.text = Localize("\u5f00\u59cb\u6e38\u620f", "\uc2dc\uc791");
            }

            if (chineseButtonText != null)
            {
                chineseButtonText.text = "\u4e2d\u6587";
            }

            if (koreanButtonText != null)
            {
                koreanButtonText.text = "\ud55c\uad6d\uc5b4";
            }

            if (tutorialBodyText != null)
            {
                tutorialBodyText.text = CurrentLanguage == LanguageOption.Korean
                    ? "\ud50c\ub808\uc774 \ubc29\ubc95\n1. WASD\ub85c \uc774\ub3d9\ud558\uace0 Space\ub85c \uc810\ud504\ud569\ub2c8\ub2e4.\n2. Shift\ub97c \ub204\ub974\uba74 \uc9c8\uc8fc\ud558\uc9c0\ub9cc \uc2a4\ud0dc\ubbf8\ub098\uac00 \uc18c\ubaa8\ub429\ub2c8\ub2e4.\n3. V\ub85c 1\uc778\uce6d/\uc81c3\uc778\uce6d \uc2dc\uc810\uc744 \uc804\ud658\ud569\ub2c8\ub2e4.\n4. \ub9c8\uc6b0\uc2a4 \uc88c\ud074\ub9ad\uc73c\ub85c \ub178\ud2b8\ubd81\uc5d0\uc11c \ucf54\ub4dc \uacf5\uaca9\uc744 \ubc1c\uc0ac\ud574 \uad50\uc218\ub4e4\uc744 \uc77c\uc2dc \uc815\uc9c0\uc2dc\ud0b5\ub2c8\ub2e4.\n5. E\ub85c \ud130\ubbf8\ub110 \uc704\uc758 \ud30c\ud3b8\uc744 \ud68d\ub4dd\ud558\uc138\uc694. \ud68d\ub4dd\ud55c \ud30c\ud3b8\uc740 \uc0ac\ub77c\uc9d1\ub2c8\ub2e4.\n6. \ub2e4\uc12f \uac1c\uc758 \ud30c\ud3b8\uc744 \ubaa8\uc73c\uba74 \uac80\uc740 \uc30d\uae30\ub465 \uc0ac\uc774\ub85c \ub3cc\uc544\uac00 \ucd5c\uc885 \ub2f5\ubcc0 \ube4c\ub4dc\ub97c \uc5c5\ub85c\ub4dc\ud558\uc138\uc694.\n\n\ud328\ubc30 \uc870\uac74\n- \uad50\uc218\uc5d0\uac8c 3\ubc88 \uc7a1\ud788\uba74 \uc2e4\ud328\ud569\ub2c8\ub2e4.\n- \uc2dc\uac04\uc774 \ubaa8\ub450 \uc9c0\ub098\uba74 \uc2e4\ud328\ud569\ub2c8\ub2e4."
                    : "\u73a9\u6cd5\u8bf4\u660e\n1. \u4f7f\u7528 WASD \u79fb\u52a8\uff0cSpace \u8df3\u8dc3\u3002\n2. \u6309住 Shift \u53ef\u75be\u8dd1\uff0c\u4f46\u4f1a\u6d88\u8017\u4f53\u529b\u3002\n3. \u6309 V \u5728\u7b2c\u4e00\u4eba\u79f0\u4e0e\u7b2c\u4e09\u4eba\u79f0\u4e4b\u95f4\u5207\u6362\u3002\n4. \u9f20\u6807\u5de6\u952e\u8ba9\u7b14\u8bb0\u672c\u53d1\u5c04\u4ee3\u7801\u653b\u51fb\uff0c\u53ef\u4f7f\u8001\u5e08\u77ed\u6682\u6682\u505c\u3002\n5. \u9760\u8fd1\u7ec8\u7aef\u540e\u6309 E \u83b7\u53d6\u7ec8\u7aef\u4e0a\u65b9\u7684\u788e\u7247\uff0c\u83b7\u53d6\u540e\u539f\u5730\u788e\u7247\u4f1a\u6d88\u5931\u3002\n6. \u96c6\u9f50 5 \u4e2a\u788e\u7247\u540e\uff0c\u56de\u5230\u53cc\u9ed1\u67f1\u4e4b\u95f4\u4e0a\u4f20\u6700\u7ec8\u7b54\u8fa9\u7a0b\u5e8f\u5373\u53ef\u80dc\u5229\u3002\n\n\u5931\u8d25\u6761\u4ef6\n- \u88ab\u6559\u5e08\u6293\u5230 3 \u6b21\u4f1a\u5931\u8d25\u3002\n- \u5012\u8ba1\u65f6\u7ed3\u675f\u4f1a\u5931\u8d25\u3002";
            }
        }

        private string Localize(string zh, string ko)
        {
            return CurrentLanguage == LanguageOption.Korean ? ko : zh;
        }

        private void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
