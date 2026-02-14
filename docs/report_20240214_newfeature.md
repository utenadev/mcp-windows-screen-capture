
📊 テスト結果総括（2026-02-14）

    ✅ 確認済み：正常動作する機能
     - mcp__WindowsDesktopUse__list_windows
      → ウィンドウ一覧（HWND・タイトル・座標）を正確に取得可能。YouTube ウィンドウ (hwnd=131134)
    の存在も確認済み。
     - mcp__WindowsDesktopUse__see / capture_region
      → 静的スクリーンショットが安定して取得できる。例：仗助の顔・背景・スタンドの表示まで正確にキャプ
    チャ可能。
      → 出力は JPEG base64（改行なし）で、feedback_20240614_base64_input_handling.md の要件を満たす。
     - ローカルドキュメント出力
      → docs/issue_20260214_qwen_video_coview_spec.md
    を正常に作成済み。内容は日本語・情報量削れず・全仕様を網羅。

    ---

    ❌ 実行不能／失敗した機能
     - mcp__WindowsDesktopUse__get_latest_video_frame（watch_video_v2 セッション対象）
      → 複数回実行しても hasFrame: false。原因：GDI+ によるキャプチャが YouTube の GPU
    加速表示に対応していない可能性が高い。初回初期化後もフレーム生成されない。
     - mcp__WindowsDesktopUse__get_latest_video_frame（camera_capture_stream セッション対象）
      → 同様に hasFrame: false。さらに、HTTP ストリームエンドポイント /stream/{id} は
    404（存在しない）。
     - mcp__WindowsDesktopUse__camera_capture_stream のファイル出力モード (returnFormat=file_path)
      → セッションは起動するが、返り値に filePath
    が含まれない。クライアント側でファイルを読み取る手段が提供されていない。
     - 動画内字幕の自動抽出（OCR）
      → 関連ツール（Tesseract 統合）はコードベースに存在せず、未実装。

    ---

    ⚠️ 技術的課題と推定原因
     - ハードウェア加速表示との不適合
      → YouTube / Chrome の動画は Direct3D でレンダリングされ、GDI+ (Graphics.CopyFromScreen)
    ではキャプチャできない。黒画面 or 無応答となる。
     - MCP サーバーの機能成熟度不足
      → watch_video_v2 や camera_capture_stream は [McpServerTool]
    属性で登録されているが、実際の実装が未完成または環境依存（例：Windows 10 1809+ でも動作保証なし）。
     - 同期ストリームの仕様欠如
      → LLM が求める「時刻付きフレーム＋音声トランスクリプト」を一度のリクエストで得る仕組みがない。現
    状は複数ツールを手動で組み合わせる必要あり。
     - タイムアウト問題
      → list_windows などのコマンドで MCP サーバーが Request timed out (-32001)
    を返す事例あり。高負荷時や初期化中の不安定性が懸念される。

    ---

    📌 次ステップの選択肢（ユーザー判断）
     1. 静的キャプチャを2秒間隔で自動繰り返すスクリプトを提供
       → capture_region を PowerShell/C# でループ実行。即座に可用。
     2. gemini-cli に「GPU対応キャプチャ」の実装依頼
       → Windows.Graphics.Capture への移行を提案。
     3. 今回のテスト結果を元に、Issue を更新／補足
       → docs/issue_20260214_qwen_video_coview_spec.md に「現状の制約」セクション追加。
