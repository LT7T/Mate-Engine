# Quick Setup Guide for AI Voice Interaction

## 🎯 Quick Start (5 minutes)

### Step 1: Get an API Key
1. Visit [OpenRouter.ai](https://openrouter.ai) and create an account
2. Generate an API key from your dashboard
3. Copy the API key (starts with `sk-or-...`)

### Step 2: Enable Voice in Mate Engine
1. Run Mate Engine
2. Right-click your avatar or press `M` key to open menu
3. Look for "Voice Settings" or add the voice system using setup script

### Step 3: Configure API
1. In Voice Settings, paste your API key
2. Select model: `anthropic/claude-3.5-sonnet` (recommended)
3. Test with the "Test Voice" button

### Step 4: Start Talking!
1. Press and hold `V` key to talk
2. Speak clearly: "Hello, how are you today?"
3. Release `V` and wait for AI response
4. Your avatar will respond with voice and animations!

---

## 🛠️ Unity Setup (For Developers)

### Automatic Setup
1. Add `VoiceSystemSetup.cs` to any GameObject in your scene
2. Set `autoSetupOnStart = true`
3. Play the scene - voice system auto-configures!

### Manual Setup
1. Add `VoiceInteractionManager` to your avatar GameObject
2. Assign AudioSource for voice responses
3. Add `VoiceInteractionUI` to your Canvas
4. Add `VoiceMenuIntegration` to your menu system

---

## 🎮 Controls

- **V Key**: Default voice activation (hold to talk)
- **Settings**: Accessible via right-click menu
- **Push-to-Talk**: Use UI button for mouse control
- **Toggle Mode**: Enable in settings for press-once activation

---

## 🔧 Configuration Options

### Basic Settings
- **API Key**: Your OpenRouter/OpenAI key
- **Model**: AI model (Claude 3.5 Sonnet recommended)
- **Volume**: Response audio level
- **Hold to Talk**: Toggle between hold/press modes

### Advanced Settings  
- **System Prompt**: Customize AI personality
- **Temperature**: Creativity level (0.7 recommended)
- **Max Tokens**: Response length (150 recommended)
- **Silence Detection**: Auto-stop recording sensitivity

---

## 💰 Cost Information

### Recommended Models & Costs
- **Claude 3.5 Sonnet**: ~$0.003 per conversation
- **GPT-4o Mini**: ~$0.0003 per conversation  
- **GPT-4o**: ~$0.0075 per conversation

### Usage Tips
- Use shorter system prompts to reduce costs
- Choose Mini models for basic conversations
- Set max_tokens to 150 or less for brief responses

---

## 🔍 Troubleshooting

### "No microphone detected"
- Check Windows Privacy Settings → Microphone
- Ensure Unity has microphone permissions
- Try different microphone device in Windows settings

### "API key invalid" 
- Verify key format: OpenRouter keys start with `sk-or-`
- Check account balance at OpenRouter.ai
- Ensure internet connectivity

### "No voice response"
- Check audio output device
- Verify TTS model in settings
- Test audio with volume slider

### "Speech not recognized"
- Speak clearly and close to microphone
- Check background noise levels
- Adjust silence threshold in advanced settings

---

## 🧪 Testing Your Setup

### Quick Test
1. Add `VoiceInteractionTester` component to any GameObject
2. Press Play in Unity
3. Use the test panel to validate all systems

### Test Checklist
- ✅ Microphone access working
- ✅ API connection successful  
- ✅ Audio processing functional
- ✅ UI integration complete
- ✅ Voice response playing

---

## 🚀 Advanced Features

### Avatar Animation Integration
- Automatically triggers `isListening` and `isTalking` animator parameters
- Supports VRM blendshape mouth movement
- Visual feedback during conversation

### Custom Voice Commands
- Extend with custom command handlers
- Integrate with existing avatar features
- Add mod support for voice commands

### Multiple Languages
- System supports any language the AI model understands
- Configure system prompt in target language
- TTS supports multiple voices and languages

---

## 📞 Support

If you encounter issues:
1. Check the documentation: `VoiceInteractionDocumentation.md`
2. Run the test suite: `VoiceInteractionTester`
3. Review Unity console for error messages
4. Verify all components are properly assigned

For additional help, include the following in support requests:
- Unity version
- Error messages from console
- API model being used
- Test results from VoiceInteractionTester

---

## 🎉 Enjoy Your AI Companion!

Your Mate Engine avatar can now:
- 🗣️ Have natural conversations
- 🎭 Show personality and emotions  
- 🤝 Be a helpful desktop companion
- 🎵 Respond with voice and animations
- ⚙️ Learn from your preferences

Press `V` and say "Hello!" to get started! 🚀