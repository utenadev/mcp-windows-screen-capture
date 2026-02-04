# このプロジェクトについて

MCP Windows Screen Capture Server は、Claude などの AI アシスタントのために設計された Windows 11 向けスクリーンキャプチャ MCP (Model Context Protocol) サーバーです。

## 概要

このサーバーは AI アシスタントに以下の機能を提供します：
- モニター、ウィンドウ、画面領域のスクリーンショットを撮影
- 利用可能なモニターと表示中のウィンドウの一覧表示
- 画面の変化を監視（連続キャプチャ）
- stdio トランスポートを介した Claude Desktop との連携

## 技術スタック

- **言語**: C# (.NET 8.0)
- **スクリーンキャプチャ**: GDI+ (Windows Graphics Device Interface)
- **MCP プロトコル**: 公式 Microsoft.ModelContextProtocol SDK
- **トランスポート**: stdio (Claude Desktop 用)

## バージョン履歴

| バージョン | 日付 | 説明 |
|---------|------|------|
| v2.1.0 | 2026-02-04 | stdio 専用化、HTTP モード削除 |
| v2.0.0 | 2026-02-04 | MCP SDK 移行 |
| v1.5.0 | 2026-01-XX | ウィンドウキャプチャツール |
| v1.4.0 | 2026-01-XX | デュアルトランスポート (HTTP + SSE) |
| v1.3.0 | 2026-01-XX | グレースフルシャットダウン |
| v1.2.0 | 2026-01-XX | ユニットテスト |
| v1.1.0 | 2026-01-XX | ツール改善 |
| v1.0.0 | 2026-01-XX | 初版リリース |

## ライセンス

MIT License - 詳細は [LICENSE](LICENSE) ファイルを参照してください。

## リンク

- [GitHub リポジトリ](https://github.com/utenadev/mcp-windows-screen-capture)
- [リリース](https://github.com/utenadev/mcp-windows-screen-capture/releases)
- [ドキュメント](README.ja.md)
- [課題トラッカー](https://github.com/utenadev/mcp-windows-screen-capture/issues)
