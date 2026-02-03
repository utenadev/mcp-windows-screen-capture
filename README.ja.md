# MCP Windows Screen Capture Server

Windows 11 向けのスクリーンキャプチャ MCP サーバー。stdio トランスポートをデフォルトとし、後方互換性のためオプションで HTTP モードをサポートしています。

> **⚠️ 実装メモ:** これは **GDI+ 版** です。Direct3D なしで確実に動作します。高性能な GPU キャプチャが必要な場合は、Direct3D/Windows Graphics Capture を自分で実装してください。この GDI+ 版は AI アシスタント用途には十分です。

## 動作要件
- Windows 11（または Windows 10 1809 以降）
- .NET 8.0 SDK

## ビルド & 実行

```bash
# ビルド
dotnet build src/WindowsScreenCaptureServer.csproj -c Release

# stdio モードで実行（デフォルト - 推奨）
dotnet run --project src/WindowsScreenCaptureServer.csproj

# HTTP モードで実行（レガシークライアント向け）
dotnet run --project src/WindowsScreenCaptureServer.csproj -- --http --ip_addr 127.0.0.1 --port 5000

# 単一ファイル公開
dotnet publish src/WindowsScreenCaptureServer.csproj -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
```

## テスト（stdio モード）

.NET 8 SDK がインストールされていれば、ビルドせずに `dotnet run` で直接 MCP サーバーをテストできます：

```bash
# 初期化のテスト
echo '{"jsonrpc":"2.0","method":"initialize","params":{"protocolVersion":"2024-11-05"},"id":1}' | dotnet run --project src/WindowsScreenCaptureServer.csproj

# list_windows ツールのテスト
echo '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"list_windows","arguments":{}},"id":2}' | dotnet run --project src/WindowsScreenCaptureServer.csproj

# list_monitors ツールのテスト
echo '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"list_monitors","arguments":{}},"id":3}' | dotnet run --project src/WindowsScreenCaptureServer.csproj
```

サーバーは JSON-RPC レスポンスを stdout に、ログを stderr に出力するため、テストとデバッグが簡単です。

## CLI オプション

### デフォルトモード（stdio）

フラグは不要です - stdio ベースの MCP サーバーとして実行されます：

```bash
dotnet run --project src/WindowsScreenCaptureServer.csproj
```

### HTTP モード（オプション）

レガシークライアントや特殊なユースケースのために `--http` フラグで有効化：
- `--http`: HTTP モードを有効化（stdio がデフォルト）
- `--ip_addr`: バインドする IP（デフォルト: `127.0.0.1`、外部アクセスには `0.0.0.0` を使用）
- `--port`: ポート番号（デフォルト: 5000）
- `--desktopNum`: デフォルトモニター番号（0=プライマリ、1=セカンダリなど）

## トランスポートモード

> **注:** HTTP モードは後方互換性と高度なユースケースのみを目的としています。stdio トランスポートはデフォルトであり、すべてのクライアントに推奨されるモードです。

このサーバーは 2 つのトランスポートモードをサポートしています：

| モード | デフォルト | 用途 |
|--------|-----------|------|
| **stdio** | ✅ はい | すべてのクライアントに推奨 - ローカル、安全、ネットワーク暴露なし |
| **HTTP** | ❌ いいえ | レガシーサポート、`--http` フラグが必要 |

### HTTP エンドポイント（--http 有効時）

| エンドポイント | トランスポート | ステータス |
|-----------|-----------|--------|
| `/mcp` | Streamable HTTP | アクティブ |
| `/sse` | レガシー SSE | 非推奨 |

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
- 「モニター1を見て」
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
- **GPUアクセラレーションアプリ**: PW_RENDERFULLCONTENT フラグ（Windows 8.1+）を使用して、Chrome、Electron、WPF アプリをキャプチャします。これは静止画スクリーンショットには適していますが、一部のアプリケーションでは制限がある場合があります。

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

### 主な機能

- **stdio トランスポート**: デフォルトのローカル専用モード - 安全、ネットワーク暴露なし
- **オプションの HTTP モード**: 後方互換性のための Streamable HTTP とレガシー SSE
- **セッション管理**: MCP-Session-Id ヘッダーによる自動クリーンアップ（HTTP モード）
- **ウィンドウ列挙**: EnumWindows API で表示中のアプリケーションを一覧表示
- **領域キャプチャ**: CopyFromScreen を使用した任意の画面領域キャプチャ
- **グレースフルシャットダウン**: Ctrl+C やプロセス終了時の適切なクリーンアップ
- **エラーハンドリング**: 意味のあるエラーメッセージを含む包括的な try-catch ブロック
- **CI/CD**: 自動テスト付き GitHub Actions

## クライアント設定例

### stdio モード（デフォルト / 推奨）

すべてのモダンな MCP クライアントは stdio トランスポートをサポートしています。これは **安全でローカル専用** のモードで、ネットワーク暴露がありません。

```json
{
  "mcpServers": {
    "windows-capture": {
      "command": "dotnet",
      "args": ["run", "--project", "C:\\path\\to\\WindowsScreenCaptureServer.csproj"]
    }
  }
}
```

または、公開済み実行ファイルの場合：

```json
{
  "mcpServers": {
    "windows-capture": {
      "command": "C:\\path\\to\\WindowsScreenCaptureServer.exe"
    }
  }
}
```

### HTTP モード（オプション / レガシー）

stdio をサポートしないクライアントでのみ必要です。サーバーを `--http` フラグ付きで実行する必要があります。

```json
{
  "mcpServers": {
    "windows-capture": {
      "url": "http://127.0.0.1:5000/mcp",
      "transport": "http"
    }
  }
}
```

## セキュリティ考慮事項

- **stdio モード**: ネットワーク暴露なし - 完全にローカルで安全
- **HTTP モード**: 必要な場合のみ有効化してください。デフォルトで localhost にバインドされます
- **Origin 検証**: HTTP エンドポイントは Origin ヘッダーを検証し、DNS リバインディング攻撃を防止
- **セッション分離**: 各クライアントは一意のセッション ID を取得し、自動的に期限切れ（1時間）

## 初回実行（HTTP モードのみ）

HTTP モードで外部アクセスを使用する場合は、PowerShell（管理者権限）で実行してください：

```powershell
# ローカルホストのみ（デフォルト - ファイアウォールルール不要）
# アクション不要

# 外部アクセス（推奨されません）
netsh advfirewall firewall add rule name="MCP Screen Capture" dir=in action=allow protocol=TCP localport=5000
```

## トラブルシューティング

| 問題 | 解決方法 |
|-------|----------|
| stdio モードが動作しない | MCP クライアント設定で実行可能ファイルのパスが正しいことを確認 |
| 接続が拒否される（HTTP モード） | ファイアウォールルールを確認し、サーバーが `--http` フラグ付きで実行されていることを確認 |
| /mcp で 404 エラー | 最新のビルドを使用し、サーバーが `--http` フラグ付きで実行されていることを確認 |
| 黒い画面が表示される | 管理者権限で実行 |
| ウィンドウが見つからない | ウィンドウが表示されていることを確認（システムトレイに最小化されていない） |

## ライセンス

MIT License - 詳細は LICENSE ファイルを参照してください。
