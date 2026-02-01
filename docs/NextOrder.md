# Windows側での次の作業手順

## 前提
WSL2での開発はここまで。残りの作業は**Windows側**で実施。

## 手順一覧

### 1. リポジトリの調整（完了済み）

✅ **LICENSEファイル**: 既に `utenadev` に更新済み

調整済み:
- `LICENSE`: `Copyright (c) 2026 utenadev(https://github.com/utenadev)`
- 必要に応じて `.gitignore` を調整

### 2. GitHubにpush

```bash
# Git設定（未設定の場合）
git config user.name "Your Name"
git config user.email "your@email.com"

# コミット＆プッシュ
git add .
git commit -m "Initial commit: MCP Windows Screen Capture Server (GDI+ version)"
git remote add origin https://github.com/YOUR_USERNAME/mcp-windows-screen-capture.git
git push -u origin main
```

### 3. Windows側でリポジトリをclone/pull

**Windows PowerShellで実行:**
```powershell
# 任意のディレクトリにclone
cd C:\Users\$env:USERNAME\Documents\Projects
git clone https://github.com/YOUR_USERNAME/mcp-windows-screen-capture.git
cd mcp-windows-screen-capture\src
```

### 4. Windows版OpenCodeで作業続行

**前提条件:**
- Windows版OpenCode CLIがインストールされていること
- .NET 8.0 SDKがインストールされていること

**Windows PowerShellで:**
```powershell
# リポジトリディレクトリでOpenCode起動
cd C:\Users\$env:USERNAME\Documents\Projects\mcp-windows-screen-capture
opencode

# または特定ファイルを開く場合
opencode src\Program.cs
```

## 動作確認手順（Windows側）

```powershell
# 1. ビルド
cd src
dotnet build -c Release

# 2. 初回のみ：ファイアウォール許可（管理者PowerShell必須）
netsh advfirewall firewall add rule name="MCP Screen Capture" dir=in action=allow protocol=TCP localport=5000 remoteip=172.16.0.0/12

# 3. 実行
dotnet run -- --ip_addr 0.0.0.0 --port 5000 --desktopNum 0
```

## WSL2側ClaudeCode設定（参考）

`~/.claude/config.json`:
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

## よくある問題と対処

| 問題 | 原因 | 対処 |
|------|------|------|
| ビルドエラー | .NET 8.0 SDK未インストール | https://dotnet.microsoft.com/download からインストール |
| WSL2から接続できない | ファイアウォール | 管理者PowerShellでファイアウォールルール追加 |
| 黒画面がキャプチャされる | 管理者権限不足 | 管理者PowerShellで`dotnet run`を実行 |
| OpenCodeが見つからない | パス未設定 | OpenCode CLIのインストールディレクトリをPATHに追加 |

## 注意事項

- **WSL2では実行しない**: GDI+のWindows API呼び出しが失敗します
- **必ずWindows側PowerShellで実行**
- シングルファイル発行も可能:
  ```powershell
  dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
  ```

## 別案: GitHub Actionsでビルド

ローカルに.NET SDKがなくても、GitHub Actionsで自動ビルド可能。

### 手順

1. `.github/workflows/build.yml` が既に作成済み
2. GitHubにpushすると自動的にActionsが実行
3. **Actionsタブ → 最新ビルド → Artifacts** からexeをダウンロード

### メリット
- Windowsローカルに.NET SDK不要
- シングルファイルexeが自動生成される
- タグ付きリリースも自動化可能

### 注意
- **実行は依然としてWindows側でのみ可能**
- ダウンロードしたexeはWindows上で実行すること
