# アーキテクチャ刷新および MCP ツールセット統合案

## 1. 現状の課題（Gemini 視点）
- **ツールの断片化**: 同じ「見る（キャプチャ）」という意図に対し、実装時期の異なるツール（`see`, `capture`, `watch`, `watch_video_v2` 等）が乱立している。
- **ボイラープレートの重複**: 各ツール内で `sessionId` の発行、セッション管理、エラーハンドリングが個別に行われており、保守性が低い。
- **LLM の混乱**: 似た名前のツールが多いと、LLM が適切なツールを選択する際に「推論の迷い」が生じ、トークン消費と誤操作の原因になる。

## 2. 刷新後のアーキテクチャ方針

### 2.1 統合インターフェースの実装
個別の静止画・動画・監視ツールを、以下の 3 つの主要な「動詞」に統合します。

1.  **`visual_list`**: ターゲット（モニター、ウィンドウ、領域）を探す。
2.  **`visual_capture`**: ターゲットの「今」を一枚の画像（静止画）として撮る。
3.  **`visual_watch`**: ターゲットの「経過」を監視・配信（動画・イベント）する。

### 2.2 セッション管理の共通化 (`SessionManager`)
現在 `DesktopUseTools` に散らばっている `Dictionary<string, ...>` によるセッション管理を、汎用的な `SessionManager` クラスに集約し、`opencode` が新しいストリーム機能を追加する際に「ロジックだけに集中」できるようにします。

---

## 3. 具体的な統合マッピング（opencode への指示用）

### 3.1 視覚系 (Visual)
| 新ツール名 | 役割 | 統合される旧ツール | 統合のメリット |
|------------|------|--------------------|----------------|
| `visual_list` | ターゲット列挙 | `list_all`, `list_monitors`, `list_windows` | 検索対象を引数（`type`）で切り替え可能にする。 |
| `visual_capture` | 静止画取得 | `capture`, `see`, `capture_window`, `capture_region` | HWND, MonitorIdx, Region を一つの `target` 引数で扱う。 |
| `visual_watch` | ストリーム/監視 | `watch`, `watch_video_v2`, `monitor`, `camera_capture_stream` | `mode` 引数（"video", "monitor", "unified"）で挙動を制御。 |
| `visual_stop` | 停止 | 全ての `stop_...` ツール | セッション ID だけで全ての監視を停止可能にする。 |

### 3.2 操作系 (Input)
| 新ツール名 | 役割 | 統合される旧ツール | 備考 |
|------------|------|--------------------|------|
| `input_mouse` | マウス操作 | `mouse_move`, `mouse_click`, `mouse_drag` | `action` で挙動を選択。 |
| `input_keyboard`| キー入力 | `keyboard_key` | 既存のセキュリティ制限を維持しつつ整理。 |
| `input_window` | ウィンドウ制御 | `close_window` | 将来的に最小化・最大化なども追加可能にする。 |

---

## 4. opencode への実装ステップ提案

1.  **Phase 1: 統合ツールの新規定義**
    - `DesktopUseTools.cs` に上記の新ツール（`visual_...`, `input_...`）を実装する。
    - 内部ロジックは、既存のサービス（`ScreenCaptureService`, `VideoCaptureService` 等）を呼び出す形にする。
2.  **Phase 2: 旧ツールの非推奨化 (Deprecated)**
    - 旧ツールの `Description` に `[DEPRECATED: Use visual_... instead]` と明記する。
    - LLM が新ツールを優先して使うように誘導する。
3.  **Phase 3: 内部サービスの整理**
    - 複数のストリームループ（`VideoCaptureService` 内と `DesktopUseTools` 内）を統一し、共通のスケジューラを使用するようにリファクタリングする。

---

## 5. 本日の「新キャプチャテスト」後の対応
YouTube キャプチャのテストで発生する「重さ」や「不安定さ」については、`ModernCaptureService` の内部で `Direct3D11CaptureFramePool` を本格的に使用する（現在の `PrintWindow` からのアップグレード）ことで最適化する余地を残しています。
統合された基盤があれば、このような「内部エンジンの交換」も容易になります。
