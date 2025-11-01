using System;

namespace FlacCapture;

/// <summary>
/// Configuration options for the FLAC Capture utility
/// </summary>
public class CaptureConfig
{
    /// <summary>
    /// Path to the M3U playlist file (default: Assests\show.m3u)
    /// </summary>
    public string M3uFilePath { get; set; } = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory, "Assests", "show.m3u");

    /// <summary>
    /// Output directory for captured files (default: current directory)
    /// </summary>
    public string OutputDirectory { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// Prefix for output filenames (default: "capture_")
    /// </summary>
    public string OutputFilePrefix { get; set; } = "capture_";

    /// <summary>
    /// HTTP timeout for downloading streams in seconds (default: 30)
    /// </summary>
    public int HttpTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// FLAC compression level (0-8, where 8 is maximum compression, default: 8)
    /// </summary>
    public int FlacCompressionLevel { get; set; } = 8;

    /// <summary>
    /// Whether to automatically convert to FLAC after capture (default: false, will prompt user)
    /// </summary>
    public bool AutoConvertToFlac { get; set; } = false;

    /// <summary>
    /// Whether to delete the WAV file after successful FLAC conversion (default: false, will prompt user)
    /// </summary>
    public bool AutoDeleteWavAfterConversion { get; set; } = false;

    /// <summary>
    /// Delay in milliseconds between playing sequential streams (default: 1000ms)
    /// </summary>
    public int StreamDelayMs { get; set; } = 1000;

    /// <summary>
    /// Whether to show detailed progress information (default: true)
    /// </summary>
    public bool VerboseOutput { get; set; } = true;

    /// <summary>
    /// Validates the configuration settings
    /// </summary>
    public bool Validate(out string errorMessage)
    {
        errorMessage = string.Empty;

        if (HttpTimeoutSeconds < 1 || HttpTimeoutSeconds > 300)
        {
            errorMessage = "HTTP timeout must be between 1 and 300 seconds";
            return false;
        }

        if (FlacCompressionLevel < 0 || FlacCompressionLevel > 8)
        {
            errorMessage = "FLAC compression level must be between 0 and 8";
            return false;
        }

        if (StreamDelayMs < 0 || StreamDelayMs > 60000)
        {
            errorMessage = "Stream delay must be between 0 and 60000 milliseconds";
            return false;
        }

        try
        {
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Cannot create output directory: {ex.Message}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the full path for the output WAV file
    /// </summary>
    public string GetOutputWavPath()
    {
        string filename = $"{OutputFilePrefix}{DateTime.Now:yyyyMMdd_HHmmss}.wav";
        return Path.Combine(OutputDirectory, filename);
    }

    /// <summary>
    /// Gets the full path for the output FLAC file
    /// </summary>
    public string GetOutputFlacPath()
    {
        string filename = $"{OutputFilePrefix}{DateTime.Now:yyyyMMdd_HHmmss}.flac";
        return Path.Combine(OutputDirectory, filename);
    }
}
