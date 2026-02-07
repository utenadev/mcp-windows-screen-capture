# 開発者ガイド

このドキュメントでは、MCP Windows Screen Capture Server のビルド、テスト、および貢献を検討している開発者向けの情報を提供します。

## アーキテクチャ

本サーバーは .NET 8 で構築されており、以下のパターンを採用しています。

- **Program.cs**: エントリポイント。コマンドライン引数の解析、サービスの初期化、MCP ホストの起動を行います。
- **Services/**: 各ドメインのコアロジック。
  - `ScreenCaptureService`: GDI+ による画面・ウィンドウキャプチャとセッション管理。
  - `AudioCaptureService`: NAudio を使用したオーディオ録音。
  - `WhisperTranscriptionService`: Whisper.net を使用した AI 文字起こし。
- **Tools/ScreenCaptureTools.cs**: MCP ツールのインターフェース定義。ツール呼び出しを各サービスにマッピングします。
- **CaptureServices/**: モダンなキャプチャ API (Windows Graphics Capture) のための基盤 (開発中)。

### 重要: Stdio プロトコルとログ
ログを出力する際は、**必ず `Console.Error.WriteLine` を使用してください**。`stdout` に書き込むと、MCP クライアント（Claude Desktop 等）との JSON-RPC 通信が壊れます。

---

## ビルド

### 要件
- Windows 11 (または 10 1803+)
- .NET 8.0 SDK

### ビルドコマンド
```bash
# プロジェクトのビルド
dotnet build src/WindowsScreenCaptureServer.csproj -c Release

# 実行ファイルは以下に生成されます:
# src/bin/Release/net8.0-windows/win-x64/WindowsScreenCaptureServer.exe
```

---

## コード品質とフォーマット

本プロジェクトでは、.NET 標準の品質管理ツールを使用しています。

### コードの自動整形
`.editorconfig` に基づいて、インデント、`using` の整理、コードスタイルを自動修正します。

```bash
# プロジェクト全体のフォーマット
dotnet format
```

### 静的解析 (Lint)
ビルド時に自動的に静的解析が走ります。警告はビルド出力に表示されます。

```bash
# スタイル違反を確認する（修正は行わない）
dotnet format --verify-no-changes
```

---

## テスト

### E2E テスト
`stdio` を介した MCP インタラクションをシミュレートする E2E テストが含まれています。
```bash
dotnet test tests/E2ETests/E2ETests.csproj
```

### 手動テスト (stdio)
パイプを使用してツール呼び出しを直接テストできます。
```bash
# 初期化のテスト
echo '{"jsonrpc":"2.0","method":"initialize","params":{"protocolVersion":"2024-11-05"},"id":1}' | path/to/WindowsScreenCaptureServer.exe
```

---

## トラブルシューティング

| 問題 | 解決策 |
|-------|--------|
| 画面が真っ黒 | 制限されたセッションで実行されていないか確認してください。管理者として実行を試してください。 |
| 高い CPU 負荷 | ストリーミング監視は GDI+ キャプチャのため CPU 負荷が高くなります。`intervalMs` を大きくするか、不要なセッションを停止してください。 |
| モデルのダウンロード失敗 | Hugging Face へのインターネット接続を確認してください。モデルは実行ファイルの `models/` サブディレクトリに保存されます。 |