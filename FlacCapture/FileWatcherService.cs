using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace FlacCapture;

/// <summary>
/// Service that monitors a directory for new M3U files and automatically processes them
/// </summary>
public class FileWatcherService : IDisposable
{
    private readonly string _inputDir;
    private readonly string _outputDir;
    private readonly ILogger _logger;
    private readonly FileSystemWatcher? _watcher;
    private readonly HashSet<string> _processedFiles;
    private readonly HashSet<string> _processingFiles;
    private readonly float _playbackVolume;
    private readonly bool _autoDeleteWav;
    private readonly bool _autoConvertFlac;
    private readonly int _flacQuality;
    private readonly SemaphoreSlim _processingLock;

    public FileWatcherService(
      string inputDir,
        string outputDir,
        ILogger logger,
     float playbackVolume = 0.7f,
        bool autoDeleteWav = true,
        bool autoConvertFlac = true,
     int flacQuality = 100)
    {
        _inputDir = inputDir;
        _outputDir = outputDir;
        _logger = logger;
_playbackVolume = playbackVolume;
        _autoDeleteWav = autoDeleteWav;
_autoConvertFlac = autoConvertFlac;
        _flacQuality = flacQuality;
        _processedFiles = new HashSet<string>();
        _processingFiles = new HashSet<string>();
   _processingLock = new SemaphoreSlim(1, 1); // Process one file at a time

    // Create directories if they don't exist
 Directory.CreateDirectory(_inputDir);
        Directory.CreateDirectory(_outputDir);

        // Set up file system watcher
        try
        {
       _watcher = new FileSystemWatcher(_inputDir)
     {
                Filter = "*.m3u",
  NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
       EnableRaisingEvents = true
            };

            _watcher.Created += OnFileCreated;
   _watcher.Changed += OnFileChanged;

     _logger.LogInformation("File watcher initialized successfully");
_logger.LogInformation($"Monitoring: {_inputDir}");
            _logger.LogInformation($"Output to: {_outputDir}");
        }
     catch (Exception ex)
        {
         _logger.LogError(ex, "Failed to initialize file watcher");
        _watcher = null;
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"New file detected: {e.Name}");
_ = ProcessFileAsync(e.FullPath);
  }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Sometimes files are created then immediately written to
        // We'll handle this by checking if we've already processed it
        if (!_processedFiles.Contains(e.FullPath) && !_processingFiles.Contains(e.FullPath))
        {
            _logger.LogInformation($"File modified: {e.Name}");
   _ = ProcessFileAsync(e.FullPath);
        }
    }

    /// <summary>
    /// Scans the input directory for existing M3U files and processes them
    /// </summary>
    public async Task ScanExistingFilesAsync()
    {
        _logger.LogInformation("Scanning for existing M3U files...");

        try
        {
   var m3uFiles = Directory.GetFiles(_inputDir, "*.m3u", SearchOption.TopDirectoryOnly);

       if (m3uFiles.Length == 0)
     {
         _logger.LogInformation("No existing M3U files found");
       return;
            }

        _logger.LogInformation($"Found {m3uFiles.Length} M3U file(s)");

   foreach (var file in m3uFiles)
            {
    if (!_processedFiles.Contains(file))
     {
        await ProcessFileAsync(file);
   }
 }
        }
        catch (Exception ex)
        {
      _logger.LogError(ex, "Error scanning existing files");
        }
    }

    private async Task ProcessFileAsync(string m3uFilePath)
    {
        // Prevent duplicate processing
   await _processingLock.WaitAsync();
  try
  {
            if (_processedFiles.Contains(m3uFilePath) || _processingFiles.Contains(m3uFilePath))
    {
 return;
         }

         _processingFiles.Add(m3uFilePath);
        }
     finally
  {
            _processingLock.Release();
        }

    bool processingSucceeded = false;

        try
 {
            // Wait a bit to ensure file is fully written
            await Task.Delay(2000);

            if (!File.Exists(m3uFilePath))
   {
       _logger.LogWarning($"File no longer exists: {m3uFilePath}");
 return;
            }

        _logger.LogInformation($"Processing: {Path.GetFileName(m3uFilePath)}");

            // Read M3U file
  string[] urls;
 try
            {
          urls = await File.ReadAllLinesAsync(m3uFilePath);
           urls = urls.Where(url => !string.IsNullOrWhiteSpace(url) && !url.StartsWith("#")).ToArray();
            }
            catch (Exception ex)
            {
    _logger.LogError(ex, $"Failed to read M3U file: {m3uFilePath}");
       return;
        }

            if (urls.Length == 0)
            {
        _logger.LogWarning($"No valid URLs found in: {Path.GetFileName(m3uFilePath)}");
         _processedFiles.Add(m3uFilePath);
             processingSucceeded = false;
        return;
            }

        _logger.LogInformation($"Found {urls.Length} stream URL(s)");

   // Generate output filename
        string baseName = Path.GetFileNameWithoutExtension(m3uFilePath);
         string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
     string outputWav = Path.Combine(_outputDir, $"{baseName}_{timestamp}.wav");
         string outputFlac = Path.ChangeExtension(outputWav, ".flac");

            // Capture the streams
         using (var captureService = new DirectStreamCapture(_playbackVolume))
            {
     try
   {
      _logger.LogInformation($"Starting capture for: {baseName}");
          await captureService.CaptureStreamToFile(urls, outputWav, _autoConvertFlac);
      _logger.LogInformation($"Capture completed: {Path.GetFileName(outputWav)}");

        // Convert to FLAC if not already done and requested
     if (!_autoConvertFlac && File.Exists(outputWav))
 {
          _logger.LogInformation("Converting to FLAC...");
     bool success = FlacConverter.ConvertToFlac(outputWav, outputFlac, _flacQuality);

         if (success)
         {
       _logger.LogInformation($"FLAC conversion successful: {Path.GetFileName(outputFlac)}");

        if (_autoDeleteWav)
       {
      try
        {
            File.Delete(outputWav);
             _logger.LogInformation("WAV file deleted (auto-cleanup)");
        }
         catch (Exception ex)
           {
           _logger.LogWarning(ex, "Could not delete WAV file");
       }
       }
 }
      else
              {
     _logger.LogWarning("FLAC conversion failed, keeping WAV file");
       }
   }

     // Mark as successfully processed
         processingSucceeded = true;
    _processedFiles.Add(m3uFilePath);
          }
     catch (Exception ex)
    {
    _logger.LogError(ex, $"Failed to capture streams from: {m3uFilePath}");
           processingSucceeded = false;
  }
  }
     }
     finally
     {
     _processingFiles.Remove(m3uFilePath);

      // Move M3U file to appropriate subdirectory to prevent reprocessing
            try
            {
           string fileName = Path.GetFileName(m3uFilePath);
       string targetSubdir = processingSucceeded ? "processed" : "failed";
        string targetDir = Path.Combine(_inputDir, targetSubdir);

         // Create subdirectory if it doesn't exist
        Directory.CreateDirectory(targetDir);

        string targetPath = Path.Combine(targetDir, fileName);

      // Handle duplicate filenames by adding timestamp
              if (File.Exists(targetPath))
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
         string extension = Path.GetExtension(fileName);
     string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
    fileName = $"{fileNameWithoutExt}_{timestamp}{extension}";
       targetPath = Path.Combine(targetDir, fileName);
          }

          File.Move(m3uFilePath, targetPath, true);

     if (processingSucceeded)
  {
         _logger.LogInformation($"M3U file moved to: {targetSubdir}/{fileName}");
              }
         else
        {
  _logger.LogWarning($"M3U file moved to: {targetSubdir}/{fileName} (processing failed)");
                }
    }
            catch (Exception ex)
  {
             _logger.LogWarning(ex, $"Could not move M3U file to {(processingSucceeded ? "processed" : "failed")} folder");

      // Even if move fails, mark as processed to prevent infinite retry loops
    _processedFiles.Add(m3uFilePath);
       }
        }
    }

    /// <summary>
    /// Starts the service (for manual polling mode)
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken, int scanIntervalSeconds = 30)
    {
        _logger.LogInformation("FileWatcher service started");
        _logger.LogInformation($"Scan interval: {scanIntervalSeconds} seconds");
        _logger.LogInformation($"Playback volume: {(_playbackVolume * 100):F0}%");
      _logger.LogInformation($"Auto-convert FLAC: {_autoConvertFlac}");
        _logger.LogInformation($"Auto-delete WAV: {_autoDeleteWav}");

        // Initial scan
        await ScanExistingFilesAsync();

        // Periodic scan (in addition to file system watcher)
while (!cancellationToken.IsCancellationRequested)
      {
    try
            {
 await Task.Delay(TimeSpan.FromSeconds(scanIntervalSeconds), cancellationToken);
     await ScanExistingFilesAsync();
            }
            catch (TaskCanceledException)
          {
  break;
        }
          catch (Exception ex)
      {
      _logger.LogError(ex, "Error in periodic scan");
      }
        }

     _logger.LogInformation("FileWatcher service stopped");
    }

    public void Dispose()
    {
      _watcher?.Dispose();
        _processingLock?.Dispose();
    }
}
