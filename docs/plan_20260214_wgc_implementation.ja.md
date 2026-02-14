# 実装計画書: GPU対応キャプチャ (Windows.Graphics.Capture) への移行

## 1. 目的
現在の GDI+ ベースのキャプチャ (`Graphics.CopyFromScreen`) では、ブラウザのハードウェア加速が有効な動画（YouTube, Netflix等）をキャプチャできず、黒画面またはフレーム未生成となる問題を解決する。Windows 10 (1809+) 標準の `Windows.Graphics.Capture` (WGC) API を実装することで、GPU レンダリングされたコンテンツの確実な取得を実現する。
また、ビデオペイロード内のタイムスタンプが固定値（00:00:00）になっている問題を修正し、動画の進行状況を正確に伝達する。

## 2. 技術的アプローチ
- **API**: `Windows.Graphics.Capture` 名前空間を使用。
- **対象**: HWND を指定した特定ウィンドウのキャプチャ。
- **利点**: 
    - GPU メモリ上のフレームに直接アクセスするため高速。
    - ハードウェア加速されたコンテンツを透過的に取得可能。
- **タイムスタンプ**: セッション開始時刻を基準とした相対経過時間（RelativeTime）を採用。

---

## 3. 実装詳細

### 3.1 `ModernCaptureService.cs` の完成
現在スタブとなっている `ModernCaptureService` を以下の手順で実装する。
1. **GraphicsCaptureItem の生成**: `CaptureHelper` (WinRT Interop) を使用し、HWND から `GraphicsCaptureItem` を作成する。
2. **FramePool の構築**: `Direct3D11CaptureFramePool` を作成し、最新フレームを取得可能にする。
3. **Bitmap 変換**: 取得したサーフェスを `Bitmap` クラスまたは直接 JPEG 符号化可能な形式へ変換する。

### 3.2 タイムスタンプの正常化 (TODO解消)
`VideoCaptureService.cs` の `CreateVideoPayload` メソッドにおけるハードコードを修正する。
1. `VideoSession` クラスに `StartTime (DateTime)` プロパティを追加。
2. ペイロード生成時に `DateTime.UtcNow - session.StartTime` を計算。
3. `Timestamp` プロパティに `"hh:mm:ss.f"` 形式（例: `00:00:05.2`）で経過時間を設定する。

### 3.3 `watch_video_v2` との統合
- `watch_video_v2` や `watch_video` が呼び出された際、優先的に `ModernCaptureService` を使用するようにロジックを修正。
- 失敗時のフォールバックとして GDI+ を維持する。

---

## 4. 修正対象ファイル

### `src/WindowsDesktopUse.Screen/CaptureServices/ModernCaptureService.cs`
- `Windows.Graphics.Capture` ロジックの実装。

### `src/WindowsDesktopUse.Screen/VideoCaptureService.cs`
- `VideoSession` の拡張（StartTime保持）。
- `CreateVideoPayload` での相対時間計算ロジック。

---

## 5. 受入基準 (Acceptance Criteria)
1. YouTube を再生中のブラウザウィンドウから、黒画面ではなく実際の映像がキャプチャされること。
2. `get_latest_video_frame` を複数回呼び出した際、`Timestamp` が `"00:00:05.2"` のように経過時間に応じて更新されていること。
3. 映像と音声の `ts` 同期が維持されていること。
