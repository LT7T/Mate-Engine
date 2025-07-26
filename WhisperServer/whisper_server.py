#!/usr/bin/env python3
"""
Local Whisper Server for Unity Voice Processing
Provides speech-to-text functionality using OpenAI Whisper
"""

import os
import sys
import argparse
import logging
import tempfile
from flask import Flask, request, jsonify
from flask_cors import CORS
import whisper
import numpy as np
import io
import wave

# Setup logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = Flask(__name__)
CORS(app)  # Enable CORS for Unity requests

# Global model variable
model = None

def load_whisper_model(model_name="base"):
    """Load Whisper model"""
    global model
    try:
        logger.info(f"Loading Whisper model: {model_name}")
        model = whisper.load_model(model_name)
        logger.info("Whisper model loaded successfully")
        return True
    except Exception as e:
        logger.error(f"Failed to load Whisper model: {e}")
        return False

@app.route('/health', methods=['GET'])
def health():
    """Health check endpoint"""
    return jsonify({
        "status": "healthy",
        "model_loaded": model is not None,
        "service": "whisper-stt"
    })

@app.route('/transcribe', methods=['POST'])
def transcribe():
    """Transcribe audio to text"""
    try:
        if model is None:
            return jsonify({"error": "Model not loaded"}), 500
            
        # Check if audio file is in request
        if 'audio' not in request.files:
            return jsonify({"error": "No audio file provided"}), 400
            
        audio_file = request.files['audio']
        if audio_file.filename == '':
            return jsonify({"error": "No audio file selected"}), 400
        
        # Get response format (default to json)
        response_format = request.form.get('response_format', 'json')
        
        # Save audio to temporary file
        with tempfile.NamedTemporaryFile(delete=False, suffix='.wav') as tmp_file:
            audio_file.save(tmp_file.name)
            
            try:
                # Transcribe audio using Whisper
                logger.info(f"Transcribing audio file: {tmp_file.name}")
                result = model.transcribe(tmp_file.name)
                
                # Clean up temporary file
                os.unlink(tmp_file.name)
                
                # Return response based on format
                if response_format == 'json':
                    return jsonify({
                        "text": result["text"],
                        "language": result.get("language", "en"),
                        "duration": result.get("duration", 0),
                        "segments": result.get("segments", [])
                    })
                else:
                    # Return plain text
                    return result["text"], 200, {'Content-Type': 'text/plain'}
                    
            except Exception as e:
                logger.error(f"Transcription error: {e}")
                # Clean up temporary file on error
                if os.path.exists(tmp_file.name):
                    os.unlink(tmp_file.name)
                return jsonify({"error": f"Transcription failed: {str(e)}"}), 500
                
    except Exception as e:
        logger.error(f"Request processing error: {e}")
        return jsonify({"error": f"Request processing failed: {str(e)}"}), 500

@app.route('/models', methods=['GET'])
def get_models():
    """Get available Whisper models"""
    available_models = [
        "tiny", "tiny.en", "base", "base.en", 
        "small", "small.en", "medium", "medium.en", 
        "large", "large-v1", "large-v2", "large-v3"
    ]
    return jsonify({
        "models": available_models,
        "current_model": getattr(model, 'name', None) if model else None
    })

def main():
    parser = argparse.ArgumentParser(description='Local Whisper Server')
    parser.add_argument('--port', type=int, default=8001, help='Port to run the server on')
    parser.add_argument('--host', type=str, default='127.0.0.1', help='Host to bind the server to')
    parser.add_argument('--model', type=str, default='base', help='Whisper model to use')
    parser.add_argument('--debug', action='store_true', help='Enable debug mode')
    
    args = parser.parse_args()
    
    # Load Whisper model
    if not load_whisper_model(args.model):
        logger.error("Failed to load Whisper model. Exiting.")
        sys.exit(1)
    
    logger.info(f"Starting Whisper server on {args.host}:{args.port}")
    logger.info(f"Using model: {args.model}")
    
    try:
        app.run(host=args.host, port=args.port, debug=args.debug, threaded=True)
    except Exception as e:
        logger.error(f"Server error: {e}")
        sys.exit(1)

if __name__ == '__main__':
    main()
