# MCP Windows Screen Capture Server

Windows 11 向けのスクリーンキャプチャ MCP サーバー。Claude Desktop 用の stdio トランスポートに対応。

## 特徴

- **画面キャプチャ**: モニター、ウィンドウ、または指定領域のキャプチャ。
- **オーディオキャプチャ**: システム音やマイクの録音。
- **音声認識**: Whisper AI を使用したローカルでの高品質な文字起こし。
- **HTTP ストリーミング**: Claude と対話しながらブラウザでライブ映像を確認。

## クイックスタート

### 1. ビルド
```bash
dotnet build src/WindowsScreenCaptureServer.csproj -c Release
```

### 2. Claude Desktop の設定
`mcpConfig.json` に以下を追加します：
```json
{
  "mcpServers": {
    "windows-capture": {
      "command": "C:\\path\\to\\WindowsScreenCaptureServer.exe",
      "args": ["--httpPort", "5000"]
    }
  }
}
```

## 利用可能な MCP ツール（概要）

| ツール名 | 説明 |
|----------|------|
| `list_all` | すべてのモニターとウィンドウを一覧表示 |
| `capture` | 任意のターゲットを画像としてキャプチャ |
| `watch` | ターゲットの監視・ストリーミングを開始 |
| `listen` | 音声を文字起こし |

詳細な引数や使用例については、[**ツールガイド**](docs/TOOLS.ja.md) を参照してください。

## ドキュメント一覧

- [**ツールリファレンス**](docs/TOOLS.ja.md) - 詳細なコマンド一覧と使用例。
- [**開発者ガイド**](docs/DEVELOPMENT.ja.md) - ビルド、テスト、アーキテクチャの詳細。
- [**Whisper 音声認識**](docs/WHISPER.ja.md) - 音声認識機能とモデルについて。

## 動作要件

- Windows 11（または Windows 10 1803 以降）
- .NET 8.0 ランタイム/SDK

## ライセンス
MIT License. 詳細は [LICENSE](LICENSE) ファイルを参照してください。