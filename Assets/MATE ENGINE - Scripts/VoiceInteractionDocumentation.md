# Voice Interaction System for Mate Engine

## Overview

This voice interaction system adds AI-powered voice capabilities to Mate Engine, allowing real-time voice conversation with your desktop avatar. The system supports OpenRouter API for LLM processing while using OpenAI SDK for compatibility.

## Features

- **Speech-to-Text**: Voice input using Unity's microphone and OpenAI Whisper API
- **AI Chat**: Real-time conversation using OpenRouter API models (Claude, GPT-4, Llama, etc.)
- **Text-to-Speech**: Natural voice responses using OpenAI TTS API
- **Visual Feedback**: Avatar animations and UI indicators during voice interaction
- **Menu Integration**: Settings panel integrated into existing Mate Engine menu system
- **Multiple Input Modes**: Hold-to-talk or voice activation
- **Audio Processing**: Automatic silence trimming and noise reduction

## Setup Instructions

### 1. API Configuration

1. **Get an API Key**:
   - Sign up at [OpenRouter.ai](https://openrouter.ai) for access to multiple LLM models
   - Or use OpenAI API key directly

2. **Configure the System**:
   - Open Mate Engine
   - Right-click avatar or press `M` to open menu
   - Navigate to Voice Settings
   - Enter your API key
   - Select preferred model (Claude 3.5 Sonnet recommended)

### 2. Unity Setup

1. **Add Voice Components**:
   - Add `VoiceInteractionManager` to your main avatar GameObject
   - Add `VoiceInteractionUI` to your UI Canvas
   - Add `VoiceMenuIntegration` to your menu system

2. **Configure Audio**:
   - Assign an AudioSource for voice responses
   - Set up microphone permissions in project settings
   - Configure audio visualization (optional)

### 3. Avatar Integration

1. **Animator Setup**:
   - Add boolean parameters: `isListening`, `isTalking`
   - Create animation states for listening and talking
   - Connect to avatar mouth movements if available

2. **Blendshape Integration** (VRM models):
   - Assign `UniversalBlendshapes` component reference
   - Configure mouth movement during speech

## Usage

### Basic Voice Interaction

1. **Default Mode**: Press and hold `V` key to talk, release to send
2. **Toggle Mode**: Press `V` once to start recording, press again to stop
3. **Push-to-Talk Button**: Use UI button for mouse-based interaction

### Voice Commands

The AI can respond to any conversational input. Example interactions:

- "How are you doing?"
- "What time is it?"
- "Tell me a joke"
- "What can you do?"
- "Change to dance animation"

### Settings Configuration

#### Basic Settings
- **API Key**: Your OpenRouter or OpenAI API key
- **Model**: Choose from available models (Claude, GPT-4, Llama, etc.)
- **Volume**: Response audio volume (0-1)
- **Hold to Talk**: Enable/disable push-to-talk mode

#### Advanced Settings
- **System Prompt**: Customize AI personality and behavior
- **Temperature**: AI creativity level (0-2)
- **Max Tokens**: Response length limit
- **Silence Threshold**: Audio level for voice detection
- **Recording Timeout**: Maximum recording duration

## API Models Supported

### Recommended Models
- `anthropic/claude-3.5-sonnet` - Best overall performance
- `openai/gpt-4o` - Excellent general purpose
- `openai/gpt-4o-mini` - Fast and cost-effective

### Available Models
- `anthropic/claude-3-haiku` - Fast responses
- `meta-llama/llama-3.1-8b-instruct` - Open source option
- `google/gemini-pro` - Google's latest model
- `mistralai/mistral-7b-instruct` - European option

## Technical Implementation

### Architecture

```
Voice Input → Microphone → Audio Processing → Whisper API
     ↓
AI Response ← LLM API ← Text Processing ← Speech-to-Text
     ↓
Audio Output ← TTS API ← Response Generation
```

### Key Components

1. **VoiceInteractionManager**: Core voice processing logic
2. **VoiceInteractionUI**: User interface and visual feedback
3. **VoiceMenuIntegration**: Menu system integration
4. **AudioUtils**: Audio format conversion and processing
5. **VoiceInteractionTester**: Testing and validation tools

### Error Handling

- Automatic fallback to system TTS if API fails
- Graceful handling of network connectivity issues
- Audio permission validation
- API rate limiting awareness

## Troubleshooting

### Common Issues

1. **No microphone detected**:
   - Check Windows privacy settings
   - Ensure microphone permissions are granted
   - Verify Unity audio settings

2. **API errors**:
   - Validate API key format
   - Check internet connectivity
   - Verify API usage limits

3. **No audio response**:
   - Check audio output device
   - Verify TTS API configuration
   - Test with audio fallback mode

4. **Poor speech recognition**:
   - Speak clearly and close to microphone
   - Adjust silence threshold settings
   - Check for background noise

### Debug Tools

Use the `VoiceInteractionTester` component to:
- Test microphone functionality
- Validate API connections
- Check audio processing pipeline
- Verify component integration

### Performance Optimization

- Use smaller models for faster responses
- Implement response caching for common queries
- Optimize audio processing settings
- Monitor API usage costs

## Integration with Existing Features

### Avatar Animations
- Listening state triggers `isListening` animator parameter
- Speaking state triggers `isTalking` animator parameter
- Mouth movement synchronized with audio playback

### Menu System
- Voice settings accessible through existing menu
- Quick toggle for voice activation
- Status indicators in main UI

### Mod Support
- Voice system respects existing mod framework
- Custom voice commands can be added via mods
- Audio processing can be extended

## Future Enhancements

### Planned Features
- Emotion detection in voice input
- Custom wake words
- Voice command shortcuts
- Multi-language support
- Local LLM support

### Extension Points
- Custom audio processors
- Additional TTS providers
- Voice command handlers
- Response generators

## API Usage and Costs

### Estimated Costs (OpenRouter)
- **Claude 3.5 Sonnet**: ~$0.003 per exchange
- **GPT-4o**: ~$0.0075 per exchange  
- **GPT-4o Mini**: ~$0.0003 per exchange

### Usage Optimization
- Set appropriate max_tokens limits
- Use cheaper models for simple interactions
- Implement conversation context management
- Cache common responses

## Security and Privacy

### Data Handling
- Voice recordings are processed securely via HTTPS
- No voice data is stored locally
- API communications are encrypted
- User data follows provider privacy policies

### Best Practices
- Keep API keys secure
- Don't share configuration files with keys
- Use environment variables for deployment
- Regular key rotation recommended

## License and Attribution

This voice interaction system is part of Mate Engine and follows the same licensing terms. It integrates with:

- **OpenAI API**: Text-to-speech and speech-to-text services
- **OpenRouter API**: Access to multiple LLM providers
- **Unity Audio**: Microphone and audio playback
- **Newtonsoft.Json**: JSON processing
- **NAudio**: Advanced audio processing (existing dependency)

## Support

For issues and support:
1. Check the troubleshooting section above
2. Test with the VoiceInteractionTester
3. Review Unity console for error messages
4. Verify API key and model configuration
5. Submit issues with detailed error logs

## Example Configuration

```json
{
  "apiKey": "your-openrouter-api-key",
  "baseUrl": "https://openrouter.ai/api/v1",
  "chatModel": "anthropic/claude-3.5-sonnet",
  "systemPrompt": "You are a friendly AI companion...",
  "temperature": 0.7,
  "maxTokens": 150,
  "holdToTalk": true,
  "responseVolume": 0.8
}
```

This configuration provides a good balance of performance, cost, and user experience.