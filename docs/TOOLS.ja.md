# MCP Windows Screen Capture ツール詳細

このドキュメントでは、本サーバーで利用可能な MCP ツールの詳細について説明します。

## ツール一覧

| カテゴリ | ツール名 | 説明 |
|----------|----------|------|
| **画面** | `list_monitors` | 利用可能なモニターの一覧を取得 |
| | `see` | モニターまたはウィンドウのスクリーンショットを撮影 |
| | `capture_region` | 画面の指定領域をキャプチャ |
| | `start_watching` | 画面キャプチャストリームを開始 |
| | `stop_watching` | キャプチャストリームを停止 |
| | `get_latest_frame`| セッションの最新フレームを取得 |
| **ウィンドウ** | `list_windows` | 表示されているアプリケーションの一覧を取得 |
| | `capture_window` | HWND を指定してウィンドウをキャプチャ |
| **統合** | `list_all` | モニターとウィンドウを統合して一覧表示 |
| | `capture` | 任意のターゲットを統合インターフェースでキャプチャ |
| | `watch` | 任意のターゲットを統合インターフェースで監視 |
| | `stop_watch` | 統合セッションの監視を停止 |
| **オーディオ** | `list_audio_devices` | マイクとシステム音源の一覧を取得 |
| | `start_audio_capture` | オーディオ録音を開始 |
| | `stop_audio_capture` | 録音を停止してデータを取得 |
| | `get_active_audio_sessions` | 実行中のオーディオセッション一覧を取得 |
| **AI/ML** | `listen` | Whisper を使用して音声をテキストに変換 |
| | `get_whisper_model_info` | Whisper モデルの情報を取得 |

---

## ツール詳細リファレンス

### スクリーン & ウィンドウキャプチャ

#### `see`
モニターまたはウィンドウのスクリーンショットを撮影します。
- **引数:**
  - `targetType` (string): "monitor" または "window" (デフォルト: "monitor")
  - `monitor` (number): モニター番号 (デフォルト: 0)
  - `hwnd` (number): ウィンドウハンドル (`targetType` が "window" の場合に必要)
  - `quality` (number): JPEG 品質 1-100 (デフォルト: 80)
  - `maxWidth` (number): 最大幅。これより大きい場合は縮小されます (デフォルト: 1920)

#### `capture` (統合)
一つのツールで何でもキャプチャします。
- **引数:**
  - `target` (string): "primary", "monitor", "window", "region"
  - `targetId` (string): モニター番号または HWND
  - `x`, `y`, `w`, `h` (number): "region" の場合に必須
  - `quality` (number): JPEG 品質 1-100 (デフォルト: 80)
  - `maxWidth` (number): 最大幅 (デフォルト: 1920)

### ストリーミング

#### `watch` (統合)
- **引数:**
  - `target` (string): "monitor" または "window"
  - `targetId` (string): モニター番号または HWND
  - `intervalMs` (number): キャプチャ間隔 (デフォルト: 1000)
  - `quality` (number): JPEG 品質 (デフォルト: 80)
  - `maxWidth` (number): 最大幅 (デフォルト: 1920)

### オーディオ & 音声認識

#### `listen`
Whisper を使用してシステム音やマイク入力を文字起こしします。
- **引数:**
  - `source` (string): "system", "microphone", "file", "audio_session"
  - `sourceId` (string): ファイルパスまたはオーディオセッション ID
  - `duration` (number): 録音秒数 (デフォルト: 10)
  - `language` (string): "auto", "ja", "en" など
  - `modelSize` (string): "tiny", "base", "small", "medium", "large"
  - `translate` (boolean): 英語に翻訳するかどうか (デフォルト: false)
