namespace WindowsDesktopUse.Core;

/// <summary>
/// Represents a frame context with image data, timestamps, and contextual information for LLM understanding.
/// This record is designed to be model-agnostic, suitable for Claude, Qwen, GPT, and other general-purpose LLMs.
/// </summary>
/// <param name="FrameBase64">JPEG-encoded frame image in base64 format.</param>
/// <param name="AbsoluteTimestamp">Absolute UTC timestamp when the frame was captured.</param>
/// <param name="RelativeTimestamp">Elapsed seconds since the session started.</param>
/// <param name="EventTag">Event tag describing what happened in this frame (e.g., "SCENE CHANGE", "Frame").</param>
/// <param name="PreviousSummary">Optional summary of the previous frame(s) for temporal context.</param>
/// <param name="Subtitle">Optional subtitle/transcript from audio at this timestamp.</param>
/// <param name="Metadata">Optional additional metadata (e.g., scene change confidence, motion vectors).</param>
public record FrameContext(
    string FrameBase64,
    DateTime AbsoluteTimestamp,
    double RelativeTimestamp,
    string EventTag,
    string? PreviousSummary = null,
    string? Subtitle = null,
    Dictionary<string, object>? Metadata = null
)
{
    /// <summary>
    /// Generates a contextual prompt for LLM that includes temporal context.
    /// This helps general-purpose LLMs understand video frames as part of a sequence, not isolated images.
    /// </summary>
    /// <returns>A formatted prompt string for LLM input.</returns>
    public string GenerateContextualPrompt()
    {
        var prompt = new System.Text.StringBuilder();

        // Frame identification
        prompt.AppendLine($"[FRAME AT {AbsoluteTimestamp:HH:mm:ss.fff} | ELAPSED: {RelativeTimestamp:F1}s]");

        // Previous context (if available)
        if (!string.IsNullOrWhiteSpace(PreviousSummary))
        {
            prompt.AppendLine($"Previous context ({RelativeTimestamp - 0.1:F1}s ago): {PreviousSummary}");
        }

        // Current event tag
        if (!string.IsNullOrWhiteSpace(EventTag) && EventTag != "Frame")
        {
            prompt.AppendLine($"Event: {EventTag}");
        }

        // Subtitle (if available)
        if (!string.IsNullOrWhiteSpace(Subtitle))
        {
            prompt.AppendLine($"Subtitle: {Subtitle}");
        }

        // Instruction for LLM
        prompt.AppendLine();
        if (!string.IsNullOrWhiteSpace(PreviousSummary))
        {
            prompt.AppendLine($"Question: Based on the previous context and the current frame, what is happening now? Describe any changes or new events.");
        }
        else
        {
            prompt.AppendLine($"Question: What is shown in this frame? Describe the scene in detail.");
        }

        return prompt.ToString();
    }

    /// <summary>
    /// Converts to a JSON-serializable dictionary for MCP tool response.
    /// </summary>
    public Dictionary<string, object> ToDictionary()
    {
        var dict = new Dictionary<string, object>
        {
            ["frame_b64"] = FrameBase64,
            ["absolute_timestamp"] = AbsoluteTimestamp.ToString("O"), // ISO 8601 format
            ["relative_timestamp"] = RelativeTimestamp,
            ["event_tag"] = EventTag,
        };

        if (!string.IsNullOrWhiteSpace(PreviousSummary))
            dict["previous_summary"] = PreviousSummary;

        if (!string.IsNullOrWhiteSpace(Subtitle))
            dict["subtitle"] = Subtitle;

        if (Metadata != null && Metadata.Count > 0)
        {
            foreach (var kvp in Metadata)
            {
                dict[$"meta_{kvp.Key}"] = kvp.Value;
            }
        }

        return dict;
    }
}

/// <summary>
/// Helper class for building FrameContext instances with optional fields.
/// </summary>
public class FrameContextBuilder
{
    private string _frameBase64 = string.Empty;
    private DateTime _absoluteTimestamp;
    private double _relativeTimestamp;
    private string _eventTag = "Frame";
    private string? _previousSummary;
    private string? _subtitle;
    private Dictionary<string, object>? _metadata;

    public FrameContextBuilder WithFrame(string base64)
    {
        _frameBase64 = base64;
        return this;
    }

    public FrameContextBuilder WithTimestamps(DateTime absolute, double relative)
    {
        _absoluteTimestamp = absolute;
        _relativeTimestamp = relative;
        return this;
    }

    public FrameContextBuilder WithEventTag(string eventTag)
    {
        _eventTag = eventTag;
        return this;
    }

    public FrameContextBuilder WithPreviousSummary(string? summary)
    {
        _previousSummary = summary;
        return this;
    }

    public FrameContextBuilder WithSubtitle(string? subtitle)
    {
        _subtitle = subtitle;
        return this;
    }

    public FrameContextBuilder WithMetadata(Dictionary<string, object>? metadata)
    {
        _metadata = metadata;
        return this;
    }

    public FrameContext Build()
    {
        return new FrameContext(
            _frameBase64,
            _absoluteTimestamp,
            _relativeTimestamp,
            _eventTag,
            _previousSummary,
            _subtitle,
            _metadata
        );
    }
}
