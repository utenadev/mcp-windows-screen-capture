# windows-desktop-use-mcp ツール詳細

このドキュメントでは，本サーバーで利用可能な MCP ツールの詳細について説明します。

## ツール一覧

| カテゴリ | ツール名 | 説明 |
|----------|----------|------|
| **視覚** | `visual_list` | モニター，ウィンドウ，またはすべてを一覧表示 |
| | `visual_capture` | モニター，ウィンドウ，または領域をキャプチャ |
| | `visual_watch` | 継続的な監視・ストリーミング |
| | `visual_stop` | アクティブなセッションを停止 |
| **聴覚** | `listen` | システム音やマイクを録音・文字起こし |
| **操作** | `input_mouse` | マウス操作（移動，クリック，ドラッグ） |
| | `input_window` | ウィンドウ操作（閉じる，最小化，最大化，復元） |
| | `keyboard_key` | ナビゲーションキー操作（セキュリティ制限あり） |
| **補助** | `read_window_text` | ウィンドウのテキストを Markdown で抽出 |

---

## 視覚ツール

### `visual_list`

モニター，ウィンドウ，またはすべての視覚ターゲットを一覧表示します。

- **引数:**
  - `type` (string): "monitor"，"window"，または "all"（デフォルト: "all"）

- **戻り値:**
  - `count`: アイテム数
  - `items`: モニターまたはウィンドウの配列

### `visual_capture`

モニター，ウィンドウ，または領域のスクリーンショットを撮影します。

- **引数:**
  - `target` (string): "monitor"，"window"，"region"，または "primary"（デフォルト: "primary"）
  - `monitorIndex` (number): "monitor" 用のモニター番号
  - `hwnd` (string): "window" 用のウィンドウハンドル
  - `x`, `y`, `w`, `h` (number): "region" 用の座標
  - `mode` (string): "normal"（品質30）または "detailed"（品質70）
  - `quality` (number): JPEG 品質 1-100（デフォルト: 30 または 70）

- **戻り値:**
  - Base64 エンコードされた画像データ

### `visual_watch`

視覚ターゲットの継続的な監視またはストリーミングを開始します。

- **引数:**
  - `mode` (string): "video"，"monitor"，または "unified"（デフォルト: "video"）
  - `target` (string): "monitor"，"window"，または "region"
  - `monitorIndex` (number): モニター番号
  - `hwnd` (string): ウィンドウハンドル
  - `x`, `y`, `w`, `h` (number): 領域座標
  - `fps` (number): 1秒あたりのフレーム数 1-30（デフォルト: 5）
  - `detectChanges` (boolean): 変化検出を有効化（デフォルト: true）
  - `threshold` (number): 変化閾値 0.05-0.20（デフォルト: 0.08）

- **戻り値:**
  - `sessionId`: 監視セッションの ID

### `visual_stop`

アクティブな視覚または入力セッションを停止します。

- **引数:**
  - `sessionId` (string): 停止するセッション ID（省略可能）
  - `type` (string): "watch"，"capture"，"audio"，"monitor"，または "all"（デフォルト: "all"）

- **戻り値:**
  - 確認メッセージ

---

## 聴覚ツール

### `listen`

システム音やマイクを録音し，Whisper AI で文字起こしを行います。

- **引数:**
  - `source` (string): "system"，"microphone"，"file"，または "audio_session"（デフォルト: "system"）
  - `sourceId` (string): ファイルパスまたはオーディオセッション ID
  - `duration` (number): 録音秒数（デフォルト: 10）
  - `language` (string): "auto" または言語コード "ja"，"en"，"zh" 等（デフォルト: "auto"）
  - `modelSize` (string): "tiny"，"base"，"small"，"medium"，"large"（デフォルト: "base"）
  - `translate` (boolean): 英語に翻訳するかどうか（デフォルト: false）

- **戻り値:**
  - 文字起こしテキスト

---

## 操作ツール

### `input_mouse`

マウス操作を行います。

- **引数:**
  - `action` (string): "move"，"click"，"drag"，または "scroll"
  - `x`, `y` (number): 対象座標
  - `button` (string): "left"，"right"，または "middle"（デフォルト: "left"）
  - `clicks` (number): クリック回数（デフォルト: 1）
  - `delta` (number): "scroll" 操作のスクロール量

- **戻り値:**
  - 確認メッセージ

### `input_window`

ウィンドウ操作を行います。

- **引数:**
  - `hwnd` (string): ウィンドウハンドル
  - `action` (string): "close"，"minimize"，"maximize"，"restore"，"activate"，または "focus"

- **戻り値:**
  - 確認メッセージ

### `keyboard_key`（セキュリティ制限あり）

ナビゲーションキーのみを操作できます。セキュリティ上の理由から，テキスト入力と修飾キー（Ctrl，Alt，Win）はブロックされています。

- **引数:**
  - `key` (string): ナビゲーションキー名
    - **許可されるキー:** `enter`，`return`，`tab`，`escape`，`esc`，`space`，`backspace`，`delete`，`del`，`left`，`up`，`right`，`down`，`home`，`end`，`pageup`，`pagedown`
    - **ブロックされるキー:** `ctrl`，`alt`，`win`，`shift`
  - `action` (string): "click"，"press"，または "release"（デフォルト: "click"）

- **戻り値:**
  - 確認メッセージ

---

## 補助ツール

### `read_window_text`

UI Automation を使用してウィンドウからテキスト内容を抽出します。

- **引数:**
  - `hwndStr` (string): ウィンドウハンドル（文字列）
  - `includeButtons` (boolean): ボタンテキストを含めるかどうか（デフォルト: false）

- **戻り値:**
  - Markdown 形式のテキスト内容

---

## HTTP ストリーミング

`visual_watch` では，HTTP 経由でフレームをストリーミングできます:

- **エンドポイント:** `http://localhost:5000/frame/{sessionId}`
- 最新のフレームを JPEG 画像で返します
