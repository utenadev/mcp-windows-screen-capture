# 計画書: MCPツールセットの統合・リファクタリング計画

## 1. 目的
増え続ける MCP ツール（現在 29 個）を、機能ベースで集約（サブコマンド化）し、LLM にとって使いやすく、保守性の高いツールセットへと再構築する。

---

## 2. 統合デザイン案

### 2.1 視覚 (Visual)
「見る」ことに関する機能を `visual_` プレフィックスで統合します。

| 新ツール名 | 集約される旧ツール | 主要な引数 |
|------------|--------------------|------------|
| `visual_list` | `list_all`, `list_monitors`, `list_windows` | `type` ("all", "monitor", "window") |
| `visual_capture` | `capture`, `see`, `capture_window`, `capture_region` | `target` ("primary", "monitor", "window", "region"), `targetId` |
| `visual_watch` | `watch`, `watch_video`, `watch_video_v1`, `monitor` | `mode` ("standard", "video", "monitor", "unified") |
| `visual_get_frame`| `get_latest_frame`, `get_latest_video_frame` | `sessionId` |
| `visual_stop` | `stop_watch`, `stop_watch_video`, `stop_monitor` | `sessionId` |

### 2.2 操作 (Input)
「動かす」ことに関する機能を `input_` プレフィックスで統合します。

| 新ツール名 | 集約される旧ツール | 主要な引数 |
|------------|--------------------|------------|
| `input_mouse` | `mouse_move`, `mouse_click`, `mouse_drag` | `action` ("move", "click", "drag"), `x`, `y`, `button` |
| `input_keyboard`| `keyboard_key` | `key`, `action` ("click", "press", "release") |
| `input_window` | `close_window` | `action` ("close"), `hwnd` |

---

## 3. 実装ステップ

### フェーズ 1: 統合ツールの実装 (After New Features)
- `DesktopUseTools.cs` に統合ツールを新規実装する。
- 既存のレガシーツールは維持するが、説明文に非推奨である旨を追記する。

### フェーズ 2: 移行期間
- LLM が新しい統合ツールを優先的に使うようにプロンプトや説明文を調整する。

### フェーズ 3: コードのクリーンアップ
- レガシーなツール定義を削除し、内部ロジックを整理する。

---

## 4. 期待される効果
- **トークンの節約**: ツール定義の総量が減り、プロンプトのコンテキストを節約できる。
- **精度の向上**: ツール選択の選択肢が整理されることで、LLM の誤操作が減少する。
- **拡張性**: 今後新しいキャプチャ方式や操作が増えても、既存ツールの引数を増やすだけで対応可能になる。
