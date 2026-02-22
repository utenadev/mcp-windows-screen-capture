using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using WindowsDesktopUse.Audio;
using WindowsDesktopUse.Core;
using WindowsDesktopUse.Input;
using WindowsDesktopUse.Screen;
using WindowsDesktopUse.Transcription;

namespace WindowsDesktopUse.App;

[McpServerToolType]
public class DesktopUseTools
{
    private static ScreenCaptureService? _capture;
    private static AudioCaptureService? _audioCapture;
    private static WhisperTranscriptionService? _whisperService;
    private static VideoCaptureService? _videoCapture;
    private static WindowsDesktopUse.Screen.HybridCaptureService? _hybridCapture;
    private static AccessibilityService? _accessibilityService;

    public static void SetCaptureService(ScreenCaptureService capture) => _capture = capture;
    public static void SetAudioCaptureService(AudioCaptureService audioCapture) => _audioCapture = audioCapture;
    public static void SetWhisperService(WhisperTranscriptionService whisperService) => _whisperService = whisperService;
    public static void SetVideoCaptureService(VideoCaptureService videoCapture) => _videoCapture = videoCapture;
    public static void SetHybridCaptureService(WindowsDesktopUse.Screen.HybridCaptureService hybridCapture) => _hybridCapture = hybridCapture;
    public static void SetAccessibilityService(AccessibilityService accessibilityService) => _accessibilityService = accessibilityService;

    // Enum for capture target types
    public enum CaptureTargetType
    {
        Monitor,
        Window,
        Region,
        Primary
    }

    // Enum for audio source types
    public enum AudioSourceType
    {
        Microphone,
        System,
        File,
        AudioSession
    }

    // Enum for mouse button types
    public enum MouseButtonName
    {
        Left,
        Right,
        Middle
    }

    // Enum for key action types
    public enum KeyActionType
    {
        Press,
        Release,
        Click
    }

    // ============ AUDIO CAPTURE TOOLS ============

    // [McpServerTool removed for v2.0 - use unified tools]
    // This method is kept for internal use only. Use 'listen' tool instead.
    public static AudioSession StartAudioCapture(
        [Description("Source: 'system', 'microphone', 'both'")] string source = "system",
        [Description("Sample rate")] int sampleRate = 44100,
        [Description("Microphone device index")] int deviceIndex = 0)
    {
        _audioCapture ??= new AudioCaptureService();

        if (!Enum.TryParse<AudioCaptureSource>(source, true, out var sourceEnum))
        {
            throw new ArgumentException($"Invalid source: {source}");
        }

        return _audioCapture.StartCapture(sourceEnum, sampleRate, deviceIndex);
    }

    // [McpServerTool removed for v2.0 - use unified tools]
    public static async Task<AudioCaptureResult> StopAudioCapture(
        [Description("Session ID")] string sessionId,
        [Description("Return format: 'base64', 'file_path'")] string returnFormat = "base64")
    {
        if (_audioCapture == null)
        {
            throw new InvalidOperationException("AudioCaptureService not initialized");
        }

        return await _audioCapture.StopCaptureAsync(sessionId, returnFormat == "base64").ConfigureAwait(false);
    }

    // ============ WHISPER SPEECH RECOGNITION TOOLS ============

    // [McpServerTool removed for v2.0 - use unified tools]
    public static async Task<TranscriptionResult> Listen(
        [Description("Source: 'microphone', 'system', 'file', 'audio_session'")] AudioSourceType source = AudioSourceType.System,
        [Description("Source ID")] string? sourceId = null,
        [Description("Language code")] string language = "auto",
        [Description("Recording duration in seconds")] int duration = 10,
        [Description("Model size: 'tiny', 'base', 'small', 'medium', 'large'")] string modelSize = "base",
        [Description("Translate to English")] bool translate = false)
    {
        if (_whisperService == null)
        {
            throw new InvalidOperationException("WhisperTranscriptionService not initialized");
        }
        if (duration < 1) throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be at least 1 second");

        if (!Enum.TryParse<WhisperModelSize>(modelSize, true, out var modelSizeEnum))
        {
            throw new ArgumentException($"Invalid model size: {modelSize}. Valid values are 'tiny', 'base', 'small', 'medium', 'large'");
        }

        string? audioFilePath = null;
        bool shouldCleanup = false;

        try
        {
            switch (source)
            {
                case AudioSourceType.File:
                    if (string.IsNullOrEmpty(sourceId))
                    {
                        throw new ArgumentException("sourceId required when source='file'");
                    }
                    audioFilePath = sourceId;
                    if (!File.Exists(audioFilePath))
                    {
                        throw new FileNotFoundException($"Audio file not found: {audioFilePath}");
                    }
                    break;

                case AudioSourceType.AudioSession:
                    if (_audioCapture == null)
                    {
                        throw new InvalidOperationException("AudioCaptureService not initialized");
                    }
                    if (string.IsNullOrEmpty(sourceId))
                    {
                        throw new ArgumentException("sourceId required when source='audio_session'");
                    }
                    var audioResult = await _audioCapture.StopCaptureAsync(sourceId, false).ConfigureAwait(false);
                    audioFilePath = Path.Combine(Path.GetTempPath(), $"whisper_temp_{Guid.NewGuid()}.wav");
                    File.WriteAllBytes(audioFilePath, Convert.FromBase64String(audioResult.AudioDataBase64));
                    shouldCleanup = true;
                    break;

                case AudioSourceType.Microphone:
                case AudioSourceType.System:
                    _audioCapture ??= new AudioCaptureService();
                    var captureSource = source == AudioSourceType.Microphone ? AudioCaptureSource.Microphone : AudioCaptureSource.System;
                    var session = _audioCapture.StartCapture(captureSource, 16000);

                    Console.Error.WriteLine($"[Listen] Recording {(source == AudioSourceType.Microphone ? "microphone" : "system")} audio for {duration} seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(duration)).ConfigureAwait(false);

                    var capturedAudio = await _audioCapture.StopCaptureAsync(session.SessionId, false).ConfigureAwait(false);
                    audioFilePath = capturedAudio.OutputPath;
                    if (string.IsNullOrEmpty(audioFilePath) || !File.Exists(audioFilePath))
                    {
                        throw new InvalidOperationException("Audio file not found after capture");
                    }
                    shouldCleanup = false;
                    break;

                default:
                    throw new ArgumentException($"Unknown source: {source}. Valid values are 'microphone', 'system', 'file', 'audio_session'");
            }

            Console.Error.WriteLine($"[Listen] Transcribing with {modelSize} model...");
            var langCode = language == "auto" ? null : language;

            var result = _whisperService.TranscribeFileAsync(
                audioFilePath,
                langCode,
                modelSizeEnum,
                translate).GetAwaiter().GetResult();

            Console.Error.WriteLine($"[Listen] Transcription complete: {result.Segments.Count} segments");

            return result;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Listen] ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
        finally
        {
            if (shouldCleanup && audioFilePath != null && File.Exists(audioFilePath))
            {
                try
                {
                    File.Delete(audioFilePath);
                }
                catch { }
            }
        }
    }

    // ============ INPUT TOOLS ============

    [McpServerTool, Description("Press or release keyboard keys. Replaces: keyboard_type, press_key.")]
    public static void KeyboardKey(
        [Description("Key name: enter, tab, escape, space, backspace, delete, left, up, right, down, home, end, pageup, pagedown")] string key,
        [Description("Action: 'press', 'release', 'click'")] KeyActionType action = KeyActionType.Click)
    {
        
        if (key == null) throw new ArgumentNullException(nameof(key), "Key cannot be null");

        var keyAction = action switch
        {
            KeyActionType.Press => KeyAction.Press,
            KeyActionType.Release => KeyAction.Release,
            KeyActionType.Click => KeyAction.Click,
            _ => KeyAction.Click
        };

        var virtualKey = key.ToUpperInvariant() switch
        {
            "ENTER" or "RETURN" => VirtualKeys.Enter,
            "TAB" => VirtualKeys.Tab,
            "ESCAPE" or "ESC" => VirtualKeys.Escape,
            "SPACE" => VirtualKeys.Space,
            "BACKSPACE" => VirtualKeys.Backspace,
            "DELETE" or "DEL" => VirtualKeys.Delete,
            "LEFT" => VirtualKeys.Left,
            "UP" => VirtualKeys.Up,
            "RIGHT" => VirtualKeys.Right,
            "DOWN" => VirtualKeys.Down,
            "HOME" => VirtualKeys.Home,
            "END" => VirtualKeys.End,
            "PAGEUP" => VirtualKeys.PageUp,
            "PAGEDOWN" => VirtualKeys.PageDown,
            _ => throw new ArgumentException($"Key '{key}' is not allowed or unknown. Allowed keys: enter, tab, escape, space, backspace, delete, arrow keys, home, end, pageup, pagedown")
        };

        InputService.PressKey(virtualKey, keyAction);
    }

    // ============ ACCESSIBILITY & MONITOR TOOLS ============

    /// <summary>
    /// Read text from a window using UI Automation and return as Markdown
    /// </summary>
    [McpServerTool, Description("[Experimental] Read text content from a window using UI Automation. Currently limited to root-level elements only. Replaces: read_window_text_v2.")]
    public static string ReadWindowText(
        [Description("Window handle (HWND) as string")] string hwndStr,
        [Description("Include buttons in output")] bool includeButtons = false)
    {
        if (_accessibilityService == null)
            throw new InvalidOperationException("AccessibilityService not initialized");

        if (!long.TryParse(hwndStr, out var hwnd))
        {
            throw new ArgumentException($"Invalid HWND format: '{hwndStr}'. Expected numeric string.");
        }

        Console.Error.WriteLine($"[ReadWindowText] Extracting text from window: {hwnd}");

        var text = _accessibilityService.ExtractWindowText(new IntPtr(hwnd), includeButtons);
        
        if (string.IsNullOrWhiteSpace(text))
        {
            return "No text content found in the window.";
        }

        return text;
    }

    // ============ NEW UNIFIED TOOLS (v2.0) ============

    private static SessionManager? _sessionManager;

    public static void SetSessionManager(SessionManager sessionManager) => _sessionManager = sessionManager;

    // Enum for visual target types
    public enum VisualTargetType
    {
        All,
        Monitor,
        Window
    }

    // Enum for visual capture modes
    public enum VisualCaptureMode
    {
        Normal,
        Detailed
    }

    // Enum for watch modes
    public enum VisualWatchMode
    {
        Video,
        Monitor,
        Unified
    }

    // Enum for mouse actions
    public enum MouseActionType
    {
        Move,
        Click,
        Drag
    }

    // Enum for window actions
    public enum WindowActionType
    {
        Close,
        Minimize,
        Maximize,
        Restore
    }

    /// <summary>
    /// Unified tool to list visual targets (monitors, windows, or all)
    /// </summary>
    [McpServerTool, Description("List monitors or windows. Replaces: list_monitors, list_windows.")]
    public static object VisualList(
        [Description("Target type: 'monitor', 'window', or 'all' (default)")] string type = "all",
        [Description("Filter by title (for windows)")] string? filter = null)
    {
        if (_capture == null)
            throw new InvalidOperationException("ScreenCaptureService not initialized");

        var targetType = Enum.TryParse<VisualTargetType>(type, true, out var parsed) ? parsed : VisualTargetType.All;

        switch (targetType)
        {
            case VisualTargetType.Monitor:
                var monitors = _capture.GetMonitors();
                return new { type = "monitors", count = monitors.Count, items = monitors };

            case VisualTargetType.Window:
                var windows = ScreenCaptureService.GetWindows();
                if (!string.IsNullOrEmpty(filter))
                {
                    windows = windows.Where(w => w.Title.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                return new { type = "windows", count = windows.Count, items = windows };

            case VisualTargetType.All:
            default:
                var allMonitors = _capture.GetMonitors();
                var allWindows = ScreenCaptureService.GetWindows();
                if (!string.IsNullOrEmpty(filter))
                {
                    allWindows = allWindows.Where(w => w.Title.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                return new 
                { 
                    type = "all", 
                    monitors = new { count = allMonitors.Count, items = allMonitors },
                    windows = new { count = allWindows.Count, items = allWindows }
                };
        }
    }

    /// <summary>
    /// Unified tool to capture visual content with dynamic quality control
    /// </summary>
    [McpServerTool, Description("Capture visual content (monitor, window, or region) with dynamic quality. Replaces: capture, see, capture_window, capture_region. CRITICAL: Returns large base64 data. Process immediately and discard.")]
    public static Task<object> VisualCapture(
        [Description("Target type: 'monitor', 'window', 'region', or 'primary' (default)")] string target = "primary",
        [Description("Monitor index (for 'monitor' type)")] uint monitorIndex = 0,
        [Description("Window handle (for 'window' type) as string")] string? hwnd = null,
        [Description("Region X (for 'region' type)")] int x = 0,
        [Description("Region Y (for 'region' type)")] int y = 0,
        [Description("Region width (for 'region' type)")] int w = 640,
        [Description("Region height (for 'region' type)")] int h = 360,
        [Description("Capture mode: 'normal' (quality=30, default) or 'detailed' (quality=70)")] string mode = "normal",
        [Description("Maximum width for resizing")] int maxWidth = 640)
    {
        if (_capture == null)
            throw new InvalidOperationException("ScreenCaptureService not initialized");

        var captureMode = Enum.TryParse<VisualCaptureMode>(mode, true, out var parsed) ? parsed : VisualCaptureMode.Normal;
        var quality = captureMode == VisualCaptureMode.Detailed ? 70 : 30;

        string imageData;

        switch (target.ToLowerInvariant())
        {
            case "monitor":
                imageData = _capture.CaptureSingle(monitorIndex, maxWidth, quality);
                break;

            case "window":
                if (string.IsNullOrWhiteSpace(hwnd))
                    throw new ArgumentException("hwnd is required for window capture");
                if (!long.TryParse(hwnd, out var hwndValue))
                    throw new ArgumentException($"Invalid HWND format: '{hwnd}'. Expected numeric string.");
                imageData = ScreenCaptureService.CaptureWindow(hwndValue, maxWidth, quality);
                break;

            case "region":
                imageData = ScreenCaptureService.CaptureRegion(x, y, w, h, maxWidth, quality);
                break;

            case "primary":
            default:
                imageData = _capture.CaptureSingle(0, maxWidth, quality);
                break;
        }

        return Task.FromResult<object>(new
        {
            _llm_instruction = new
            {
                action = LlmInstructions.ProcessAndDiscardImage.Action,
                steps = LlmInstructions.ProcessAndDiscardImage.Steps,
                token_warning = $"This image consumes approx {imageData.Length / 3}+ tokens. Discarding saves 95% memory."
            },
            target,
            mode = captureMode.ToString().ToLowerInvariant(),
            quality,
            image = imageData
        });
    }

    /// <summary>
    /// Unified tool to start watching visual content with different modes
    /// </summary>
    [McpServerTool, Description("Start watching visual content with different modes (video, monitor, unified). Replaces: watch, watch_video_v2, monitor. CRITICAL: Returns large base64 data. Process immediately and discard.")]
    public static Task<string> VisualWatch(
        McpServer server,
        [Description("Watch mode: 'video', 'monitor', or 'unified' (default: video)")] string mode = "video",
        [Description("Target type: 'monitor', 'window', or 'region'")] string target = "monitor",
        [Description("Monitor index (for 'monitor' type)")] uint monitorIndex = 0,
        [Description("Window handle (for 'window' type) as string")] string? hwnd = null,
        [Description("Region X (for 'region' type)")] int x = 0,
        [Description("Region Y (for 'region' type)")] int y = 0,
        [Description("Region width (for 'region' type)")] int w = 640,
        [Description("Region height (for 'region' type)")] int h = 360,
        [Description("Frame rate (fps), default 5")] int fps = 5,
        [Description("Enable change detection, default true")] bool detectChanges = true,
        [Description("Change threshold (0.05-0.20), default 0.08")] double threshold = 0.08,
        [Description("Enable timestamp overlay on captured frames, default false")] bool overlay = false,
        [Description("Enable contextual prompt generation for LLM, default false")] bool context = false)
    {
        if (_sessionManager == null)
            throw new InvalidOperationException("SessionManager not initialized");
        if (_capture == null)
            throw new InvalidOperationException("ScreenCaptureService not initialized");

        var watchMode = Enum.TryParse<VisualWatchMode>(mode, true, out var parsed) ? parsed : VisualWatchMode.Video;
        var session = new UnifiedSession
        {
            Type = SessionType.Watch,
            Target = target,
            Metadata = new Dictionary<string, object>
            {
                ["mode"] = mode,
                ["target"] = target,
                ["fps"] = fps,
                ["detectChanges"] = detectChanges,
                ["threshold"] = threshold,
                ["overlay"] = overlay,
                ["context"] = context
            }
        };

        var sessionId = _sessionManager.RegisterSession(session);
        var intervalMs = 1000 / fps;

        _ = Task.Run(async () =>
        {
            var nextCaptureTime = DateTime.UtcNow;
            
            while (!session.Cts.IsCancellationRequested)
            {
                var waitMs = (int)(nextCaptureTime - DateTime.UtcNow).TotalMilliseconds;
                if (waitMs > 0)
                {
                    try { await Task.Delay(waitMs, session.Cts.Token); }
                    catch (OperationCanceledException) { break; }
                }

                try
                {
                    string imageData;
                    string? eventTag = null;
                    var quality = 30; // Normal mode quality
                    
                    switch (target.ToLowerInvariant())
                    {
                        case "window":
                            if (string.IsNullOrWhiteSpace(hwnd)) continue;
                            if (!long.TryParse(hwnd, out var hwndValue))
                            {
                                Console.Error.WriteLine($"[VisualWatch] Invalid HWND format: '{hwnd}'");
                                continue;
                            }
                            // Use HybridCaptureService for GPU-accelerated capture
                            if (_hybridCapture != null)
                            {
                                var bitmap = await _hybridCapture.CaptureWindowAsync(new IntPtr(hwndValue));
                                if (bitmap == null)
                                {
                                    Console.Error.WriteLine($"[VisualWatch] Failed to capture window: {hwnd}");
                                    continue;
                                }
                                // Apply overlay if enabled
                                if (overlay)
                                {
                                    var elapsed = DateTime.UtcNow - session.StartTime;
                                    ImageOverlayService.OverlayTimestamp(bitmap, elapsed);
                                    if (eventTag != null)
                                        ImageOverlayService.OverlayEventTag(bitmap, eventTag);
                                }
                                using (bitmap)
                                using (var ms = new MemoryStream())
                                {
                                    bitmap.Save(ms, ImageFormat.Jpeg);
                                    imageData = Convert.ToBase64String(ms.ToArray());
                                }
                            }
                            else
                            {
                                imageData = ScreenCaptureService.CaptureWindow(hwndValue, 640, quality);
                            }
                            break;
                        case "region":
                            imageData = ScreenCaptureService.CaptureRegion(x, y, w, h, 640, quality);
                            break;
                        case "monitor":
                        default:
                            imageData = _capture.CaptureSingle(monitorIndex, 640, quality);
                            break;
                    }

                    var captureTime = DateTime.UtcNow;
                    var ts = (captureTime - session.StartTime).TotalSeconds;
                    var timestamp = captureTime.ToLocalTime().ToString("HH:mm:ss.f");
                    nextCaptureTime = nextCaptureTime.AddMilliseconds(intervalMs);

                    // Build notification data
                    var dataDict = new Dictionary<string, object>
                    {
                        ["_llm_instruction"] = new Dictionary<string, object>
                        {
                            ["action"] = LlmInstructions.ProcessAndDiscardImage.Action,
                            ["logging_policy"] = LlmInstructions.ProcessAndDiscardImage.LoggingPolicy ?? string.Empty,
                            ["steps"] = LlmInstructions.ProcessAndDiscardImage.Steps,
                            ["token_warning"] = $"This image consumes approx {imageData.Length / 3}+ tokens. Discarding saves 95% memory."
                        },
                        ["type"] = "visual_watch",
                        ["sessionId"] = sessionId,
                        ["mode"] = mode,
                        ["ts"] = Math.Round(ts, 1),
                        ["timestamp"] = timestamp,
                        ["image"] = imageData
                    };

                    // Add contextual prompt if enabled
                    if (context)
                    {
                        var frameContext = new FrameContextBuilder()
                            .WithFrame(imageData)
                            .WithTimestamps(captureTime, ts)
                            .WithEventTag(eventTag ?? "Frame")
                            .Build();
                        dataDict["prompt"] = frameContext.GenerateContextualPrompt();
                    }

                    var notificationData = new Dictionary<string, object?>
                    {
                        ["level"] = "info",
                        ["data"] = dataDict
                    };

                    await server.SendNotificationAsync("notifications/message", notificationData);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[VisualWatch] Error: {ex.Message}");
                    nextCaptureTime = nextCaptureTime.AddMilliseconds(intervalMs);
                }
            }
        }, session.Cts.Token);

        return Task.FromResult(sessionId);
    }

    /// <summary>
    /// Unified tool to stop any visual or input session
    /// </summary>
    [McpServerTool, Description("Stop any active session by ID or type. Replaces: stop_capture, stop_watch, stop_monitor.")]
    public static string VisualStop(
        [Description("Session ID to stop")] string sessionId,
        [Description("Stop all sessions of this type: 'watch', 'capture', 'audio', 'monitor', or 'all' (default)")] string type = "all")
    {
        if (_sessionManager == null)
            throw new InvalidOperationException("SessionManager not initialized");

        if (type.ToLowerInvariant() == "all" && string.IsNullOrEmpty(sessionId))
        {
            _sessionManager.StopAllSessions();
            return "Stopped all sessions";
        }

        if (!string.IsNullOrEmpty(sessionId))
        {
            if (_sessionManager.StopSession(sessionId))
                return $"Stopped session {sessionId}";
            return $"Session not found: {sessionId}";
        }

        if (Enum.TryParse<SessionType>(type, true, out var sessionType))
        {
            var count = _sessionManager.StopSessionsByType(sessionType);
            return $"Stopped {count} sessions of type {type}";
        }

        return "Invalid session type or ID";
    }

    /// <summary>
    /// Unified tool for mouse operations
    /// </summary>
    [McpServerTool, Description("Perform mouse operations (move, click, drag). Replaces: mouse_move, mouse_click.")]
    public static string InputMouse(
        [Description("Mouse action: 'move', 'click', or 'drag'")] string action,
        [Description("X coordinate")] int x,
        [Description("Y coordinate")] int y,
        [Description("End X coordinate (for drag)")] int? endX = null,
        [Description("End Y coordinate (for drag)")] int? endY = null,
        [Description("Mouse button: 'left' (default), 'right', or 'middle'")] string button = "left",
        [Description("Number of clicks (for click action), default 1")] int clicks = 1)
    {
        var actionType = Enum.TryParse<MouseActionType>(action, true, out var parsed) ? parsed : MouseActionType.Move;
        var buttonName = Enum.TryParse<MouseButtonName>(button, true, out var btn) ? btn : MouseButtonName.Left;

        switch (actionType)
        {
            case MouseActionType.Move:
                InputService.MoveMouse(x, y);
                return $"Mouse moved to ({x}, {y})";

            case MouseActionType.Click:
                InputService.MoveMouse(x, y);
                var mouseButton = buttonName switch
                {
                    MouseButtonName.Left => MouseButton.Left,
                    MouseButtonName.Right => MouseButton.Right,
                    MouseButtonName.Middle => MouseButton.Middle,
                    _ => MouseButton.Left
                };
                InputService.ClickMouseAsync(mouseButton, clicks).Wait();
                return $"Mouse clicked {clicks} time(s) at ({x}, {y}) with {buttonName} button";

            case MouseActionType.Drag:
                if (!endX.HasValue || !endY.HasValue)
                    throw new ArgumentException("endX and endY are required for drag action");
                InputService.DragMouseAsync(x, y, endX.Value, endY.Value).Wait();
                return $"Mouse dragged from ({x}, {y}) to ({endX.Value}, {endY.Value})";

            default:
                throw new ArgumentException($"Unknown action: {action}");
        }
    }

    /// <summary>
    /// Unified tool for window operations
    /// </summary>
    [McpServerTool, Description("Perform window operations (close, minimize, maximize, restore). Replaces: close_window.")]
    public static string InputWindow(
        [Description("Window handle (HWND) as string")] string hwnd,
        [Description("Window action: 'close' (default), 'minimize', 'maximize', or 'restore'")] string action = "close")
    {
        if (string.IsNullOrWhiteSpace(hwnd))
            throw new ArgumentException("hwnd is required");
        if (!long.TryParse(hwnd, out var hwndValue))
            throw new ArgumentException($"Invalid HWND format: '{hwnd}'. Expected numeric string.");
        
        var actionType = Enum.TryParse<WindowActionType>(action, true, out var parsed) ? parsed : WindowActionType.Close;
        var windowHandle = new IntPtr(hwndValue);

        switch (actionType)
        {
            case WindowActionType.Close:
                InputService.TerminateWindowProcess(windowHandle);
                return $"Window {hwndValue} closed";

            case WindowActionType.Minimize:
                InputService.MinimizeWindow(windowHandle);
                return $"Window {hwndValue} minimized";

            case WindowActionType.Maximize:
                InputService.MaximizeWindow(windowHandle);
                return $"Window {hwndValue} maximized";

            case WindowActionType.Restore:
                InputService.RestoreWindow(windowHandle);
                return $"Window {hwndValue} restored";

            default:
                throw new ArgumentException($"Unknown action: {action}");
        }
    }
}
