# MCP ツールリファレンス

このドキュメントでは、`windows-desktop-use-mcp` が提供する 9 つの統合ツールの詳細について説明します。

---

## 1. 視覚系ツール (Visual)

### `visual_list`
利用可能なキャプチャターゲット（モニターおよびウィンドウ）を一覧表示します。
- **引数:**
  - `type` (string): "all", "monitor", "window" (デフォルト: "all")
  - `filter` (string): ウィンドウタイトルで絞り込むキーワード
- **戻り値:** `{ type, monitors, windows }` オブジェクト

### `visual_capture`
特定のターゲットのスクリーンショットを取得します。
- **引数:**
  - `target` (string): "primary", "monitor", "window", "region" (デフォルト: "primary")
  - `hwnd` (string): ウィンドウハンドル（target="window" の場合に必須）
  - `monitorIndex` (number): モニター番号
  - `x, y, w, h` (number): 領域指定（target="region" の場合に必須）
  - `mode` (string): "normal" (低画質・節約), "detailed" (高画質・詳細解析)
  - `maxWidth` (number): リサイズ後の最大幅 (デフォルト: 640)
- **戻り値:** 画像データ (Base64) と LLM 向け処理指示

### `visual_watch`
継続的な監視やストリーミングを開始します。
- **引数:**
  - `mode` (string): "video", "monitor", "unified" (デフォルト: "video")
  - `target` (string): "monitor", "window", "region"
  - `hwnd` (string): 対象ウィンドウハンドル
  - `fps` (number): フレームレート (デフォルト: 5)
  - `overlay` (boolean): 画像にタイムコード等を焼き込むか (デフォルト: false)
  - `context` (boolean): AI 向け文脈プロンプトを生成するか (デフォルト: false)
- **戻り値:** セッション ID

### `visual_stop`
実行中の監視やストリーミングセッションを停止します。
- **引数:**
  - `sessionId` (string): 停止するセッションの ID
  - `type` (string): 全停止する場合は "all" を指定

---

## 2. 操作系ツール (Input)

### `input_mouse`
マウス操作（移動、クリック、ドラッグ）を実行します。
- **引数:**
  - `action` (string): "move", "click", "drag"
  - `x, y` (number): 操作座標
  - `endX, endY` (number): ドラッグの終了座標
  - `button` (string): "left", "right", "middle"
  - `clicks` (number): クリック回数（2 でダブルクリック）

### `input_window`
ウィンドウの状態を制御します。
- **引数:**
  - `hwnd` (string): 対象ウィンドウハンドル
  - `action` (string): "close", "minimize", "maximize", "restore"

### `keyboard_key`
安全なナビゲーションキー操作のみを実行します。
- **引数:**
  - `key` (string): `enter`, `tab`, `escape`, `space`, `left`, `right`, `up`, `down`, `pageup`, `pagedown` 等
  - `action` (string): "click", "press", "release"

---

## 3. 補助・音声ツール (Utility)

### `read_window_text`
UI Automation を使用してウィンドウ内のテキスト構造を抽出します。
- **引数:**
  - `hwnd` (string): 対象ウィンドウハンドル
  - `includeButtons` (boolean): ボタン名を含めるか (デフォルト: false)
- **戻り値:** Markdown 形式の構造化テキスト

### `listen`
システム音やマイクを録音し、文字起こしを行います。
- **引数:**
  - `duration` (number): 録音秒数 (デフォルト: 10)
  - `language` (string): 言語コード (例: "ja")
  - `modelSize` (string): "tiny", "base", "small" (デフォルト: "base")
