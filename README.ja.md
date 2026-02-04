# MCP Windows Screen Capture Server

Windows 11 向けのスクリーンキャプチャ MCP サーバー。Claude Desktop 用の stdio トランスポートに対応。

## このプロジェクトについて

AI アシスタントのための Model Context Protocol (MCP) を使用した Windows スクリーンキャプチャサーバー。

> **⚠️ 実装メモ:** これは **GDI+ 版** です。Direct3D なしで確実に動作します。高性能な GPU キャプチャが必要な場合は、Direct3D/Windows Graphics Capture を自分で実装してください。この GDI+ 版は AI アシスタント用途には十分です。

## 動作要件

- Windows 11（または Windows 10 1809 以降）
- .NET 8.0 SDK

## ビルド & 実行

```bash
# ビルド
dotnet build src/WindowsScreenCaptureServer.csproj -c Release

# 実行可能ファイル: src/bin/Release/net8.0-windows/win-x64/WindowsScreenCaptureServer.exe
```

## テスト（stdio モード）

.NET 8 SDK がインストールされていれば、MCP サーバーを直接テストできます：

```bash
# 初期化のテスト
echo '{"jsonrpc":"2.0","method":"initialize","params":{"protocolVersion":"2024-11-05"},"id":1}' | src/bin/Release/net8.0-windows/win-x64/WindowsScreenCaptureServer.exe

# list_windows ツールのテスト
echo '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"list_windows","arguments":{}},"id":2}' | src/bin/Release/net8.0-windows/win-x64/WindowsScreenCaptureServer.exe

# list_monitors ツールのテスト
echo '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"list_monitors","arguments":{}},"id":3}' | src/bin/Release/net8.0-windows/win-x64/WindowsScreenCaptureServer.exe
```

サーバーは JSON-RPC レスポンスを stdout に、ログを stderr に出力するため、テストとデバッグが簡単です。

## 利用可能な MCP ツール

### スクリーンキャプチャツール

| ツール | 説明 |
|--------|------|
| `list_monitors` | 利用可能なモニター/ディスプレイの一覧を取得 |
| `see` | 指定したモニターのスクリーンショットを撮影（目で見るような感覚） |
| `start_watching` | 連続的な画面キャプチャストリームを開始（ライブ映像のようなもの） |
| `stop_watching` | 実行中の画面キャプチャストリームを停止 |

### ウィンドウキャプチャツール

| ツール | 説明 |
|--------|------|
| `list_windows` | 表示されている Windows アプリケーションの一覧を取得（hwnd, タイトル, 位置, サイズ） |
| `capture_window` | 特定のウィンドウを HWND（ウィンドウハンドル）で指定してキャプチャ |
| `capture_region` | 任意の画面領域をキャプチャ（x, y, width, height） |

### ツール使用例

Claude に以下のように尋ねてみてください:
- 「今画面に何が映ってる？」
- 「モニター0を見て」
- 「開いているウィンドウを一覧表示して」
- 「Visual Studio のウィンドウをキャプチャして」
- 「(100,100) から (500,500) の領域をキャプチャして」
- 「画面を監視して、変化があったら教えて」

### ツールパラメータ例

```json
// ウィンドウ一覧を取得
{"method": "list_windows"}

// 特定のウィンドウをキャプチャ
{"method": "capture_window", "arguments": {"hwnd": 123456, "quality": 80}}

// 領域をキャプチャ
{"method": "capture_region", "arguments": {"x": 100, "y": 100, "w": 800, "h": 600}}
```

## 制約と考慮事項

### ウィンドウキャプチャの制約
- **最小化されたウィンドウ**: 最小化されたウィンドウは正しくキャプチャされないか、古い内容が表示される場合があります。キャプチャ前に対象のウィンドウが表示されていることを確認してください。
- **GPU アクセラレーションアプリ**: PW_RENDERFULLCONTENT フラグ（Windows 8.1+）を使用して、Chrome、Electron、WPF アプリをキャプチャします。これは静止画スクリーンショットには適していますが、一部のアプリケーションでは制限がある場合があります。

### パフォーマンス考慮事項
- **静止画スクリーンショット**: ✅ 完全にサポートされています - 単一のスクリーンショットや定期的なキャプチャ（数秒ごと）が可能です
- **高頻度動画キャプチャ**: ⚠️ 推奨されません - CPU 負荷が高くなります。動画やストリーミングのユースケースでは、代わりに Desktop Duplication API（DirectX ベース）を検討してください。
- **最適なユースケース**: 定期的なモニタリング、ドキュメント用スクリーンショット、自動化テスト

## アーキテクチャ & 実装

### リファクタリング履歴

| バージョン | 変更内容 | ステータス |
|---------|---------|--------|
| v1.0 | 初版（SSE のみ実装） | ✅ マージ済 |
| v1.1 | ツール名（動詞）、inputSchema、エラーハンドリング | ✅ マージ済 |
| v1.2 | ユニットテスト、CI 改善 | ✅ マージ済 |
| v1.3 | グレースフルシャットダウン（IHostApplicationLifetime） | ✅ マージ済 |
| v1.4 | **デュアルトランスポート**（Streamable HTTP + SSE） | ✅ マージ済 |
| v1.5 | **ウィンドウキャプチャ**（list_windows, capture_window, capture_region） | ✅ マージ済 |
| v2.0 | **MCP SDK 移行**（Microsoft.ModelContextProtocol） | ✅ マージ済 |
| v2.1 | **stdio 専用化**（HTTP モード削除） | 🔄 作業中 |

### 主な機能

- **公式 MCP SDK**: Microsoft.ModelContextProtocol を使用してプロトコル準拠
- **stdio トランスポート**: デフォルトのローカル専用モード - 安全、ネットワーク暴露なし
- **ウィンドウ列挙**: EnumWindows API で表示中のアプリケーションを一覧表示
- **領域キャプチャ**: CopyFromScreen を使用した任意の画面領域キャプチャ
- **グレースフルシャットダウン**: Ctrl+C やプロセス終了時の適切なクリーンアップ
- **エラーハンドリング**: 意味のあるエラーメッセージを含む包括的な try-catch ブロック
- **CI/CD**: 自動 E2E テスト付き GitHub Actions

## Claude Desktop の設定

### 1. サーバーをビルド

```bash
dotnet build src/WindowsScreenCaptureServer.csproj -c Release
```

### 2. Claude Desktop の設定を開く

1. Claude Desktop を起動
2. 設定（歯車アイコン）をクリック
3. 「MCP Servers」に移動
4. 「Add MCP Server」をクリック

### 3. Windows Screen Capture サーバーを追加

実行可能ファイルのパスを設定します：

```json
{
  "mcpServers": {
    "windows-capture": {
      "command": "C:\\workspace\\mcp-windows-screen-capture\\src\\bin\\Release\\net8.0-windows\\win-x64\\WindowsScreenCaptureServer.exe"
    }
  }
}
```

パスをシステム上の `WindowsScreenCaptureServer.exe` の実際の場所に置き換えてください。

### 4. 保存して再起動

「Save」をクリックし、Claude Desktop を再起動します。

## 使用例

### 画面キャプチャ

```
「今画面に何が映ってる？」
「モニター0のスクリーンショットを撮って」
```

### ウィンドウキャプチャ

```
「開いているウィンドウを一覧表示して」
「Visual Studio のウィンドウをキャプチャして」
「表示されているウィンドウをすべて見せて」
```

### 連続監視

```
「画面を監視して、変化があったら教えて」
「画面の変化を監視して」
```

## トラブルシューティング

| 問題 | 解決方法 |
|-------|----------|
| stdio モードが動作しない | MCP クライアント設定で実行可能ファイルのパスが正しいことを確認 |
| サーバーが見つからない | `WindowsScreenCaptureServer.exe` のパスが存在することを確認 |
| 黒い画面が表示される | 管理者権限で実行 |
| ウィンドウが見つからない | ウィンドウが表示されていることを確認（システムトレイに最小化されていない） |
| Claude Desktop に接続できない | Claude Desktop のログを確認（設定 > 開発者 > ログを開く） |

## ライセンス

MIT License - 詳細は LICENSE ファイルを参照してください。
