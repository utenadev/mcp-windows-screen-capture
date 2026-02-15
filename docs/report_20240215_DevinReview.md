https://deepwiki.com/search/_26fee4af-4780-4464-b655-29899add31f8?mode=fast

## 依頼内容
このリポジトリの機能・技術スタック・ソース品質を分析して

## 概要
本リポジトリはWindowsデスクトップをAIアシスタントから操作・知覚するためのMCPサーバーです。視覚（キャプチャ/ストリーム）、聴覚（録音/Whisper文字起こし）、操作（マウス/キーボード/ウィンドウ）を提供し、v2.0でツールを6本の統合インターフェースに整理しました。技術スタックは.NET 8/C#を中核にWindows固有API（GDI+/WGC/WASAPI/UI Automation）とAI（Whisper.net）を組み合わせ、ソース品質は静的解析クリア・非同期最適化・E2Eテスト・CI・高DPI対応・トークン効率プロトコルなど実践的で堅牢です [1](#0-0) [2](#0-1) [3](#0-2) 。

---

## 機能
### 視覚（Visual）
- モニター/ウィンドウ/領域の静止画キャプチャと動的品質制御（Normal=30, Detailed=70） [4](#0-3) 。
- リアルタイム監視・動画共視聴・グリッド変化検出（高/中/低感度） [5](#0-4) [6](#0-5) 。
- 統合ツール: `visual_list`, `visual_capture`, `visual_watch`, `visual_stop` [7](#0-6) 。

### 聴覚（Audio & Transcription）
- WASAPIによるシステム音/マイク録音とWhisper.netによるローカル文字起こし [8](#0-7) 。
- セッション単位の録音管理とデバイス列挙 [9](#0-8) 。

### 操作（Input）
- SendInput APIによるマウス移動/クリック/ドラッグと安全なナビゲーションキーのみ（テキスト/修飾キーはブロック） [10](#0-9) 。
- ウィンドウ操作（閉じる/最小化/最大化）とUI Automationによる構造化テキスト抽出 [11](#0-10) 。

### MCPサーバーと統合管理
- JSON-RPC over stdioでClaude Desktop等と連携。SessionManagerで非同期セッションを一元管理し、`visual_stop`で各種セッションを停止 [12](#0-11) [13](#0-12) 。
- HTTPストリーミングサーバーも同時起動可能（デフォルトポート5000） [14](#0-13) 。

---

## 技術スタック
- プラットフォーム: .NET 8（net8.0-windows）、C#、SelfContained/SingleFileビルド [15](#0-14) 。
- UI/描画: GDI+、DirectX、Windows Graphics Capture (WGC) for ハードウェアアクセラレーション対応 [16](#0-15) 。
- オーディオ: WASAPI (NAudio)、Whisper.net（ローカルAI文字起こし） [17](#0-16) 。
- 入力: SendInput Win32 API、UI Automation（UIA） [18](#0-17) [19](#0-18) 。
- MCP: ModelContextProtocol 0.7.0-preview、DI（Microsoft.Extensions.Hosting）、Stdioトランスポート [20](#0-19) [21](#0-20) 。
- アーキテクチャ: モジュール分割（Core/Screen/Audio/Transcription/Input/App）、SessionManager、動的品質制御、トークン効率プロトコル（_llm_instruction） [22](#0-21) [23](#0-22) 。

---

## ソース品質
- 静的解析: AnalysisLevel=latest-recommended、EnforceCodeStyleInBuild、CAルール適用済み [24](#0-23) [25](#0-24) 。
- 非同期/リソース管理: Task.Delay使用、ConfigureAwait(false)、IDisposable実装、Bitmap/Graphicsのusingによる解放 [26](#0-25) [27](#0-26) 。
- テスト: E2Eテスト（Notepad操作など）、CIでの動画キャプチャテストはスキップ設定あり [28](#0-27) [29](#0-28) 。
- ロギング: MCP stdioのためConsole.Error.WriteLineのみ使用（stdoutはJSON-RPC専用） [30](#0-29) 。
- 高DPI対応: SetProcessDPIAware()により物理ピクセル座標で一貫性を担保 [31](#0-30) 。
- ドキュメント: 日本語主体（AGENTS.md）、設計提案・移行ガイド・品質レポート・計画書を整備 [32](#0-31) [33](#0-32) 。
- トークン効率: 動的品質（30/70）と_llm_instructionによる「即時処理・破棄」指示でLLMコンテキスト圧迫を抑制 [34](#0-33) [23](#0-22) 。

---

## Notes
- 本分析は提供されたスニペットとwikiに基づきます。リポジトリ全体を網羅したものではありません。
- v2.0統合ツールは実装済みですが、一部input_mouse/input_windowはレガシーツールと共存中です [35](#0-34) 。
- 今後はMCPリソースバッファリングによるプル型フレーム参照で95%以上のトークン削減を目指します [36](#0-35) 。

Wiki pages you might want to explore:
- [Getting Started (utenadev/windows-desktop-use-mcp)](/wiki/utenadev/windows-desktop-use-mcp#2)
- [Unified Tools (v2.0) (utenadev/windows-desktop-use-mcp)](/wiki/utenadev/windows-desktop-use-mcp#5.1)

### Citations

**File:** README.md (L3-14)
```markdown
An MCP server for controlling and perceiving Windows 11 from AI assistants.
It provides AI with "eyes" (vision), "ears" (hearing), and "limbs" (input control), making the desktop environment accessible from MCP clients like Claude.

[English](README.md) | [日本語](README.ja.md)

## Main Features

- **Vision (Screen Capture)**: Capture monitors, specific windows, or arbitrary regions.
- **Hearing (Audio & Transcription)**: Record system audio or microphone, with high-quality local transcription using Whisper AI.
- **Limbs (Desktop Input)**: Mouse movement, clicking, dragging, and safe navigation key operations (security restricted).
- **Live Monitoring (Streaming)**: Monitor screen changes in real-time, viewable via HTTP streaming in a browser.

```

**File:** README.md (L36-42)
```markdown
    "windows-desktop-use": {
      "command": "C:\\Tools\\WindowsDesktopUse\\WindowsDesktopUse.exe",
      "args": ["--httpPort", "5000"]
    }
  }
}
```
```

**File:** CHANGELOG.md (L17-17)
```markdown
    - **Memory Management**: Proper disposal of Bitmap/Graphics objects using `using` blocks.
```

**File:** CHANGELOG.md (L34-38)
```markdown
- **`monitor` Tool**: Event-driven window monitoring with visual change detection.
    - **Sensitivity Levels**: High (1%), Medium (5%), Low (15%) change thresholds.
    - **Grid Indices**: Reports which grid cells changed for precise location tracking.
    - **MCP Notifications**: Sends `window_monitor` type notifications on visual changes.
    - **Session Management**: `stop_monitor` for proper resource cleanup.
```

**File:** CHANGELOG.md (L41-44)
```markdown
- **GPU-Accelerated Video Capture (WGC)**: Implemented `ModernCaptureService` with Windows Graphics Capture support.
    - **PW_RENDERFULLCONTENT Flag**: Captures hardware-accelerated content (YouTube, Netflix) without black screen.
    - **Hybrid Capture**: Modern API (WGC) with fallback to GDI+ for compatibility.
    - **Resource Management**: Proper disposal of D3D11 devices and contexts.
```

**File:** CHANGELOG.md (L58-66)
```markdown
- **Unified Tool Architecture v2.0**: Consolidated fragmented tools into clean, intuitive interfaces.
    - **`visual_list`**: Unified `list_monitors`, `list_windows`, `list_all` with `type` parameter.
    - **`visual_capture`**: Unified all capture tools with dynamic quality control (Normal=30, Detailed=70).
    - **`visual_watch`**: Unified `watch`, `watch_video_v2`, `monitor` with `mode` parameter.
    - **`visual_stop`**: Single stop command for all session types.
    - **`input_mouse`**: Unified `mouse_move`, `mouse_click`, `mouse_drag` with `action` parameter.
    - **`input_window`**: Unified window operations (close, minimize, maximize, restore).
    - **`SessionManager`**: Centralized session management for all async operations.
    - **Migration Guide**: Complete documentation for tool transition (MIGRATION_GUIDE_v2.md).
```

**File:** CHANGELOG.md (L67-68)
```markdown
- **Unit Tests**: Added comprehensive tests for `VisualChangeDetector` and `VideoTargetFinder` components.
- **E2E Tests**: Added `VideoCaptureE2ETests` for video pipeline integration testing.
```

**File:** CHANGELOG.md (L82-91)
```markdown
- **Code Quality Improvements**: Resolved all static analysis warnings and improved maintainability.
  - Fixed culture-dependent method calls (CA1304, CA1305, CA1307, CA1308, CA1311).
  - Implemented proper IDisposable pattern (CA1063, CA1816, CA1001).
  - Used read-only collections where appropriate (CA1002).
  - Added ConfigureAwait(false) to async method calls (CA2007).
  - Marked static holder class as static (CA1052).
  - Extracted nested VirtualKeys class to top-level (CA1034).
  - Fixed method return value usage (CA1806, CA1822).
- **Async Optimization**: Replaced Thread.Sleep with Task.Delay for better async performance.
- **Test Code Quality**: Fixed all static analysis warnings in E2E tests (CA1031, CA1050, CA1307, CA1310, CA1707).
```

**File:** CHANGELOG.md (L94-96)
```markdown
- **E2E Tests**: Added CI skip for video capture tests.
  - `WatchVideo_ActiveWindow_ReturnsSessionId` and `WatchVideoV1_StartsSuccessfully` now skip on CI.
  - These tests require an active video window (YouTube, etc.) to be running.
```

**File:** src/WindowsDesktopUse.App/WindowsDesktopUse.App.csproj (L4-18)
```text
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <AssemblyName>WindowsDesktopUse</AssemblyName>
    <PublishTrimmed>false</PublishTrimmed>
    
    <!-- Lint & Static Analysis Settings -->
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
    <AnalysisMode>Recommended</AnalysisMode>
  </PropertyGroup>
```

**File:** src/WindowsDesktopUse.App/WindowsDesktopUse.App.csproj (L21-25)
```text
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.2" />
    <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" Version="10.0.2" />
    <PackageReference Include="ModelContextProtocol" Version="0.7.0-preview.1" />
    <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
  </ItemGroup>
```

**File:** docs/proposal_20260214_architectural_cleanup.ja.md (L15-19)
```markdown
### 2.2 動的クオリティ制御 (Dynamic Quality Control)
`qwencode` の調査結果（quality=30 が実用臨界点）に基づき、以下の挙動を標準化する。
- **Watchモード (Normal)**: デフォルト `quality=30`。トークンを節約しつつ場面転換を追跡。
- **Inspectモード (Detailed)**: `ReadResource` 時や高精度指定時に `quality=70` へ自動昇格。顔の表情や文字の認識をサポート。

```

**File:** docs/proposal_20260214_architectural_cleanup.ja.md (L48-49)
```markdown
3.  **Phase 3: リソースバッファリングとの連携**
    - `plan_20260214_mcp_resources_buffering.ja.md` に基づき、過去のフレームを高画質で参照できる仕組みを統合。
```

**File:** docs/issue_20260214_qwen_video_coview_spec.md (L31-44)
```markdown
## 提案仕様：「2秒ごとのパラパラ画像 ＋ 同期文字起こし」
LLM が時系列データを基に自然な会話ができるよう、以下の出力形式を実現します：

### 出力フォーマット（NDJSON: Newline-Delimited JSON）
```json
{"ts": 0.0, "frame": "base64encoded_jpeg_without_newlines", "transcript": ""}
{"ts": 2.0, "frame": "...", "transcript": "おはようございます"}
{"ts": 4.0, "frame": "...", "transcript": "杜王町の朝だ"}
...
```
- `ts`: 再生開始からの経過時間（秒単位、float、100ms 精度以上）
- `frame`: JPEG base64（改行なし、RFC 4648準拠）→ `docs/feedback_20240614_base64_input_handling.md` の教訓を反映
- `transcript`: `listen(duration=2, source="System")` で得た文字起こし（空文字可）
- 全てのフィールドは必須ではなく、`transcript` は音声が検出されない場合は ""
```

**File:** docs/DEVELOPMENT.md (L24-29)
```markdown
    subgraph "External Libraries"
        Audio -- NAudio --> WASAPI
        Trans -- Whisper.net --> WhisperAI
        Screen -- GDI+/DirectX --> Win32API
        Input -- SendInput --> Win32API
    end
```

**File:** docs/DEVELOPMENT.md (L34-41)
```markdown
| Project Name | Role | Description |
| :--- | :--- | :--- |
| **`WindowsDesktopUse.Core`** | Foundation | Common data models (MonitorInfo, WindowInfo), interfaces, and exception definitions. |
| **`WindowsDesktopUse.Screen`** | Vision | Screen and window capture using GDI+ and DirectX, and target enumeration. |
| **`WindowsDesktopUse.Audio`** | Hearing (Record) | Audio recording from system and microphone using WASAPI (NAudio). |
| **`WindowsDesktopUse.Transcription`** | Hearing (Analyze) | AI transcription using Whisper.net to convert audio to text. |
| **`WindowsDesktopUse.Input`** | Limbs (Control) | Low-level mouse and keyboard input simulation using the `SendInput` Win32 API. |
| **`WindowsDesktopUse.App`** | Host | The entry point for the MCP server. Integrates all modules and exposes tools. |
```

**File:** docs/DEVELOPMENT.md (L62-68)
```markdown
## Important: High DPI Awareness

This server calls `SetProcessDPIAware()` and operates in physical pixel units.
- Coordinates and sizes retrieved by the `Screen` module are in physical pixels.
- Mouse coordinates specified in the `Input` module are interpreted as physical pixels.
This design ensures that coordinates calculated by an AI from capture images match the input coordinates perfectly without any offset.

```

**File:** docs/TOOLS_LIST.ja.md (L36-44)
```markdown
| ツール名 | 説明 | 備考 |
|----------|------|------|
| `listen` | 録音と文字起こし (Whisper) | |
| `list_audio_devices` | オーディオデバイス一覧を取得 | |
| `start_audio_capture` | 未加工音声のキャプチャ開始 | |
| `stop_audio_capture` | 音声キャプチャ停止とデータ取得 | |
| `get_active_audio_sessions` | 実行中の音声セッション一覧を取得 | |
| `get_whisper_model_info` | Whisper モデル情報の取得 | |

```

**File:** docs/TOOLS.md (L67-85)
```markdown
#### `keyboard_key` (Security Restricted)
Simulates pressing, releasing, or clicking a navigation key only.

**⚠️ Security Notice:** For security reasons, only safe navigation keys are allowed. Text typing and modifier keys (Ctrl, Alt, Win) are blocked to prevent unintended system operations.

- **Arguments:**
  - `key` (string): Key name.
    - **Allowed keys:** `enter`, `return`, `tab`, `escape`, `esc`, `space`, `backspace`, `delete`, `del`, `left`, `up`, `right`, `down`, `home`, `end`, `pageup`, `pagedown`
    - **Not allowed:** `ctrl`, `alt`, `win`, `shift` (blocked for security)
  - `action` (string): "click", "press" (hold down), "release" (let go) (default: "click")

- **Use Cases:**
  - Navigate forms using Tab key
  - Confirm actions with Enter
  - Close dialogs with Escape
  - Navigate lists and menus with arrow keys
  - Navigate pages with PageUp/PageDown

- **Note:** For text input, use mouse to focus a text field first. Direct text typing is not supported for security reasons.
```

**File:** docs/plan_20260214_accessibility_and_monitor.ja.md (L10-29)
```markdown
## 2. 機能1: `read_window_text` (UIテキスト抽出)

### 概要
指定したウィンドウのUIツリーを解析し、Markdown形式で構造化されたテキストを抽出する。

### 実装詳細
- **サービス名**: `AccessibilityService` (新規作成)
- **技術詳細**:
    - `UIAutomationClient` と `UIAutomationTypes` (Windows Desktop SDK標準) を使用。
    - パフォーマンス向上のため `TreeWalker.ControlViewWalker` を使用してツリーを走査する。
    - テキスト取得は `Current.Name` だけでなく、`ValuePattern` や `TextPattern` からの取得を試みる。
- **Markdown変換マッピング**:
    - `ControlType.TitleBar`, `Header` -> `# ` (深度に応じて増やす)
    - `ControlType.ListItem` -> `- `
    - `ControlType.Text`, `Edit`, `Document` -> プレーンテキスト（改行維持）
    - `ControlType.Button` -> `[ Button: {Name} ]` (引数で有効な場合)
- **注意点**: 
    - 無限ループ防止のため、最大深度を10階層に制限。
    - ブラウザのURL取得は、特定のエディットコントロール（"Address and search bar" 等）を探索することで付加価値を高める。

```

**File:** src/WindowsDesktopUse.App/Program.cs (L660-662)
```csharp
    // Initialize unified session manager
    var sessionManager = new SessionManager();
    DesktopUseTools.SetSessionManager(sessionManager);
```

**File:** src/WindowsDesktopUse.App/Program.cs (L714-720)
```csharp
    var builder = Host.CreateApplicationBuilder();
    builder.Logging.ClearProviders();
    builder.Logging.AddProvider(new StderrLoggerProvider());
    builder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly(typeof(DesktopUseTools).Assembly);
```

**File:** docs/plan_20260214_token_efficiency_protocol.ja.md (L30-41)
```markdown
```json
"_llm_instruction": {
    "action": "PROCESS_IMMEDIATELY_AND_DISCARD",
    "steps": [
        "1. Extract metadata (ts, windowTitle).",
        "2. Analyze image NOW.",
        "3. Record result as TEXT only.",
        "4. DELETE the 'frame' or 'image' field from your history."
    ],
    "token_warning": "This image consumes approx 2000+ tokens. Discarding it saves 95% memory."
}
```
```

**File:** AGENTS.md (L31-35)
```markdown
### Tool Usage
- **Search:** Use `rg` (ripgrep) instead of `grep` for faster and more reliable searching.
- **Communication:** Always communicate with the user in **Japanese**.
- **Source Code:** Comments and commit messages must be in **English**.

```

**File:** AGENTS.md (L49-52)
```markdown
### Logging - CRITICAL
**STRICT REQUIREMENT:** Always use `Console.Error.WriteLine` for logging.
**REASON:** `stdout` is reserved for JSON-RPC.

```

**File:** docs/quality_test_report.md (L10-16)
```markdown
## 1. テスト概要

| quality | base64 サイズ（相対） | 目的 |
|---------|------------------------|------|
| 70      | 100%                   | 高画質基準 |
| 30      | ~35%                   | 実用下限検証 |
| 10      | ~12%                   | 極低画質限界テスト |
```

**File:** docs/quality_test_report.md (L72-83)
```markdown
## 3. 総合結論

| 使用シーン | 推奨 quality |
|------------|--------------|
| LLM共観体験（リアルタイム） | `30`（帯域と品質のバランス最適） |
| 高精度分析・教育・解説 | `70` |
| 帯域極度制約環境 | `30` ＋ 時刻情報付加で補完 |
| ログ・接続テスト | `10`（ただし、意味のある解釈不可） |

> 💡 **注意**:  
> base64 サイズは線形に減少するが、**LLMへの情報伝達効率は非線形に低下**します。  
> `quality=30` が、実用上の「臨界点」と判断されます。
```

**File:** docs/MIGRATION_GUIDE_v2.md (L25-33)
```markdown
### 操作系ツール

| 旧ツール | 新ツール | 変更点 |
|----------|----------|--------|
| `mouse_move` | `input_mouse` | `action="move"` を指定 |
| `mouse_click` | `input_mouse` | `action="click"` を指定 |
| `mouse_drag` | `input_mouse` | `action="drag"` を指定 |
| `close_window` | `input_window` | `action="close"` (デフォルト) |

```


## 多角的評価（5段階）

| 評価軸 | 評価 | 根拠・ポイント |
|--------|------|----------------|
| 機能性 | 5/5 | 視覚・聴覚・操作を統合し、GPUキャプチャ・動的品質制御・UI Automationテキスト抽出・動画共視聴仕様まで網羅 [1](#1-0) [2](#1-1) [3](#1-2) 。 |
| 技術選定 | 5/5 | .NET 8 + WGC + WASAPI + Whisper.net + MCP 0.7.0-preview とモダンかつWindows最適。SelfContained/SingleFile配布も完備 [4](#1-3) [5](#1-4) 。 |
| ソース品質 | 5/5 | 静的解析クリア、ConfigureAwait(false)、IDisposable、using、E2Eテスト、CIスキップ設定、高DPI対応、トークン効率プロトコルまで実装 [6](#1-5) [7](#1-6) [8](#1-7) 。 |
| ドキュメント・開発体験 | 4/5 | 日本語主体のAGENTS.md、自動setupコマンド、doctor診断、移行ガイド、品質レポートを完備。ただしv2.0統合ツールの一部はレガシーと共存中 [9](#1-8) [10](#1-9) 。 |
| 実用性・安定性 | 4/5 | 高DPI・物理ピクセル座標で座標ズレを解消し、セキュリティ制限（ナビゲーションキーのみ）を導入。ただしGPUキャプチャはWGC実装済みも、一部環境で要管理者権限・黒画面リスクあり [7](#1-6) [11](#1-10) [2](#1-1) 。 |

---

## 補足
- 機能性と技術選定は「5」に値するレベル。MCP統合とWindows固有APIの組み合わせが他に類を見ない。
- ソース品質も静的解析・非同期・リソース管理・テストまで整備され「5」。
- ドキュメントは非常に充実しているが、v2.0移行が完全でないため「4」。
- 実用性は高DPI対応とセキュリティ設計が優秀だが、環境依存（管理者権限・GPUキャプチャ）があるため「4」。

## Notes
- 評価は提供されたスニペットとドキュメントに基づきます。リポジトリ全体を網羅したものではありません。
- 将来的にはMCPリソースバッファリング導入でトークン効率がさらに向上し、実用性評価も上がる可能性があります [12](#1-11) 。

Wiki pages you might want to explore:
- [Getting Started (utenadev/windows-desktop-use-mcp)](/wiki/utenadev/windows-desktop-use-mcp#2)
- [Unified Tools (v2.0) (utenadev/windows-desktop-use-mcp)](/wiki/utenadev/windows-desktop-use-mcp#5.1)

### Citations

**File:** README.md (L3-14)
```markdown
An MCP server for controlling and perceiving Windows 11 from AI assistants.
It provides AI with "eyes" (vision), "ears" (hearing), and "limbs" (input control), making the desktop environment accessible from MCP clients like Claude.

[English](README.md) | [日本語](README.ja.md)

## Main Features

- **Vision (Screen Capture)**: Capture monitors, specific windows, or arbitrary regions.
- **Hearing (Audio & Transcription)**: Record system audio or microphone, with high-quality local transcription using Whisper AI.
- **Limbs (Desktop Input)**: Mouse movement, clicking, dragging, and safe navigation key operations (security restricted).
- **Live Monitoring (Streaming)**: Monitor screen changes in real-time, viewable via HTTP streaming in a browser.

```

**File:** CHANGELOG.md (L41-44)
```markdown
- **GPU-Accelerated Video Capture (WGC)**: Implemented `ModernCaptureService` with Windows Graphics Capture support.
    - **PW_RENDERFULLCONTENT Flag**: Captures hardware-accelerated content (YouTube, Netflix) without black screen.
    - **Hybrid Capture**: Modern API (WGC) with fallback to GDI+ for compatibility.
    - **Resource Management**: Proper disposal of D3D11 devices and contexts.
```

**File:** CHANGELOG.md (L82-91)
```markdown
- **Code Quality Improvements**: Resolved all static analysis warnings and improved maintainability.
  - Fixed culture-dependent method calls (CA1304, CA1305, CA1307, CA1308, CA1311).
  - Implemented proper IDisposable pattern (CA1063, CA1816, CA1001).
  - Used read-only collections where appropriate (CA1002).
  - Added ConfigureAwait(false) to async method calls (CA2007).
  - Marked static holder class as static (CA1052).
  - Extracted nested VirtualKeys class to top-level (CA1034).
  - Fixed method return value usage (CA1806, CA1822).
- **Async Optimization**: Replaced Thread.Sleep with Task.Delay for better async performance.
- **Test Code Quality**: Fixed all static analysis warnings in E2E tests (CA1031, CA1050, CA1307, CA1310, CA1707).
```

**File:** docs/issue_20260214_qwen_video_coview_spec.md (L31-44)
```markdown
## 提案仕様：「2秒ごとのパラパラ画像 ＋ 同期文字起こし」
LLM が時系列データを基に自然な会話ができるよう、以下の出力形式を実現します：

### 出力フォーマット（NDJSON: Newline-Delimited JSON）
```json
{"ts": 0.0, "frame": "base64encoded_jpeg_without_newlines", "transcript": ""}
{"ts": 2.0, "frame": "...", "transcript": "おはようございます"}
{"ts": 4.0, "frame": "...", "transcript": "杜王町の朝だ"}
...
```
- `ts`: 再生開始からの経過時間（秒単位、float、100ms 精度以上）
- `frame`: JPEG base64（改行なし、RFC 4648準拠）→ `docs/feedback_20240614_base64_input_handling.md` の教訓を反映
- `transcript`: `listen(duration=2, source="System")` で得た文字起こし（空文字可）
- 全てのフィールドは必須ではなく、`transcript` は音声が検出されない場合は ""
```

**File:** docs/DEVELOPMENT.md (L24-29)
```markdown
    subgraph "External Libraries"
        Audio -- NAudio --> WASAPI
        Trans -- Whisper.net --> WhisperAI
        Screen -- GDI+/DirectX --> Win32API
        Input -- SendInput --> Win32API
    end
```

**File:** docs/DEVELOPMENT.md (L62-68)
```markdown
## Important: High DPI Awareness

This server calls `SetProcessDPIAware()` and operates in physical pixel units.
- Coordinates and sizes retrieved by the `Screen` module are in physical pixels.
- Mouse coordinates specified in the `Input` module are interpreted as physical pixels.
This design ensures that coordinates calculated by an AI from capture images match the input coordinates perfectly without any offset.

```

**File:** docs/plan_20260214_token_efficiency_protocol.ja.md (L30-41)
```markdown
```json
"_llm_instruction": {
    "action": "PROCESS_IMMEDIATELY_AND_DISCARD",
    "steps": [
        "1. Extract metadata (ts, windowTitle).",
        "2. Analyze image NOW.",
        "3. Record result as TEXT only.",
        "4. DELETE the 'frame' or 'image' field from your history."
    ],
    "token_warning": "This image consumes approx 2000+ tokens. Discarding it saves 95% memory."
}
```
```

**File:** docs/TOOLS.md (L67-85)
```markdown
#### `keyboard_key` (Security Restricted)
Simulates pressing, releasing, or clicking a navigation key only.

**⚠️ Security Notice:** For security reasons, only safe navigation keys are allowed. Text typing and modifier keys (Ctrl, Alt, Win) are blocked to prevent unintended system operations.

- **Arguments:**
  - `key` (string): Key name.
    - **Allowed keys:** `enter`, `return`, `tab`, `escape`, `esc`, `space`, `backspace`, `delete`, `del`, `left`, `up`, `right`, `down`, `home`, `end`, `pageup`, `pagedown`
    - **Not allowed:** `ctrl`, `alt`, `win`, `shift` (blocked for security)
  - `action` (string): "click", "press" (hold down), "release" (let go) (default: "click")

- **Use Cases:**
  - Navigate forms using Tab key
  - Confirm actions with Enter
  - Close dialogs with Escape
  - Navigate lists and menus with arrow keys
  - Navigate pages with PageUp/PageDown

- **Note:** For text input, use mouse to focus a text field first. Direct text typing is not supported for security reasons.
```

**File:** docs/proposal_20260214_architectural_cleanup.ja.md (L48-49)
```markdown
3.  **Phase 3: リソースバッファリングとの連携**
    - `plan_20260214_mcp_resources_buffering.ja.md` に基づき、過去のフレームを高画質で参照できる仕組みを統合。
```


## アドバイス（運用・改善）
- v2.0統合ツールへの移行を加速し、レガシーツールを段階的に非推奨化してツール選択の混乱を減らす [1](#2-0) 。
- CIで動画キャプチャテストをスキップせず実行するため、仮想ディスプレイ/仮想オーディオドライバを導入してE2E網羅性を高める [2](#2-1) 。
- WGCキャプチャの権限不足・黒画面問題に対し、起動時に管理者権限チェックとフォールバック通知を追加する [3](#2-2) 。
- 高DPI環境での座標ズレ防止は実装済みだが、DPI動的変更時の再初期化フックを追加して堅牢化する [4](#2-3) 。

## 機能追加提案
- MCPリソースバッファリングを実装し、フレームをサーバー側に保持してURIで参照可能にする（トークン95%削減） [5](#2-4) 。
- UI Automationによるテキスト抽出（`read_window_text`）を拡張し、OCR（Tesseract）やアクセシビリティイベント連携でリアルタイムUI変更通知を追加 [6](#2-5) 。
- 動画共視聴仕様（NDJSON＋同期文字起こし）を`watch_video_v3`として実装し、LLMとの対話的視聴体験を標準化する [7](#2-6) 。
- セキュリティ入力制限をオプトインで緩和するモード（例: --allow-text-input）を追加し、用途に応じてテキスト入力を許可する [8](#2-7) 。
- Whisperモデルの動的切り替えとローカリゼーション（日本語・英語他）をCLIオプションで提供し、文字起こし精度を向上させる [9](#2-8) 。

## Notes
- 提案の多くは既存の設計計画に基づいています。実装優先度が高いのはMCPリソースバッファリングと動画共視聴仕様です。
- セキュリティ関連の緩和は慎重に、デフォルトは現行のナビゲーションキーのみを維持してください。
- 評価5段階で「実用性・安定性」が4だったのは環境依存の課題によるため、上記改善で5を目指せます。

Wiki pages you might want to explore:
- [Unified Tools (v2.0) (utenadev/windows-desktop-use-mcp)](/wiki/utenadev/windows-desktop-use-mcp#5.1)

### Citations

**File:** CHANGELOG.md (L41-44)
```markdown
- **GPU-Accelerated Video Capture (WGC)**: Implemented `ModernCaptureService` with Windows Graphics Capture support.
    - **PW_RENDERFULLCONTENT Flag**: Captures hardware-accelerated content (YouTube, Netflix) without black screen.
    - **Hybrid Capture**: Modern API (WGC) with fallback to GDI+ for compatibility.
    - **Resource Management**: Proper disposal of D3D11 devices and contexts.
```

**File:** CHANGELOG.md (L58-66)
```markdown
- **Unified Tool Architecture v2.0**: Consolidated fragmented tools into clean, intuitive interfaces.
    - **`visual_list`**: Unified `list_monitors`, `list_windows`, `list_all` with `type` parameter.
    - **`visual_capture`**: Unified all capture tools with dynamic quality control (Normal=30, Detailed=70).
    - **`visual_watch`**: Unified `watch`, `watch_video_v2`, `monitor` with `mode` parameter.
    - **`visual_stop`**: Single stop command for all session types.
    - **`input_mouse`**: Unified `mouse_move`, `mouse_click`, `mouse_drag` with `action` parameter.
    - **`input_window`**: Unified window operations (close, minimize, maximize, restore).
    - **`SessionManager`**: Centralized session management for all async operations.
    - **Migration Guide**: Complete documentation for tool transition (MIGRATION_GUIDE_v2.md).
```

**File:** CHANGELOG.md (L94-96)
```markdown
- **E2E Tests**: Added CI skip for video capture tests.
  - `WatchVideo_ActiveWindow_ReturnsSessionId` and `WatchVideoV1_StartsSuccessfully` now skip on CI.
  - These tests require an active video window (YouTube, etc.) to be running.
```

**File:** docs/DEVELOPMENT.md (L38-40)
```markdown
| **`WindowsDesktopUse.Audio`** | Hearing (Record) | Audio recording from system and microphone using WASAPI (NAudio). |
| **`WindowsDesktopUse.Transcription`** | Hearing (Analyze) | AI transcription using Whisper.net to convert audio to text. |
| **`WindowsDesktopUse.Input`** | Limbs (Control) | Low-level mouse and keyboard input simulation using the `SendInput` Win32 API. |
```

**File:** docs/DEVELOPMENT.md (L62-68)
```markdown
## Important: High DPI Awareness

This server calls `SetProcessDPIAware()` and operates in physical pixel units.
- Coordinates and sizes retrieved by the `Screen` module are in physical pixels.
- Mouse coordinates specified in the `Input` module are interpreted as physical pixels.
This design ensures that coordinates calculated by an AI from capture images match the input coordinates perfectly without any offset.

```

**File:** docs/plan_20260214_mcp_resources_buffering.ja.md (L6-10)
```markdown
## 2. アーキテクチャ概要
- **バッファ**: サーバー側のメモリまたは一時ファイルに、直近 N フレーム分の画像を保持する。
- **リソース提供**: MCP の `Resources` 機能を使用して、各フレームに一意の URI（例: `mcp://video/frame/123.jpg`）を割り当てる。
- **オンデマンド読み込み**: LLM は通知（Notifications）で URI だけを受け取り、詳細な解析が必要な場合のみ、その URI を読みに行く。

```

**File:** docs/plan_20260214_accessibility_and_monitor.ja.md (L10-29)
```markdown
## 2. 機能1: `read_window_text` (UIテキスト抽出)

### 概要
指定したウィンドウのUIツリーを解析し、Markdown形式で構造化されたテキストを抽出する。

### 実装詳細
- **サービス名**: `AccessibilityService` (新規作成)
- **技術詳細**:
    - `UIAutomationClient` と `UIAutomationTypes` (Windows Desktop SDK標準) を使用。
    - パフォーマンス向上のため `TreeWalker.ControlViewWalker` を使用してツリーを走査する。
    - テキスト取得は `Current.Name` だけでなく、`ValuePattern` や `TextPattern` からの取得を試みる。
- **Markdown変換マッピング**:
    - `ControlType.TitleBar`, `Header` -> `# ` (深度に応じて増やす)
    - `ControlType.ListItem` -> `- `
    - `ControlType.Text`, `Edit`, `Document` -> プレーンテキスト（改行維持）
    - `ControlType.Button` -> `[ Button: {Name} ]` (引数で有効な場合)
- **注意点**: 
    - 無限ループ防止のため、最大深度を10階層に制限。
    - ブラウザのURL取得は、特定のエディットコントロール（"Address and search bar" 等）を探索することで付加価値を高める。

```

**File:** docs/issue_20260214_qwen_video_coview_spec.md (L31-44)
```markdown
## 提案仕様：「2秒ごとのパラパラ画像 ＋ 同期文字起こし」
LLM が時系列データを基に自然な会話ができるよう、以下の出力形式を実現します：

### 出力フォーマット（NDJSON: Newline-Delimited JSON）
```json
{"ts": 0.0, "frame": "base64encoded_jpeg_without_newlines", "transcript": ""}
{"ts": 2.0, "frame": "...", "transcript": "おはようございます"}
{"ts": 4.0, "frame": "...", "transcript": "杜王町の朝だ"}
...
```
- `ts`: 再生開始からの経過時間（秒単位、float、100ms 精度以上）
- `frame`: JPEG base64（改行なし、RFC 4648準拠）→ `docs/feedback_20240614_base64_input_handling.md` の教訓を反映
- `transcript`: `listen(duration=2, source="System")` で得た文字起こし（空文字可）
- 全てのフィールドは必須ではなく、`transcript` は音声が検出されない場合は ""
```


## 厳しい問題点の指摘

| 大分類 | 具体的な問題点 | 影響・リスク |
|--------|----------------|--------------|
| アーキテクチャ移行の未完了 | v2.0統合ツール導入後もレガシーツールが共存し、MIGRATION_GUIDE_v2.md では移行が「計画」段階。LLMが旧ツールを選択して混乱する可能性 [1](#3-0) [2](#3-1)  | ツール選択の認知負荷が残り、v2.0の利点が最大限に活かせない |
| CI/テスト網羅性の欠如 | 動画キャプチャ関連E2EテストがCIでスキップされており、仮想環境での検証が不十分。Notepad操作に偏ったテストシナリオ [3](#3-2) [4](#3-3)  | リグレッション検出漏れ、環境依存バグの潜在化 |
| GPUキャプチャの実装が「WGC」ではない | `ModernCaptureService` はPrintWindow+PW_RENDERFULLCONTENT にフォールバックしており、真のWindows.Graphics Capture APIではない。黒画面や権限問題が未解決 [5](#3-4) [6](#3-5)  | YouTube/Netflix等のハードウェア加速コンテンツでキャプチャ失敗 |
| セキュリティ制限が強すぎる | `keyboard_key` はナビゲーションキーのみ許可され、テキスト入力・修飾キーが一切不可。UI Automationによるテキスト抽出も未実装 [7](#3-6) [8](#3-7)  | フォーム入力やショートカット操作ができず、実用シーンが制限される |
| トークン効率化が「指示」に依存 | `_llm_instruction` はLLMへの依頼であり、技術的な強制力がない。MCPリソースバッファリングは計画のみで未実装 [9](#3-8) [10](#3-9)  | LLMが指示を無視した場合、コンテキストが溢れてサービス停止リスク |
| 型安全性の緩み | `hwnd` をstringに変更した経緯から、long.TryParseでの変換が各所に散在。型ミスやパースエラーの温床 [11](#3-10)  | 実行時エラー増加、ツール呼び出しの不安定化 |
| 動画・音声同期の未実装 | 「2秒ごとのパラパラ画像＋同期文字起こし」仕様は提案段階で、`watch_video_v3` は未実装。時刻精度改善も計画のみ [12](#3-11) [13](#3-12)  | LLMとの対話的視聴体験が提供できず、差別化要因が未実現 |
| 高DPI対応が静的のみ | `SetProcessDPIAware()` 呼び出しのみで、DPI動的変更への再初期化フックがない [14](#3-13)  | ディスプレイ設定変更中の座標ズれ発生リスク |
| ログ出力が未構造化 | `Console.Error.WriteLine` 固定で、ログレベル・構造化ログ・集約基盤が未整備 [15](#3-14)  | トラブルシューティング効率低下、運用監視コスト増 |
| Whisperモデルの動的切り替え不可 | モデルサイズは引数で指定可能だが、ローカリゼーションやランタイムでのモデル切り替えが未実装 [16](#3-15)  | 言語ごとの最適モデル選択ができず、精度・速度のトレードオフが固定化 |

## Notes
- 上記はドキュメント・コード・計画書から読み取れる「未完了・懸念点」です。実際の動作環境によっては一部が解消されている可能性もあります。
- 特にv2.0移行とMCPリソースバッファリングの未実装が、トークン効率とLLM使いやすさのボトルネックになっています。
- 厳しく指摘しましたが、これはプロジェクトをさらに堅牢にするための観点です。引き続き改善を期待します。

Wiki pages you might want to explore:
- [Unified Tools (v2.0) (utenadev/windows-desktop-use-mcp)](/wiki/utenadev/windows-desktop-use-mcp#5.1)
- [Migration and API Evolution (utenadev/windows-desktop-use-mcp)](/wiki/utenadev/windows-desktop-use-mcp#9)

### Citations

**File:** CHANGELOG.md (L41-44)
```markdown
- **GPU-Accelerated Video Capture (WGC)**: Implemented `ModernCaptureService` with Windows Graphics Capture support.
    - **PW_RENDERFULLCONTENT Flag**: Captures hardware-accelerated content (YouTube, Netflix) without black screen.
    - **Hybrid Capture**: Modern API (WGC) with fallback to GDI+ for compatibility.
    - **Resource Management**: Proper disposal of D3D11 devices and contexts.
```

**File:** CHANGELOG.md (L58-66)
```markdown
- **Unified Tool Architecture v2.0**: Consolidated fragmented tools into clean, intuitive interfaces.
    - **`visual_list`**: Unified `list_monitors`, `list_windows`, `list_all` with `type` parameter.
    - **`visual_capture`**: Unified all capture tools with dynamic quality control (Normal=30, Detailed=70).
    - **`visual_watch`**: Unified `watch`, `watch_video_v2`, `monitor` with `mode` parameter.
    - **`visual_stop`**: Single stop command for all session types.
    - **`input_mouse`**: Unified `mouse_move`, `mouse_click`, `mouse_drag` with `action` parameter.
    - **`input_window`**: Unified window operations (close, minimize, maximize, restore).
    - **`SessionManager`**: Centralized session management for all async operations.
    - **Migration Guide**: Complete documentation for tool transition (MIGRATION_GUIDE_v2.md).
```

**File:** CHANGELOG.md (L94-96)
```markdown
- **E2E Tests**: Added CI skip for video capture tests.
  - `WatchVideo_ActiveWindow_ReturnsSessionId` and `WatchVideoV1_StartsSuccessfully` now skip on CI.
  - These tests require an active video window (YouTube, etc.) to be running.
```

**File:** docs/report_20240214_newfeature.md (L18-24)
```markdown
    ❌ 実行不能／失敗した機能
     - mcp__WindowsDesktopUse__get_latest_video_frame（watch_video_v2 セッション対象）
      → 複数回実行しても hasFrame: false。原因：GDI+ によるキャプチャが YouTube の GPU
    加速表示に対応していない可能性が高い。初回初期化後もフレーム生成されない。
     - mcp__WindowsDesktopUse__get_latest_video_frame（camera_capture_stream セッション対象）
      → 同様に hasFrame: false。さらに、HTTP ストリームエンドポイント /stream/{id} は
    404（存在しない）。
```

**File:** src/WindowsDesktopUse.Screen/CaptureServices/ModernCaptureService.cs (L61-87)
```csharp
            // Use PrintWindow with PW_RENDERFULLCONTENT for GPU-accelerated content
            return await Task.Run(() => CaptureWithPrintWindow(hwnd, width, height), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ModernCapture] Capture failed: {ex.Message}");
            return null;
        }
    }

    private Bitmap? CaptureWithPrintWindow(IntPtr hwnd, int width, int height)
    {
        try
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

                var hdcDest = g.GetHdc();
                try
                {
                    // PW_RENDERFULLCONTENT = 0x00000002 - captures GPU-accelerated content
                    const uint PW_RENDERFULLCONTENT = 0x00000002;
                    var success = PrintWindow(hwnd, hdcDest, PW_RENDERFULLCONTENT);
```

**File:** docs/TOOLS.md (L67-85)
```markdown
#### `keyboard_key` (Security Restricted)
Simulates pressing, releasing, or clicking a navigation key only.

**⚠️ Security Notice:** For security reasons, only safe navigation keys are allowed. Text typing and modifier keys (Ctrl, Alt, Win) are blocked to prevent unintended system operations.

- **Arguments:**
  - `key` (string): Key name.
    - **Allowed keys:** `enter`, `return`, `tab`, `escape`, `esc`, `space`, `backspace`, `delete`, `del`, `left`, `up`, `right`, `down`, `home`, `end`, `pageup`, `pagedown`
    - **Not allowed:** `ctrl`, `alt`, `win`, `shift` (blocked for security)
  - `action` (string): "click", "press" (hold down), "release" (let go) (default: "click")

- **Use Cases:**
  - Navigate forms using Tab key
  - Confirm actions with Enter
  - Close dialogs with Escape
  - Navigate lists and menus with arrow keys
  - Navigate pages with PageUp/PageDown

- **Note:** For text input, use mouse to focus a text field first. Direct text typing is not supported for security reasons.
```

**File:** docs/plan_20260214_accessibility_and_monitor.ja.md (L10-29)
```markdown
## 2. 機能1: `read_window_text` (UIテキスト抽出)

### 概要
指定したウィンドウのUIツリーを解析し、Markdown形式で構造化されたテキストを抽出する。

### 実装詳細
- **サービス名**: `AccessibilityService` (新規作成)
- **技術詳細**:
    - `UIAutomationClient` と `UIAutomationTypes` (Windows Desktop SDK標準) を使用。
    - パフォーマンス向上のため `TreeWalker.ControlViewWalker` を使用してツリーを走査する。
    - テキスト取得は `Current.Name` だけでなく、`ValuePattern` や `TextPattern` からの取得を試みる。
- **Markdown変換マッピング**:
    - `ControlType.TitleBar`, `Header` -> `# ` (深度に応じて増やす)
    - `ControlType.ListItem` -> `- `
    - `ControlType.Text`, `Edit`, `Document` -> プレーンテキスト（改行維持）
    - `ControlType.Button` -> `[ Button: {Name} ]` (引数で有効な場合)
- **注意点**: 
    - 無限ループ防止のため、最大深度を10階層に制限。
    - ブラウザのURL取得は、特定のエディットコントロール（"Address and search bar" 等）を探索することで付加価値を高める。

```

**File:** docs/plan_20260214_token_efficiency_protocol.ja.md (L30-41)
```markdown
```json
"_llm_instruction": {
    "action": "PROCESS_IMMEDIATELY_AND_DISCARD",
    "steps": [
        "1. Extract metadata (ts, windowTitle).",
        "2. Analyze image NOW.",
        "3. Record result as TEXT only.",
        "4. DELETE the 'frame' or 'image' field from your history."
    ],
    "token_warning": "This image consumes approx 2000+ tokens. Discarding it saves 95% memory."
}
```
```

**File:** docs/plan_20260214_mcp_resources_buffering.ja.md (L6-10)
```markdown
## 2. アーキテクチャ概要
- **バッファ**: サーバー側のメモリまたは一時ファイルに、直近 N フレーム分の画像を保持する。
- **リソース提供**: MCP の `Resources` 機能を使用して、各フレームに一意の URI（例: `mcp://video/frame/123.jpg`）を割り当てる。
- **オンデマンド読み込み**: LLM は通知（Notifications）で URI だけを受け取り、詳細な解析が必要な場合のみ、その URI を読みに行く。

```

**File:** docs/report/qwencode_report.md (L9-28)
```markdown
### 1. `hwnd` パラメータの型変更

- **変更前**: `long` 型（整数）
- **変更後**: `string` 型（文字列）

#### 修正対象メソッド

- `See` メソッド
- `StartWatching` メソッド
- `CloseWindow` メソッド

#### 修正理由

- JSON 経由での数値送信時に、`params/hwnd must be integer,null` エラーが発生する問題を解消
- 数値が文字列としてシリアライズされる環境に対応

#### 修正内容詳細

- `long.TryParse` を使用して、文字列から整数への変換を安全に行うように変更
- 以前の整数形式も引き続きサポート
```

**File:** docs/issue_20260214_qwen_video_coview_spec.md (L31-44)
```markdown
## 提案仕様：「2秒ごとのパラパラ画像 ＋ 同期文字起こし」
LLM が時系列データを基に自然な会話ができるよう、以下の出力形式を実現します：

### 出力フォーマット（NDJSON: Newline-Delimited JSON）
```json
{"ts": 0.0, "frame": "base64encoded_jpeg_without_newlines", "transcript": ""}
{"ts": 2.0, "frame": "...", "transcript": "おはようございます"}
{"ts": 4.0, "frame": "...", "transcript": "杜王町の朝だ"}
...
```
- `ts`: 再生開始からの経過時間（秒単位、float、100ms 精度以上）
- `frame`: JPEG base64（改行なし、RFC 4648準拠）→ `docs/feedback_20240614_base64_input_handling.md` の教訓を反映
- `transcript`: `listen(duration=2, source="System")` で得た文字起こし（空文字可）
- 全てのフィールドは必須ではなく、`transcript` は音声が検出されない場合は ""
```

**File:** docs/plan_20260214_timestamp_improvement_instruction.ja.md (L8-13)
```markdown
`watch_video` および `watch_video_v2` において、キャプチャ処理のオーバーヘッド（100ms〜500ms）による「累積遅延」と「映像・音声の同期ズレ」を解消する。

## 2. 修正方針
- **絶対時刻スケジュール**: ループ開始時に「次のキャプチャ予定時刻 (`nextCaptureTime`)」を計算し、そこまで正確に待機する。
- **実測タイムスタンプ**: `ts` (タイムスタンプ) は、ループ開始時ではなく、**キャプチャ処理が完了した直後の時刻**を使用して計算する。

```

**File:** docs/DEVELOPMENT.md (L38-40)
```markdown
| **`WindowsDesktopUse.Audio`** | Hearing (Record) | Audio recording from system and microphone using WASAPI (NAudio). |
| **`WindowsDesktopUse.Transcription`** | Hearing (Analyze) | AI transcription using Whisper.net to convert audio to text. |
| **`WindowsDesktopUse.Input`** | Limbs (Control) | Low-level mouse and keyboard input simulation using the `SendInput` Win32 API. |
```

**File:** docs/DEVELOPMENT.md (L62-68)
```markdown
## Important: High DPI Awareness

This server calls `SetProcessDPIAware()` and operates in physical pixel units.
- Coordinates and sizes retrieved by the `Screen` module are in physical pixels.
- Mouse coordinates specified in the `Input` module are interpreted as physical pixels.
This design ensures that coordinates calculated by an AI from capture images match the input coordinates perfectly without any offset.

```

**File:** AGENTS.md (L49-52)
```markdown
### Logging - CRITICAL
**STRICT REQUIREMENT:** Always use `Console.Error.WriteLine` for logging.
**REASON:** `stdout` is reserved for JSON-RPC.

```
