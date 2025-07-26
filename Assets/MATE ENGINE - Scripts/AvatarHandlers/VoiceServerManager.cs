using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace MateEngine.Voice
{
    /// <summary>
    /// Manages local Python servers for Whisper (STT) and TTS
    /// Automatically starts servers when Unity app launches
    /// </summary>
    public class VoiceServerManager : MonoBehaviour
    {
        [Header("Server Configuration")]
        public string whisperServerPort = "8001";
        public string ttsServerPort = "8002";
        public bool autoStartServers = true;
        public bool killServersOnExit = true;
        
        [Header("Paths")]
        public string pythonExecutablePath = "Python/python.exe";
        public string whisperServerScript = "WhisperServer/whisper_server.py";
        public string ttsServerScript = "TTSServer/tts_server.py";
        
        [Header("Status")]
        [ReadOnly] public bool whisperServerRunning = false;
        [ReadOnly] public bool ttsServerRunning = false;
        [ReadOnly] public bool serversInitialized = false;
        
        // Private members
        private Process whisperProcess;
        private Process ttsProcess;
        private bool isStartingServers = false;
        
        // Server URLs
        public string WhisperServerUrl => $"http://localhost:{whisperServerPort}";
        public string TTSServerUrl => $"http://localhost:{ttsServerPort}";
        
        // Events
        public static event Action OnServersReady;
        public static event Action OnServersError;
        
        void Start()
        {
            if (autoStartServers)
            {
                StartCoroutine(InitializeServers());
            }
        }
        
        void OnApplicationQuit()
        {
            if (killServersOnExit)
            {
                StopAllServers();
            }
        }
        
        void OnDestroy()
        {
            if (killServersOnExit)
            {
                StopAllServers();
            }
        }
        
        public IEnumerator InitializeServers()
        {
            if (isStartingServers || serversInitialized)
            {
                yield break;
            }
            
            isStartingServers = true;
            UnityEngine.Debug.Log("[VoiceServerManager] Initializing local voice servers...");
            
            // Check if servers are already running
            yield return StartCoroutine(CheckExistingServers());
            
            if (!whisperServerRunning)
            {
                yield return StartCoroutine(StartWhisperServer());
            }
            
            if (!ttsServerRunning)
            {
                yield return StartCoroutine(StartTTSServer());
            }
            
            // Wait for servers to be ready
            yield return StartCoroutine(WaitForServersReady());
            
            serversInitialized = whisperServerRunning && ttsServerRunning;
            isStartingServers = false;
            
            if (serversInitialized)
            {
                UnityEngine.Debug.Log("[VoiceServerManager] ✅ All voice servers ready!");
                OnServersReady?.Invoke();
            }
            else
            {
                UnityEngine.Debug.LogError("[VoiceServerManager] ❌ Failed to start voice servers");
                OnServersError?.Invoke();
            }
        }
        
        private IEnumerator CheckExistingServers()
        {
            // Check Whisper server
            yield return StartCoroutine(CheckServerHealth(WhisperServerUrl + "/health", (running) => whisperServerRunning = running));
            
            // Check TTS server
            yield return StartCoroutine(CheckServerHealth(TTSServerUrl + "/health", (running) => ttsServerRunning = running));
        }
        
        private IEnumerator CheckServerHealth(string healthUrl, Action<bool> callback)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(healthUrl))
            {
                request.timeout = 2;
                yield return request.SendWebRequest();
                
                bool isRunning = request.result == UnityWebRequest.Result.Success;
                callback(isRunning);
                
                if (isRunning)
                {
                    UnityEngine.Debug.Log($"[VoiceServerManager] Server already running at {healthUrl}");
                }
            }
        }
        
        private IEnumerator StartWhisperServer()
        {
            UnityEngine.Debug.Log("[VoiceServerManager] Starting Whisper server...");
            
            string pythonPath = GetAbsolutePath(pythonExecutablePath);
            string scriptPath = GetAbsolutePath(whisperServerScript);
            
            if (!File.Exists(pythonPath))
            {
                UnityEngine.Debug.LogError($"[VoiceServerManager] Python executable not found: {pythonPath}");
                yield break;
            }
            
            if (!File.Exists(scriptPath))
            {
                UnityEngine.Debug.LogError($"[VoiceServerManager] Whisper server script not found: {scriptPath}");
                yield break;
            }
            
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = $"\"{scriptPath}\" --port {whisperServerPort}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(scriptPath)
                };
                
                whisperProcess = Process.Start(startInfo);
                
                if (whisperProcess != null)
                {
                    UnityEngine.Debug.Log($"[VoiceServerManager] Whisper server started (PID: {whisperProcess.Id})");
                }
                else
                {
                    UnityEngine.Debug.LogError("[VoiceServerManager] Failed to start Whisper server process");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[VoiceServerManager] Error starting Whisper server: {e.Message}");
            }
            
            yield return null;
        }
        
        private IEnumerator StartTTSServer()
        {
            UnityEngine.Debug.Log("[VoiceServerManager] Starting TTS server...");
            
            string pythonPath = GetAbsolutePath(pythonExecutablePath);
            string scriptPath = GetAbsolutePath(ttsServerScript);
            
            if (!File.Exists(pythonPath))
            {
                UnityEngine.Debug.LogError($"[VoiceServerManager] Python executable not found: {pythonPath}");
                yield break;
            }
            
            if (!File.Exists(scriptPath))
            {
                UnityEngine.Debug.LogError($"[VoiceServerManager] TTS server script not found: {scriptPath}");
                yield break;
            }
            
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = $"\"{scriptPath}\" --port {ttsServerPort}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(scriptPath)
                };
                
                ttsProcess = Process.Start(startInfo);
                
                if (ttsProcess != null)
                {
                    UnityEngine.Debug.Log($"[VoiceServerManager] TTS server started (PID: {ttsProcess.Id})");
                }
                else
                {
                    UnityEngine.Debug.LogError("[VoiceServerManager] Failed to start TTS server process");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[VoiceServerManager] Error starting TTS server: {e.Message}");
            }
            
            yield return null;
        }
        
        private IEnumerator WaitForServersReady()
        {
            int maxAttempts = 30; // 30 seconds timeout
            int attempts = 0;
            
            while (attempts < maxAttempts && (!whisperServerRunning || !ttsServerRunning))
            {
                yield return new WaitForSeconds(1f);
                attempts++;
                
                // Check server health
                if (!whisperServerRunning)
                {
                    yield return StartCoroutine(CheckServerHealth(WhisperServerUrl + "/health", (running) => whisperServerRunning = running));
                }
                
                if (!ttsServerRunning)
                {
                    yield return StartCoroutine(CheckServerHealth(TTSServerUrl + "/health", (running) => ttsServerRunning = running));
                }
                
                UnityEngine.Debug.Log($"[VoiceServerManager] Waiting for servers... ({attempts}/{maxAttempts}) Whisper: {whisperServerRunning}, TTS: {ttsServerRunning}");
            }
        }
        
        public void StopAllServers()
        {
            UnityEngine.Debug.Log("[VoiceServerManager] Stopping voice servers...");
            
            try
            {
                if (whisperProcess != null && !whisperProcess.HasExited)
                {
                    whisperProcess.Kill();
                    whisperProcess.Dispose();
                    whisperProcess = null;
                    UnityEngine.Debug.Log("[VoiceServerManager] Whisper server stopped");
                }
                
                if (ttsProcess != null && !ttsProcess.HasExited)
                {
                    ttsProcess.Kill();
                    ttsProcess.Dispose();
                    ttsProcess = null;
                    UnityEngine.Debug.Log("[VoiceServerManager] TTS server stopped");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[VoiceServerManager] Error stopping servers: {e.Message}");
            }
            
            whisperServerRunning = false;
            ttsServerRunning = false;
            serversInitialized = false;
        }
        
        private string GetAbsolutePath(string relativePath)
        {
            string appPath = Application.dataPath;
            // Go up one level from Assets to get the app root
            string appRoot = Directory.GetParent(appPath).FullName;
            return Path.Combine(appRoot, relativePath);
        }
        
        public bool AreServersReady()
        {
            return serversInitialized && whisperServerRunning && ttsServerRunning;
        }
        
        [ContextMenu("Start Servers")]
        public void StartServersManually()
        {
            StartCoroutine(InitializeServers());
        }
        
        [ContextMenu("Stop Servers")]
        public void StopServersManually()
        {
            StopAllServers();
        }
        
        [ContextMenu("Check Server Status")]
        public void CheckServerStatus()
        {
            StartCoroutine(CheckExistingServers());
        }
    }
}
