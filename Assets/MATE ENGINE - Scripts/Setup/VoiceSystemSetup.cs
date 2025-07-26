using UnityEngine;
using MateEngine.Voice.Tests;
using UnityEngine.UI;

namespace MateEngine.Voice
{
    /// <summary>
    /// Runtime setup and integration helper for voice interaction system
    /// This script can be used to automatically set up the voice system at runtime
    /// </summary>
    public class VoiceSystemSetup : MonoBehaviour
    {
        [Header("Auto Setup Configuration")]
        public bool autoSetupOnStart = true;
        public bool createUIIfMissing = true;
        public bool integrateWithExistingMenu = true;
        
        [Header("Setup Preferences")]
        public KeyCode voiceActivationKey = KeyCode.V;
        public string defaultModel = "anthropic/claude-3.5-sonnet";
        public string defaultSystemPrompt = "You are a friendly AI companion living on the user's desktop. Keep responses brief and conversational, around 1-2 sentences. Show personality and be helpful.";
        
        [Header("References (Optional - will be found automatically)")]
        public GameObject avatarObject;
        public Canvas mainCanvas;
        public GameObject menuParent;
        
        void Start()
        {
            if (autoSetupOnStart)
            {
                SetupVoiceSystem();
            }
        }
        
        [ContextMenu("Setup Voice System")]
        public void SetupVoiceSystem()
        {
            Debug.Log("[VoiceSetup] Setting up voice interaction system...");
            
            // Step 1: Setup core voice manager
            VoiceInteractionManager voiceManager = SetupVoiceManager();
            
            // Step 2: Setup local voice processing
            SetupLocalVoiceProcessing(voiceManager);
            
            // Step 3: Setup UI components
            if (createUIIfMissing)
            {
                SetupVoiceUI(voiceManager);
            }
            
            // Step 4: Integrate with existing menu
            if (integrateWithExistingMenu)
            {
                SetupMenuIntegration(voiceManager);
            }
            
            // Step 5: Configure default settings
            ConfigureDefaultSettings(voiceManager);
            
            Debug.Log("[VoiceSetup] Voice interaction system setup complete!");
        }
        
        private void SetupLocalVoiceProcessing(VoiceInteractionManager voiceManager)
        {
            Debug.Log("[VoiceSetup] Setting up local voice processing...");
            
            // Create VoiceServerManager if it doesn't exist
            VoiceServerManager serverManager = FindFirstObjectByType<VoiceServerManager>();
            if (serverManager == null)
            {
                GameObject serverObj = new GameObject("VoiceServerManager");
                serverManager = serverObj.AddComponent<VoiceServerManager>();
                Debug.Log("[VoiceSetup] Created VoiceServerManager");
            }
            
            // Create LocalVoiceProcessor if it doesn't exist
            LocalVoiceProcessor processor = FindFirstObjectByType<LocalVoiceProcessor>();
            if (processor == null)
            {
                GameObject processorObj = new GameObject("LocalVoiceProcessor");
                processor = processorObj.AddComponent<LocalVoiceProcessor>();
                processor.serverManager = serverManager;
                Debug.Log("[VoiceSetup] Created LocalVoiceProcessor");
            }
            
            // Configure voice manager to use local processing
            voiceManager.useLocalVoiceProcessing = true;
            voiceManager.voiceServerManager = serverManager;
            voiceManager.localVoiceProcessor = processor;
            
            Debug.Log("[VoiceSetup] Local voice processing configured");
        }
        
        private VoiceInteractionManager SetupVoiceManager()
        {
            // Check if voice manager already exists
            VoiceInteractionManager existing = FindFirstObjectByType<VoiceInteractionManager>();
            if (existing != null)
            {
                Debug.Log("[VoiceSetup] VoiceInteractionManager already exists");
                return existing;
            }
            
            // Find or create avatar object
            GameObject target = avatarObject;
            if (target == null)
            {
                // Try to find avatar by common names/components
                target = GameObject.FindWithTag("Avatar");
                if (target == null)
                {
                    Animator avatarAnimator = FindFirstObjectByType<Animator>();
                    if (avatarAnimator != null)
                    {
                        target = avatarAnimator.gameObject;
                    }
                }
                if (target == null)
                {
                    // Create a dedicated voice manager object
                    target = new GameObject("VoiceInteractionManager");
                }
            }
            
            // Add voice manager component
            VoiceInteractionManager voiceManager = target.GetComponent<VoiceInteractionManager>();
            if (voiceManager == null)
            {
                voiceManager = target.AddComponent<VoiceInteractionManager>();
            }
            
            // Setup audio source for responses
            AudioSource audioSource = target.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = target.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = 0.8f;
            }
            voiceManager.responseAudioSource = audioSource;
            
            // Try to find animator for avatar integration
            Animator animator = target.GetComponent<Animator>();
            if (animator == null)
            {
                animator = FindFirstObjectByType<Animator>();
            }
            voiceManager.avatarAnimator = animator;
            
            // Try to find blendshape controller
            UniversalBlendshapes blendshapes = FindFirstObjectByType<UniversalBlendshapes>();
            voiceManager.blendshapeController = blendshapes;
            
            Debug.Log("[VoiceSetup] VoiceInteractionManager created and configured");
            return voiceManager;
        }
        
        private void SetupVoiceUI(VoiceInteractionManager voiceManager)
        {
            // Check if UI already exists
            VoiceInteractionUI existing = FindFirstObjectByType<VoiceInteractionUI>();
            if (existing != null)
            {
                Debug.Log("[VoiceSetup] VoiceInteractionUI already exists");
                return;
            }
            
            // Find or create canvas
            Canvas canvas = mainCanvas;
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("VoiceUI Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create voice UI object
            GameObject voiceUIObj = new GameObject("VoiceInteractionUI");
            voiceUIObj.transform.SetParent(canvas.transform, false);
            
            VoiceInteractionUI voiceUI = voiceUIObj.AddComponent<VoiceInteractionUI>();
            
            // Create basic UI elements
            CreateBasicVoiceUI(voiceUIObj, voiceUI);
            
            Debug.Log("[VoiceSetup] VoiceInteractionUI created");
        }
        
        private void CreateBasicVoiceUI(GameObject parent, VoiceInteractionUI voiceUI)
        {
            // Create status text
            GameObject statusTextObj = new GameObject("VoiceStatusText");
            statusTextObj.transform.SetParent(parent.transform, false);
            
            RectTransform statusRect = statusTextObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 0.9f);
            statusRect.anchorMax = new Vector2(0.5f, 0.9f);
            statusRect.anchoredPosition = Vector2.zero;
            statusRect.sizeDelta = new Vector2(400, 50);
            
            TMPro.TextMeshProUGUI statusText = statusTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            statusText.text = "Voice interaction disabled";
            statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            statusText.fontSize = 16;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.color = Color.white;
            
            voiceUI.statusText = statusText;
            
            // Create microphone icon (simple colored image)
            GameObject micIconObj = new GameObject("MicrophoneIcon");
            micIconObj.transform.SetParent(parent.transform, false);
            
            RectTransform micRect = micIconObj.AddComponent<RectTransform>();
            micRect.anchorMin = new Vector2(0.05f, 0.9f);
            micRect.anchorMax = new Vector2(0.05f, 0.9f);
            micRect.anchoredPosition = Vector2.zero;
            micRect.sizeDelta = new Vector2(30, 30);
            
            Image micImage = micIconObj.AddComponent<Image>();
            micImage.color = Color.gray;
            
            voiceUI.microphoneIcon = micImage;
            
            Debug.Log("[VoiceSetup] Basic UI elements created");
        }
        
        private void SetupMenuIntegration(VoiceInteractionManager voiceManager)
        {
            // Check if menu integration already exists
            VoiceMenuIntegration existing = FindFirstObjectByType<VoiceMenuIntegration>();
            if (existing != null)
            {
                Debug.Log("[VoiceSetup] VoiceMenuIntegration already exists");
                return;
            }
            
            // Find existing menu system
            GameObject menu = menuParent;
            if (menu == null)
            {
                // Try to find menu by common names
                menu = GameObject.Find("Menu");
                if (menu == null) menu = GameObject.Find("Settings");
                if (menu == null) menu = GameObject.Find("UI");
            }
            
            if (menu == null)
            {
                Debug.LogWarning("[VoiceSetup] Could not find existing menu system for integration");
                return;
            }
            
            // Add menu integration component
            VoiceMenuIntegration menuIntegration = menu.AddComponent<VoiceMenuIntegration>();
            
            // Try to find existing UI elements to integrate with
            Button[] buttons = menu.GetComponentsInChildren<Button>();
            Toggle[] toggles = menu.GetComponentsInChildren<Toggle>();
            
            // Look for suitable integration points
            foreach (Button button in buttons)
            {
                if (button.name.ToLower().Contains("setting") || button.name.ToLower().Contains("option"))
                {
                    menuIntegration.voiceSettingsButton = button;
                    break;
                }
            }
            
            Debug.Log("[VoiceSetup] VoiceMenuIntegration created");
        }
        
        private void ConfigureDefaultSettings(VoiceInteractionManager voiceManager)
        {
            if (voiceManager.voiceSettings == null)
            {
                voiceManager.voiceSettings = new VoiceSettings();
            }
            
            var settings = voiceManager.voiceSettings;
            
            // Configure default values
            settings.activationKey = voiceActivationKey;
            settings.chatModel = defaultModel;
            settings.systemPrompt = defaultSystemPrompt;
            settings.holdToTalk = true;
            settings.maxRecordingTime = 30f;
            settings.silenceThreshold = 0.05f;
            settings.silenceTimeout = 2f;
            settings.maxTokens = 150;
            settings.temperature = 0.7f;
            settings.responseVolume = 0.8f;
            settings.enableVoiceFeedback = true;
            
            // Load any existing settings from PlayerPrefs
            string savedApiKey = PlayerPrefs.GetString("Voice_ApiKey", "");
            if (!string.IsNullOrEmpty(savedApiKey))
            {
                settings.apiKey = savedApiKey;
            }
            
            Debug.Log("[VoiceSetup] Default settings configured");
        }
        
        // Public methods for manual setup
        [ContextMenu("Test Voice System")]
        public void TestVoiceSystem()
        {
            VoiceInteractionTester tester = FindFirstObjectByType<VoiceInteractionTester>();
            if (tester == null)
            {
                GameObject testObj = new GameObject("VoiceInteractionTester");
                tester = testObj.AddComponent<VoiceInteractionTester>();
            }
            
            tester.TestVoiceSystemManually();
        }
        
        [ContextMenu("Create Test UI")]
        public void CreateTestUI()
        {
            VoiceInteractionTester tester = FindFirstObjectByType<VoiceInteractionTester>();
            if (tester == null)
            {
                GameObject testObj = new GameObject("VoiceInteractionTester");
                tester = testObj.AddComponent<VoiceInteractionTester>();
                tester.showDebugUI = true;
                
                // Create basic test UI
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject testPanel = new GameObject("VoiceTestPanel");
                    testPanel.transform.SetParent(canvas.transform, false);
                    
                    RectTransform rect = testPanel.AddComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.1f, 0.1f);
                    rect.anchorMax = new Vector2(0.9f, 0.9f);
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    
                    Image background = testPanel.AddComponent<Image>();
                    background.color = new Color(0, 0, 0, 0.8f);
                    
                    tester.testPanel = testPanel;
                }
            }
        }
        
        // Utility methods
        public bool IsVoiceSystemSetup()
        {
            return FindFirstObjectByType<VoiceInteractionManager>() != null;
        }
        
        public void EnableVoiceSystem()
        {
            VoiceInteractionManager voiceManager = FindFirstObjectByType<VoiceInteractionManager>();
            if (voiceManager != null)
            {
                voiceManager.ToggleVoiceInteraction(true);
            }
        }
        
        public void DisableVoiceSystem()
        {
            VoiceInteractionManager voiceManager = FindFirstObjectByType<VoiceInteractionManager>();
            if (voiceManager != null)
            {
                voiceManager.ToggleVoiceInteraction(false);
            }
        }
    }
}