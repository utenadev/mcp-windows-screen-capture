# MCP Windows Screen Capture Server

Windows 11 向けのスクリーンキャプチャ MCP サーバー。`--ip_addr`、`--port`、`--desktopNum` オプションに対応。

> **⚠️ 実装メモ:** これは **GDI+ 版** です。Direct3D なしで確実に動作します。高性能な GPU キャプチャが必要な場合は、Direct3D/Windows Graphics Capture を自分で実装してください。この GDI+ 版は AI アシスタント用途には十分な速度です。

## 動作要件
- Windows 11（または Windows 10 1809 以降）
- .NET 8.0 SDK

## ビルド & 実行

```bash
# ビルド
dotnet build -c Release

# CLI オプション付きで実行（WSL2 接続には必須）
dotnet run -- --ip_addr 0.0.0.0 --port 5000 --desktopNum 0

# 単一ファイル公開
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
```

## CLI オプション
- `--ip_addr`: バインドする IP（WSL2 接続には `0.0.0.0`、ローカルのみは `127.0.0.1`）
- `--port`: ポート番号（デフォルト: 5000）
- `--desktopNum`: デフォルトモニター番号（0=プライマリ、1=セカンダリなど）

## Claude Code 設定

### Windows（ネイティブ）
`~/.claude/config.json`:
```json
{
  "mcpServers": {
    "windows-capture": {
      "command": "curl",
      "args": ["-N", "http://127.0.0.1:5000/sse"]
    }
  }
}
```

### WSL2
```json
{
  "mcpServers": {
    "windows-capture": {
      "command": "bash",
      "args": [
        "-c",
        "WIN_IP=$(ip route | grep default | awk '{print $3}'); curl -N http://${WIN_IP}:5000/sse"
      ]
    }
  }
}
```

## 初回セットアップ（ファイアウォール）
PowerShell（管理者権限）で実行:
```powershell
# WSL2 サブネットのみ許可（セキュア）
netsh advfirewall firewall add rule name="MCP Screen Capture" dir=in action=allow protocol=TCP localport=5000 remoteip=172.16.0.0/12
```
