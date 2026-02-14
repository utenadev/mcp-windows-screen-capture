# 実装設計書: メモリ効率化プロトコル (Token Efficiency Protocol)

## 1. 目的
`watch_video`, `watch_video_v2`, `get_latest_video_frame` 等の画像返却ツールにおいて、巨大な base64 データが LLM の会話履歴（トークン）を圧迫し、連続処理が停止する問題を解決する。MCP サーバー側から LLM に対して「画像を即座に処理し、履歴に含めず破棄する」よう明示的に指示する仕組みを導入する。

## 2. 実装の柱

### 2.1 ツール定義（Schema）での警告
ツール（`McpServerTool`）の `Description` に、メモリ効率のための処理パターン（Process-and-Discard）を明記する。

### 2.2 レスポンスへの処理指示 (`_llm_instruction`) の注入
画像を含むレスポンス JSON に、LLM 専用の指示フィールドを追加する。

---

## 3. 具体的な実装タスク

### 3.1 `DesktopUseTools.cs` のツール説明文更新
以下のツール説明に、「画像破棄の重要性」を追記する。
- `get_latest_video_frame`
- `watch_video`
- `watch_video_v2`

**追記内容（例）**:
> "CRITICAL: This tool returns large base64 data. To avoid token overflow, you MUST extract metadata, analyze the image briefly, and IMMEDIATELY discard the base64 data. Do NOT store it in your context."

### 3.2 ペイロードへの指示フィールド追加
`VideoPayload` (in `Models.cs`) または `DesktopUseTools` 内での匿名型レスポンスに、以下の指示を追加する。

```json
"_llm_instruction": {
    "action": "PROCESS_IMMEDIATELY_AND_DISCARD",
    "steps": [
        "1. Extract metadata (ts, windowTitle).",
        "2. Analyze image NOW.",
        "3. Record result as TEXT only.",
        "4. DELETE the 'frame' or 'image' field from your history."
    ],
    "token_warning": "This image consumes approx 2000+ tokens. Discarding it saves 95% memory."
}
```

### 3.3 通知 (Notifications) への組み込み
`watch_video_v2` の `SendNotificationAsync` で送るデータにも、同様の指示フィールドを含める。

---

## 4. 修正対象ファイル

### `src/WindowsDesktopUse.App/DesktopUseTools.cs`
- 各ツールの属性 `[Description]` の更新。
- `WatchVideoV2` 内の `notificationData` 辞書に `_llm_instruction` を追加。
- `GetLatestVideoFrame` の戻り値（Dictionary）に `_llm_instruction` を追加。

### `src/WindowsDesktopUse.Core/Models.cs`
- 指示情報を構造化して送るための `LlmInstruction` レコード（任意）の追加。

---

## 5. 受入基準 (Acceptance Criteria)
1. Claude Desktop 等のツール定義画面で、画像破棄に関する警告が表示されること。
2. ツールの実行結果に `_llm_instruction` フィールドが含まれ、LLM がそれを読み取れること。
3. LLM が指示に従い、数回以上の連続キャプチャを行ってもトークンオーバーフローが発生しにくくなること。
