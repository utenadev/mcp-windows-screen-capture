# Whisper Speech Recognition

The server integrates OpenAI's Whisper model via [Whisper.net](https://github.com/terryno/Whisper.net) for local high-quality speech-to-text.

## Supported Models

Models are automatically downloaded from Hugging Face on first use.

| Model | Size | Best For |
|-------|------|----------|
| **Tiny** | 39 MB | Fast preview, low accuracy |
| **Base** | 74 MB | Recommended default |
| **Small** | 244 MB | High quality (Good for Japanese) |
| **Medium** | 769 MB | File processing, slow |
| **Large** | 1550 MB | Maximum accuracy, very slow |

## Features

- **Local Processing**: Audio never leaves your machine (except for the initial model download).
- **Auto Language Detection**: Whisper can detect the spoken language automatically.
- **Translation**: Can translate non-English speech directly to English.
- **System Audio Support**: Can transcribe whatever is playing on your Windows machine (YouTube, meetings, etc.).

## Performance Tips

- **CPU Only**: Currently, this implementation uses the CPU version of Whisper.net.
- **Japanese Transcription**: The `small` model provides significantly better results for Japanese than the `base` model.
- **Long Audio**: For audio longer than 30 seconds, consider using `start_audio_capture` followed by `listen` with the `audio_session` source.
