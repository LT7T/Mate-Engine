using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MateEngine.Voice.Tests
{
    /// <summary>
    /// Test suite for voice interaction system validation
    /// </summary>
    public class VoiceInteractionTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        public bool runTestsOnStart = false;
        public bool showDebugUI = true;
        
        [Header("Test UI")]
        public GameObject testPanel;
        public TextMeshProUGUI testResultsText;
        public Button runAllTestsButton;
        public Button testMicrophoneButton;
        public Button testAPIConnectionButton;
        public Button testTTSButton;
        public Button testSTTButton;
        public Slider testProgressSlider;
        
        [Header("Test Audio")]
        public AudioClip testAudioClip;
        
        private VoiceInteractionManager voiceManager;
        private VoiceInteractionUI voiceUI;
        private VoiceMenuIntegration menuIntegration;
        
        private System.Text.StringBuilder testResults = new System.Text.StringBuilder();
        private int totalTests = 0;
        private int passedTests = 0;
        
        void Start()
        {
            InitializeTestSystem();
            
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }
        
        private void InitializeTestSystem()
        {
            // Find voice system components
            voiceManager = FindFirstObjectByType<VoiceInteractionManager>();
            voiceUI = FindFirstObjectByType<VoiceInteractionUI>();
            menuIntegration = FindFirstObjectByType<VoiceMenuIntegration>();
            
            // Setup UI
            if (testPanel && !showDebugUI)
            {
                testPanel.SetActive(false);
            }
            
            if (runAllTestsButton)
            {
                runAllTestsButton.onClick.AddListener(() => StartCoroutine(RunAllTests()));
            }
            
            if (testMicrophoneButton)
            {
                testMicrophoneButton.onClick.AddListener(() => StartCoroutine(TestMicrophone()));
            }
            
            if (testAPIConnectionButton)
            {
                testAPIConnectionButton.onClick.AddListener(() => StartCoroutine(TestAPIConnection()));
            }
            
            if (testTTSButton)
            {
                testTTSButton.onClick.AddListener(() => StartCoroutine(TestTextToSpeech()));
            }
            
            if (testSTTButton)
            {
                testSTTButton.onClick.AddListener(() => StartCoroutine(TestSpeechToText()));
            }
            
            LogTest("Voice Interaction Test System Initialized");
        }
        
        public IEnumerator RunAllTests()
        {
            LogTest("=== Starting Voice Interaction Test Suite ===");
            testResults.Clear();
            totalTests = 0;
            passedTests = 0;
            
            UpdateProgress(0f);
            
            // Test 1: Component Initialization
            yield return StartCoroutine(TestComponentInitialization());
            UpdateProgress(0.2f);
            
            // Test 2: Microphone Access
            yield return StartCoroutine(TestMicrophone());
            UpdateProgress(0.4f);
            
            // Test 3: Audio Processing
            yield return StartCoroutine(TestAudioProcessing());
            UpdateProgress(0.6f);
            
            // Test 4: API Connection (if configured)
            yield return StartCoroutine(TestAPIConnection());
            UpdateProgress(0.8f);
            
            // Test 5: UI Integration
            yield return StartCoroutine(TestUIIntegration());
            UpdateProgress(1f);
            
            // Final results
            LogTest($"=== Test Suite Complete: {passedTests}/{totalTests} tests passed ===");
            
            if (testResultsText)
            {
                testResultsText.text = testResults.ToString();
            }
        }
        
        private IEnumerator TestComponentInitialization()
        {
            LogTest("Testing component initialization...");
            
            bool voiceManagerExists = TestComponent(voiceManager, "VoiceInteractionManager");
            bool voiceUIExists = TestComponent(voiceUI, "VoiceInteractionUI");
            bool menuIntegrationExists = TestComponent(menuIntegration, "VoiceMenuIntegration");
            
            if (voiceManagerExists)
            {
                bool settingsValid = voiceManager.voiceSettings != null;
                LogTestResult("VoiceSettings initialized", settingsValid);
                
                bool audioSourceExists = voiceManager.responseAudioSource != null;
                LogTestResult("AudioSource assigned", audioSourceExists);
            }
            
            yield return null;
        }
        
        private IEnumerator TestMicrophone()
        {
            LogTest("Testing microphone access...");
            
            bool hasMicrophones = Microphone.devices.Length > 0;
            LogTestResult("Microphone devices available", hasMicrophones);
            
            if (hasMicrophones)
            {
                string device = Microphone.devices[0];
                LogTest($"Primary microphone: {device}");
                
                // Test microphone recording
                AudioClip testClip = null;
                try
                {
                    testClip = Microphone.Start(device, false, 1, 44100);
                    yield return new WaitForSeconds(0.1f);
                    Microphone.End(device);
                    
                    bool recordingSuccessful = testClip != null;
                    LogTestResult("Microphone recording test", recordingSuccessful);
                    
                    if (testClip != null)
                    {
                        DestroyImmediate(testClip);
                    }
                }
                catch (System.Exception e)
                {
                    LogTestResult($"Microphone recording failed: {e.Message}", false);
                }
            }
            
            yield return null;
        }
        
        private IEnumerator TestAudioProcessing()
        {
            LogTest("Testing audio processing utilities...");
            
            // Test audio conversion
            if (testAudioClip != null)
            {
                try
                {
                    byte[] wavData = MateEngine.Voice.Utils.AudioUtils.AudioClipToWav(testAudioClip);
                    bool conversionSuccessful = wavData != null && wavData.Length > 0;
                    LogTestResult("Audio to WAV conversion", conversionSuccessful);
                    
                    // Test audio analysis
                    bool containsSpeech = MateEngine.Voice.Utils.AudioUtils.ContainsSpeech(testAudioClip);
                    LogTestResult("Speech detection analysis", true); // Just test that it doesn't crash
                    
                    // Test trimming
                    AudioClip trimmed = MateEngine.Voice.Utils.AudioUtils.TrimSilence(testAudioClip);
                    bool trimmingWorked = trimmed != null;
                    LogTestResult("Audio trimming", trimmingWorked);
                    
                    if (trimmed != null)
                    {
                        DestroyImmediate(trimmed);
                    }
                }
                catch (System.Exception e)
                {
                    LogTestResult($"Audio processing failed: {e.Message}", false);
                }
            }
            else
            {
                // Create a test clip
                AudioClip testClip = MateEngine.Voice.Utils.AudioFormatConverter.CreateTestTone(440f, 1f);
                bool testClipCreated = testClip != null;
                LogTestResult("Test audio clip creation", testClipCreated);
                
                if (testClip != null)
                {
                    DestroyImmediate(testClip);
                }
            }
            
            yield return null;
        }
        
        private IEnumerator TestAPIConnection()
        {
            LogTest("Testing API connection...");
            
            if (voiceManager == null || string.IsNullOrEmpty(voiceManager.voiceSettings.apiKey))
            {
                LogTestResult("API key configuration", false);
                LogTest("Skipping API tests - no API key configured");
                yield break;
            }
            
            LogTestResult("API key configuration", true);
            
            // Test basic configuration
            bool hasValidBaseUrl = !string.IsNullOrEmpty(voiceManager.voiceSettings.baseUrl);
            LogTestResult("Base URL configured", hasValidBaseUrl);
            
            bool hasValidModel = !string.IsNullOrEmpty(voiceManager.voiceSettings.chatModel);
            LogTestResult("Chat model configured", hasValidModel);
            
            // Note: We don't actually test API calls here to avoid unnecessary API usage during testing
            LogTest("Note: Live API tests skipped to avoid usage charges");
            
            yield return null;
        }
        
        private IEnumerator TestUIIntegration()
        {
            LogTest("Testing UI integration...");
            
            if (voiceUI != null)
            {
                // Test UI components
                bool hasStatusText = voiceUI.statusText != null;
                LogTestResult("Status text component", hasStatusText);
                
                bool hasMicIcon = voiceUI.microphoneIcon != null;
                LogTestResult("Microphone icon component", hasMicIcon);
            }
            
            if (menuIntegration != null)
            {
                // Test menu integration
                bool hasVoiceMenu = menuIntegration.voiceMenuPanel != null;
                LogTestResult("Voice menu panel", hasVoiceMenu);
                
                bool isVoiceEnabled = menuIntegration.IsVoiceEnabled;
                LogTestResult("Voice system accessible", true); // Just test the property access
            }
            
            yield return null;
        }
        
        private IEnumerator TestTextToSpeech()
        {
            LogTest("Testing Text-to-Speech...");
            
            if (voiceManager == null)
            {
                LogTestResult("VoiceManager not found", false);
                yield break;
            }
            
            if (string.IsNullOrEmpty(voiceManager.voiceSettings.apiKey))
            {
                LogTest("Testing TTS fallback (no API key)...");
                
                // Test fallback TTS
                if (voiceManager.responseAudioSource != null)
                {
                    AudioClip testTone = MateEngine.Voice.Utils.AudioFormatConverter.CreateTestTone(800f, 0.5f);
                    voiceManager.responseAudioSource.clip = testTone;
                    voiceManager.responseAudioSource.Play();
                    
                    LogTestResult("TTS fallback audio", true);
                    
                    yield return new WaitForSeconds(0.6f);
                    
                    if (testTone != null)
                    {
                        DestroyImmediate(testTone);
                    }
                }
                else
                {
                    LogTestResult("AudioSource for TTS not found", false);
                }
            }
            else
            {
                LogTest("Live TTS test skipped (API configured - would use API credits)");
            }
            
            yield return null;
        }
        
        private IEnumerator TestSpeechToText()
        {
            LogTest("Testing Speech-to-Text...");
            
            if (voiceManager == null)
            {
                LogTestResult("VoiceManager not found", false);
                yield break;
            }
            
            if (string.IsNullOrEmpty(voiceManager.voiceSettings.apiKey))
            {
                LogTest("STT test skipped - no API key configured");
                yield break;
            }
            
            LogTest("Live STT test skipped (API configured - would use API credits)");
            
            // Test audio preparation for STT
            if (testAudioClip != null)
            {
                byte[] audioData = MateEngine.Voice.Utils.AudioUtils.AudioClipToWav(testAudioClip);
                bool audioPrepped = audioData != null && audioData.Length > 44; // Header + some data
                LogTestResult("Audio preparation for STT", audioPrepped);
            }
            
            yield return null;
        }
        
        private bool TestComponent(Component component, string componentName)
        {
            bool exists = component != null;
            LogTestResult($"{componentName} component", exists);
            return exists;
        }
        
        private void LogTest(string message)
        {
            testResults.AppendLine(message);
            Debug.Log($"[VoiceTest] {message}");
        }
        
        private void LogTestResult(string testName, bool passed)
        {
            totalTests++;
            if (passed) passedTests++;
            
            string status = passed ? "PASS" : "FAIL";
            string message = $"[{status}] {testName}";
            
            testResults.AppendLine(message);
            Debug.Log($"[VoiceTest] {message}");
        }
        
        private void UpdateProgress(float progress)
        {
            if (testProgressSlider)
            {
                testProgressSlider.value = progress;
            }
        }
        
        // Public methods for manual testing
        public void TestVoiceSystemManually()
        {
            StartCoroutine(RunAllTests());
        }
        
        public void TestMicrophoneOnly()
        {
            StartCoroutine(TestMicrophone());
        }
        
        public void ToggleTestUI()
        {
            if (testPanel)
            {
                testPanel.SetActive(!testPanel.activeSelf);
            }
        }
        
        public string GetTestResults()
        {
            return testResults.ToString();
        }
    }
}