using System.Drawing;
using System.Drawing.Imaging;

namespace WindowsDesktopUse.Screen;

/// <summary>
/// Visual change detection using grid-based pixel sampling
/// </summary>
public class VisualChangeDetector : IDisposable
{
    private Bitmap? _previousFrame;
    private DateTime _lastKeyFrameTime;
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Grid size for sampling (8x8 or 16x16)
    /// </summary>
    public int GridSize { get; set; } = 16;

    /// <summary>
    /// Change threshold percentage (5-10%)
    /// </summary>
    public double ChangeThreshold { get; set; } = 0.08; // 8%

    /// <summary>
    /// Pixel color difference threshold
    /// </summary>
    public int PixelThreshold { get; set; } = 30;

    /// <summary>
    /// Key frame interval in seconds (force send even if no change)
    /// </summary>
    public int KeyFrameInterval { get; set; } = 10;

    /// <summary>
    /// Analyze frame and detect changes
    /// </summary>
    public ChangeAnalysisResult AnalyzeFrame(Bitmap frame)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(VisualChangeDetector));

        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var timeSinceLastKeyFrame = (now - _lastKeyFrameTime).TotalSeconds;

            // Force key frame if interval exceeded
            if (timeSinceLastKeyFrame >= KeyFrameInterval)
            {
                UpdatePreviousFrame(frame);
                _lastKeyFrameTime = now;
                return new ChangeAnalysisResult(true, true, 1.0, "Key Frame");
            }

            // First frame
            if (_previousFrame == null)
            {
                UpdatePreviousFrame(frame);
                _lastKeyFrameTime = now;
                return new ChangeAnalysisResult(true, true, 1.0, "Initial Frame");
            }

            // Check if dimensions match
            if (_previousFrame.Width != frame.Width || _previousFrame.Height != frame.Height)
            {
                UpdatePreviousFrame(frame);
                return new ChangeAnalysisResult(true, true, 1.0, "Dimension Change");
            }

            // Perform grid-based comparison
            var changeRatio = CalculateChangeRatio(frame);
            var hasSignificantChange = changeRatio >= ChangeThreshold;

            if (hasSignificantChange)
            {
                UpdatePreviousFrame(frame);
                return new ChangeAnalysisResult(true, true, changeRatio, "Scene Change");
            }

            return new ChangeAnalysisResult(false, false, changeRatio, "No Change");
        }
    }

    /// <summary>
    /// Calculate change ratio using grid-based sampling
    /// </summary>
    private double CalculateChangeRatio(Bitmap currentFrame)
    {
        if (_previousFrame == null) return 1.0;

        var width = currentFrame.Width;
        var height = currentFrame.Height;
        
        var cols = Math.Min(GridSize, width);
        var rows = Math.Min(GridSize, height);
        
        var cellWidth = width / cols;
        var cellHeight = height / rows;
        
        var changedCells = 0;
        var totalCells = cols * rows;

        // Use LockBits for fast pixel access with safe copy
        var currentData = currentFrame.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);
        
        var previousData = _previousFrame.LockBits(
            new Rectangle(0, 0, _previousFrame.Width, _previousFrame.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        try
        {
            var stride = currentData.Stride;
            var currentBytes = new byte[stride * height];
            var previousBytes = new byte[stride * height];
            
            // Copy pixel data to managed arrays
            System.Runtime.InteropServices.Marshal.Copy(currentData.Scan0, currentBytes, 0, currentBytes.Length);
            System.Runtime.InteropServices.Marshal.Copy(previousData.Scan0, previousBytes, 0, previousBytes.Length);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    // Sample center pixel of each cell
                    var sampleX = col * cellWidth + cellWidth / 2;
                    var sampleY = row * cellHeight + cellHeight / 2;
                    
                    // Ensure within bounds
                    sampleX = Math.Min(sampleX, width - 1);
                    sampleY = Math.Min(sampleY, height - 1);

                    var offset = sampleY * stride + sampleX * 4;

                    // Compare RGB values (ignore alpha)
                    var bDiff = Math.Abs(currentBytes[offset] - previousBytes[offset]);
                    var gDiff = Math.Abs(currentBytes[offset + 1] - previousBytes[offset + 1]);
                    var rDiff = Math.Abs(currentBytes[offset + 2] - previousBytes[offset + 2]);

                    if (bDiff > PixelThreshold || gDiff > PixelThreshold || rDiff > PixelThreshold)
                    {
                        changedCells++;
                    }
                }
            }
        }
        finally
        {
            currentFrame.UnlockBits(currentData);
            _previousFrame.UnlockBits(previousData);
        }

        return (double)changedCells / totalCells;
    }

    private void UpdatePreviousFrame(Bitmap frame)
    {
        _previousFrame?.Dispose();
        _previousFrame = new Bitmap(frame);
    }

    /// <summary>
    /// Reset detector state
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _previousFrame?.Dispose();
            _previousFrame = null;
            _lastKeyFrameTime = DateTime.MinValue;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            lock (_lock)
            {
                _previousFrame?.Dispose();
                _previousFrame = null;
            }
            _disposed = true;
        }
    }
}

/// <summary>
/// Result of change analysis
/// </summary>
public record ChangeAnalysisResult(
    bool HasChange,
    bool ShouldSend,
    double ChangeRatio,
    string EventTag
);
