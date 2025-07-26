using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MateEngine.Voice
{
    /// <summary>
    /// Integrates voice interaction controls into the existing Mate Engine menu system
    /// </summary>
    public class VoiceMenuIntegration : MonoBehaviour
    {
        [Header("Menu Integration")]
        public GameObject voiceMenuPanel;
        public Button voiceSettingsButton;
        public TextMeshProUGUI voiceStatusText;
        public GameObject voiceIndicatorIcon;
        
        [Header("Quick Controls")]
        public Toggle quickVoiceToggle;
        public Button pushToTalkButton;
        public Slider quickVolumeSlider;
        
        [Header("Settings Panel")]
        public TMP_InputField apiKeyField;
        public TMP_Dropdown modelDropdown;
        public TMP_InputField systemPromptField;
        public Toggle holdToTalkToggle;
        public Slider temperatureSlider;
        public Slider maxTokensSlider;
        public TextMeshProUGUI temperatureLabel;
        public TextMeshProUGUI maxTokensLabel;
        
        private VoiceInteractionManager voiceManager;
        private VoiceInteractionUI voiceUI;
        private bool isVoiceMenuOpen = false;
        
        void Start()
        {
            Initialize();
            SetupEventListeners();
            LoadSettings();
        }
        
        void Update()
        {
            UpdateVoiceStatus();
        }
        
        private void Initialize()
        {
            // Find voice system components
            voiceManager = FindFirstObjectByType<VoiceInteractionManager>();
            voiceUI = FindFirstObjectByType<VoiceInteractionUI>();
            
            if (voiceManager == null)
            {
                Debug.LogWarning("[VoiceMenuIntegration] VoiceInteractionManager not found. Creating default instance.");
                CreateVoiceManagerInstance();
            }
            
            // Initialize menu state
            if (voiceMenuPanel) voiceMenuPanel.SetActive(false);
            if (voiceIndicatorIcon) voiceIndicatorIcon.SetActive(false);
            
            // Setup dropdown options
            SetupModelDropdown();
        }
        
        private void CreateVoiceManagerInstance()
        {
            // Create a basic voice manager if none exists
            GameObject voiceObj = new GameObject("VoiceInteractionManager");
            voiceManager = voiceObj.AddComponent<VoiceInteractionManager>();
            
            // Add audio source for responses
            AudioSource audioSource = voiceObj.AddComponent<AudioSource>();
            voiceManager.responseAudioSource = audioSource;
            
            // Try to find avatar animator
            Animator avatarAnimator = FindFirstObjectByType<Animator>();
            if (avatarAnimator) voiceManager.avatarAnimator = avatarAnimator;
            
            Debug.Log("[VoiceMenuIntegration] Created default VoiceInteractionManager instance");
        }
        
        private void SetupEventListeners()
        {
            // Menu controls
            if (voiceSettingsButton)
            {
                voiceSettingsButton.onClick.AddListener(ToggleVoiceMenu);
            }
            
            if (quickVoiceToggle)
            {
                quickVoiceToggle.onValueChanged.AddListener(OnQuickVoiceToggle);
            }
            
            if (pushToTalkButton)
            {
                // Add event trigger for push-to-talk
                var eventTrigger = pushToTalkButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (eventTrigger == null)
                {
                    eventTrigger = pushToTalkButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                }
                
                var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
                pointerDown.callback.AddListener((data) => OnPushToTalkStart());
                eventTrigger.triggers.Add(pointerDown);
                
                var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
                pointerUp.callback.AddListener((data) => OnPushToTalkEnd());
                eventTrigger.triggers.Add(pointerUp);
            }
            
            if (quickVolumeSlider)
            {
                quickVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
            
            // Settings panel controls
            if (apiKeyField)
            {
                apiKeyField.onEndEdit.AddListener(OnApiKeyChanged);
            }
            
            if (modelDropdown)
            {
                modelDropdown.onValueChanged.AddListener(OnModelChanged);
            }
            
            if (systemPromptField)
            {
                systemPromptField.onEndEdit.AddListener(OnSystemPromptChanged);
            }
            
            if (holdToTalkToggle)
            {
                holdToTalkToggle.onValueChanged.AddListener(OnHoldToTalkChanged);
            }
            
            if (temperatureSlider)
            {
                temperatureSlider.onValueChanged.AddListener(OnTemperatureChanged);
            }
            
            if (maxTokensSlider)
            {
                maxTokensSlider.onValueChanged.AddListener(OnMaxTokensChanged);
            }
        }
        
        private void SetupModelDropdown()
        {
            if (modelDropdown == null) return;
            
            modelDropdown.ClearOptions();
            
            var models = new[]
            {
                "anthropic/claude-3.5-sonnet",
                "openai/gpt-4o",
                "openai/gpt-4o-mini", 
                "anthropic/claude-3-haiku",
                "meta-llama/llama-3.1-8b-instruct",
                "google/gemini-pro",
                "mistralai/mistral-7b-instruct",
                "openai/gpt-3.5-turbo"
            };
            
            foreach (string model in models)
            {
                modelDropdown.options.Add(new TMP_Dropdown.OptionData(model));
            }
            
            modelDropdown.RefreshShownValue();
        }
        
        public void ToggleVoiceMenu()
        {
            isVoiceMenuOpen = !isVoiceMenuOpen;
            
            if (voiceMenuPanel)
            {
                voiceMenuPanel.SetActive(isVoiceMenuOpen);
            }
            
            if (isVoiceMenuOpen)
            {
                RefreshSettingsUI();
            }
        }
        
        private void RefreshSettingsUI()
        {
            if (voiceManager == null) return;
            
            var settings = voiceManager.voiceSettings;
            
            // Update UI elements with current settings
            if (apiKeyField) 
            {
                string maskedKey = string.IsNullOrEmpty(settings.apiKey) ? "" : new string('*', settings.apiKey.Length);
                apiKeyField.text = maskedKey;
            }
            
            if (systemPromptField) 
                systemPromptField.text = settings.systemPrompt;
            
            if (holdToTalkToggle) 
                holdToTalkToggle.isOn = settings.holdToTalk;
            
            if (temperatureSlider) 
            {
                temperatureSlider.value = settings.temperature;
                if (temperatureLabel) temperatureLabel.text = $"Temperature: {settings.temperature:F1}";
            }
            
            if (maxTokensSlider) 
            {
                maxTokensSlider.value = settings.maxTokens;
                if (maxTokensLabel) maxTokensLabel.text = $"Max Tokens: {(int)settings.maxTokens}";
            }
            
            if (quickVolumeSlider) 
                quickVolumeSlider.value = settings.responseVolume;
            
            // Set model dropdown
            if (modelDropdown)
            {
                for (int i = 0; i < modelDropdown.options.Count; i++)
                {
                    if (modelDropdown.options[i].text == settings.chatModel)
                    {
                        modelDropdown.value = i;
                        break;
                    }
                }
            }
        }
        
        private void UpdateVoiceStatus()
        {
            if (voiceManager == null) return;
            
            // Update status text
            if (voiceStatusText)
            {
                string status = "Voice: Disabled";
                
                if (voiceManager.enabled)
                {
                    if (voiceManager.IsRecording)
                        status = "Voice: Listening...";
                    else if (voiceManager.IsProcessing)
                        status = "Voice: Processing...";
                    else
                        status = $"Voice: Ready (Press {voiceManager.voiceSettings.activationKey})";
                }
                
                voiceStatusText.text = status;
            }
            
            // Update indicator icon
            if (voiceIndicatorIcon)
            {
                bool showIndicator = voiceManager.enabled && (voiceManager.IsRecording || voiceManager.IsProcessing);
                voiceIndicatorIcon.SetActive(showIndicator);
            }
            
            // Update quick toggle
            if (quickVoiceToggle && quickVoiceToggle.isOn != voiceManager.enabled)
            {
                quickVoiceToggle.SetIsOnWithoutNotify(voiceManager.enabled);
            }
        }
        
        // Event handlers
        private void OnQuickVoiceToggle(bool enabled)
        {
            if (voiceManager)
            {
                voiceManager.ToggleVoiceInteraction(enabled);
                SaveSettings();
            }
        }
        
        private void OnPushToTalkStart()
        {
            if (voiceManager && voiceManager.enabled)
            {
                voiceManager.StartRecordingManual();
            }
        }
        
        private void OnPushToTalkEnd()
        {
            if (voiceManager && voiceManager.enabled)
            {
                voiceManager.StopRecordingManual();
            }
        }
        
        private void OnVolumeChanged(float value)
        {
            if (voiceManager)
            {
                voiceManager.voiceSettings.responseVolume = value;
                SaveSettings();
            }
        }
        
        private void OnApiKeyChanged(string value)
        {
            if (voiceManager && !string.IsNullOrEmpty(value) && value != new string('*', value.Length))
            {
                voiceManager.SetApiKey(value);
                SaveSettings();
            }
        }
        
        private void OnModelChanged(int index)
        {
            if (voiceManager && modelDropdown && index < modelDropdown.options.Count)
            {
                string selectedModel = modelDropdown.options[index].text;
                voiceManager.SetModel(selectedModel);
                SaveSettings();
            }
        }
        
        private void OnSystemPromptChanged(string value)
        {
            if (voiceManager)
            {
                voiceManager.voiceSettings.systemPrompt = value;
                SaveSettings();
            }
        }
        
        private void OnHoldToTalkChanged(bool value)
        {
            if (voiceManager)
            {
                voiceManager.voiceSettings.holdToTalk = value;
                SaveSettings();
            }
        }
        
        private void OnTemperatureChanged(float value)
        {
            if (voiceManager)
            {
                voiceManager.voiceSettings.temperature = value;
                if (temperatureLabel) temperatureLabel.text = $"Temperature: {value:F1}";
                SaveSettings();
            }
        }
        
        private void OnMaxTokensChanged(float value)
        {
            if (voiceManager)
            {
                voiceManager.voiceSettings.maxTokens = value;
                if (maxTokensLabel) maxTokensLabel.text = $"Max Tokens: {(int)value}";
                SaveSettings();
            }
        }
        
        private void LoadSettings()
        {
            // Settings are loaded by the VoiceInteractionManager
            // This just ensures UI is synchronized
            if (isVoiceMenuOpen)
            {
                RefreshSettingsUI();
            }
        }
        
        private void SaveSettings()
        {
            // Settings are automatically saved by individual components
            // Additional persistence logic can be added here if needed
        }
        
        // Public methods for external integration
        public void ShowVoiceMenu()
        {
            isVoiceMenuOpen = true;
            if (voiceMenuPanel) voiceMenuPanel.SetActive(true);
            RefreshSettingsUI();
        }
        
        public void HideVoiceMenu()
        {
            isVoiceMenuOpen = false;
            if (voiceMenuPanel) voiceMenuPanel.SetActive(false);
        }
        
        public void TestVoiceSystem()
        {
            if (voiceManager)
            {
                StartCoroutine(voiceManager.SendTestMessage("Voice system test successful!"));
            }
        }
        
        public bool IsVoiceEnabled => voiceManager != null && voiceManager.enabled;
        public bool IsVoiceMenuOpen => isVoiceMenuOpen;
    }
}