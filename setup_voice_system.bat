@echo off
REM Automated setup script for Mate Engine Voice System
REM This script installs Python dependencies and sets up the voice servers

echo ================================================
echo Mate Engine Voice System Setup
echo ================================================

REM Check for optimal Python version for TTS compatibility  
REM TTS requires Python >=3.9.0, <3.12, so Python 3.11 is optimal
set PYTHON_CMD=python

REM Check for Python 3.11 first (optimal for TTS)
python3.11 --version >nul 2>&1
if %errorlevel% equ 0 (
    echo Python 3.11 found - optimal for TTS compatibility
    set PYTHON_CMD=python3.11
    goto :version_found
)

REM Check for default Python
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.11 from python.org for optimal TTS compatibility
    pause
    exit /b 1
)

:version_found
echo Using Python command: %PYTHON_CMD%
%PYTHON_CMD% -c "import sys; print(f'Python {sys.version_info.major}.{sys.version_info.minor}.{sys.version_info.micro}'); exit(1 if sys.version_info >= (3, 12) else 0)"
if %errorlevel% neq 0 (
    echo WARNING: Python 3.12+ detected. TTS requires Python 3.9-3.11
    echo Consider installing Python 3.11 for full compatibility
    echo Proceeding anyway...
)

REM Create virtual environment if it doesn't exist
if not exist "venv" (
    echo Creating virtual environment with compatible Python...
    %PYTHON_CMD% -m venv venv
    if %errorlevel% neq 0 (
        echo ERROR: Failed to create virtual environment
        pause
        exit /b 1
    )
)

REM Activate virtual environment
echo Activating virtual environment...
call venv\Scripts\activate.bat
if %errorlevel% neq 0 (
    echo ERROR: Failed to activate virtual environment
    pause
    exit /b 1
)

REM Upgrade pip
echo Upgrading pip...
python -m pip install --upgrade pip

REM Install Whisper server dependencies
echo ================================================
echo Installing Whisper Server Dependencies...
echo ================================================
cd WhisperServer
pip install -r requirements.txt
if %errorlevel% neq 0 (
    echo ERROR: Failed to install Whisper dependencies
    cd ..
    pause
    exit /b 1
)
cd ..

REM Install TTS server dependencies
echo ================================================
echo Installing TTS Server Dependencies...
echo ================================================
cd TTSServer
pip install -r requirements.txt
if %errorlevel% neq 0 (
    echo ERROR: Failed to install TTS dependencies
    cd ..
    pause
    exit /b 1
)
cd ..

REM Test installations
echo ================================================
echo Testing Installations...
echo ================================================

echo Testing Whisper installation...
python -c "import whisper; print('Whisper: OK')"
if %errorlevel% neq 0 (
    echo WARNING: Whisper import test failed
)

echo Testing TTS installation...
python -c "import TTS; print('TTS: OK')"
if %errorlevel% neq 0 (
    echo WARNING: TTS import test failed
)

echo Testing Flask installation...
python -c "import flask; print('Flask: OK')"
if %errorlevel% neq 0 (
    echo WARNING: Flask import test failed
)

echo ================================================
echo Setup Complete!
echo ================================================
echo.
echo Voice servers are ready to use.
echo The Unity application will automatically start them when needed.
echo.
echo Manual server testing:
echo - Whisper: python WhisperServer/whisper_server.py
echo - TTS: python TTSServer/tts_server.py
echo.
pause
