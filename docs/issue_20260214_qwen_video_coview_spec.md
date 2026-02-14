【機能要望】LLM とユーザーが動画を「一緒に見る」体験を実現するための同期キャプチャ機能：2秒間隔パラパラ画像＋音声文字起こしのサポート

## 背景
`windows-desktop-use-mcp` プロジェクトの根本的な目的は、
> **「LLM が Windows デスクトップという“箱庭”に出てきて、ユーザーと一緒に何かをする」**
という共体験型インタラクションの実現です。

その中でも、「動画を一緒に見る」ことは、非常に重要なユースケースです。
- 教育・解説（例：アニメの演出分析）
- エンタメ共有（例：「このシーン、どう思う？」）
- 開発支援（例：UI 動作の確認）

しかし、現状の MCP サーバーでは、以下の課題があります：
- スクリーンショットは静的（`see` / `capture_region`）→ 時系列が失われる
- 連続キャプチャ（`watch`）はあるが、**時刻情報・音声同期・LLM 向け出力フォーマットが整備されていない**
- 動画内字幕の自動読み取り（OCR）機能なし
- base64 出力に改行が含まれると API エラーになる（→ `docs/feedback_20240614_base64_input_handling.md` 参照）

つまり、**「LLM が“今見ているフレーム”について即座にコメントできる」環境が未整備**です。

## 現状の MCP サーバー機能一覧（確認済み）
| 機能 | 対応状況 | 備考 |
|------|----------|------|
| `mcp__WindowsDesktopUse__list_windows` | ✅ | ウィンドウ一覧取得可能 |
| `mcp__WindowsDesktopUse__see` / `capture_region` | ✅ | 静的スクリーンショット（JPEG, base64） |
| `mcp__WindowsDesktopUse__watch` | ✅ | 指定間隔（ms）での連続キャプチャ（但し、出力は個別JSON） |
| `mcp__WindowsDesktopUse__listen` | ✅ | システム音／マイク録音 → Whisper による文字起こし（duration 指定可） |
| `mcp__WindowsDesktopUse__watch_video_v1` | ⚠️（プロトタイプ） | 音声＋映像の統合ストリーム（RelativeTime 付き）あり |
| 字幕OCR（Tesseract等） | ❌ | 現在のコードベースには実装なし |

## 提案仕様：「2秒ごとのパラパラ画像 ＋ 同期文字起こし」
LLM が時系列データを基に自然な会話ができるよう、以下の出力形式を実現します：

### 出力フォーマット（NDJSON: Newline-Delimited JSON）
```json
{"ts": 0.0, "frame": "base64encoded_jpeg_without_newlines", "transcript": ""}
{"ts": 2.0, "frame": "...", "transcript": "おはようございます"}
{"ts": 4.0, "frame": "...", "transcript": "杜王町の朝だ"}
...
```
- `ts`: 再生開始からの経過時間（秒単位、float、100ms 精度以上）
- `frame`: JPEG base64（改行なし、RFC 4648準拠）→ `docs/feedback_20240614_base64_input_handling.md` の教訓を反映
- `transcript`: `listen(duration=2, source="System")` で得た文字起こし（空文字可）
- 全てのフィールドは必須ではなく、`transcript` は音声が検出されない場合は ""

### 入力オプション（CLI または JSON-RPC）
```bash
dotnet run --project src/WindowsDesktopUse.App/WindowsDesktopUse.App.csproj \
  -- --watch-interval-ms 2000 --enable-audio-sync true
```
- `--watch-interval-ms`: デフォルト 1000ms → 2000ms に変更可能
- `--enable-audio-sync`: 音声キャプチャを並列で有効化（デフォルト false）
- 対象ウィンドウは `targetId` で指定（例：`hwnd=131134`）

## 実現までのステップ（開発フェーズ分け）

### Phase 1: 最小限の動作確認（PoC｜1～2日で実装可能）
- [ ] `WatchService` を追加：`Timer` + `CaptureRegionAsync` + `ListenAsync` を同期制御
- [ ] 出力先：標準出力（NDJSON）または指定ファイル（`--output ndjson://stdout`）
- [ ] 時刻同期： `DateTimeOffset.UtcNow` を基準に、各フレーム・音声バッチの `ts` を計算
- [ ] base64 正規化：`Replace("\n", "").Replace("\r", "")` を必須処理として組み込む
- [ ] ログ出力：`Console.Error.WriteLine($"[WATCH] Frame @ {ts:F3}s emitted")`（ログは stderr）

### Phase 2: 精度・利便性向上（中長期）
- [ ] 動画プレイヤー領域の自動検出：
  - 黒い矩形（ビデオ領域）＋再生ボタン位置から ROI を推定
  - 差分検出（OpenCVSharp の軽量版 or pure C# image diff）
- [ ] 字幕OCR追加：
  - フレームから「黒背景＋白文字」領域を抽出 → Tesseract OCR（`tessdata/jpn.traineddata`）
  - 出力は `subtitle: "ダイヤモンドは砕けない"` として NDJSON に追加
- [ ] `watch_video_v1` の `RelativeTime` 機能との統合：
  - 音声・映像のタイムラインを同一基準で管理
  - LLM 側で「t=3.2s のフレーム」と「t=3.1s～3.3s の音声」を関連付けることが可能

## 関連ドキュメント・既存知見
- `docs/feedback_20240614_base64_input_handling.md`：base64 改行問題の教訓 → 必ず正規化
- `AGENTS.md`：
  - `.NET 8.0-windows` 対応
  - `Console.Error` でのログ出力必須
  - nullable / implicit usings 有効
- `CONTRIBUTING.md`：コードスタイル（PascalCase, `_field` prefix）に従う

## 受入基準（Acceptance Criteria）
- [ ] LLM が `ts=0.0`, `ts=2.0`, `ts=4.0` のフレームと対応する文字起こしを受信できる
- [ ] 時刻誤差は ±100ms 以内（高負荷時も保証）
- [ ] base64 出力に改行が含まれていない（自動検証テスト追加推奨）
- [ ] 音声が無くてもフレームは欠落せず出力される（堅牢性）