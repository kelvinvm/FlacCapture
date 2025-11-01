# FLAC Capture Utility

A Windows utility for bit-perfect audio capture using WASAPI (Windows Audio Session API) loopback recording. This tool captures internet audio streams and saves them in high-quality WAV format, with optional FLAC conversion using built-in Windows Media Foundation or external FLAC encoder.

## Features

- **Bit-Perfect Capture**: Uses WASAPI loopback to capture exactly what Windows is playing, without any quality loss
- **M3U Playlist Support**: Reads M3U playlist files and captures all listed streams sequentially
- **High-Quality Output**: Captures at the native Windows audio format (typically 48kHz, 32-bit float, stereo)
- **Internal FLAC Encoding**: Uses Windows Media Foundation for FLAC conversion (no external dependencies)
- **Fallback Support**: Automatically falls back to external flac.exe if Media Foundation isn't available
- **Real-time Monitoring**: Shows progress as each stream is downloaded and played

## How It Works

1. **WASAPI Loopback Capture**: The utility uses Windows' built-in loopback recording feature to capture the exact audio output from your sound card. This is the same audio you would hear through your speakers/headphones.

2. **Stream Playback**: The application downloads and plays each stream URL from the M3U file, while simultaneously recording everything through WASAPI.

3. **WAV Output**: Captures are saved as uncompressed WAV files in the same format as your Windows audio output.

4. **FLAC Conversion** (Optional): After capture, you can convert the WAV file to FLAC format using:
   - **Primary**: Windows Media Foundation encoder (built-in, no external dependencies)
   - **Fallback**: External flac.exe if Media Foundation FLAC codec isn't available

## Requirements

- Windows 10 or later (with Media Foundation support)
- .NET 9.0 Runtime
- Active internet connection for streaming
- **(Optional)** FLAC encoder (flac.exe) if Windows Media Foundation FLAC codec is not installed
  - Download from: https://xiph.org/flac/download.html
  - Place in the same folder as the executable or in your system PATH

## Usage

### Basic Usage

1. Place your M3U playlist file at: `FlacCapture\Assests\show.m3u`

2. Run the program:
   ```
   FlacCapture.exe
   ```

3. The utility will:
   - Read the M3U file
   - Initialize WASAPI loopback capture
   - Download and play each stream
   - Capture all audio to a WAV file
   - Offer to convert to FLAC format (using internal encoder with automatic fallback)

### M3U File Format

Your M3U file should contain one URL per line:

```
http://example.com/stream1.mp3
http://example.com/stream2.mp3
http://example.com/stream3.mp3
```

Lines starting with `#` are treated as comments and ignored.

### Stopping Early

Press `Ctrl+C` at any time to stop capture early. The WAV file will be saved with whatever was captured up to that point.

## Output Files

- **WAV File**: `capture_YYYYMMDD_HHMMSS.wav`
  - Uncompressed, bit-perfect audio capture
  - Format matches your Windows audio output (typically 48kHz/32-bit/Stereo)
  
- **FLAC File**: `capture_YYYYMMDD_HHMMSS.flac` (if converted)
  - Lossless compression (typically 50-60% smaller than WAV)
  - Preserves exact audio quality
  - Encoded using Windows Media Foundation or external flac.exe

## Technical Details

### WASAPI Loopback Recording

WASAPI (Windows Audio Session API) loopback mode captures the mixed audio output from all applications. This means:

- ? **Bit-perfect**: No resampling or quality loss
- ? **System audio**: Captures exactly what Windows is playing
- ? **No drivers needed**: Uses built-in Windows APIs
- ?? **Captures ALL system audio**: Any sounds from other apps will also be recorded

### Internal FLAC Encoding

The utility now uses **Windows Media Foundation** for FLAC encoding, which means:

- ? **No external dependencies**: Works out-of-the-box on most Windows 10/11 systems
- ? **Native integration**: Uses Microsoft's built-in codec infrastructure
- ? **Automatic fallback**: If Media Foundation FLAC isn't available, automatically tries external flac.exe
- ? **High quality**: Produces standard-compliant FLAC files

### Why WAV First, Then FLAC?

1. **Real-time Performance**: Writing uncompressed WAV is faster and more reliable during capture
2. **Bit-Perfect Guarantee**: No encoding overhead during the critical capture phase
3. **Flexibility**: You can keep the WAV, convert to FLAC, or convert to other formats later
4. **Safety**: If conversion fails, you still have the original capture

## Troubleshooting

### No audio is captured
- Make sure audio is actually playing through your default Windows output device
- Check Windows Sound Settings to ensure the correct playback device is selected
- Try playing something in another app while capturing to verify WASAPI is working

### FLAC conversion fails
- The utility will automatically try both internal (Media Foundation) and external (flac.exe) methods
- If both fail, the WAV file is still saved and can be converted manually later
- To use external encoder: download flac.exe from https://xiph.org/flac/download.html
- You can manually convert the WAV file later using: `flac -8 input.wav`

### "Media Foundation FLAC encoder not available"
- This is normal on some Windows installations
- The utility automatically falls back to external flac.exe
- If flac.exe is also not found, you'll get instructions on how to obtain it
- The WAV file is always saved regardless of FLAC conversion status

### Streams fail to download
- Check your internet connection
- Verify the URLs in your M3U file are accessible
- Some streams may have authentication or regional restrictions

### File is too large
- WAV files are uncompressed and can be very large (about 10 MB per minute at 48kHz/32-bit/stereo)
- Use FLAC conversion to reduce size by 40-50% without any quality loss
- For even smaller files, you could manually convert to MP3 or AAC (with quality loss)

## Dependencies

- **NAudio** (v2.2.1): Audio capture and playback library
- **NAudio.Lame** (v2.1.0): Additional audio format support
- **.NET 9.0**: Runtime framework
- **Windows Media Foundation**: Built-in Windows API for FLAC encoding (optional but recommended)

## License

This is a utility tool. Please ensure you have the right to record and store the audio streams you capture.

## Credits

Built with NAudio library by Mark Heath and contributors.
FLAC format by Xiph.Org Foundation.
Windows Media Foundation by Microsoft.
