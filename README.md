# FLAC Capture

A Windows/Docker utility for bit-perfect audio capture using WASAPI loopback recording. Captures internet audio streams from M3U playlists and saves them as high-quality FLAC files.

## Features

- ? **Bit-Perfect WASAPI Loopback** - Exact digital copy of audio output
- ?? **M3U Playlist Support** - Process multiple streams automatically
- ?? **Internal FLAC Encoding** - Windows Media Foundation with flac.exe fallback
- ?? **Docker Support** - Containerized for Synology NAS deployment
- ?? **Automatic File Monitoring** - Service mode watches directory for new M3U files
- ?? **Comprehensive Logging** - Structured logs for monitoring and debugging

## Quick Start

### Windows (Interactive Mode)

```bash
# Build and run
cd FlacCapture
dotnet run

# Or use the published executable
FlacCapture.exe
```

Place your M3U file at `FlacCapture\Assests\show.m3u` and follow the prompts.

### Docker (Service Mode)

```bash
# Build and start container
docker-compose up -d --build

# Monitor logs
docker logs -f flaccapture

# Drop M3U file in input directory
# Output FLAC files appear in output directory
```

For Synology NAS deployment, see [Docker Deployment Guide](docs/DOCKER_DEPLOYMENT.md).

## Documentation

### Getting Started
- ?? **[Quick Start Guide](docs/QUICKSTART.md)** - Get running in 5 minutes
- ?? **[Container Quick Start](docs/CONTAINER_QUICKSTART.md)** - Docker usage reference
- ?? **[User Guide](docs/README.md)** - Complete feature documentation

### Deployment
- ?? **[Docker Deployment](docs/DOCKER_DEPLOYMENT.md)** - Full Synology NAS setup guide
- ?? **[Containerization Summary](docs/CONTAINERIZATION_SUMMARY.md)** - Architecture overview

### Technical Details
- ??? **[Audio Levels Explained](docs/AUDIO_LEVELS_EXPLAINED.md)** - Understanding WASAPI loopback
- ?? **[FLAC Encoding Update](docs/INTERNAL_FLAC_UPDATE.md)** - Internal encoding implementation
- ?? **[File Locking Fix](docs/FILE_LOCKING_FIX.md)** - Handling file access issues

### Advanced Usage
- ?? **[Usage Examples](docs/EXAMPLES.md)** - Common scenarios and tips

## Architecture

```
???????????????????????????????????????
?  Input: M3U Playlist File ?
?  (Internet stream URLs)      ?
???????????????????????????????????????
      ?
???????????????????????????????????????
?  Download & Play Streams     ?
?  (HTTP streaming)            ?
???????????????????????????????????????
 ?
???????????????????????????????????????
?  WASAPI Loopback Capture            ?
?  (Bit-perfect digital copy)         ?
???????????????????????????????????????
      ?
???????????????????????????????????????
?  WAV File (Uncompressed)            ?
?  48kHz/32-bit/Stereo       ?
???????????????????????????????????????
        ?
???????????????????????????????????????
?  FLAC Encoding            ?
?  (Lossless compression ~50%)        ?
???????????????????????????????????????
   ?
???????????????????????????????????????
?  Output: FLAC File      ?
?  (Bit-perfect, compressed)     ?
???????????????????????????????????????
```

## System Requirements

### Windows (Interactive Mode)
- Windows 10 or later
- .NET 9.0 Runtime
- Audio output device (for playback/capture)

### Docker (Service Mode)
- Docker or Docker Desktop
- Synology NAS with Container Manager (or any Linux system)
- Volume mounts for input/output directories

## Key Technologies

- **NAudio** - Audio capture and processing
- **Windows Media Foundation** - FLAC encoding
- **WASAPI** - Windows Audio Session API
- **.NET 9** - Modern C# runtime
- **Docker** - Containerization

## Project Structure

```
FlacCapture/
??? FlacCapture/      # Main application
?   ??? Program.cs           # Entry point (interactive + service modes)
?   ??? WasapiFlacCapture.cs # Audio capture engine
?   ??? FlacConverter.cs   # FLAC encoding
?   ??? FileWatcherService.cs # Directory monitoring
?   ??? ConsoleLogger.cs   # Logging infrastructure
??? docs/    # Documentation
?   ??? QUICKSTART.md
?   ??? DOCKER_DEPLOYMENT.md
?   ??? ...
??? Dockerfile        # Container build
??? docker-compose.yml  # Container orchestration
??? README.md      # This file
```

## Usage Modes

### 1. Interactive Mode (Original)
Run directly on Windows for one-time captures:
```bash
FlacCapture.exe
```

### 2. Service Mode (Container)
Run as Docker container for automated processing:
```bash
docker-compose up -d
```

Automatically processes M3U files dropped in input directory.

## Configuration

### Interactive Mode
Edit `Program.cs` or use default settings.

### Service Mode
Configure via environment variables in `docker-compose.yml`:

| Variable | Default | Description |
|----------|---------|-------------|
| `SCAN_INTERVAL_SECONDS` | 30 | How often to check for new files |
| `PLAYBACK_VOLUME` | 0.7 | Monitoring volume (doesn't affect capture) |
| `AUTO_DELETE_WAV` | true | Delete WAV after FLAC conversion |
| `AUTO_CONVERT_FLAC` | true | Automatically convert to FLAC |
| `FLAC_QUALITY` | 100 | FLAC quality (0-100) |

## Contributing

This is a personal project, but suggestions and bug reports are welcome via GitHub Issues.

## License

This project is provided as-is for personal use. See individual library licenses for NAudio and other dependencies.

## Credits

- **NAudio** by Mark Heath - Audio processing library
- **FLAC** by Xiph.Org Foundation - Lossless audio codec
- **Windows Media Foundation** by Microsoft - Audio encoding infrastructure

## Support

For questions and issues:
1. Check the [documentation](docs/)
2. Review [usage examples](docs/EXAMPLES.md)
3. Open a GitHub Issue

## Version History

- **v1.0.0** - Initial release
  - WASAPI loopback capture
  - M3U playlist support
  - Internal FLAC encoding
  - Docker containerization
  - Automatic file monitoring

---

**Ready to capture!** Start with the [Quick Start Guide](docs/QUICKSTART.md) or [Docker Deployment Guide](docs/DOCKER_DEPLOYMENT.md).
