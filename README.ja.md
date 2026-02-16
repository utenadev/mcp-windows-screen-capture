# windows-desktop-use-mcp

Windows 11 を AI から自在に操作・認識するための MCP サーバー。
AI に Windows の「目（視覚）」「耳（聴覚）」「手足（操作）」を与え、Claude などの MCP クライアントからデスクトップ環境を利用可能にします。

[English](README.md) | [日本語](README.ja.md)

## 主な機能

- **視覚 (Vision)**: モニター、特定のウィンドウ、または任意領域のキャプチャ。GPUアクセラレーション対応（YouTube/Netflix等のキャプチャが可能）。
- **聴覚 (Hearing)**: システム音やマイクの録音、および Whisper AI による高品質なローカル文字起こし。
- **操作 (Input)**: マウス移動、クリック、ドラッグ、ウィンドウ制御、安全なナビゲーションキー操作。
- **解析 (Analysis)**: UI Automation によるウィンドウテキストの構造化抽出 (Markdown) および AI 向け視覚最適化。

## クイックスタート

### ビルド済み実行ファイル（推奨）

1. [Releases](../../releases) から最新の `WindowsDesktopUse.zip` をダウンロード・展開。
2. `WindowsDesktopUse.exe setup` を実行して Claude Desktop に自動登録。
3. Claude Desktop を再起動。

## 利用可能な MCP ツール

### 視覚系（Visual）
- **`visual_list`**: モニターやウィンドウの一覧を取得。
- **`visual_capture`**: 静止画を取得。`mode="detailed"` で高画質化が可能。
- **`visual_watch`**: 同期ストリーミング。`overlay=true` でタイムコードを焼き込み、`context=true` で AI 向け文脈プロンプトを自動生成。
- **`visual_stop`**: 全ての視覚・音声セッションを停止。

### 操作系（Input）
- **`input_mouse`**: マウスの移動、クリック、ドラッグ。
- **`input_window`**: ウィンドウの終了、最小化、最大化、復元。
- **`keyboard_key`**: Enter, Tab, 矢印キー等の安全なキー操作。

### 補助・音声（Hearing & Utility）
- **`listen`**: 音声の録音と文字起こし。
- **`read_window_text`**: ウィンドウ内テキストの構造化抽出。

---

## 💡 AI アシスタントを賢く動かすコツ

本サーバーは、AI がトークン制限を回避しながら長時間のデスクトップ監視を行えるよう、以下の機能を備えています。

### 1. 視覚的ヒントの活用
`visual_watch` 実行時に `overlay=true` を指定すると、画像内に `[00:00:05.2]` のようなタイムコードが焼き込まれます。これにより、AI は OCR を通じて動画の進行を正確に把握できます。

### 2. 時系列文脈の提示
`context=true` を指定すると、前回のフレームとの差分を意識したプロンプトが自動生成されます。AI は「前回は〇〇でしたが、今回は？」という問いかけを受け取るため、動画の連続性を理解しやすくなります。

### 3. メモリ管理の徹底
各ツールのレスポンスには、画像を即座に処理して破棄するよう促す `_llm_instruction` が含まれています。AI に「指示を遵守して image は即時破棄せよ」と伝えることで、長時間のキャプチャが可能になります。

---

## ドキュメント一覧
- [**ツールリファレンス**](docs/TOOLS.ja.md) - 全コマンドの詳細な引数と使用例。
- [**開発者ガイド**](docs/DEVELOPMENT.ja.md) - アーキテクチャとビルド手順。
- [**画質テスト報告書**](docs/quality_test_report.md) - 画質設定による情報量の違いについて。

## 動作要件
- Windows 11 / 10 1803+
- .NET 8.0 ランタイム

## ライセンス
MIT License.
