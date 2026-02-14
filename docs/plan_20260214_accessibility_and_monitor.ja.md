# 設計指示書: UIテキスト抽出およびウィンドウ監視機能の追加

## 1. 目的
本プロジェクトに以下の2つの新機能を追加し、LLMによるデスクトップ自動化の精度と効率を向上させる。
- **UI Automation (UIA) による構造化テキスト抽出 (`read_window_text`)**
- **グリッドベースのイベント駆動型ウィンドウ監視 (`monitor`)**

---

## 2. 機能1: `read_window_text` (UIテキスト抽出)

### 概要
指定したウィンドウのUIツリーを解析し、Markdown形式で構造化されたテキストを抽出する。

### 実装詳細
- **サービス名**: `AccessibilityService` (新規作成)
- **技術詳細**:
    - `UIAutomationClient` と `UIAutomationTypes` (Windows Desktop SDK標準) を使用。
    - パフォーマンス向上のため `TreeWalker.ControlViewWalker` を使用してツリーを走査する。
    - テキスト取得は `Current.Name` だけでなく、`ValuePattern` や `TextPattern` からの取得を試みる。
- **Markdown変換マッピング**:
    - `ControlType.TitleBar`, `Header` -> `# ` (深度に応じて増やす)
    - `ControlType.ListItem` -> `- `
    - `ControlType.Text`, `Edit`, `Document` -> プレーンテキスト（改行維持）
    - `ControlType.Button` -> `[ Button: {Name} ]` (引数で有効な場合)
- **注意点**: 
    - 無限ループ防止のため、最大深度を10階層に制限。
    - ブラウザのURL取得は、特定のエディットコントロール（"Address and search bar" 等）を探索することで付加価値を高める。

---

## 3. 機能2: `monitor` (ウィンドウ監視)

### 概要
指定したウィンドウの視覚的変化を監視し、変化を検知した際にMCP通知を送信する。

### 実装詳細
- **既存クラスの拡張 (`VisualChangeDetector.cs`)**:
    - `ChangeAnalysisResult` レコードに `List<int> ChangedGridIndices` を追加。
    - `CalculateChangeRatio` メソッドを拡張し、全体の比率だけでなく、各グリッドセル（0〜GridMode-1）ごとの変化有無を判定してインデックスを返すように修正。
- **判定モード（Sensitivity）の定数値**:
    - `High`: 閾値 0.01 (1%) - 微細な変化も検知。
    - `Medium`: 閾値 0.05 (5%) - 標準的な動き。
    - `Low`: 閾値 0.15 (15%) - 大きな画面転換のみ。
- **通知プロトコル**:
    - `McpServer.SendNotificationAsync("notifications/message", ...)` を使用。
    - `type: "window_monitor"` を付与し、LLMが「動きがあった」と判断できるようにする。
- **ライフサイクル管理**:
    - `DesktopUseTools` 内に `Dictionary<string, CancellationTokenSource>` 等で監視セッションを保持。
    - `stop_monitor` ツールで確実にタスクをキャンセルし、リソース（Bitmap等）を解放する。

---

## 4. プロジェクト構成への影響

### 既存コードへの修正箇所
1.  **`src/WindowsDesktopUse.Screen/VisualChangeDetector.cs`**:
    - グリッドごとの変化判定ロジックの追加。
2.  **`src/WindowsDesktopUse.App/DesktopUseTools.cs`**:
    - `read_window_text`, `monitor`, `stop_monitor` ツールの定義追加。
3.  **`src/WindowsDesktopUse.Core/Models.cs`**:
    - 必要に応じて監視セッション用のモデルを追加。

### 新規ファイル
- `src/WindowsDesktopUse.App/AccessibilityService.cs`: UIA操作の責務を分離。

---

## 5. テスト計画 (E2E)
- **`read_window_text`**: メモ帳（Notepad）を起動し、入力したテキストが Markdown として正しく取得できるか確認。
- **`monitor`**: ウィンドウを最小化/元に戻す、あるいはテキストを入力した際に、適切な変化通知が飛ぶかを確認。
