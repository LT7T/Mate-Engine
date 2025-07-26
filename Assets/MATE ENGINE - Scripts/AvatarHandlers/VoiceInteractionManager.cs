        private IEnumerator AnimateLipSync(AudioClip audioClip)
        {
            // TODO: Implement lip sync animation logic
            float duration = audioClip ? audioClip.length : 0f;
            float timer = 0f;
            while (timer < duration)
            {
                // Example: animate blendshapes or avatar mouth
                // blendshapeController?.SetLipSyncValue(Random.value);
                yield return null;
                timer += Time.deltaTime;
            }
            // Reset blendshape/mouth after lip sync
            // blendshapeController?.SetLipSyncValue(0f);
        }
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using MateEngine.Voice.Utils;

namespace MateEngine.Voice
{
    [System.Serializable]
    public class VoiceSettings
    {
        [Header("OpenRouter/OpenAI Configuration")]
        public string apiKey = "";
        public string baseUrl = "https://openrouter.ai/api/v1";
        public string chatModel = "anthropic/claude-3.5-sonnet";
        public string ttsModel = "tts-1";
        public string sttModel = "whisper-1";
        
        [Header("Voice Recording")]
        public KeyCode activationKey = KeyCode.V;
        public bool holdToTalk = true;
        public float maxRecordingTime = 30f;
        public float silenceThreshold = 0.1f;
        public float silenceTimeout = 2f;
        
        [Header("AI Response")]
        public float maxTokens = 150;
        public float temperature = 0.7f;
        public string systemPrompt = "You are a friendly AI companion living on the user's desktop. Keep responses brief and conversational. Show personality and be helpful.";
        
        [Header("Audio")]
        public float responseVolume = 1f;
        public bool enableVoiceFeedback = true;
    }

    public class VoiceInteractionManager : MonoBehaviour
    {
        [Header("Settings")]
        public VoiceSettings voiceSettings;
        
        [Header("Audio Components")]
        public AudioSource responseAudioSource;
        public string microphoneDevice;
        
        [Header("UI Feedback")]
        public GameObject listeningIndicator;
        public GameObject processingIndicator;
        public UnityEngine.UI.Text statusText;
        
        [Header("Avatar Integration")]
        public Animator avatarAnimator;
        public UniversalBlendshapes blendshapeController;
        
        [Header("Local Voice Processing")]
        public bool useLocalVoiceProcessing = true;
        public VoiceServerManager voiceServerManager;
        public LocalVoiceProcessor localVoiceProcessor;
        
        // Private members
        private AudioClip recordedClip;
        private bool isRecording = false;
        private bool isProcessing = false;
        private float recordingStartTime;
        private List<float> audioSamples = new List<float>();
        private Coroutine silenceDetectionCoroutine;
        
        // Public properties for UI integration
        public bool IsRecording => isRecording;
        public bool IsProcessing => isProcessing;
        
        // Animation hashes
        private static readonly int isTalkingParam = Animator.StringToHash("isTalking");
        private static readonly int isListeningParam = Animator.StringToHash("isListening");
        
        void Start()
        {
            InitializeVoiceSystem();
        }
        
        void Update()
        {
            HandleVoiceInput();
        }
        
        private void InitializeVoiceSystem()
        {
            // Initialize microphone
            if (Microphone.devices.Length > 0)
            {
                microphoneDevice = Microphone.devices[0];
                Debug.Log($"[VoiceInteraction] Using microphone: {microphoneDevice}");
            }
            else
            {
                Debug.LogError("[VoiceInteraction] No microphone devices found!");
                enabled = false;
                return;
            }
            
            // Initialize local voice processing if enabled
            if (useLocalVoiceProcessing)
            {
                InitializeLocalVoiceProcessing();
            }
            else
            {
                // Validate API settings for cloud processing
                if (string.IsNullOrEmpty(voiceSettings.apiKey))
                {
                    Debug.LogWarning("[VoiceInteraction] API key not set. Voice interaction will not work.");
                }
            }
            
            // Initialize UI
            if (listeningIndicator) listeningIndicator.SetActive(false);
            if (processingIndicator) processingIndicator.SetActive(false);
            if (statusText) statusText.text = useLocalVoiceProcessing ? "Starting voice servers..." : "Press V to talk";
        }
        
        private void InitializeLocalVoiceProcessing()
        {
            // Find or create voice server manager
            if (voiceServerManager == null)
            {
                voiceServerManager = FindFirstObjectByType<VoiceServerManager>();
                if (voiceServerManager == null)
                {
                    GameObject serverObj = new GameObject("VoiceServerManager");
                    voiceServerManager = serverObj.AddComponent<VoiceServerManager>();
                    Debug.Log("[VoiceInteraction] Created VoiceServerManager");
                }
            }
            
            // Find or create local voice processor
            if (localVoiceProcessor == null)
            {
                localVoiceProcessor = FindFirstObjectByType<LocalVoiceProcessor>();
                if (localVoiceProcessor == null)
                {
                    GameObject processorObj = new GameObject("LocalVoiceProcessor");
                    localVoiceProcessor = processorObj.AddComponent<LocalVoiceProcessor>();
                    localVoiceProcessor.serverManager = voiceServerManager;
                    Debug.Log("[VoiceInteraction] Created LocalVoiceProcessor");
                }
            }
            
            // Subscribe to events
            VoiceServerManager.OnServersReady += OnVoiceServersReady;
            VoiceServerManager.OnServersError += OnVoiceServersError;
            LocalVoiceProcessor.OnSpeechRecognized += OnSpeechRecognized;
            LocalVoiceProcessor.OnSpeechGenerated += OnSpeechGenerated;
            LocalVoiceProcessor.OnProcessingError += OnVoiceProcessingError;
            
            Debug.Log("[VoiceInteraction] Local voice processing initialized");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            VoiceServerManager.OnServersReady -= OnVoiceServersReady;
            VoiceServerManager.OnServersError -= OnVoiceServersError;
            LocalVoiceProcessor.OnSpeechRecognized -= OnSpeechRecognized;
            LocalVoiceProcessor.OnSpeechGenerated -= OnSpeechGenerated;
            LocalVoiceProcessor.OnProcessingError -= OnVoiceProcessingError;
        }
        
        private void OnVoiceServersReady()
        {
            Debug.Log("[VoiceInteraction] Voice servers ready!");
            if (statusText) statusText.text = "Press V to talk";
        }
        
        private void OnVoiceServersError()
        {
            Debug.LogError("[VoiceInteraction] Voice servers failed to start!");
            if (statusText) statusText.text = "Voice system error";
        }
        
        private void OnSpeechRecognized(string text)
        {
            Debug.Log($"[VoiceInteraction] Speech recognized: \"{text}\"");
            StartCoroutine(ProcessVoiceCommand(text));
        }
        
        private void OnSpeechGenerated(AudioClip audioClip)
        {
            Debug.Log("[VoiceInteraction] Speech generated, playing audio");
            PlayResponseAudio(audioClip);
        }
        
        private void OnVoiceProcessingError(string error)
        {
            Debug.LogError($"[VoiceInteraction] Voice processing error: {error}");
            if (statusText) statusText.text = $"Error: {error}";
            isProcessing = false;
            if (processingIndicator) processingIndicator.SetActive(false);
        }
        
        private IEnumerator ProcessVoiceCommand(string userMessage)
        {
            Debug.Log($"[VoiceInteraction] Processing voice command: \"{userMessage}\"");
            if (statusText) statusText.text = "Thinking...";
            
            if (useLocalVoiceProcessing)
            {
                // For local processing, we can either use the cloud LLM for chat or implement a local LLM
                // For now, we'll use the existing cloud chat API and local TTS
                yield return StartCoroutine(SendChatRequestLocal(userMessage));
            }
            else
            {
                // Use existing cloud processing
                yield return StartCoroutine(SendChatRequest(userMessage));
            }
        }
        
        private IEnumerator SendChatRequestLocal(string userMessage)
        {
            // This uses the same chat API as before but will use local TTS for the response
            var messages = new[]
            {
                new { role = "system", content = voiceSettings.systemPrompt },
                new { role = "user", content = userMessage }
            };
            
            var requestData = new
            {
                model = voiceSettings.chatModel,
                messages = messages,
                max_tokens = (int)voiceSettings.maxTokens,
                temperature = voiceSettings.temperature
            };
            
            string jsonData = JsonConvert.SerializeObject(requestData);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            
            string url = voiceSettings.baseUrl.TrimEnd('/') + "/chat/completions";
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {voiceSettings.apiKey}");
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                    
                    if (response.ContainsKey("choices"))
                    {
                        var choices = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response["choices"].ToString());
                        if (choices.Count > 0)
                        {
                            var message = JsonConvert.DeserializeObject<Dictionary<string, object>>(choices[0]["message"].ToString());
                            string aiResponse = message["content"].ToString();
                            
                            Debug.Log($"[VoiceInteraction] AI Response: {aiResponse}");
                            
                            if (statusText) statusText.text = $"AI: {aiResponse}";
                            
                            // Convert AI response to speech using local TTS
                            if (localVoiceProcessor != null)
                            {
                                localVoiceProcessor.ProcessTextToSpeech(aiResponse);
                                // Audio will be played in OnSpeechGenerated callback
                            }
                            else
                            {
                                // Fallback to cloud TTS
                                yield return StartCoroutine(SendTextToSpeechRequest(aiResponse));
                            }
                        }
                    }
                    
                    // Reset processing state
                    isProcessing = false;
                    if (processingIndicator) processingIndicator.SetActive(false);
                    if (statusText && statusText.text.StartsWith("AI:")) 
                    {
                        // Keep the AI response visible for a moment, then reset
                        yield return new WaitForSeconds(2f);
                        if (statusText) statusText.text = "Press V to talk";
                    }
                }
                else
                {
                    Debug.LogError($"[VoiceInteraction] Chat Error: {request.error}");
                    if (statusText) statusText.text = "AI request failed";
                    isProcessing = false;
                    if (processingIndicator) processingIndicator.SetActive(false);
                }
            }
        }
        
        private void HandleVoiceInput()
        {
            if (isProcessing) return;
            
            if (voiceSettings.holdToTalk)
            {
                // Hold to talk mode
                if (Input.GetKeyDown(voiceSettings.activationKey) && !isRecording)
                {
                    StartRecording();
                }
                else if (Input.GetKeyUp(voiceSettings.activationKey) && isRecording)
                {
                    StopRecording();
                }
            }
            else
            {
                // Toggle mode
                if (Input.GetKeyDown(voiceSettings.activationKey))
                {
                    if (!isRecording)
                    {
                        StartRecording();
                    }
                    else
                    {
                        StopRecording();
                    }
                }
            }
            
            // Check for recording timeout
            if (isRecording && Time.time - recordingStartTime > voiceSettings.maxRecordingTime)
            {
                StopRecording();
            }
        }
        
        private void StartRecording()
        {
            if (string.IsNullOrEmpty(voiceSettings.apiKey))
            {
                if (statusText) statusText.text = "API key not configured!";
                return;
            }
            
            isRecording = true;
            recordingStartTime = Time.time;
            
            // Start microphone recording
            recordedClip = Microphone.Start(microphoneDevice, false, (int)voiceSettings.maxRecordingTime, 44100);
            
            // Update UI
            if (listeningIndicator) listeningIndicator.SetActive(true);
            if (statusText) statusText.text = "Listening...";
            
            // Update avatar animation
            if (avatarAnimator) avatarAnimator.SetBool(isListeningParam, true);
            
            // Start silence detection if not in hold-to-talk mode
            if (!voiceSettings.holdToTalk)
            {
                silenceDetectionCoroutine = StartCoroutine(DetectSilence());
            }
            
            Debug.Log("[VoiceInteraction] Started recording");
        }
        
        private void StopRecording()
        {
            if (!isRecording) return;
            
            isRecording = false;
            Microphone.End(microphoneDevice);
            
            // Update UI
            if (listeningIndicator) listeningIndicator.SetActive(false);
            if (processingIndicator) processingIndicator.SetActive(true);
            if (statusText) statusText.text = "Processing...";
            
            // Update avatar animation
            if (avatarAnimator) avatarAnimator.SetBool(isListeningParam, false);
            
            // Stop silence detection
            if (silenceDetectionCoroutine != null)
            {
                StopCoroutine(silenceDetectionCoroutine);
                silenceDetectionCoroutine = null;
            }
            
            Debug.Log("[VoiceInteraction] Stopped recording");
            
            // Process the recorded audio
            StartCoroutine(ProcessVoiceInput());
        }
        
        private IEnumerator DetectSilence()
        {
            float silenceTimer = 0f;
            
            while (isRecording)
            {
                if (recordedClip != null)
                {
                    // Get current audio level
                    float[] samples = new float[128];
                    int micPosition = Microphone.GetPosition(microphoneDevice);
                    if (micPosition > 128)
                    {
                        recordedClip.GetData(samples, micPosition - 128);
                        
                        float level = 0f;
                        foreach (float sample in samples)
                        {
                            level += Mathf.Abs(sample);
                        }
                        level /= samples.Length;
                        
                        if (level < voiceSettings.silenceThreshold)
                        {
                            silenceTimer += Time.deltaTime;
                            if (silenceTimer >= voiceSettings.silenceTimeout)
                            {
                                StopRecording();
                                yield break;
                            }
                        }
                        else
                        {
                            silenceTimer = 0f;
                        }
                    }
                }
                
                yield return null;
            }
        }
        
        private IEnumerator ProcessVoiceInput()
        {
            isProcessing = true;
            
            try
            {
                // Trim silence and apply basic processing
                AudioClip processedClip = AudioUtils.TrimSilence(recordedClip, voiceSettings.silenceThreshold);
                if (processedClip == null || !AudioUtils.ContainsSpeech(processedClip))
                {
                    if (statusText) statusText.text = "No speech detected";
                    yield break;
                }
                
                if (useLocalVoiceProcessing && localVoiceProcessor != null)
                {
                    // Use local voice processing
                    localVoiceProcessor.ProcessSpeechToText(processedClip);
                    // Processing continues in OnSpeechRecognized callback
                }
                else
                {
                    // Use cloud API processing
                    byte[] audioData = AudioUtils.AudioClipToWav(processedClip);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[VoiceInteraction] Error processing voice input: {e.Message}");
                if (statusText) statusText.text = "Error processing voice";
                isProcessing = false;
                yield break;
            }
            if (!useLocalVoiceProcessing || localVoiceProcessor == null)
            {
                // Use cloud API processing
                byte[] audioData = AudioUtils.AudioClipToWav(processedClip);
                yield return StartCoroutine(SendSpeechToTextRequest(audioData));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[VoiceInteraction] Error processing voice input: {e.Message}");
                if (statusText) statusText.text = "Error processing voice";
                isProcessing = false;
                if (processingIndicator) processingIndicator.SetActive(false);
            }
            finally
            {
                // Only reset UI state if using cloud processing (local processing handles this in callbacks)
                if (!useLocalVoiceProcessing)
                {
                    isProcessing = false;
                    if (processingIndicator) processingIndicator.SetActive(false);
                    if (statusText) statusText.text = "Press V to talk";
                }
            }
        }
        
        private IEnumerator SendSpeechToTextRequest(byte[] audioData)
        {
            string transcription = "";
            
            // Create form data for multipart/form-data request
            var form = new WWWForm();
            form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");
            form.AddField("model", voiceSettings.sttModel);
            
            string url = voiceSettings.baseUrl.TrimEnd('/') + "/audio/transcriptions";
            
            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                request.SetRequestHeader("Authorization", $"Bearer {voiceSettings.apiKey}");
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                    if (response.ContainsKey("text"))
                    {
                        transcription = response["text"].ToString();
                        Debug.Log($"[VoiceInteraction] Transcription: {transcription}");
                        
                        if (!string.IsNullOrWhiteSpace(transcription))
                        {
                            yield return StartCoroutine(SendChatRequest(transcription));
                        }
                        else
                        {
                            if (statusText) statusText.text = "No speech detected";
                        }
                    }
                }
                else
                {
                    Debug.LogError($"[VoiceInteraction] STT Error: {request.error}");
                    if (statusText) statusText.text = "Speech recognition failed";
                }
            }
        }
        
        private IEnumerator SendChatRequest(string userMessage)
        {
            var messages = new[]
            {
                new { role = "system", content = voiceSettings.systemPrompt },
                new { role = "user", content = userMessage }
            };
            
            var requestData = new
            {
                model = voiceSettings.chatModel,
                messages = messages,
                max_tokens = (int)voiceSettings.maxTokens,
                temperature = voiceSettings.temperature
            };
            
            string jsonData = JsonConvert.SerializeObject(requestData);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            
            string url = voiceSettings.baseUrl.TrimEnd('/') + "/chat/completions";
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {voiceSettings.apiKey}");
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                    
                    if (response.ContainsKey("choices"))
                    {
                        var choices = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response["choices"].ToString());
                        if (choices.Count > 0)
                        {
                            var message = JsonConvert.DeserializeObject<Dictionary<string, object>>(choices[0]["message"].ToString());
                            string aiResponse = message["content"].ToString();
                            
                            Debug.Log($"[VoiceInteraction] AI Response: {aiResponse}");
                            
                            if (statusText) statusText.text = $"AI: {aiResponse}";
                            
                            // Convert AI response to speech
                            yield return StartCoroutine(SendTextToSpeechRequest(aiResponse));
                        }
                    }
                }
                else
                {
                    Debug.LogError($"[VoiceInteraction] Chat Error: {request.error}");
                    if (statusText) statusText.text = "AI request failed";
                }
            }
        }
        
        private IEnumerator SendTextToSpeechRequest(string text)
        {
            var requestData = new
            {
                model = voiceSettings.ttsModel,
                input = text,
                voice = "alloy"
            };
            
            string jsonData = JsonConvert.SerializeObject(requestData);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            
            string url = voiceSettings.baseUrl.TrimEnd('/') + "/audio/speech";
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {voiceSettings.apiKey}");
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    // Convert response to AudioClip and play
                    yield return StartCoroutine(PlayAudioResponse(request.downloadHandler.data));
                }
                else
                {
                    Debug.LogError($"[VoiceInteraction] TTS Error: {request.error}");
                    // Fallback to system TTS if available
                    PlayTextToSpeechFallback(text);
                }
            }
        }
        
        private IEnumerator PlayAudioResponse(byte[] audioData)
        {
            if (responseAudioSource && voiceSettings.enableVoiceFeedback)
            {
                // Update avatar animation
                if (avatarAnimator) avatarAnimator.SetBool(isTalkingParam, true);
                
                // Convert MP3/audio data to AudioClip
                AudioClip audioClip = ConvertBytesToAudioClip(audioData);
                
                if (audioClip != null)
                {
                    responseAudioSource.volume = voiceSettings.responseVolume;
                    responseAudioSource.clip = audioClip;
                    responseAudioSource.Play();
                    
                    // Animate mouth/blendshapes during speech
                    if (blendshapeController)
                    {
                        StartCoroutine(AnimateMouthDuringSpeech(audioClip.length));
                    }
                    
                    // Wait for audio to finish
                    yield return new WaitForSeconds(audioClip.length);
                }
                
                // Update avatar animation
                if (avatarAnimator) avatarAnimator.SetBool(isTalkingParam, false);
            }
        }
        
        private void PlayTextToSpeechFallback(string text)
        {
            // Fallback for when TTS API fails - could use Windows SAPI or Unity's built-in TTS
            Debug.Log($"[VoiceInteraction] TTS Fallback: {text}");
            // This is a placeholder - actual implementation would depend on platform
            
            // For now, create a simple test tone to indicate response
            if (responseAudioSource && voiceSettings.enableVoiceFeedback)
            {
                AudioClip testTone = AudioFormatConverter.CreateTestTone(800f, 0.5f);
                responseAudioSource.clip = testTone;
                responseAudioSource.Play();
            }
        }
        
        private IEnumerator AnimateMouthDuringSpeech(float duration)
        {
            if (blendshapeController == null) yield break;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                // Simple mouth animation - random values for demonstration
                float mouthOpen = Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f;
                
                // Apply to mouth blendshapes if available
                // This would need to be customized based on the specific VRM model
                // blendshapeController.SetBlendshapeWeight("mouth", mouthOpen);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        private AudioClip ConvertBytesToAudioClip(byte[] audioData)
        {
            // Try to convert audio data to AudioClip
            // For MP3 data from OpenAI TTS, we need a more sophisticated converter
            return AudioFormatConverter.ConvertMP3ToAudioClip(audioData, "TTSResponse");
        }
        
        private void PlayResponseAudio(AudioClip audioClip)
        {
            if (responseAudioSource && voiceSettings.enableVoiceFeedback && audioClip != null)
            {
                Debug.Log($"[VoiceInteraction] Playing response audio: {audioClip.length}s");
                
                // Set volume
                responseAudioSource.volume = voiceSettings.responseVolume;
                
                // Play the audio
                responseAudioSource.clip = audioClip;
                responseAudioSource.Play();
                
                // Update avatar animation
                if (avatarAnimator) avatarAnimator.SetBool(isTalkingParam, true);
                
                // Start lip sync coroutine
                StartCoroutine(AnimateLipSync(audioClip));
                
                // Stop talking animation when audio finishes
                StartCoroutine(WaitForAudioComplete(audioClip.length));
            }
            else
            {
                Debug.LogWarning("[VoiceInteraction] Cannot play audio - AudioSource not configured or audio clip is null");
            }
        }
        
        private IEnumerator WaitForAudioComplete(float duration)
        {
            yield return new WaitForSeconds(duration);
            
            // Reset avatar animation
            if (avatarAnimator) avatarAnimator.SetBool(isTalkingParam, false);
            
            Debug.Log("[VoiceInteraction] Response audio completed");
        }
        
        public void SetApiKey(string apiKey)
        {
            voiceSettings.apiKey = apiKey;
        }
        
        public void SetModel(string model)
        {
            voiceSettings.chatModel = model;
        }
        
        public void ToggleVoiceInteraction(bool enabled)
        {
            this.enabled = enabled;
            if (!enabled && isRecording)
            {
                StopRecording();
            }
        }
        
        public void StartRecordingManual()
        {
            if (!isRecording)
            {
                StartRecording();
            }
        }
        
        public void StopRecordingManual()
        {
            if (isRecording)
            {
                StopRecording();
            }
        }
        
        public IEnumerator SendTestMessage(string message)
        {
            if (string.IsNullOrEmpty(voiceSettings.apiKey))
            {
                Debug.LogWarning("[VoiceInteraction] Cannot test - API key not configured");
                yield break;
            }
            
            isProcessing = true;
            if (processingIndicator) processingIndicator.SetActive(true);
            if (statusText) statusText.text = "Testing TTS...";
            
            yield return StartCoroutine(SendTextToSpeechRequest(message));
            
            isProcessing = false;
            if (processingIndicator) processingIndicator.SetActive(false);
            if (statusText) statusText.text = "Test completed";
        }
    }
}