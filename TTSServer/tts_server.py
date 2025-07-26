#!/usr/bin/env python3
"""
Local TTS Server for Unity Voice Processing
Provides text-to-speech functionality using Coqui TTS
"""

import os
import sys
import argparse
import logging
import tempfile
import io
from flask import Flask, request, jsonify, send_file
from flask_cors import CORS
import torch
from TTS.api import TTS
import numpy as np
import wave
import soundfile as sf

# Setup logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = Flask(__name__)
CORS(app)  # Enable CORS for Unity requests

# Global TTS model variable
tts_model = None

def load_tts_model():
    """Load TTS model"""
    global tts_model
    try:
        logger.info("Loading TTS model...")
        
        # Use a fast, high-quality model that works well for English
        # You can change this to other models as needed
        model_name = "tts_models/en/ljspeech/tacotron2-DDC"
        
        # Check if CUDA is available
        device = "cuda" if torch.cuda.is_available() else "cpu"
        logger.info(f"Using device: {device}")
        
        tts_model = TTS(model_name=model_name).to(device)
        logger.info("TTS model loaded successfully")
        return True
        
    except Exception as e:
        logger.error(f"Failed to load TTS model: {e}")
        # Fallback to a simpler model
        try:
            logger.info("Trying fallback model...")
            tts_model = TTS(model_name="tts_models/en/ljspeech/glow-tts")
            logger.info("Fallback TTS model loaded successfully")
            return True
        except Exception as e2:
            logger.error(f"Fallback model also failed: {e2}")
            return False

@app.route('/health', methods=['GET'])
def health():
    """Health check endpoint"""
    return jsonify({
        "status": "healthy",
        "model_loaded": tts_model is not None,
        "service": "coqui-tts",
        "cuda_available": torch.cuda.is_available()
    })

@app.route('/synthesize', methods=['POST'])
def synthesize():
    """Synthesize speech from text"""
    try:
        if tts_model is None:
            return jsonify({"error": "TTS model not loaded"}), 500
        
        # Get request data
        data = request.get_json()
        if not data or 'text' not in data:
            return jsonify({"error": "No text provided"}), 400
        
        text = data['text']
        if not text.strip():
            return jsonify({"error": "Empty text provided"}), 400
        
        # Get optional parameters
        response_format = data.get('response_format', 'wav')
        speed = data.get('speed', 1.0)
        
        logger.info(f"Synthesizing text: '{text[:50]}{'...' if len(text) > 50 else ''}'")
        
        # Generate speech
        try:
            # Create temporary file for output
            with tempfile.NamedTemporaryFile(delete=False, suffix='.wav') as tmp_file:
                tmp_filename = tmp_file.name
            
            # Synthesize speech to file
            tts_model.tts_to_file(text=text, file_path=tmp_filename)
            
            # Read the generated audio file
            audio_data, sample_rate = sf.read(tmp_filename)
            
            # Clean up temporary file
            os.unlink(tmp_filename)
            
            # Apply speed adjustment if needed
            if speed != 1.0:
                # Simple speed adjustment by resampling
                import librosa
                audio_data = librosa.effects.time_stretch(audio_data, rate=speed)
            
            # Convert to the requested format
            if response_format.lower() == 'wav':
                # Create WAV file in memory
                wav_buffer = io.BytesIO()
                sf.write(wav_buffer, audio_data, sample_rate, format='WAV')
                wav_buffer.seek(0)
                
                return send_file(
                    wav_buffer,
                    mimetype='audio/wav',
                    as_attachment=True,
                    download_name='speech.wav'
                )
            else:
                return jsonify({"error": f"Unsupported format: {response_format}"}), 400
                
        except Exception as e:
            logger.error(f"Speech synthesis error: {e}")
            return jsonify({"error": f"Speech synthesis failed: {str(e)}"}), 500
            
    except Exception as e:
        logger.error(f"Request processing error: {e}")
        return jsonify({"error": f"Request processing failed: {str(e)}"}), 500

@app.route('/voices', methods=['GET'])
def get_voices():
    """Get available voices (placeholder - Coqui TTS models are pre-trained)"""
    return jsonify({
        "voices": ["default"],
        "current_voice": "default",
        "note": "Voice selection depends on the loaded TTS model"
    })

@app.route('/models', methods=['GET'])
def get_models():
    """Get available TTS models"""
    # This is a subset of available models - you can extend this list
    available_models = [
        "tts_models/en/ljspeech/tacotron2-DDC",
        "tts_models/en/ljspeech/glow-tts",
        "tts_models/en/ljspeech/speedy-speech",
        "tts_models/en/ljspeech/tacotron2-DCA",
        "tts_models/en/vctk/vits",
        "tts_models/en/sam/tacotron-DDC"
    ]
    
    current_model = getattr(tts_model, 'model_name', 'unknown') if tts_model else None
    
    return jsonify({
        "models": available_models,
        "current_model": current_model
    })

def main():
    parser = argparse.ArgumentParser(description='Local TTS Server')
    parser.add_argument('--port', type=int, default=8002, help='Port to run the server on')
    parser.add_argument('--host', type=str, default='127.0.0.1', help='Host to bind the server to')
    parser.add_argument('--debug', action='store_true', help='Enable debug mode')
    
    args = parser.parse_args()
    
    # Load TTS model
    if not load_tts_model():
        logger.error("Failed to load TTS model. Exiting.")
        sys.exit(1)
    
    logger.info(f"Starting TTS server on {args.host}:{args.port}")
    
    try:
        app.run(host=args.host, port=args.port, debug=args.debug, threaded=True)
    except Exception as e:
        logger.error(f"Server error: {e}")
        sys.exit(1)

if __name__ == '__main__':
    main()
