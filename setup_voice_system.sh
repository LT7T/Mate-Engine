#!/bin/bash
# Automated setup script for Mate Engine Voice System (Linux/macOS)
# This script installs Python dependencies and sets up the voice servers

echo "================================================"
echo "Mate Engine Voice System Setup"
echo "================================================"

# Check if Python is available
if ! command -v python3 &> /dev/null; then
    echo "ERROR: Python3 is not installed or not in PATH"
    echo "Please install Python 3.8+ from python.org or your package manager"
    exit 1
fi

echo "Python found. Checking version..."
python3 -c "import sys; print(f'Python {sys.version_info.major}.{sys.version_info.minor}.{sys.version_info.micro}')"

# Create virtual environment if it doesn't exist
if [ ! -d "venv" ]; then
    echo "Creating Python virtual environment..."
    python3 -m venv venv
    if [ $? -ne 0 ]; then
        echo "ERROR: Failed to create virtual environment"
        exit 1
    fi
fi

# Activate virtual environment
echo "Activating virtual environment..."
source venv/bin/activate
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to activate virtual environment"
    exit 1
fi

# Upgrade pip
echo "Upgrading pip..."
python -m pip install --upgrade pip

# Install Whisper server dependencies
echo "================================================"
echo "Installing Whisper Server Dependencies..."
echo "================================================"
cd WhisperServer
pip install -r requirements.txt
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to install Whisper dependencies"
    cd ..
    exit 1
fi
cd ..

# Install TTS server dependencies
echo "================================================"
echo "Installing TTS Server Dependencies..."
echo "================================================"
cd TTSServer
pip install -r requirements.txt
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to install TTS dependencies"
    cd ..
    exit 1
fi
cd ..

# Test installations
echo "================================================"
echo "Testing Installations..."
echo "================================================"

echo "Testing Whisper installation..."
python -c "import whisper; print('Whisper: OK')" 2>/dev/null
if [ $? -ne 0 ]; then
    echo "WARNING: Whisper import test failed"
fi

echo "Testing TTS installation..."
python -c "import TTS; print('TTS: OK')" 2>/dev/null
if [ $? -ne 0 ]; then
    echo "WARNING: TTS import test failed"
fi

echo "Testing Flask installation..."
python -c "import flask; print('Flask: OK')" 2>/dev/null
if [ $? -ne 0 ]; then
    echo "WARNING: Flask import test failed"
fi

echo "================================================"
echo "Setup Complete!"
echo "================================================"
echo ""
echo "Voice servers are ready to use."
echo "The Unity application will automatically start them when needed."
echo ""
echo "Manual server testing:"
echo "- Whisper: python WhisperServer/whisper_server.py"
echo "- TTS: python TTSServer/tts_server.py"
echo ""
