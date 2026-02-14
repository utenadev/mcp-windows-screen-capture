# Windows Desktop Use MCP ツールリスト (2026-02-14版)

現在、この MCP サーバーは計 29 個のツールを公開しています。ツールの増加に伴い、今後は機能ベースでの集約を検討しています。

## 1. 視覚ツール (Visual Tools)

| ツール名 | 説明 | 備考 |
|----------|------|------|
| `list_all` | モニターとウィンドウを一覧表示 | 推奨 |
| `list_monitors` | モニターの一覧を取得 | |
| `list_windows` | ウィンドウの一覧を取得 | |
| `capture` | モニター/ウィンドウ/領域をキャプチャ | **統合ツール** |
| `see` | 簡易スクリーンショット撮影 | レガシー |
| `capture_window` | ウィンドウをキャプチャ | `capture` へ統合可能 |
| `capture_region` | 領域をキャプチャ | `capture` へ統合可能 |
| `watch` | ストリーミング開始 | **統合ツール** |
| `start_watching` | ストリーミング開始 | レガシー |
| `stop_watch` | ストリーミング停止 | |
| `stop_watching` | ストリーミング停止 | レガシー |
| `get_latest_frame` | 最新の静止画フレームを取得 | |
| `camera_capture_stream` | 高速領域ストリーミング (15fps) | 特殊用途 |

### ビデオ特化ツール (Video Tools)
動画コンテンツ（YouTube等）の消費に最適化されたツール群です。

| ツール名 | 説明 | 備考 |
|----------|------|------|
| `watch_video` | 高効率ビデオストリーム開始 | 変化検出機能付き |
| `stop_watch_video` | ビデオストリーム停止 | |
| `get_latest_video_frame` | 最新のビデオフレーム（メタデータ付）取得 | |
| `watch_video_v1` | **[PROTOTYPE]** 映像・音声同期ストリーム | スパイラル1機能 |
| `stop_watch_video_v1` | プロトタイプストリーム停止 | |

## 2. 音声ツール (Audio & Transcription Tools)

| ツール名 | 説明 | 備考 |
|----------|------|------|
| `listen` | 録音と文字起こし (Whisper) | |
| `list_audio_devices` | オーディオデバイス一覧を取得 | |
| `start_audio_capture` | 未加工音声のキャプチャ開始 | |
| `stop_audio_capture` | 音声キャプチャ停止とデータ取得 | |
| `get_active_audio_sessions` | 実行中の音声セッション一覧を取得 | |
| `get_whisper_model_info` | Whisper モデル情報の取得 | |

## 3. 操作ツール (Input Tools)

| ツール名 | 説明 | 備考 |
|----------|------|------|
| `mouse_move` | マウス移動 | |
| `mouse_click` | マウスボタンクリック | |
| `mouse_drag` | ドラッグ＆ドロップ | |
| `keyboard_key` | ナビゲーションキー入力 | セキュリティ制限あり |
| `close_window` | ウィンドウを閉じる | HWND 指定 |

---

## 4. 今後の整理案 (サブコマンド化 / 集約)

現在、ツール名が冗長になっているものを、以下のように「引数による分岐」へ集約することを計画しています。

### 集約案A: `capture` ツールへの完全統合
`capture_window`, `capture_region`, `see` を廃止し、`capture` の `target` 引数に集約します。
- `capture(target="window", targetId="12345")`
- `capture(target="region", x=0, y=0, w=100, h=100)`

### 集約案B: `watch` ツールへの集約
`watch`, `start_watching`, `watch_video`, `watch_video_v1` を `watch` に集約し、モードを指定させます。
- `watch(type="video", target="YouTube")`
- `watch(type="unified", enableAudio=true)` (旧 watch_video_v1)

### 集約案C: `mouse` / `keyboard` の統合
`mouse_move`, `mouse_click` を `mouse(action="click", x=100, y=100)` のように統合します。

このような整理を行うことで、LLM がツールを選択する際の迷いを減らし、プロンプトのトークン消費も抑えることが可能になります。
