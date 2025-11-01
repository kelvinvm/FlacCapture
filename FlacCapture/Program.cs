using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Lame;
using Microsoft.Extensions.Logging;

namespace FlacCapture;

class Program
{
    static async Task Main(string[] args)
    {
        // Check if running in service mode (for container)
        bool serviceMode = args.Length > 0 && args[0] == "--service";

        if (serviceMode)
        {
            await RunServiceModeAsync();
        }
        else
        {
            await RunInteractiveModeAsync();
        }
    }

    /// <summary>
    /// Service mode for Docker container - monitors directory for M3U files
    /// </summary>
    static async Task RunServiceModeAsync()
    {
        // Get configuration from environment variables
        string inputDir = Environment.GetEnvironmentVariable("INPUT_DIR") ?? "/app/input";
        string outputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR") ?? "/app/output";

        float playbackVolume = float.TryParse(Environment.GetEnvironmentVariable("PLAYBACK_VOLUME"), out var vol)
            ? vol : 0.7f;

        bool autoDeleteWav = bool.TryParse(Environment.GetEnvironmentVariable("AUTO_DELETE_WAV"), out var delWav)
             ? delWav : true;

        bool autoConvertFlac = bool.TryParse(Environment.GetEnvironmentVariable("AUTO_CONVERT_FLAC"), out var conv)
             ? conv : true;

        int flacQuality = int.TryParse(Environment.GetEnvironmentVariable("FLAC_QUALITY"), out var qual)
             ? qual : 100;

        int scanInterval = int.TryParse(Environment.GetEnvironmentVariable("SCAN_INTERVAL_SECONDS"), out var interval)
             ? interval : 30;

        // Create logger
        var loggerFactory = new ConsoleLoggerFactory(LogLevel.Information);
        var logger = loggerFactory.CreateLogger("FlacCapture.Service");

        logger.LogInformation("=================================================");
        logger.LogInformation("FLAC Capture Service - Container Mode");
        logger.LogInformation("=================================================");
        logger.LogInformation($"Version: 1.0.1");
        logger.LogInformation($"Input Directory: {inputDir}");
        logger.LogInformation($"Output Directory: {outputDir}");
        logger.LogInformation($"Scan Interval: {scanInterval} seconds");
        logger.LogInformation($"Playback Volume: {playbackVolume * 100:F0}%");
        logger.LogInformation($"Auto-Convert FLAC: {autoConvertFlac}");
        logger.LogInformation($"Auto-Delete WAV: {autoDeleteWav}");
        logger.LogInformation($"FLAC Quality: {flacQuality}");
        logger.LogInformation("=================================================");

        // Set up cancellation token for graceful shutdown
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            logger.LogInformation("Shutdown signal received...");
            cts.Cancel();
        };

        // Create and run file watcher service
        using var fileWatcher = new FileWatcherService(
            inputDir,
            outputDir,
       logger,
        playbackVolume,
            autoDeleteWav,
      autoConvertFlac,
     flacQuality);

        try
        {
            await fileWatcher.RunAsync(cts.Token, scanInterval);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Service stopped gracefully");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Service crashed with unhandled exception");
            Environment.Exit(1);
        }

        logger.LogInformation("Service shutdown complete");
    }

    /// <summary>
    /// Interactive mode - original functionality
    /// </summary>
    static async Task RunInteractiveModeAsync()
    {
        Console.WriteLine("FLAC Capture Utility - Bit-Perfect WASAPI Recording");
        Console.WriteLine("====================================================\n");

        string m3uPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assests", "show.m3u");
        string outputWav = $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
        string outputFlac = Path.ChangeExtension(outputWav, ".flac");

        if (!File.Exists(m3uPath))
        {
            Console.WriteLine($"Error: M3U file not found at {m3uPath}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        var captureService = new WasapiFlacCapture(playbackVolume: 0.7f); // 70% monitoring volume

        try
        {
            Console.WriteLine($"Reading playlist: {m3uPath}");
            var urls = await File.ReadAllLinesAsync(m3uPath);

            // Filter out empty lines and comments
            var validUrls = urls.Where(url => !string.IsNullOrWhiteSpace(url) && !url.StartsWith("#")).ToArray();

            Console.WriteLine($"Found {validUrls.Length} stream(s)");
            Console.WriteLine($"Output file: {outputWav}\n");

            await captureService.CaptureStreamToFile(validUrls, outputWav);

            Console.WriteLine($"\nCapture completed successfully!");
            Console.WriteLine($"WAV output saved to: {Path.GetFullPath(outputWav)}");

            // Offer FLAC conversion
            Console.Write("\nConvert to FLAC format? (y/n): ");
            var response = Console.ReadLine()?.Trim().ToLower();

            if (response == "y" || response == "yes")
            {
                if (FlacConverter.ConvertToFlac(outputWav, outputFlac))
                {
                    Console.WriteLine($"\nFinal output: {Path.GetFullPath(outputFlac)}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            captureService.Dispose();
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
