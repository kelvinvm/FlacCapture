# Quick Start Guide

## Setup (5 minutes)

1. **Build the project** (if you haven't already):
   ```bash
   cd E:\Dev\FlacCapture\FlacCapture
   dotnet build -c Release
   ```

2. **Verify your M3U file**:
   - Location: `E:\Dev\FlacCapture\FlacCapture\Assests\show.m3u`
   - Contains: URLs to audio streams (one per line)

3. **(Optional) Install FLAC encoder**:
   - Download from: https://xiph.org/flac/download.html
   - Extract `flac.exe` to the same folder as `FlacCapture.exe`
   - Or add to your Windows PATH

## Run Your First Capture

### Option 1: From Visual Studio
1. Press `F5` or click "Start"
2. Follow the on-screen prompts

### Option 2: From Command Line
```bash
cd E:\Dev\FlacCapture\FlacCapture\bin\Release\net9.0
FlacCapture.exe
```

### Option 3: From Windows Explorer
Double-click `FlacCapture.exe` in the bin folder

## What to Expect

```
FLAC Capture Utility - Bit-Perfect WASAPI Recording
====================================================

Reading playlist: E:\Dev\FlacCapture\FlacCapture\Assests\show.m3u
Found 5 stream(s)
Output file: capture_20250101_120000.wav

Initializing WASAPI loopback capture...
Capture format: 48000Hz, 32bit, 2ch
This is bit-perfect capture of the audio output.

WASAPI loopback capture started.
Playing streams... Press Ctrl+C to stop early.

[1/5] Playing: http://drdemento.com/...14971737a-1103-128.mp3
  Downloading stream...
  Playing audio...
[1/5] Completed.
```

## Tips

? **Make sure audio is playing**: You should hear the streams through your speakers/headphones  
? **Close other audio apps**: To avoid capturing unwanted sounds  
? **Use headphones**: For a cleaner capture (prevents microphone feedback)  
? **Check disk space**: ~22 MB per minute for WAV, ~10 MB per minute for FLAC  

## Stopping Early

Press `Ctrl+C` to stop capture at any time. Your WAV file will be saved with everything captured up to that point.

## After Capture

You'll be prompted:
```
Convert to FLAC format? (y/n):
```

- Type `y` to convert to FLAC (smaller file, lossless)
- Type `n` to keep only the WAV file

If you convert to FLAC, you'll then be asked:
```
Delete the original WAV file? (y/n):
```

- Type `y` to delete the WAV and keep only the FLAC
- Type `n` to keep both files

## Output Location

Files are saved in: `E:\Dev\FlacCapture\FlacCapture\bin\Release\net9.0\`

Look for files named: `capture_YYYYMMDD_HHMMSS.wav` (or `.flac`)

## Troubleshooting

### "M3U file not found"
- Check that `show.m3u` exists in the `Assests` folder
- Verify the path matches your setup

### "No audio is captured"
- Make sure the streams are actually playing (you should hear them)
- Check Windows Sound settings (default playback device)
- Try playing music from another app while capturing

### "Error playing stream"
- Check your internet connection
- Verify the URLs in your M3U file are accessible
- Try opening one of the URLs in a web browser

### "FLAC encoder not found"
- Download flac.exe from https://xiph.org/flac/download.html
- Place it in the same folder as FlacCapture.exe
- Or manually convert later: `flac -8 capture.wav`

## Next Steps

?? Read `README.md` for detailed information  
?? Check `EXAMPLES.md` for advanced usage scenarios  
?? Modify `Program.cs` to customize behavior  
?? Enjoy your high-quality audio captures!

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review the detailed README.md
3. Examine the code in Program.cs
4. Test with a simple, single-stream M3U file first

## System Requirements

- Windows 10 or later (for WASAPI support)
- .NET 9.0 Runtime
- ~30 MB free RAM
- Disk space: ~22 MB per minute of audio (WAV) or ~10 MB per minute (FLAC)
- Active internet connection for streaming
- Audio output device (speakers/headphones)

Happy capturing! ??
