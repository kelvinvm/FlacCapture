using System;
using System.Diagnostics;
using System.IO;
using NAudio.Wave;
using NAudio.MediaFoundation;

namespace FlacCapture;

/// <summary>
/// Helper class to convert WAV files to FLAC format using NAudio's MediaFoundation encoder
/// </summary>
public static class FlacConverter
{
    /// <summary>
    /// Converts a WAV file to FLAC format using Windows Media Foundation
    /// </summary>
    /// <param name="wavFile">Input WAV file path</param>
    /// <param name="flacFile">Output FLAC file path</param>
    /// <param name="quality">Quality level 0-100 (default: 100 for maximum quality)</param>
    /// <returns>True if conversion successful, false otherwise</returns>
    public static bool ConvertToFlac(string wavFile, string flacFile, int quality = 100)
    {
        if (!File.Exists(wavFile))
        {
            Console.WriteLine($"Error: WAV file not found: {wavFile}");
            return false;
        }

        try
        {
            Console.WriteLine($"\nConverting to FLAC (quality: {quality})...");

            // Retry logic to handle file access issues
            WaveFileReader? reader = null;
            int retries = 3;
            int delayMs = 200;

            for (int attempt = 0; attempt < retries; attempt++)
            {
                try
                {
                    reader = new WaveFileReader(wavFile);
                    break; // Success!
                }
                catch (IOException) when (attempt < retries - 1)
                {
                    Console.WriteLine($"  Waiting for file to be released (attempt {attempt + 1}/{retries})...");
                    Thread.Sleep(delayMs);
                    delayMs *= 2; // Exponential backoff
                }
            }

            if (reader == null)
            {
                // Final attempt without catching
                reader = new WaveFileReader(wavFile);
            }

            using (reader)
            {
                var format = reader.WaveFormat;

                Console.WriteLine($"  Input format: {format.SampleRate}Hz, {format.BitsPerSample}bit, {format.Channels}ch");
                Console.WriteLine("  Encoding to FLAC...");

                try
                {
                    // Try to get FLAC MediaType
                    var outputMediaType = MediaFoundationEncoder.SelectMediaType(
                     AudioSubtypes.MFAudioFormat_FLAC,
                   format,
                           quality);

                    // Convert using MediaFoundation encoder
                    MediaFoundationApi.Startup();
                    using (var encoder = new MediaFoundationEncoder(outputMediaType))
                    {
                        encoder.Encode(flacFile, reader);
                    }
                    MediaFoundationApi.Shutdown();
                }
                catch (Exception mfEx)
                {
                    Console.WriteLine($"  MediaFoundation FLAC encoder not available: {mfEx.Message}");
                    Console.WriteLine("  Falling back to external flac.exe method...");
                    reader.Close(); // Close reader before fallback
                    return ConvertToFlacExternal(wavFile, flacFile);
                }
            } // reader is disposed here

            // Get file sizes for comparison
            var wavInfo = new FileInfo(wavFile);
            var flacInfo = new FileInfo(flacFile);
            double compressionRatio = (1.0 - ((double)flacInfo.Length / wavInfo.Length)) * 100;

            Console.WriteLine($"  ✓ Conversion complete!");
            Console.WriteLine($"  WAV size:  {FormatFileSize(wavInfo.Length)}");
            Console.WriteLine($"  FLAC size: {FormatFileSize(flacInfo.Length)}");
            Console.WriteLine($"  Compression: {compressionRatio:F1}% reduction");
            Console.WriteLine($"  Output: {flacFile}");

            // Ask about deleting WAV file
            Console.Write("\nDelete the original WAV file? (y/n): ");
            var response = Console.ReadLine()?.Trim().ToLower();
            if (response == "y" || response == "yes")
            {
                // Wait a bit before deleting to ensure all handles are released
                Thread.Sleep(100);
                try
                {
                    File.Delete(wavFile);
                    Console.WriteLine("Original WAV file deleted.");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Could not delete WAV file: {ex.Message}");
                    Console.WriteLine("You can manually delete it later.");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during FLAC conversion: {ex.Message}");
            Console.WriteLine("Trying alternative method...");
            return ConvertToFlacExternal(wavFile, flacFile);
        }
    }

    /// <summary>
    /// Fallback method: Converts WAV to FLAC using external flac.exe encoder
    /// </summary>
    private static bool ConvertToFlacExternal(string wavFile, string flacFile)
    {
        string? flacPath = FindFlacExecutable();

        if (flacPath == null)
        {
            Console.WriteLine("\nNote: FLAC encoder (flac.exe) not found.");
            Console.WriteLine("To enable FLAC conversion:");
            Console.WriteLine("1. Download from: https://xiph.org/flac/download.html");
            Console.WriteLine("2. Extract flac.exe to the same folder as this application");
            Console.WriteLine($"\nWAV file saved at: {Path.GetFullPath(wavFile)}");
            Console.WriteLine("You can manually convert it to FLAC later.");
            return false;
        }

        try
        {
            Console.WriteLine("Using external FLAC encoder...");

            var startInfo = new ProcessStartInfo
            {
                FileName = flacPath,
                Arguments = $"-8 --verify --best \"{wavFile}\" -o \"{flacFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine("Error: Failed to start FLAC encoder");
                return false;
            }

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                // Get file sizes for comparison
                var wavInfo = new FileInfo(wavFile);
                var flacInfo = new FileInfo(flacFile);
                double compressionRatio = (1.0 - ((double)flacInfo.Length / wavInfo.Length)) * 100;

                Console.WriteLine($"  ✓ Conversion complete!");
                Console.WriteLine($"  WAV size:  {FormatFileSize(wavInfo.Length)}");
                Console.WriteLine($"  FLAC size: {FormatFileSize(flacInfo.Length)}");
                Console.WriteLine($"  Compression: {compressionRatio:F1}% reduction");
                Console.WriteLine($"  Output: {flacFile}");

                // Ask about deleting WAV file
                Console.Write("\nDelete the original WAV file? (y/n): ");
                var response = Console.ReadLine()?.Trim().ToLower();
                if (response == "y" || response == "yes")
                {
                    File.Delete(wavFile);
                    Console.WriteLine("Original WAV file deleted.");
                }

                return true;
            }
            else
            {
                Console.WriteLine($"FLAC conversion failed with exit code {process.ExitCode}");
                if (!string.IsNullOrEmpty(error))
                    Console.WriteLine($"Error: {error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during FLAC conversion: {ex.Message}");
            return false;
        }
    }

    private static string? FindFlacExecutable()
    {
        // Check in current directory
        string localFlac = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "flac.exe");
        if (File.Exists(localFlac))
            return localFlac;

        // Check in PATH
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (pathEnv != null)
        {
            foreach (var path in pathEnv.Split(Path.PathSeparator))
            {
                try
                {
                    string fullPath = Path.Combine(path, "flac.exe");
                    if (File.Exists(fullPath))
                        return fullPath;
                }
                catch
                {
                    // Ignore invalid paths
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Formats file size in human-readable format
    /// </summary>
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:F2} {sizes[order]}";
    }
}
