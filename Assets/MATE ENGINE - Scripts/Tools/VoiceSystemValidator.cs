using UnityEngine;

namespace MateEngine.Voice
{
    /// <summary>
    /// Simple validator to ensure voice system is properly set up
    /// Add this to any GameObject to validate the voice interaction system
    /// </summary>
    public class VoiceSystemValidator : MonoBehaviour
    {
        [Header("Validation Results")]
        [ReadOnly] public bool voiceManagerFound = false;
        [ReadOnly] public bool audioSourceConfigured = false;
        [ReadOnly] public bool microphoneAvailable = false;
        [ReadOnly] public bool apiKeyConfigured = false;
        [ReadOnly] public bool uiComponentsFound = false;
        [ReadOnly] public bool menuIntegrationFound = false;
        
        [Header("System Status")]
        [ReadOnly] public string overallStatus = "Not Validated";
        [ReadOnly] public string recommendations = "";
        
        [Header("Controls")]
        public bool validateOnStart = true;
        
        void Start()
        {
            if (validateOnStart)
            {
                ValidateSystem();
            }
        }
        
        [ContextMenu("Validate Voice System")]
        public void ValidateSystem()
        {
            Debug.Log("[VoiceValidator] Validating voice interaction system...");
            
            // Reset validation state
            ResetValidation();
            
            // Run validation checks
            ValidateVoiceManager();
            ValidateAudioConfiguration();
            ValidateMicrophoneAccess();
            ValidateAPIConfiguration();
            ValidateUIComponents();
            ValidateMenuIntegration();
            
            // Determine overall status
            DetermineOverallStatus();
            
            // Provide recommendations
            GenerateRecommendations();
            
            Debug.Log($"[VoiceValidator] Validation complete. Status: {overallStatus}");
        }
        
        private void ResetValidation()
        {
            voiceManagerFound = false;
            audioSourceConfigured = false;
            microphoneAvailable = false;
            apiKeyConfigured = false;
            uiComponentsFound = false;
            menuIntegrationFound = false;
            overallStatus = "Validating...";
            recommendations = "";
        }
        
        private void ValidateVoiceManager()
        {
            VoiceInteractionManager voiceManager = FindFirstObjectByType<VoiceInteractionManager>();
            voiceManagerFound = voiceManager != null;
            
            if (voiceManagerFound)
            {
                Debug.Log("[VoiceValidator] ✅ VoiceInteractionManager found");
            }
            else
            {
                Debug.LogWarning("[VoiceValidator] ❌ VoiceInteractionManager not found");
            }
        }
        
        private void ValidateAudioConfiguration()
        {
            VoiceInteractionManager voiceManager = FindFirstObjectByType<VoiceInteractionManager>();
            if (voiceManager != null)
            {
                audioSourceConfigured = voiceManager.responseAudioSource != null;
                
                if (audioSourceConfigured)
                {
                    Debug.Log("[VoiceValidator] ✅ AudioSource configured for voice responses");
                }
                else
                {
                    Debug.LogWarning("[VoiceValidator] ❌ AudioSource not assigned to VoiceInteractionManager");
                }
            }
        }
        
        private void ValidateMicrophoneAccess()
        {
            microphoneAvailable = Microphone.devices.Length > 0;
            
            if (microphoneAvailable)
            {
                Debug.Log($"[VoiceValidator] ✅ Microphone available: {Microphone.devices[0]}");
            }
            else
            {
                Debug.LogError("[VoiceValidator] ❌ No microphone devices found");
            }
        }
        
        private void ValidateAPIConfiguration()
        {
            VoiceInteractionManager voiceManager = FindFirstObjectByType<VoiceInteractionManager>();
            if (voiceManager != null && voiceManager.voiceSettings != null)
            {
                apiKeyConfigured = !string.IsNullOrEmpty(voiceManager.voiceSettings.apiKey);
                
                if (apiKeyConfigured)
                {
                    Debug.Log("[VoiceValidator] ✅ API key configured");
                }
                else
                {
                    Debug.LogWarning("[VoiceValidator] ⚠️ API key not configured (voice system will not work)");
                }
            }
        }
        
        private void ValidateUIComponents()
        {
            VoiceInteractionUI voiceUI = FindFirstObjectByType<VoiceInteractionUI>();
            uiComponentsFound = voiceUI != null;
            
            if (uiComponentsFound)
            {
                Debug.Log("[VoiceValidator] ✅ VoiceInteractionUI found");
            }
            else
            {
                Debug.LogWarning("[VoiceValidator] ⚠️ VoiceInteractionUI not found (UI feedback will be limited)");
            }
        }
        
        private void ValidateMenuIntegration()
        {
            VoiceMenuIntegration menuIntegration = FindFirstObjectByType<VoiceMenuIntegration>();
            menuIntegrationFound = menuIntegration != null;
            
            if (menuIntegrationFound)
            {
                Debug.Log("[VoiceValidator] ✅ VoiceMenuIntegration found");
            }
            else
            {
                Debug.LogWarning("[VoiceValidator] ⚠️ VoiceMenuIntegration not found (menu integration limited)");
            }
        }
        
        private void DetermineOverallStatus()
        {
            int criticalPassed = 0;
            int criticalTotal = 3; // VoiceManager, AudioSource, Microphone
            
            if (voiceManagerFound) criticalPassed++;
            if (audioSourceConfigured) criticalPassed++;
            if (microphoneAvailable) criticalPassed++;
            
            if (criticalPassed == criticalTotal)
            {
                if (apiKeyConfigured)
                {
                    overallStatus = "✅ READY - Voice system fully functional";
                }
                else
                {
                    overallStatus = "⚠️ SETUP NEEDED - Configure API key to enable voice features";
                }
            }
            else
            {
                overallStatus = $"❌ INCOMPLETE - {criticalPassed}/{criticalTotal} critical components ready";
            }
        }
        
        private void GenerateRecommendations()
        {
            var recs = new System.Text.StringBuilder();
            
            if (!voiceManagerFound)
            {
                recs.AppendLine("• Add VoiceSystemSetup component and run auto-setup");
            }
            
            if (!audioSourceConfigured)
            {
                recs.AppendLine("• Assign AudioSource to VoiceInteractionManager.responseAudioSource");
            }
            
            if (!microphoneAvailable)
            {
                recs.AppendLine("• Check microphone permissions in Windows Privacy Settings");
            }
            
            if (!apiKeyConfigured)
            {
                recs.AppendLine("• Get API key from OpenRouter.ai and configure in voice settings");
            }
            
            if (!uiComponentsFound)
            {
                recs.AppendLine("• Add VoiceInteractionUI for better user feedback");
            }
            
            if (!menuIntegrationFound)
            {
                recs.AppendLine("• Add VoiceMenuIntegration for settings access");
            }
            
            if (recs.Length == 0)
            {
                recs.AppendLine("🎉 System is properly configured! Press V to start talking!");
            }
            
            recommendations = recs.ToString();
        }
        
        public string GetValidationReport()
        {
            return $@"Voice System Validation Report
============================

Core Components:
• Voice Manager: {(voiceManagerFound ? "✅" : "❌")}
• Audio Source: {(audioSourceConfigured ? "✅" : "❌")}
• Microphone: {(microphoneAvailable ? "✅" : "❌")}

Configuration:
• API Key: {(apiKeyConfigured ? "✅" : "❌")}
• UI Components: {(uiComponentsFound ? "✅" : "⚠️")}
• Menu Integration: {(menuIntegrationFound ? "✅" : "⚠️")}

Status: {overallStatus}

Recommendations:
{recommendations}";
        }
        
        public bool IsSystemReady()
        {
            return voiceManagerFound && audioSourceConfigured && microphoneAvailable && apiKeyConfigured;
        }
    }
    
    // Helper attribute for read-only fields in inspector
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
    
    #if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            UnityEditor.EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
    #endif
}