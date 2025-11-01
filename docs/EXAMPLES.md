# Usage Examples for FlacCapture

## Example 1: Basic Usage

The simplest way to use FlacCapture:

1. Place your M3U file at `Assests\show.m3u`
2. Run `FlacCapture.exe`
3. Wait for capture to complete
4. Choose whether to convert to FLAC when prompted

**Expected Output:**
```
FLAC Capture Utility - Bit-Perfect WASAPI Recording
====================================================

Reading playlist: E:\Dev\FlacCapture\FlacCapture\Assests\show.m3u
Found 5 stream(s)
Output file: capture_20250101_123456.wav

Initializing WASAPI loopback capture...
Capture format: 48000Hz, 32bit, 2ch
This is bit-perfect capture of the audio output.

WASAPI loopback capture started.
Playing streams... Press Ctrl+C to stop early.

[1/5] Playing: http://example.com/stream1.mp3
  Downloading stream...
  Playing audio...
[1/5] Completed.

[2/5] Playing: http://example.com/stream2.mp3
...
```

## Example 2: Custom M3U File Location

If your M3U file is elsewhere, you can modify Program.cs:

```csharp
string m3uPath = @"C:\Music\MyStreams\playlist.m3u";
```

## Example 3: Using Configuration (Future Enhancement)

You could extend the program to use the CaptureConfig class:

```csharp
var config = new CaptureConfig
{
    M3uFilePath = @"C:\Music\playlist.m3u",
    OutputDirectory = @"C:\Recordings",
    OutputFilePrefix = "radio_show_",
    AutoConvertToFlac = true,
    AutoDeleteWavAfterConversion = true,
    FlacCompressionLevel = 8
};

if (!config.Validate(out string error))
{
    Console.WriteLine($"Configuration error: {error}");
    return;
}

// Use config in your capture service
```

## Example 4: Capturing a Single Stream

For a single stream, create an M3U file with one line:

**single_stream.m3u:**
```
http://example.com/audio.mp3
```

## Example 5: Manual FLAC Conversion

If you skip the automatic conversion, you can convert later using the FLAC command-line tool:

```bash
# Maximum compression (smallest file)
flac -8 --best --verify capture_20250101_123456.wav

# Fast compression
flac -0 capture_20250101_123456.wav

# With custom output name
flac -8 capture_20250101_123456.wav -o my_recording.flac
```

## Example 6: Batch Processing Multiple M3U Files

Create a batch script to process multiple playlists:

**capture_all.bat:**
```batch
@echo off
for %%f in (*.m3u) do (
    echo Processing %%f...
    copy /Y "%%f" "Assests\show.m3u"
    FlacCapture.exe
)
```

## Example 7: Scheduling Automatic Captures

Use Windows Task Scheduler to automatically capture at specific times:

1. Open Task Scheduler
2. Create a new task
3. Set trigger (e.g., daily at 2:00 PM)
4. Set action: Start program `FlacCapture.exe`
5. Set working directory to your FlacCapture folder

## Example 8: Monitoring Capture Quality

Check the captured WAV file properties:

```powershell
# PowerShell: Get audio file details
Get-Item capture_*.wav | Select-Object Name, Length, LastWriteTime

# Check with ffprobe (if installed)
ffprobe -i capture_20250101_123456.wav
```

Expected output should show:
- Sample rate: 48000 Hz (or your system's default)
- Bit depth: 32-bit float
- Channels: 2 (stereo)

## Example 9: Handling Large Captures

For very long recordings:

```csharp
// Monitor available disk space before starting
var drive = new DriveInfo(Path.GetPathRoot(outputPath));
long requiredSpace = estimatedDurationMinutes * 60 * 48000 * 4 * 2; // rough estimate
if (drive.AvailableFreeSpace < requiredSpace)
{
    Console.WriteLine("Warning: Low disk space!");
}
```

## Example 10: Error Recovery

If a capture fails mid-stream:

1. The WAV file up to that point is still valid
2. You can manually trim and edit it using audio software
3. Use tools like Audacity to clean up:
   - Remove silence at start/end
   - Normalize audio levels
   - Split into separate tracks

## Audio Quality Metrics

### WAV File Sizes (approximate)
- 1 minute at 48kHz/32-bit/stereo ? 22 MB
- 1 hour ? 1.3 GB
- 2 hours ? 2.6 GB

### FLAC File Sizes (approximate)
- 1 minute ? 10-15 MB (depends on audio complexity)
- 1 hour ? 600-900 MB
- 2 hours ? 1.2-1.8 GB

### Compression Ratios
- FLAC typically achieves 40-60% compression
- Music: ~50% compression
- Speech: ~60% compression
- Silence: ~90% compression

## Troubleshooting Commands

### Check if WASAPI is capturing
```powershell
# Windows Sound Settings
control mmsys.cpl

# Check default audio device
Get-WmiObject -Class Win32_SoundDevice | Select-Object Name, Status
```

### Verify stream URLs
```powershell
# Test URL accessibility
Invoke-WebRequest -Uri "http://example.com/stream.mp3" -Method Head
```

### Convert WAV to other formats
```bash
# To MP3 (with quality loss)
ffmpeg -i capture.wav -b:a 320k output.mp3

# To AAC (with quality loss)
ffmpeg -i capture.wav -c:a aac -b:a 256k output.m4a

# To Opus (with quality loss)
ffmpeg -i capture.wav -c:a libopus -b:a 256k output.opus
```

## Advanced: Stream Metadata Preservation

To preserve metadata from the streams, you could extend the code:

```csharp
// Read metadata from HTTP headers
var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
var contentType = response.Content.Headers.ContentType?.MediaType;
var contentLength = response.Content.Headers.ContentLength;

Console.WriteLine($"Stream type: {contentType}");
Console.WriteLine($"Stream size: {contentLength / 1024.0 / 1024.0:F2} MB");
```

## Tips for Best Quality

1. **Close other applications**: Minimize background noise in the capture
2. **Set volume to 100%**: Use full scale for best signal-to-noise ratio
3. **Use exclusive mode**: If possible, configure Windows audio for exclusive mode
4. **Disable audio enhancements**: Turn off Windows sound effects and enhancements
5. **Monitor CPU usage**: Ensure the system isn't dropping samples due to high load

## Integration Examples

### With Cloud Storage
```csharp
// Upload to cloud after capture
await UploadToAzureBlobStorage(outputFlac);
await UploadToDropbox(outputFlac);
```

### With Transcription Services
```csharp
// Send to speech-to-text
var transcript = await AzureSpeechService.TranscribeAsync(outputFlac);
File.WriteAllText("transcript.txt", transcript);
```

### With Metadata Tagging
```csharp
// Add ID3 tags or FLAC comments
var tagger = TagLib.File.Create(outputFlac);
tagger.Tag.Title = "Radio Show - " + DateTime.Now.ToShortDateString();
tagger.Tag.Artist = "Dr. Demento";
tagger.Save();
```
