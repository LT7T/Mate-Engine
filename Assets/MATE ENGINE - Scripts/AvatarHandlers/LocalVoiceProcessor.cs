using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using MateEngine.Voice.Utils;

namespace MateEngine.Voice
{
    /// <summary>
    /// Handles communication with local Whisper and TTS servers
    /// Processes voice input and generates speech output locally
    /// </summary>
    public class LocalVoiceProcessor : MonoBehaviour
    {
        [Header("Server References")]
        public VoiceServerManager serverManager;
        
        [Header("Processing Settings")]
        public float speechToTextTimeout = 30f;
        public float textToSpeechTimeout = 20f;
        public int maxRetries = 3;
        
        [Header("Status")]
        [ReadOnly] public bool isProcessingSpeech = false;
        [ReadOnly] public bool isGeneratingSpeech = false;
        
        // Events
        public static event Action<string> OnSpeechRecognized;
        public static event Action<AudioClip> OnSpeechGenerated;
        public static event Action<string> OnProcessingError;
        
        void Start()
        {
            if (serverManager == null)
            {
                serverManager = FindFirstObjectByType<VoiceServerManager>();
            }
            
            if (serverManager == null)
            {
                Debug.LogError("[LocalVoiceProcessor] VoiceServerManager not found!");
                enabled = false;
            }
        }
        
        /// <summary>
        /// Process recorded audio to text using local Whisper server
        /// </summary>
        public void ProcessSpeechToText(AudioClip audioClip)
        {
            if (audioClip == null)
            {
                Debug.LogError("[LocalVoiceProcessor] AudioClip is null");
                return;
            }
            
            if (!serverManager.AreServersReady())
            {
                Debug.LogError("[LocalVoiceProcessor] Servers not ready");
                OnProcessingError?.Invoke("Voice servers not ready");
                return;
            }
            
            StartCoroutine(ProcessSpeechToTextCoroutine(audioClip));
        }
        
        /// <summary>
        /// Generate speech from text using local TTS server
        /// </summary>
        public void ProcessTextToSpeech(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogError("[LocalVoiceProcessor] Text is null or empty");
                return;
            }
            
            if (!serverManager.AreServersReady())
            {
                Debug.LogError("[LocalVoiceProcessor] Servers not ready");
                OnProcessingError?.Invoke("Voice servers not ready");
                return;
            }
            
            StartCoroutine(ProcessTextToSpeechCoroutine(text));
        }
        
        private IEnumerator ProcessSpeechToTextCoroutine(AudioClip audioClip)
        {
            isProcessingSpeech = true;
            
            try
            {
                Debug.Log("[LocalVoiceProcessor] Converting audio to WAV...");
                
                // Convert AudioClip to WAV format
                byte[] audioData = AudioUtils.AudioClipToWav(audioClip);
                if (audioData == null || audioData.Length == 0)
                {
                    Debug.LogError("[LocalVoiceProcessor] Failed to convert audio to WAV");
                    OnProcessingError?.Invoke("Failed to convert audio");
                    yield break;
                }
                
                Debug.Log($"[LocalVoiceProcessor] Sending {audioData.Length} bytes to Whisper server...");
                
                // Create multipart form data
                WWWForm form = new WWWForm();
                form.AddBinaryData("audio", audioData, "audio.wav", "audio/wav");
                form.AddField("response_format", "json");
                
                string whisperUrl = serverManager.WhisperServerUrl + "/transcribe";
                
                using (UnityWebRequest request = UnityWebRequest.Post(whisperUrl, form))
                {
                    request.timeout = (int)speechToTextTimeout;
                    
                    yield return request.SendWebRequest();
                    
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            string responseText = request.downloadHandler.text;
                            Debug.Log($"[LocalVoiceProcessor] Whisper response: {responseText}");
                            
                            // Parse JSON response
                            var response = JsonUtility.FromJson<WhisperResponse>(responseText);
                            
                            if (!string.IsNullOrEmpty(response.text))
                            {
                                string transcribedText = response.text.Trim();
                                Debug.Log($"[LocalVoiceProcessor] ✅ Transcribed: \"{transcribedText}\"");
                                OnSpeechRecognized?.Invoke(transcribedText);
                            }
                            else
                            {
                                Debug.LogWarning("[LocalVoiceProcessor] Empty transcription result");
                                OnProcessingError?.Invoke("No speech detected");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[LocalVoiceProcessor] Error parsing Whisper response: {e.Message}");
                            OnProcessingError?.Invoke("Failed to parse speech recognition result");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[LocalVoiceProcessor] Whisper server error: {request.error}");
                        OnProcessingError?.Invoke($"Speech recognition failed: {request.error}");
                    }
                }
            }
            finally
            {
                isProcessingSpeech = false;
            }
        }
        
        private IEnumerator ProcessTextToSpeechCoroutine(string text)
        {
            isGeneratingSpeech = true;
            
            try
            {
                Debug.Log($"[LocalVoiceProcessor] Generating speech for: \"{text}\"");
                
                // Create JSON request
                var requestData = new TTSRequest
                {
                    text = text,
                    voice = "default",
                    response_format = "wav"
                };
                
                string jsonData = JsonUtility.ToJson(requestData);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                
                string ttsUrl = serverManager.TTSServerUrl + "/synthesize";
                
                using (UnityWebRequest request = new UnityWebRequest(ttsUrl, "POST"))
                {
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.timeout = (int)textToSpeechTimeout;
                    
                    yield return request.SendWebRequest();
                    
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            byte[] audioData = request.downloadHandler.data;
                            Debug.Log($"[LocalVoiceProcessor] Received {audioData.Length} bytes of audio data");
                            
                            // Convert WAV data to AudioClip
                            AudioClip speechClip = AudioUtils.WavToAudioClip(audioData);
                            
                            if (speechClip != null)
                            {
                                Debug.Log($"[LocalVoiceProcessor] ✅ Generated speech clip: {speechClip.length}s");
                                OnSpeechGenerated?.Invoke(speechClip);
                            }
                            else
                            {
                                Debug.LogError("[LocalVoiceProcessor] Failed to convert audio data to AudioClip");
                                OnProcessingError?.Invoke("Failed to convert audio data");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[LocalVoiceProcessor] Error processing TTS response: {e.Message}");
                            OnProcessingError?.Invoke("Failed to process speech synthesis result");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[LocalVoiceProcessor] TTS server error: {request.error}");
                        OnProcessingError?.Invoke($"Speech synthesis failed: {request.error}");
                    }
                }
            }
            finally
            {
                isGeneratingSpeech = false;
            }
        }
        
        public bool IsProcessing()
        {
            return isProcessingSpeech || isGeneratingSpeech;
        }
    }
    
    [System.Serializable]
    public class WhisperResponse
    {
        public string text;
        public string language;
        public float duration;
        public WordSegment[] segments;
    }
    
    [System.Serializable]
    public class WordSegment
    {
        public int id;
        public int seek;
        public float start;
        public float end;
        public string text;
        public float[] tokens;
        public float temperature;
        public float avg_logprob;
        public float compression_ratio;
        public float no_speech_prob;
    }
    
    [System.Serializable]
    public class TTSRequest
    {
        public string text;
        public string voice;
        public string response_format;
        public float speed = 1.0f;
    }
}
