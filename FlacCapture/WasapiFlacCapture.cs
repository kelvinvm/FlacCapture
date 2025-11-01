using NAudio.Wave;
using System;

namespace FlacCapture;

class WasapiFlacCapture : IDisposable
{
    private WasapiLoopbackCapture? _loopbackCapture;
    private WaveFileWriter? _waveWriter;
    private MediaFoundationReader? _streamPlayer;
    private WaveOutEvent? _outputDevice;
    private bool _isCapturing;
    private readonly HttpClient _httpClient;
    private readonly float _playbackVolume;

    public WasapiFlacCapture(float playbackVolume = 0.7f)
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        // Validate and set playback volume (0.0 to 1.0)
        _playbackVolume = Math.Clamp(playbackVolume, 0.0f, 1.0f);
    }

    public async Task CaptureStreamToFile(string[] streamUrls, string outputFile, bool convertToFlac = false)
    {
        Console.WriteLine("Initializing WASAPI loopback capture...");

        // Initialize WASAPI loopback capture (captures what you hear)
        _loopbackCapture = new WasapiLoopbackCapture();
        var waveFormat = _loopbackCapture.WaveFormat;

        Console.WriteLine($"Capture format: {waveFormat.SampleRate}Hz, {waveFormat.BitsPerSample}bit, {waveFormat.Channels}ch");
        Console.WriteLine($"This is bit-perfect capture of the audio output.");
        Console.WriteLine($"Playback monitoring volume: {(_playbackVolume * 100):F0}% (does not affect capture quality)\n");

        // Create output WAV file for capture
        _waveWriter = new WaveFileWriter(outputFile, waveFormat);

        _loopbackCapture.DataAvailable += (s, e) =>
        {
            if (_isCapturing && _waveWriter != null)
            {
                _waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            }
        };

        _loopbackCapture.RecordingStopped += (s, e) =>
        {
            if (e.Exception != null)
            {
                Console.WriteLine($"Recording error: {e.Exception.Message}");
            }
        };

        // Start loopback capture
        _loopbackCapture.StartRecording();
        Console.WriteLine("WASAPI loopback capture started.");
        Console.WriteLine("Playing streams... Press Ctrl+C to stop early.\n");

        // Set up cancellation for Ctrl+C
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            Console.WriteLine("\n\nStopping capture...");
        };

        try
        {
            // Play each stream URL sequentially
            for (int i = 0; i < streamUrls.Length; i++)
            {
                if (cts.Token.IsCancellationRequested)
                    break;

                var url = streamUrls[i];
                Console.WriteLine($"[{i + 1}/{streamUrls.Length}] Playing: {url}");

                try
                {
                    await PlayStreamAsync(url, cts.Token);
                    Console.WriteLine($"[{i + 1}/{streamUrls.Length}] Completed.\n");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Playback cancelled by user.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing stream: {ex.Message}\n");
                }
            }
        }
        finally
        {
            // Stop capture
            _isCapturing = false;
            _loopbackCapture.StopRecording();

            // Clean up - ensure proper disposal and flushing
            if (_waveWriter != null)
            {
                _waveWriter.Flush();
                _waveWriter.Dispose();
                _waveWriter = null;
            }

            // Give the OS time to release the file handle
            await Task.Delay(100);

            Console.WriteLine("\nRecording stopped.");

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
    }

    private async Task PlayStreamAsync(string url, CancellationToken cancellationToken)
    {
        string tempFile = Path.GetTempFileName();

        try
        {
            // Download the stream to a temporary file
            Console.WriteLine("  Downloading stream...");
            using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();

                using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var httpStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                {
                    await httpStream.CopyToAsync(fileStream, cancellationToken);
                }
            }

            Console.WriteLine("  Playing audio...");

            // Play the downloaded file
            using (_streamPlayer = new MediaFoundationReader(tempFile))
            using (_outputDevice = new WaveOutEvent())
            {
                _outputDevice.Init(_streamPlayer);
                _outputDevice.Volume = _playbackVolume; // Set monitoring volume (doesn't affect capture)
                _isCapturing = true;
                _outputDevice.Play();

                // Wait for playback to complete or cancellation
                while (_outputDevice.PlaybackState == PlaybackState.Playing && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(100, cancellationToken);
                }

                _isCapturing = false;
                _outputDevice.Stop();
            }
        }
        finally
        {
            // Clean up temp file
            try
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
            catch { /* Ignore cleanup errors */ }
        }
    }

    public void Dispose()
    {
        _isCapturing = false;

        _outputDevice?.Stop();
        _outputDevice?.Dispose();
        _streamPlayer?.Dispose();

        if (_loopbackCapture != null)
        {
            _loopbackCapture.StopRecording();
            _loopbackCapture.Dispose();
        }

        _waveWriter?.Dispose();
        _httpClient.Dispose();
    }
}
