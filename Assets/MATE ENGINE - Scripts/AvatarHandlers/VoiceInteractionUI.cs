using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MateEngine.Voice
{
    public class VoiceInteractionUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject voicePanel;
        public Button voiceToggleButton;
        public TextMeshProUGUI statusText;
        public Image microphoneIcon;
        public Image aiThinkingIcon;
        public Slider volumeSlider;
        public Toggle holdToTalkToggle;
        public TMP_InputField apiKeyInput;
        public TMP_Dropdown modelDropdown;
        public Button testVoiceButton;
        
        [Header("Visual Feedback")]
        public Color listeningColor = Color.green;
        public Color processingColor = Color.yellow;
        public Color idleColor = Color.white;
        public float pulseSpeed = 2f;
        
        [Header("Audio Visualization")]
        public RectTransform audioWaveform;
        public GameObject waveformBarPrefab;
        public int waveformBars = 32;
        
        private VoiceInteractionManager voiceManager;
        private bool isVoiceEnabled = false;
        private RectTransform[] waveformBars_;
        private float basePulseScale = 1f;
        
        void Start()
        {
            InitializeUI();
            SetupEventListeners();
            CreateWaveformVisualization();
        }
        
        void Update()
        {
            UpdateVisualFeedback();
        }
        
        private void InitializeUI()
        {
            voiceManager = FindObjectOfType<VoiceInteractionManager>();
            if (voiceManager == null)
            {
                Debug.LogError("[VoiceUI] VoiceInteractionManager not found!");
                return;
            }
            
            // Initialize UI state
            if (voicePanel) voicePanel.SetActive(false);
            if (microphoneIcon) microphoneIcon.color = idleColor;
            if (aiThinkingIcon) aiThinkingIcon.gameObject.SetActive(false);
            if (statusText) statusText.text = "Voice interaction disabled";
            
            // Load saved settings
            LoadVoiceSettings();
        }
        
        private void SetupEventListeners()
        {
            if (voiceToggleButton)
            {
                voiceToggleButton.onClick.AddListener(ToggleVoiceInteraction);
            }
            
            if (volumeSlider)
            {
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
            
            if (holdToTalkToggle)
            {
                holdToTalkToggle.onValueChanged.AddListener(OnHoldToTalkChanged);
            }
            
            if (apiKeyInput)
            {
                apiKeyInput.onEndEdit.AddListener(OnApiKeyChanged);
            }
            
            if (modelDropdown)
            {
                SetupModelDropdown();
                modelDropdown.onValueChanged.AddListener(OnModelChanged);
            }
            
            if (testVoiceButton)
            {
                testVoiceButton.onClick.AddListener(TestVoiceInteraction);
            }
        }
        
        private void CreateWaveformVisualization()
        {
            if (audioWaveform == null || waveformBarPrefab == null) return;
            
            waveformBars_ = new RectTransform[waveformBars];
            
            for (int i = 0; i < waveformBars; i++)
            {
                GameObject bar = Instantiate(waveformBarPrefab, audioWaveform);
                waveformBars_[i] = bar.GetComponent<RectTransform>();
                
                // Position bars evenly
                float x = (float)i / (waveformBars - 1) * audioWaveform.rect.width - audioWaveform.rect.width / 2;
                waveformBars_[i].anchoredPosition = new Vector2(x, 0);
            }
        }
        
        private void UpdateVisualFeedback()
        {
            if (voiceManager == null) return;
            
            // Update microphone icon color and pulse
            if (microphoneIcon)
            {
                Color targetColor = idleColor;
                float pulseScale = 1f;
                
                if (voiceManager.IsRecording)
                {
                    targetColor = listeningColor;
                    pulseScale = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.2f;
                }
                else if (voiceManager.IsProcessing)
                {
                    targetColor = processingColor;
                    pulseScale = 1f + Mathf.Sin(Time.time * pulseSpeed * 2f) * 0.1f;
                }
                
                microphoneIcon.color = Color.Lerp(microphoneIcon.color, targetColor, Time.deltaTime * 5f);
                microphoneIcon.transform.localScale = Vector3.Lerp(microphoneIcon.transform.localScale, Vector3.one * pulseScale, Time.deltaTime * 10f);
            }
            
            // Update AI thinking icon
            if (aiThinkingIcon)
            {
                bool shouldShow = voiceManager.IsProcessing;
                aiThinkingIcon.gameObject.SetActive(shouldShow);
                
                if (shouldShow)
                {
                    aiThinkingIcon.transform.Rotate(0, 0, Time.deltaTime * 90f);
                }
            }
            
            // Update waveform visualization
            UpdateWaveform();
        }
        
        private void UpdateWaveform()
        {
            if (waveformBars_ == null || !voiceManager.IsRecording) return;
            
            // Get audio data from microphone
            AudioSource micSource = voiceManager.GetComponent<AudioSource>();
            if (micSource && micSource.clip)
            {
                float[] spectrum = new float[64];
                AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
                
                for (int i = 0; i < waveformBars.Length && i < spectrum.Length; i++)
                {
                    float height = spectrum[i] * 1000f; // Scale for visibility
                    Vector2 sizeDelta = waveformBars_[i].sizeDelta;
                    sizeDelta.y = Mathf.Lerp(sizeDelta.y, height, Time.deltaTime * 10f);
                    waveformBars_[i].sizeDelta = sizeDelta;
                }
            }
        }
        
        private void SetupModelDropdown()
        {
            if (modelDropdown == null) return;
            
            modelDropdown.ClearOptions();
            
            // Add common OpenRouter models
            var models = new[]
            {
                "anthropic/claude-3.5-sonnet",
                "openai/gpt-4o",
                "openai/gpt-4o-mini",
                "anthropic/claude-3-haiku",
                "meta-llama/llama-3.1-8b-instruct",
                "google/gemini-pro",
                "mistralai/mistral-7b-instruct"
            };
            
            foreach (string model in models)
            {
                modelDropdown.options.Add(new TMP_Dropdown.OptionData(model));
            }
            
            modelDropdown.RefreshShownValue();
        }
        
        public void ToggleVoiceInteraction()
        {
            isVoiceEnabled = !isVoiceEnabled;
            
            if (voiceManager)
            {
                voiceManager.ToggleVoiceInteraction(isVoiceEnabled);
            }
            
            UpdateUIState();
            SaveVoiceSettings();
        }
        
        private void UpdateUIState()
        {
            if (statusText)
            {
                statusText.text = isVoiceEnabled ? "Voice interaction enabled" : "Voice interaction disabled";
            }
            
            if (voiceToggleButton)
            {
                var buttonText = voiceToggleButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText)
                {
                    buttonText.text = isVoiceEnabled ? "Disable Voice" : "Enable Voice";
                }
            }
        }
        
        private void OnVolumeChanged(float value)
        {
            if (voiceManager)
            {
                voiceManager.voiceSettings.responseVolume = value;
            }
            SaveVoiceSettings();
        }
        
        private void OnHoldToTalkChanged(bool value)
        {
            if (voiceManager)
            {
                voiceManager.voiceSettings.holdToTalk = value;
            }
            SaveVoiceSettings();
        }
        
        private void OnApiKeyChanged(string value)
        {
            if (voiceManager)
            {
                voiceManager.SetApiKey(value);
            }
            SaveVoiceSettings();
        }
        
        private void OnModelChanged(int index)
        {
            if (voiceManager && modelDropdown && index < modelDropdown.options.Count)
            {
                string selectedModel = modelDropdown.options[index].text;
                voiceManager.SetModel(selectedModel);
            }
            SaveVoiceSettings();
        }
        
        private void TestVoiceInteraction()
        {
            if (voiceManager && !string.IsNullOrEmpty(voiceManager.voiceSettings.apiKey))
            {
                StartCoroutine(voiceManager.SendTestMessage("Hello, this is a test of the voice interaction system."));
            }
            else
            {
                if (statusText) statusText.text = "Please configure API key first";
            }
        }
        
        public void ShowVoicePanel()
        {
            if (voicePanel) voicePanel.SetActive(true);
        }
        
        public void HideVoicePanel()
        {
            if (voicePanel) voicePanel.SetActive(false);
        }
        
        private void LoadVoiceSettings()
        {
            if (voiceManager == null) return;
            
            // Load from PlayerPrefs
            string apiKey = PlayerPrefs.GetString("Voice_ApiKey", "");
            float volume = PlayerPrefs.GetFloat("Voice_Volume", 1f);
            bool holdToTalk = PlayerPrefs.GetInt("Voice_HoldToTalk", 1) == 1;
            string model = PlayerPrefs.GetString("Voice_Model", "anthropic/claude-3.5-sonnet");
            isVoiceEnabled = PlayerPrefs.GetInt("Voice_Enabled", 0) == 1;
            
            // Apply settings
            voiceManager.voiceSettings.apiKey = apiKey;
            voiceManager.voiceSettings.responseVolume = volume;
            voiceManager.voiceSettings.holdToTalk = holdToTalk;
            voiceManager.voiceSettings.chatModel = model;
            voiceManager.ToggleVoiceInteraction(isVoiceEnabled);
            
            // Update UI
            if (apiKeyInput) apiKeyInput.text = apiKey;
            if (volumeSlider) volumeSlider.value = volume;
            if (holdToTalkToggle) holdToTalkToggle.isOn = holdToTalk;
            
            if (modelDropdown)
            {
                for (int i = 0; i < modelDropdown.options.Count; i++)
                {
                    if (modelDropdown.options[i].text == model)
                    {
                        modelDropdown.value = i;
                        break;
                    }
                }
            }
            
            UpdateUIState();
        }
        
        private void SaveVoiceSettings()
        {
            if (voiceManager == null) return;
            
            PlayerPrefs.SetString("Voice_ApiKey", voiceManager.voiceSettings.apiKey);
            PlayerPrefs.SetFloat("Voice_Volume", voiceManager.voiceSettings.responseVolume);
            PlayerPrefs.SetInt("Voice_HoldToTalk", voiceManager.voiceSettings.holdToTalk ? 1 : 0);
            PlayerPrefs.SetString("Voice_Model", voiceManager.voiceSettings.chatModel);
            PlayerPrefs.SetInt("Voice_Enabled", isVoiceEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        public void OnMicrophonePermissionResult(bool granted)
        {
            if (!granted)
            {
                if (statusText) statusText.text = "Microphone permission required";
                isVoiceEnabled = false;
                UpdateUIState();
            }
        }
        
        // Public methods for external integration
        public void SetVoiceStatus(string status)
        {
            if (statusText) statusText.text = status;
        }
        
        public void ShowProcessingIndicator(bool show)
        {
            if (aiThinkingIcon) aiThinkingIcon.gameObject.SetActive(show);
        }
        
        public void SetMicrophoneActive(bool active)
        {
            if (microphoneIcon)
            {
                microphoneIcon.color = active ? listeningColor : idleColor;
            }
        }
    }
}