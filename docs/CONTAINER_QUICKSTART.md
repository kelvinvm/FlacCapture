# FLAC Capture - Container Quick Reference

## ?? Quick Start

```bash
# 1. Create directories on your Synology NAS
mkdir -p /volume1/audio/{input,output,logs}

# 2. Clone or copy the project
cd /volume1/docker
git clone https://github.com/your-repo/FlacCapture
cd FlacCapture

# 3. Edit docker-compose.yml (adjust volume paths if needed)
# 4. Start the container
sudo docker-compose up -d --build

# 5. Watch the logs
sudo docker logs -f flaccapture
```

## ?? Usage Workflow

```
1. Drop M3U file     ? /volume1/audio/input/show.m3u
2. Container detects ? Logs: "New file detected: show.m3u"
3. Auto-processing   ? Downloads & captures streams
4. FLAC conversion   ? Creates show_20251101_143022.flac
5. Cleanup          ? Moves M3U to input/processed/
6. Output ready    ? /volume1/audio/output/show_20251101_143022.flac
```

## ?? Monitoring

### View Logs
```bash
# Real-time logs
docker logs -f flaccapture

# Last 100 lines
docker logs --tail 100 flaccapture

# Search logs
docker logs flaccapture | grep "ERROR"
```

### Log Output Example
```
[2025-11-01 14:30:22.456] [INFO ] FileWatcher service started
[2025-11-01 14:30:45.123] [INFO ] New file detected: show.m3u
[2025-11-01 14:30:47.234] [INFO ] Found 5 stream URL(s)
[2025-11-01 14:35:22.789] [INFO ] Capture completed
[2025-11-01 14:35:45.123] [INFO ] FLAC conversion successful
```

## ?? Configuration

### Environment Variables (in docker-compose.yml)

```yaml
environment:
  # How often to scan for new M3U files (seconds)
  - SCAN_INTERVAL_SECONDS=30
  
  # Monitoring volume (doesn't affect capture quality)
  - PLAYBACK_VOLUME=0.7
  
  # Delete WAV files after FLAC conversion
  - AUTO_DELETE_WAV=true
  
  # Automatically convert to FLAC
  - AUTO_CONVERT_FLAC=true
  
  # FLAC quality (0-100, higher = better/slower)
  - FLAC_QUALITY=100
```

### Volume Mounts

```yaml
volumes:
  - /volume1/audio/input:/app/input  # Drop M3U files here
  - /volume1/audio/output:/app/output  # FLAC files saved here
  - /volume1/audio/logs:/app/logs      # Future log files
```

## ??? Common Tasks

### Restart Container
```bash
docker-compose restart
```

### Update Container
```bash
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

### Check Container Status
```bash
docker ps | grep flaccapture
```

### Execute Commands in Container
```bash
# Open bash shell
docker exec -it flaccapture /bin/bash

# Check FLAC encoder
docker exec flaccapture which flac

# List output files
docker exec flaccapture ls -lh /app/output
```

### View Resource Usage
```bash
docker stats flaccapture
```

## ?? Troubleshooting

### Container Not Starting
```bash
# Check logs for errors
docker logs flaccapture

# Verify volume paths exist
ls -la /volume1/audio/input
ls -la /volume1/audio/output
```

### Files Not Being Processed
```bash
# Verify file watcher is running
docker logs flaccapture | grep "FileWatcher"

# Check permissions
ls -la /volume1/audio/input

# Verify M3U file format
cat /volume1/audio/input/test.m3u
```

### No Output Files
```bash
# Check if capture completed
docker logs flaccapture | grep "Capture completed"

# Check output directory
docker exec flaccapture ls -lh /app/output

# Check conversion status
docker logs flaccapture | grep "FLAC"
```

## ?? Expected Behavior

### File Lifecycle

```
M3U file placed
      ?
Container detects (within SCAN_INTERVAL_SECONDS)
      ?
Reads URLs from M3U
   ?
Downloads each stream
 ?
Captures audio (bit-perfect)
      ?
Saves as WAV
      ?
Converts to FLAC (if AUTO_CONVERT_FLAC=true)
      ?
Deletes WAV (if AUTO_DELETE_WAV=true)
      ?
Moves M3U to processed/ folder
      ?
FLAC file ready in output directory
```

### Processing Time

Approximate times for 1-hour show:
- Download: 2-5 minutes (depends on stream speed)
- Capture: Real-time (60 minutes)
- FLAC conversion: 30-60 seconds
- **Total: ~62-66 minutes**

### File Sizes

For 1-hour audio at 48kHz/32-bit/Stereo:
- **WAV**: ~1.3 GB (uncompressed)
- **FLAC**: ~650 MB (50% compression)
- **Disk space needed**: 1.95 GB during processing, 650 MB final

## ?? Security Notes

- Container runs as non-root (when possible)
- No ports exposed (no external access)
- Only outbound connections (to stream URLs)
- Logs may contain stream URLs (consider privacy)

## ? Performance Tips

1. **Use SSD for output** - Faster writes during capture
2. **Wired network** - More reliable stream downloads
3. **Adjust scan interval** - Lower = more responsive, higher = less CPU
4. **One stream at a time** - Current design (prevents overload)

## ?? Directory Structure Inside Container

```
/app/
??? FlacCapture.dll      # Main application
??? input/           # Input directory (mounted)
?   ??? *.m3u           # New M3U files
?   ??? processed/    # Processed M3U files
??? output/      # Output directory (mounted)
?   ??? *.flac              # Captured FLAC files
??? logs/               # Log directory (mounted)
```

## ?? M3U File Format

**Simple format:**
```
http://stream.example.com/segment1.mp3
http://stream.example.com/segment2.mp3
http://stream.example.com/segment3.mp3
```

**With comments (ignored):**
```
# Morning Show - November 1, 2025
http://stream.example.com/segment1.mp3
http://stream.example.com/segment2.mp3
# This line is ignored
http://stream.example.com/segment3.mp3
```

## ??? Advanced Options

### Custom Docker Compose Override

Create `docker-compose.override.yml`:
```yaml
version: '3.8'

services:
  flaccapture:
    environment:
      - SCAN_INTERVAL_SECONDS=10  # Faster scanning
    deploy:
      resources:
        limits:
          cpus: '2.0'
   memory: 2G
```

### Build Custom Image

```bash
# Build with specific tag
docker build -t flaccapture:v1.0 .

# Build for different platform
docker buildx build --platform linux/amd64 -t flaccapture:amd64 .
```

## ?? Support

For detailed documentation, see:
- `DOCKER_DEPLOYMENT.md` - Complete deployment guide
- `README.md` - Application documentation
- `AUDIO_LEVELS_EXPLAINED.md` - Audio capture details

## ?? Updates & Changelog

Check GitHub releases for latest updates and bug fixes.

---

**Ready to go!** Drop an M3U file in `/volume1/audio/input` and watch the logs with `docker logs -f flaccapture`
