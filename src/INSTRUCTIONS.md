# Windows Screen Capture Server 接続・テスト手順

このガイドは、MCP サーバーの `windows-capture` ツールに接続し、実際にスクリーンショットを取得するための手順です。

> 🔔 **重要**: このサーバーは **対話型デスクトップセッション（ユーザーがログオンしている状態）で実行する必要があります**。  
> サービスやバックグラウンドタスク、WSL2 のみでは `Graphics.CopyFromScreen` が失敗し、HTTP 500 エラーになります。

---

## ✅ 前提条件
- Windows 10/11（デスクトップ環境）
- ユーザーがログオン中（画面がロックされていない）
- `t\Artifacts\build_4\WindowsScreenCaptureServer.exe` が存在すること

---

## 🚀 手順

### 1. サーバーを起動（管理者権限のコマンドプロンプトで実行）

1. Windows の「コマンドプロンプト」を右クリック → **「管理者として実行」**
2. 以下のコマンドを実行：

```cmd
cd C:\workspace\mcp-windows-screen-capture\t\Artifacts\build_4
WindowsScreenCaptureServer.exe --ip_addr 127.0.0.1 --port 5001
```

✅ 成功すると以下のような出力が表示されます：
```
[Server] Started on http://127.0.0.1:5001
[Server] Default monitor: 0
[Capture] Found 1 monitors
```

> 💡 ウィンドウを閉じないでください。サーバーが終了します。

---

### 2. 別のコマンドプロンプト（通常権限でOK）でテスト

#### (a) `/sse` に接続して `clientId` を取得
```cmd
curl -s http://127.0.0.1:5001/sse --max-time 2
```

出力例：
```
event: endpoint
data: {"uri":"/message?clientId=033464bc-4887-41f1-8117-1b49524e4bea"}
```
→ `clientId=033464bc-4887-41f1-8117-1b49524e4bea` の部分をメモします。

#### (b) `capture_screen` を送信
```cmd
curl -X POST "http://127.0.0.1:5001/message?clientId=033464bc-4887-41f1-8117-1b49524e4bea" ^
  -H "Content-Type: application/json" ^
  -d "{\"method\":\"capture_screen\",\"params\":{\"monitor\":0},\"id\":1}"
```

✅ 成功時レスポンス例：
```json
{
  "id": 1,
  "result": {
    "content": [
      { "type": "image", "data": "data:image/jpeg;base64,/9j/4AAQSkZJRg..." },
      { "type": "text", "text": "Monitor 0" }
    ]
  }
}
```

#### (c) （オプション）画像を保存
```cmd
:: 応答を保存
curl -X POST "http://127.0.0.1:5001/message?clientId=xxx" -H "Content-Type: application/json" -d "{...}" > response.json

:: Base64 から JPEG 生成（PowerShell）
powershell -Command ^
  "$j = Get-Content response.json | ConvertFrom-Json; ^
   $imgData = $j.result.content | Where-Object type -eq 'image' | Select-Object -First 1; ^
   [System.IO.File]::WriteAllBytes('screen.jpg', [System.Convert]::FromBase64String($imgData.data.Split(',')[1]))"
```

---

## 🔍 トラブルシューティング

| 問題 | 対処 |
|------|------|
| HTTP 500 エラー | サーバーを**管理者コマンドプロンプトで起動**し、画面がアンロックされているか確認 |
| `clientId` が取れない | サーバーが起動していない / ポートが異なる → `netstat -ano ^| findstr :5001` で確認 |
| `list_monitors` が空 | モニターが検出されない → `Get-WmiObject Win32_DesktopMonitor` で確認 |

---

## 📎 付録: テスト用バッチスクリプト
`src\test.bat` を作成し、サーバー起動後、1クリックでテストできます（詳細は同ディレクトリ参照）。