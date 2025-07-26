# Local Voice System Documentation

## Overview

The Mate Engine now includes a complete local voice processing system that provides real-time voice interaction with your avatar using AI models that run entirely on your machine. This eliminates the need for cloud APIs and provides enhanced privacy.

## What's New

### Local Voice Processing
- **Whisper Integration**: Local speech-to-text using OpenAI Whisper
- **Coqui TTS**: High-quality local text-to-speech synthesis  
- **Automated Setup**: Python servers start automatically with Unity
- **No Cloud Dependencies**: Everything runs locally (except for LLM chat which can use cloud or local models)

### Key Benefits
- ✅ **Privacy**: No audio data sent to cloud services
- ✅ **Cost**: No API costs for voice processing
- ✅ **Speed**: Local processing can be faster than cloud APIs
- ✅ **Reliability**: Works offline for voice processing
- ✅ **Quality**: High-quality voice synthesis with multiple models

## Quick Setup

### Automated Installation

1. **Run the setup script:**
   ```bash
   # Windows
   setup_voice_system.bat
   
   # Linux/macOS  
   ./setup_voice_system.sh
   ```

2. **Launch Unity application**
   - Voice servers start automatically
   - Wait for "Voice servers ready!" message
   - Press **V** to start talking!

### Manual Installation

If you prefer manual setup:

1. **Install Python 3.8+** from [python.org](https://python.org)

2. **Create virtual environment:**
   ```bash
   python -m venv venv
   source venv/bin/activate  # Linux/macOS
   venv\Scripts\activate.bat  # Windows
   ```

3. **Install dependencies:**
   ```bash
   cd WhisperServer && pip install -r requirements.txt && cd ..
   cd TTSServer && pip install -r requirements.txt && cd ..
   ```

## How It Works

### Architecture

```
Unity Application
├── VoiceServerManager (manages Python servers)
├── LocalVoiceProcessor (handles HTTP communication)
├── VoiceInteractionManager (coordinates everything)
└── Python Servers (run in background)
    ├── Whisper Server (port 8001) - Speech to Text
    └── TTS Server (port 8002) - Text to Speech
```

### Voice Interaction Flow

1. **User presses V** → Unity starts recording audio
2. **User speaks** → Audio is captured via microphone
3. **User releases V** → Audio sent to local Whisper server
4. **Whisper processes** → Returns transcribed text
5. **Unity sends text** → To LLM (cloud or local) for AI response
6. **AI responds** → Text response received
7. **Text sent to TTS** → Local TTS server generates speech
8. **Avatar speaks** → Audio played with lip sync animations

## Configuration

### Voice System Settings

In Unity, configure these settings:

**VoiceInteractionManager:**
- `useLocalVoiceProcessing`: Enable/disable local processing
- `voiceServerManager`: Reference to server manager
- `localVoiceProcessor`: Reference to voice processor

**VoiceServerManager:**
- `whisperServerPort`: Port for Whisper server (default: 8001)
- `ttsServerPort`: Port for TTS server (default: 8002)
- `autoStartServers`: Start servers automatically (default: true)
- `pythonExecutablePath`: Path to Python executable

### Model Selection

**Whisper Models** (edit `WhisperServer/whisper_server.py`):
- `tiny`: Fast, basic quality (39MB)
- `base`: Balanced speed/quality (74MB) - **Recommended**
- `small`: Better quality (244MB)
- `medium`: High quality (769MB)
- `large`: Best quality (1550MB)

**TTS Models** (edit `TTSServer/tts_server.py`):
- `tacotron2-DDC`: Default, good quality
- `glow-tts`: Faster alternative
- `vits`: Higher quality option

## Performance Optimization

### For Faster Processing
- Use `base` or `tiny` Whisper models
- Enable GPU acceleration if available
- Close unnecessary applications
- Use SSD storage for model files

### For Better Quality  
- Use `small` or `medium` Whisper models
- Use higher-quality TTS models
- Ensure adequate RAM (8GB+)
- Use dedicated GPU for processing

### GPU Acceleration

For NVIDIA GPUs with CUDA:

1. **Install CUDA toolkit** (version 11.8 or 12.x)
2. **Install PyTorch with CUDA:**
   ```bash
   pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
   ```
3. **Verify GPU usage:**
   ```python
   import torch
   print(torch.cuda.is_available())  # Should return True
   ```

## Troubleshooting

### Common Issues

**"Voice servers failed to start"**
- Check if Python is installed and in PATH
- Verify ports 8001/8002 are not in use
- Run setup script again
- Check antivirus blocking Python processes

**"No speech detected"**
- Check microphone permissions in OS settings
- Test microphone in other applications
- Adjust silence threshold in Unity settings
- Ensure microphone is not muted

**"Python not found"**
- Install Python from python.org
- Ensure Python is added to system PATH
- Try using full path to python.exe in VoiceServerManager

**"Slow processing"**
- Use smaller Whisper model (base or tiny)
- Enable GPU acceleration
- Increase system RAM
- Close other applications

**"Poor audio quality"**
- Check microphone levels and quality
- Use external USB microphone
- Reduce background noise
- Adjust Unity audio settings

### Server Debugging

**Check server status:**
- Whisper health: http://localhost:8001/health
- TTS health: http://localhost:8002/health

**Manual server testing:**
```bash
# Test Whisper server
python WhisperServer/whisper_server.py --port 8001

# Test TTS server  
python TTSServer/tts_server.py --port 8002
```

**View server logs:**
- Check Unity console for server startup messages
- Python servers output logs to console
- Look for error messages and stack traces

## System Requirements

### Minimum
- **OS**: Windows 10, macOS 10.15, Ubuntu 18.04+
- **RAM**: 4GB (8GB recommended)
- **Storage**: 2GB free space for AI models
- **Python**: 3.8 or higher
- **Internet**: Only for LLM chat (not voice processing)

### Recommended
- **RAM**: 16GB for better performance
- **GPU**: NVIDIA GPU with CUDA support
- **Storage**: SSD for faster model loading
- **CPU**: Multi-core processor for better performance

## Advanced Configuration

### Custom Ports

If default ports conflict, modify in Unity:

```csharp
// VoiceServerManager.cs
public string whisperServerPort = "8001";  // Change as needed
public string ttsServerPort = "8002";      // Change as needed
```

### Custom Models

**Switch Whisper model:**
```python
# In whisper_server.py
model_name = "small"  # Change from "base" to desired model
```

**Switch TTS model:**
```python
# In tts_server.py  
model_name = "tts_models/en/ljspeech/glow-tts"  # Change model
```

### Local LLM Integration

For complete offline operation, you can integrate local LLMs:

1. **Use Ollama** for local LLM hosting
2. **Modify Unity** to send chat requests to local endpoint
3. **Configure** VoiceInteractionManager to use local URL

## Privacy & Security

### Data Handling
- Audio is processed locally and not stored
- Temporary files are automatically cleaned up
- No audio data sent to cloud services
- Text conversations may use cloud LLMs (optional)

### Network Traffic
- Only LLM chat requests use internet (if using cloud LLMs)
- Voice processing is completely local
- No telemetry or analytics data sent

## Migration from Cloud

If upgrading from cloud-based voice:

1. **Run setup script** to install local components
2. **Enable local processing** in VoiceInteractionManager
3. **Test functionality** with voice interaction
4. **Optional**: Keep cloud as fallback by setting `useLocalVoiceProcessing = false`

## Deployment

When distributing your application:

1. **Include Python servers** in your build
2. **Add setup scripts** for easy installation
3. **Document requirements** for end users
4. **Test on clean systems** before release

### Distribution Checklist
- [ ] WhisperServer/ folder included
- [ ] TTSServer/ folder included  
- [ ] setup_voice_system scripts included
- [ ] VOICE_SETUP_GUIDE.md included
- [ ] Requirements documented
- [ ] Tested on target platforms

---

**Note**: Initial setup downloads AI models which may take several minutes. Subsequent launches are much faster as models are cached locally.
