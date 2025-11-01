using NAudio.Wave;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FlacCapture;

/// <summary>
/// Linux-compatible audio capture that downloads streams directly without WASAPI
/// </summary>
class DirectStreamCapture : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly float _playbackVolume;
    private bool _isCapturing;

    public DirectStreamCapture(float playbackVolume = 0.7f)
 {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(30); // Longer timeout for large files
        _playbackVolume = playbackVolume;
    }

    public async Task CaptureStreamToFile(string[] streamUrls, string outputFile, bool convertToFlac = false)
    {
        Console.WriteLine("Initializing direct stream capture (Linux mode)...");
    Console.WriteLine($"Output file: {outputFile}\n");

        string tempCombinedWav = Path.Combine(Path.GetTempPath(), $"combined_{Guid.NewGuid()}.wav");
        
     try
        {
            var tempFiles = new List<string>();
            
        // Set up cancellation for Ctrl+C
   var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (s, e) =>
{
                e.Cancel = true;
   cts.Cancel();
         Console.WriteLine("\n\nStopping capture...");
     };

         // Download each stream
      for (int i = 0; i < streamUrls.Length; i++)
            {
 if (cts.Token.IsCancellationRequested)
         break;

   var url = streamUrls[i];
        Console.WriteLine($"[{i + 1}/{streamUrls.Length}] Downloading: {url}");

         try
      {
     string tempFile = await DownloadStreamAsync(url, cts.Token);
          tempFiles.Add(tempFile);
                  Console.WriteLine($"[{i + 1}/{streamUrls.Length}] Downloaded successfully.\n");
             }
   catch (OperationCanceledException)
                {
           Console.WriteLine("Download cancelled by user.");
      break;
                }
                catch (Exception ex)
    {
  Console.WriteLine($"Error downloading stream: {ex.Message}\n");
                }
    }

       if (tempFiles.Count == 0)
  {
        Console.WriteLine("No streams were downloaded successfully.");
  return;
       }

  // Combine all downloaded files into one WAV
     Console.WriteLine($"\nCombining {tempFiles.Count} stream(s) into WAV file...");
            await CombineAudioFilesAsync(tempFiles, tempCombinedWav);

       // Convert to output format (ensure it's WAV)
       ConvertToWav(tempCombinedWav, outputFile);

          Console.WriteLine($"\nCapture completed successfully!");
   Console.WriteLine($"WAV output saved to: {Path.GetFullPath(outputFile)}");

  // Clean up temp files
  foreach (var tempFile in tempFiles)
            {
   try { File.Delete(tempFile); } catch { }
            }
    try { File.Delete(tempCombinedWav); } catch { }

  // Auto-convert to FLAC if requested
        if (convertToFlac && File.Exists(outputFile))
          {
          string flacFile = Path.ChangeExtension(outputFile, ".flac");
         Console.WriteLine("\nAuto-converting to FLAC...");

                if (FlacConverter.ConvertToFlac(outputFile, flacFile))
    {
    Console.WriteLine($"FLAC output: {flacFile}");
     }
        }
        }
   catch (Exception ex)
        {
    Console.WriteLine($"\nError: {ex.Message}");
        throw;
 }
    }

    private async Task<string> DownloadStreamAsync(string url, CancellationToken cancellationToken)
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"stream_{Guid.NewGuid()}.tmp");

   try
        {
  Console.WriteLine("  Downloading...");
    using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
          response.EnsureSuccessStatusCode();

          long? contentLength = response.Content.Headers.ContentLength;
        string sizeInfo = contentLength.HasValue ? $" ({FormatFileSize(contentLength.Value)})" : "";
             Console.WriteLine($"  Size: {sizeInfo}");

      using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
        using (var httpStream = await response.Content.ReadAsStreamAsync(cancellationToken))
          {
   await httpStream.CopyToAsync(fileStream, cancellationToken);
    }
 }

            return tempFile;
    }
     catch
        {
     try { File.Delete(tempFile); } catch { }
            throw;
        }
    }

    private async Task CombineAudioFilesAsync(List<string> inputFiles, string outputFile)
    {
        if (inputFiles.Count == 1)
    {
// Single file, just copy it
  File.Copy(inputFiles[0], outputFile, true);
            return;
        }

        // Multiple files - need to combine them
   WaveFileWriter? waveFileWriter = null;

        try
        {
            foreach (var inputFile in inputFiles)
         {
    using var reader = new MediaFoundationReader(inputFile);
     
      if (waveFileWriter == null)
     {
   // Create writer with format from first file
     waveFileWriter = new WaveFileWriter(outputFile, reader.WaveFormat);
   }

    // Copy audio data
        byte[] buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];
     int bytesRead;
           while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                waveFileWriter.Write(buffer, 0, bytesRead);
      }
      }
        }
        finally
        {
       waveFileWriter?.Dispose();
        }

        await Task.CompletedTask;
    }

    private void ConvertToWav(string inputFile, string outputFile)
    {
        // If input is already WAV and we're not changing anything, just copy
        if (Path.GetExtension(inputFile).Equals(".wav", StringComparison.OrdinalIgnoreCase))
        {
     File.Copy(inputFile, outputFile, true);
            return;
        }

     // Otherwise, decode to WAV
    using var reader = new MediaFoundationReader(inputFile);
  using var writer = new WaveFileWriter(outputFile, reader.WaveFormat);
     
        byte[] buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];
 int bytesRead;
        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
   writer.Write(buffer, 0, bytesRead);
   }
    }

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

    public void Dispose()
  {
      _httpClient.Dispose();
    }
}
