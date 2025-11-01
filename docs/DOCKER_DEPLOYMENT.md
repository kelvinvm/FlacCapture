# FLAC Capture - Docker Container Deployment Guide

## Overview

This guide explains how to deploy the FLAC Capture utility as a Docker container on your Synology NAS. The container automatically monitors a directory for M3U playlist files, captures the audio streams, converts them to FLAC format, and saves the output.

## Architecture

```
????????????????????????????????????????????????????????
? Synology NAS                   ?
?          ?
?  ?????????????????????????????????????????????????? ?
?  ?  Docker Container: flaccapture            ? ?
?  ?   ? ?
?  ?  ???????????????????????????????????????????? ? ?
?  ?  ?  FileWatcherService   ? ? ?
?  ?  ?  - Monitors /app/input for *.m3u files  ? ? ?
?  ?  ?  - Processes streams automatically     ? ? ?
?  ?  ?  - Logs to console (captured by Docker) ? ? ?
?  ?  ???????????????????????????????????????????? ? ?
?  ?           ? ?
?  ?  Volumes:           ? ?
?  ?  - /app/input  ? /volume1/audio/input         ? ?
?  ?  - /app/output ? /volume1/audio/output        ? ?
?  ?  - /app/logs   ? /volume1/audio/logs     ? ?
?  ?????????????????????????????????????????????????? ?
????????????????????????????????????????????????????????
```

## Prerequisites

### On Synology NAS

1. **Docker package installed**
   - Open Package Center
   - Install "Docker" or "Container Manager"

2. **Shared folders created**
   - `/volume1/audio/input` - Drop M3U files here
   - `/volume1/audio/output` - FLAC files saved here
   - `/volume1/audio/logs` - (Optional) Additional logs

3. **SSH access enabled** (for initial setup)
   - Control Panel ? Terminal & SNMP ? Enable SSH service

## Quick Start

### Option 1: Using Docker Compose (Recommended)

1. **Upload files to Synology**
   ```bash
   # On your local machine
   scp -r FlacCapture/ admin@your-nas-ip:/volume1/docker/FlacCapture
   ```

2. **SSH into Synology**
   ```bash
   ssh admin@your-nas-ip
   cd /volume1/docker/FlacCapture
   ```

3. **Edit docker-compose.yml**
   ```bash
   vi docker-compose.yml
   # Update volume paths to match your NAS structure
   ```

4. **Build and start**
   ```bash
   sudo docker-compose up -d --build
   ```

5. **View logs**
```bash
   sudo docker-compose logs -f
   ```

### Option 2: Using Synology Docker UI

1. **Build the image**
   ```bash
   ssh admin@your-nas-ip
   cd /volume1/docker/FlacCapture
   sudo docker build -t flaccapture:latest .
   ```

2. **Create container via Docker UI**
   - Open Docker app on Synology
   - Go to "Image" ? Find `flaccapture:latest`
   - Click "Launch"
   - Configure as shown below

3. **Container settings:**
   - **Container Name:** `flaccapture`
   - **Enable auto-restart:** ?
   - **Volumes:**
     ```
     /volume1/audio/input  ? /app/input
     /volume1/audio/output ? /app/output
     /volume1/audio/logs   ? /app/logs
     ```
 - **Environment Variables:**
     ```
     SCAN_INTERVAL_SECONDS = 30
     PLAYBACK_VOLUME = 0.7
     AUTO_DELETE_WAV = true
     AUTO_CONVERT_FLAC = true
     FLAC_QUALITY = 100
     TZ = America/New_York
     ```
   - **Command:** `--service`

## Configuration

### Environment Variables

| Variable | Description | Default | Options |
|----------|-------------|---------|---------|
| `INPUT_DIR` | Input directory for M3U files | `/app/input` | Any valid path |
| `OUTPUT_DIR` | Output directory for captures | `/app/output` | Any valid path |
| `SCAN_INTERVAL_SECONDS` | How often to scan for files | `30` | 10-300 |
| `PLAYBACK_VOLUME` | Monitoring volume (doesn't affect capture) | `0.7` | 0.0-1.0 |
| `AUTO_DELETE_WAV` | Delete WAV after FLAC conversion | `true` | true/false |
| `AUTO_CONVERT_FLAC` | Automatically convert to FLAC | `true` | true/false |
| `FLAC_QUALITY` | FLAC quality level | `100` | 0-100 |
| `TZ` | Timezone for timestamps | `America/New_York` | Any TZ string |

### Volume Mounts

The container expects three volume mounts:

1. **Input Directory** (`/app/input`)
   - Drop your M3U files here
   - Files are automatically processed
   - Processed files moved to `input/processed/` subdirectory

2. **Output Directory** (`/app/output`)
   - FLAC files (and optionally WAV files) saved here
   - Naming: `{m3u-filename}_{timestamp}.flac`
   - Example: `show_20251101_143022.flac`

3. **Log Directory** (`/app/logs`)
   - (Optional) For future log file support
   - Currently logs go to Docker stdout/stderr

## Usage

### 1. Place M3U File

Create or upload an M3U file to the input directory:

**Example: `/volume1/audio/input/morning_show.m3u`**
```
http://stream.example.com/segment1.mp3
http://stream.example.com/segment2.mp3
http://stream.example.com/segment3.mp3
```

### 2. Automatic Processing

The container will:
1. Detect the new file (within `SCAN_INTERVAL_SECONDS`)
2. Log: `New file detected: morning_show.m3u`
3. Read the URLs from the M3U file
4. Download and play each stream (capturing via loopback)
5. Save as WAV file
6. Convert to FLAC (if `AUTO_CONVERT_FLAC=true`)
7. Delete WAV (if `AUTO_DELETE_WAV=true`)
8. Move M3U to `input/processed/` folder
9. Log: `Processing complete: morning_show_20251101_143022.flac`

### 3. Retrieve Output

**Via File Station:**
- Navigate to `/volume1/audio/output`
- Download your FLAC files

**Via SMB/CIFS:**
```
\\your-nas-ip\audio\output\
```

**Via SSH/SCP:**
```bash
scp admin@your-nas-ip:/volume1/audio/output/*.flac ./local-folder/
```

## Monitoring

### View Real-Time Logs

```bash
# SSH into Synology
ssh admin@your-nas-ip

# Follow logs
sudo docker logs -f flaccapture

# Last 100 lines
sudo docker logs --tail 100 flaccapture

# With timestamps
sudo docker logs -t flaccapture
```

### Log Format

```
[2025-11-01 14:30:22.456] [INFO ] FlacCapture.Service: FileWatcher service started
[2025-11-01 14:30:22.458] [INFO ] FlacCapture.Service: Monitoring: /app/input
[2025-11-01 14:30:22.460] [INFO ] FlacCapture.Service: Output to: /app/output
[2025-11-01 14:30:45.123] [INFO ] FlacCapture.Service: New file detected: show.m3u
[2025-11-01 14:30:47.234] [INFO ] FlacCapture.Service: Processing: show.m3u
[2025-11-01 14:30:47.345] [INFO ] FlacCapture.Service: Found 5 stream URL(s)
[2025-11-01 14:30:47.456] [INFO ] FlacCapture.Service: Starting capture for: show
[2025-11-01 14:35:22.789] [INFO ] FlacCapture.Service: Capture completed: show_20251101_143022.wav
[2025-11-01 14:35:22.890] [INFO ] FlacCapture.Service: Converting to FLAC...
[2025-11-01 14:35:45.123] [INFO ] FlacCapture.Service: FLAC conversion successful: show_20251101_143022.flac
[2025-11-01 14:35:45.234] [INFO ] FlacCapture.Service: WAV file deleted (auto-cleanup)
[2025-11-01 14:35:45.345] [INFO ] FlacCapture.Service: M3U file moved to: processed/show.m3u
```

### Log Levels

| Level | Description | When Used |
|-------|-------------|-----------|
| INFO | Normal operations | File detected, processing started/completed |
| WARN | Non-critical issues | File access issues, conversion warnings |
| ERROR | Errors | Processing failures, network errors |
| CRIT | Critical failures | Service crashes, unrecoverable errors |

## Troubleshooting

### Container Won't Start

**Check Docker logs:**
```bash
sudo docker logs flaccapture
```

**Common issues:**
- Volume paths don't exist ? Create them first
- Permission issues ? Check folder permissions
- Port conflicts ? (Not applicable for this container)

### No Files Being Processed

**Check file watcher status:**
```bash
sudo docker logs flaccapture | grep "FileWatcher"
```

**Verify:**
- M3U files are in the correct directory (`/volume1/audio/input`)
- Files have `.m3u` extension (case-sensitive on Linux)
- Container has read permission on input directory
- Container is actually running: `sudo docker ps | grep flaccapture`

### Audio Capture Not Working

**Note:** This container uses **WASAPI loopback**, which is Windows-specific. On Linux/NAS:

**Option 1: Modify for Linux (requires code changes)**
- Use PulseAudio or ALSA instead of WASAPI
- Or use direct HTTP streaming without loopback

**Option 2: Keep Windows-based (current approach)**
- Run on a Windows machine with Docker Desktop
- Use Synology NAS only for storage via SMB mounts

**Current limitation:** The WASAPI loopback feature requires Windows. For Synology (Linux), you'll need to modify the capture method.

### Files Not Converting to FLAC

**Check logs for:**
```
MediaFoundation FLAC encoder not available
```

**Solution:** The container includes the `flac` command-line tool as fallback
```bash
# Verify flac is installed in container
sudo docker exec flaccapture which flac
# Should output: /usr/bin/flac
```

### Disk Space Issues

**Monitor output directory:**
```bash
# SSH into Synology
df -h /volume1/audio/output
```

**Auto-cleanup recommendations:**
- Set `AUTO_DELETE_WAV=true` to save space
- Periodically archive old FLAC files
- Set up Synology scheduled tasks to clean old files

## Maintenance

### Update Container

```bash
# Stop container
sudo docker-compose down

# Rebuild with latest code
sudo docker-compose build --no-cache

# Start updated container
sudo docker-compose up -d
```

### View Container Stats

```bash
# Resource usage
sudo docker stats flaccapture

# Container info
sudo docker inspect flaccapture
```

### Backup Configuration

```bash
# Backup docker-compose.yml and .env files
scp admin@your-nas-ip:/volume1/docker/FlacCapture/docker-compose.yml ./backup/
```

### Clean Up Old Logs

```bash
# Docker auto-rotates logs (configured in docker-compose.yml)
# Max 10MB per file, 3 files kept

# Manual cleanup if needed:
sudo docker-compose down
sudo rm -f /var/lib/docker/containers/$(sudo docker inspect -f '{{.Id}}' flaccapture)/$(sudo docker inspect -f '{{.Id}}' flaccapture)-json.log
sudo docker-compose up -d
```

## Advanced Configuration

### Custom Scan Intervals

For high-volume processing:
```yaml
environment:
  - SCAN_INTERVAL_SECONDS=10  # Check every 10 seconds
```

For low-volume processing:
```yaml
environment:
  - SCAN_INTERVAL_SECONDS=300  # Check every 5 minutes
```

### Network Configuration

If streams require specific network settings:
```yaml
services:
  flaccapture:
    # ... other settings ...
    dns:
      - 8.8.8.8
      - 8.8.4.4
    extra_hosts:
      - "stream.example.com:192.168.1.100"
```

### Resource Limits

To prevent overloading your NAS:
```yaml
services:
  flaccapture:
    # ... other settings ...
 deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 2G
        reservations:
    cpus: '1.0'
    memory: 512M
```

## Security Considerations

1. **File Permissions**
   ```bash
   # Ensure proper permissions on NAS
   sudo chmod 755 /volume1/audio/input
   sudo chmod 755 /volume1/audio/output
   ```

2. **Network Security**
   - Container doesn't expose any ports
   - Only outbound connections (to stream URLs)
   - Consider firewall rules if needed

3. **Data Privacy**
   - Captured audio stored locally on NAS
   - No external services used (except stream sources)
   - Logs may contain stream URLs

## Performance

### Expected Resource Usage

| Metric | Idle | Processing |
|--------|------|------------|
| CPU | < 1% | 10-30% |
| Memory | 50-100 MB | 200-500 MB |
| Disk I/O | Minimal | High (during capture) |
| Network | Minimal | High (during stream download) |

### Optimization Tips

1. **Storage:**
   - Use SSD for output directory if available
   - Keep input directory on HDD (low I/O)

2. **Network:**
   - Use wired connection for NAS
   - QoS settings for stream URLs if needed

3. **Processing:**
   - Process one stream at a time (current implementation)
   - Adjust `SCAN_INTERVAL_SECONDS` based on load

## Support

### Get Help

1. **Check logs first:**
   ```bash
   sudo docker logs --tail 200 flaccapture
   ```

2. **Verify configuration:**
   ```bash
   sudo docker inspect flaccapture
   ```

3. **Test manually:**
 ```bash
   # Place a test M3U file
   echo "http://example.com/test.mp3" > /volume1/audio/input/test.m3u
   
   # Watch logs
   sudo docker logs -f flaccapture
   ```

### Common Commands Reference

```bash
# Start container
sudo docker-compose up -d

# Stop container
sudo docker-compose down

# Restart container
sudo docker-compose restart

# View logs (live)
sudo docker logs -f flaccapture

# View logs (last 100 lines)
sudo docker logs --tail 100 flaccapture

# Execute command in container
sudo docker exec -it flaccapture /bin/bash

# Check container status
sudo docker ps | grep flaccapture

# Remove container and volumes
sudo docker-compose down -v
```

## What's Next

Consider these enhancements:

1. **Web UI** - Add a web interface for monitoring
2. **Notifications** - Email/webhook notifications on completion
3. **Scheduling** - Process specific M3U files at scheduled times
4. **Metadata** - Extract and embed metadata in FLAC files
5. **Cloud Upload** - Auto-upload to cloud storage after processing

## License

See main README.md for license information.

## Credits

- Built with NAudio for audio processing
- Uses FLAC encoder for compression
- Containerized for Synology NAS deployment
