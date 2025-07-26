#!/bin/bash
# Automated setup script for Mate Engine Voice System (Linux/macOS)
# This script installs Python dependencies and sets up the voice servers

echo "================================================"
echo "Mate Engine Voice System Setup"
echo "================================================"

# Check for optimal Python version for TTS compatibility
# TTS requires Python >=3.9.0, <3.12, so Python 3.11 is optimal
PYTHON_CMD=""

if command -v conda &> /dev/null; then
    echo "Conda detected. Checking for Python 3.11 environment..."
    if conda env list | grep -q "voice-py311"; then
        echo "Using existing Python 3.11 conda environment..."
        PYTHON_CMD="conda run -n voice-py311 python"
    else
        echo "Creating Python 3.11 conda environment for TTS compatibility..."
        conda create -n voice-py311 python=3.11 -y
        PYTHON_CMD="conda run -n voice-py311 python"
    fi
elif command -v python3.11 &> /dev/null; then
    echo "Python 3.11 found locally..."
    PYTHON_CMD="python3.11"
elif command -v python3 &> /dev/null; then
    echo "Python3 found. Checking version compatibility..."
    VERSION=$(python3 -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')")
    if python3 -c "import sys; exit(0 if (sys.version_info.major == 3 and 9 <= sys.version_info.minor < 12) else 1)"; then
        echo "Python $VERSION is compatible with TTS"
        PYTHON_CMD="python3"
    else
        echo "WARNING: Python $VERSION may not be compatible with TTS (requires 3.9-3.11)"
        echo "Proceeding anyway, but TTS installation may fail..."
        PYTHON_CMD="python3"
    fi
else
    echo "ERROR: No compatible Python found"
    echo "Please install Python 3.11 for optimal TTS compatibility"
    exit 1
fi

echo "Using Python command: $PYTHON_CMD"
$PYTHON_CMD -c "import sys; print(f'Python {sys.version_info.major}.{sys.version_info.minor}.{sys.version_info.micro}')"

# Create virtual environment if it doesn't exist
if [ ! -d "venv" ]; then
    echo "Creating virtual environment with compatible Python..."
    $PYTHON_CMD -m venv venv
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
