# FlacCapture - Portainer Deployment Guide for Synology NAS

## Overview

This guide explains how to deploy the FlacCapture container on your Synology NAS using **Portainer**, a popular web-based container management interface. This is an alternative to the command-line Docker approach.

## Why Portainer?

- ??? **Web-based UI** - No SSH or command-line needed
- ?? **Visual Monitoring** - See logs, stats, and container status in browser
- ?? **Easy Updates** - Rebuild and redeploy with a few clicks
- ??? **Configuration Management** - Edit environment variables through UI

## Prerequisites

### 1. Synology NAS Setup

- **DSM Version**: 7.0 or later recommended
- **Docker Package**: Installed (or "Container Manager" on newer DSM)
- **Portainer**: Installed and accessible

### 2. Shared Folders

Create these shared folders on your Synology NAS:

```
/volume1/audio/input   ? For M3U playlist files
/volume1/audio/output  ? For captured FLAC files
/volume1/audio/logs    ? For application logs
/volume1/docker/       ? For Docker projects
```

**To create shared folders:**
1. Open **Control Panel** ? **Shared Folder**
2. Click **Create** ? **Create Shared Folder**
3. Name: `audio`, Location: `volume1`
4. Create subfolders: `input`, `output`, `logs`

### 3. Portainer Installation

If Portainer is not already installed:

**Option A: Via Docker Package (Older DSM)**
1. Open **Docker** app
2. Go to **Registry** ? Search "portainer"
3. Download **portainer/portainer-ce**
4. Launch with these settings:
 - Port: 9000 ? 9000
   - Volume: `/var/run/docker.sock` ? `/var/run/docker.sock`

**Option B: Via Container Manager (DSM 7.2+)**
1. Open **Container Manager**
2. Go to **Registry** ? Search "portainer"
3. Download **portainer/portainer-ce**
4. Create container with volume binding

**Option C: Via Command Line**
```bash
sudo docker run -d \
  --name=portainer \
  --restart=always \
  -p 9000:9000 \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -v portainer_data:/data \
  portainer/portainer-ce:latest
```

**Access Portainer:**
- URL: `http://your-nas-ip:9000`
- Create admin account on first visit

## Installation Methods

### Method 1: Using Docker Compose (Recommended)

This is the easiest method for Portainer.

#### Step 1: Upload Project Files

1. **Connect via File Station or SMB**
   ```
   \\your-nas-ip\docker\
   ```

2. **Create project folder**
   ```
   /volume1/docker/FlacCapture/
   ```

3. **Upload these files:**
   - `docker-compose.yml`
   - `Dockerfile`
   - `.dockerignore`
   - `FlacCapture/` folder (with all source code)

**Quick Upload via SSH (alternative):**
```bash
# From your local machine
scp -r FlacCapture/ admin@your-nas-ip:/volume1/docker/
```

#### Step 2: Deploy via Portainer

1. **Open Portainer** ? `http://your-nas-ip:9000`

2. **Select your environment** (usually "local")

3. **Go to Stacks** ? Click **Add stack**

4. **Configure stack:**
   - **Name**: `flaccapture`
   - **Build method**: Select **Upload**
   - Click **Upload** and select your `docker-compose.yml`

5. **Or paste this docker-compose.yml:**

```yaml
version: '3.8'

services:
  flaccapture:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: flaccapture
    restart: unless-stopped
    
    volumes:
      # Map to your Synology NAS directories
      - /volume1/audio/input:/app/input
      - /volume1/audio/output:/app/output
      - /volume1/audio/logs:/app/logs
    
    environment:
    # Scan interval in seconds (how often to check for new M3U files)
      - SCAN_INTERVAL_SECONDS=30
      
    # Playback monitoring volume (0.0 to 1.0) - doesn't affect capture quality
      - PLAYBACK_VOLUME=0.7
      
      # Automatically delete WAV files after successful FLAC conversion
 - AUTO_DELETE_WAV=true
      
      # Auto-convert to FLAC after capture
      - AUTO_CONVERT_FLAC=true
      
      # FLAC quality (0-100, default 100)
      - FLAC_QUALITY=100
      
      # Timezone (optional, adjust to your location)
      - TZ=America/New_York
    
    network_mode: "bridge"
    
    logging:
      driver: "json-file"
      options:
max-size: "10m"
        max-file: "3"

networks:
  default:
    name: flaccapture_network
```

6. **Edit environment variables** if needed:
   - Adjust `SCAN_INTERVAL_SECONDS` (10-300)
   - Change `TZ` to your timezone
   - Modify other settings as needed

7. **Deploy the stack:**
   - Click **Deploy the stack**
   - Portainer will build and start the container
   - Wait for build to complete (2-5 minutes)

#### Step 3: Verify Deployment

1. **Check container status:**
   - Go to **Containers** in Portainer
   - Look for `flaccapture` container
   - Status should be **running** (green)

2. **View logs:**
   - Click on the container name
   - Click **Logs**
   - You should see:
   ```
   [INFO] FLAC Capture Service - Container Mode
   [INFO] FileWatcher service started
   [INFO] Monitoring: /app/input
   ```

3. **Check resources:**
   - Click **Stats** tab
   - Monitor CPU, Memory usage

### Method 2: Using Portainer Web Editor

If you prefer not to upload files:

#### Step 1: Create Stack with Web Editor

1. **Open Portainer** ? **Stacks** ? **Add stack**

2. **Name**: `flaccapture`

3. **Build method**: Select **Web editor**

4. **Paste the docker-compose.yml content** (see above)

5. **Note**: This method requires the image to be pre-built or available on Docker Hub

**For pre-built image deployment:**

```yaml
version: '3.8'

services:
  flaccapture:
    image: flaccapture:latest  # Use your pre-built image
  container_name: flaccapture
    restart: unless-stopped
    
 volumes:
      - /volume1/audio/input:/app/input
   - /volume1/audio/output:/app/output
    - /volume1/audio/logs:/app/logs
    
    environment:
      - SCAN_INTERVAL_SECONDS=30
      - PLAYBACK_VOLUME=0.7
      - AUTO_DELETE_WAV=true
    - AUTO_CONVERT_FLAC=true
      - FLAC_QUALITY=100
      - TZ=America/New_York
```

#### Step 2: Build Image First (if needed)

If the image doesn't exist, build it via SSH first:

```bash
ssh admin@your-nas-ip
cd /volume1/docker/FlacCapture
sudo docker build -t flaccapture:latest .
```

Then use Method 2 with the pre-built image.

### Method 3: Using Portainer Images

For advanced users who have the image in a registry:

1. **Go to** ? **Images** ? **Import**
2. Import your pre-built FlacCapture image
3. **Go to** ? **Containers** ? **Add container**
4. Configure manually (volumes, environment, etc.)

## Configuration

### Environment Variables

Edit these in Portainer after deployment:

1. **Go to Containers** ? Click **flaccapture**
2. Click **Duplicate/Edit**
3. Scroll to **Env** section
4. Modify variables:

| Variable | Default | Description |
|----------|---------|-------------|
| `SCAN_INTERVAL_SECONDS` | 30 | How often to check for M3U files |
| `PLAYBACK_VOLUME` | 0.7 | Monitoring volume (doesn't affect capture) |
| `AUTO_DELETE_WAV` | true | Delete WAV after FLAC conversion |
| `AUTO_CONVERT_FLAC` | true | Automatically convert to FLAC |
| `FLAC_QUALITY` | 100 | FLAC quality (0-100) |
| `TZ` | America/New_York | Timezone for timestamps |

5. Click **Deploy the container** to apply changes

### Volume Mappings

Verify in **Volumes** section:

| Container Path | Synology Path | Purpose |
|---------------|---------------|---------|
| `/app/input` | `/volume1/audio/input` | Drop M3U files here |
| `/app/output` | `/volume1/audio/output` | FLAC files saved here |
| `/app/logs` | `/volume1/audio/logs` | Application logs |

## Usage

### 1. Drop M3U File

**Via File Station:**
1. Open **File Station**
2. Navigate to `audio/input`
3. Upload your M3U file

**Via SMB/CIFS:**
```
\\your-nas-ip\audio\input\
```

**Example M3U file (`show.m3u`):**
```
http://stream.example.com/segment1.mp3
http://stream.example.com/segment2.mp3
http://stream.example.com/segment3.mp3
```

### 2. Monitor Progress

**Via Portainer:**
1. **Containers** ? Click **flaccapture**
2. Click **Logs** tab
3. Watch real-time progress:
   ```
   [INFO] New file detected: show.m3u
   [INFO] Found 3 stream URL(s)
   [INFO] Starting capture for: show
   [INFO] [1/3] Playing: http://...
   [INFO] Capture completed
   [INFO] FLAC conversion successful
   ```

**Via Synology Logs:**
1. Open **Container Manager** (or Docker)
2. Select **flaccapture** container
3. Click **Details** ? **Log** tab

### 3. Retrieve Output

**Via File Station:**
- Navigate to `audio/output`
- Download FLAC files

**Via SMB/CIFS:**
```
\\your-nas-ip\audio\output\
```

## Monitoring & Management

### Container Health

**In Portainer:**
1. **Containers** ? **flaccapture**
2. Check **Status**: Should be "running"
3. **Stats** tab shows:
   - CPU usage
   - Memory usage
   - Network I/O

**Expected resource usage:**
- **Idle**: CPU < 1%, Memory ~50-100 MB
- **Processing**: CPU 10-30%, Memory 200-500 MB

### Viewing Logs

**Real-time logs:**
1. **Containers** ? **flaccapture** ? **Logs**
2. Enable **Auto-refresh logs**
3. Adjust **Lines** (100, 500, 1000, All)

**Search logs:**
- Use the search box to filter by keyword
- Search for "ERROR" to find issues
- Search for "Completed" to see finished captures

**Download logs:**
- Click **Download** button to save logs locally

### Container Actions

**From Portainer Containers page:**

| Action | Purpose |
|--------|---------|
| **Stop** | Gracefully stop container |
| **Restart** | Restart container (after config changes) |
| **Kill** | Force stop container |
| **Remove** | Delete container (keeps volumes) |
| **Logs** | View container logs |
| **Inspect** | View detailed configuration |
| **Stats** | Monitor resource usage |
| **Console** | Open shell inside container |

## Updating the Application

### Method 1: Rebuild via Portainer

1. **Go to Stacks** ? Select **flaccapture**
2. Click **Editor** to modify if needed
3. Click **Update the stack**
4. Enable **Re-pull image and redeploy**
5. Click **Update**

### Method 2: Pull and Restart

1. **Update source files** on Synology (if changed)
2. **Go to Stacks** ? **flaccapture**
3. Click **Stop** ? **Start**
4. Or click **Restart**

### Method 3: Rebuild Image

**Via Portainer Console:**
1. **Containers** ? **flaccapture** ? **Console**
2. Connect using `/bin/bash`
3. Or use SSH:
   ```bash
   ssh admin@your-nas-ip
   cd /volume1/docker/FlacCapture
   sudo docker-compose down
   sudo docker-compose build --no-cache
   sudo docker-compose up -d
   ```

## Troubleshooting

### Container Won't Start

**Check in Portainer:**
1. **Containers** ? **flaccapture** ? **Logs**
2. Look for errors in startup messages

**Common issues:**

| Error | Solution |
|-------|----------|
| "Volume path not found" | Create missing folders in File Station |
| "Permission denied" | Check folder permissions (755 or 777) |
| "Port already in use" | N/A (this container doesn't expose ports) |
| "Build failed" | Check Dockerfile and source code are uploaded |

### No Files Being Processed

**Verify file watcher:**
1. Check logs for "FileWatcher service started"
2. Verify M3U file is in `/volume1/audio/input`
3. Confirm file extension is `.m3u` (lowercase)
4. Check file permissions (should be readable)

**Debug steps:**
```bash
# Via Portainer Console or SSH
sudo docker exec -it flaccapture /bin/bash
ls -la /app/input
cat /app/input/test.m3u
```

### FLAC Conversion Fails

**Check logs for:**
```
MediaFoundation FLAC encoder not available
Using external FLAC encoder...
```

**Verify flac.exe is available:**
```bash
sudo docker exec flaccapture which flac
# Should output: /usr/bin/flac
```

**If missing, rebuild container** (unlikely, as Dockerfile includes it)

### Can't Access Logs

**If logs don't show in Portainer:**
1. Check container is running
2. Try **Console** tab instead
3. Access via SSH:
   ```bash
   sudo docker logs flaccapture
   sudo docker logs -f flaccapture  # Follow mode
 ```

### Disk Space Issues

**Monitor disk usage:**
1. **Storage** ? **Storage Manager**
2. Check `volume1` free space

**Clean up:**
1. Enable `AUTO_DELETE_WAV=true`
2. Archive old FLAC files periodically
3. Delete processed M3U files from `input/processed/`

## Advanced Configuration

### Custom Network

**Create isolated network:**
1. **Networks** ? **Add network**
2. Name: `flaccapture_isolated`
3. Driver: `bridge`

**Update stack to use it:**
```yaml
networks:
flaccapture_isolated:
    external: true

services:
  flaccapture:
networks:
      - flaccapture_isolated
```

### Resource Limits

**Add resource constraints:**
1. **Containers** ? **flaccapture** ? **Duplicate/Edit**
2. Scroll to **Resources and Limits**
3. Set limits:
- CPU: `2.0` (2 cores max)
   - Memory: `2GB`

**Or in docker-compose.yml:**
```yaml
services:
  flaccapture:
    deploy:
 resources:
        limits:
  cpus: '2.0'
    memory: 2G
        reservations:
     cpus: '1.0'
        memory: 512M
```

### Scheduled Tasks

**For periodic cleanup or maintenance:**
1. **Control Panel** ? **Task Scheduler**
2. **Create** ? **Scheduled Task** ? **User-defined script**
3. Example script:
   ```bash
   #!/bin/bash
   # Clean up old FLAC files (older than 90 days)
   find /volume1/audio/output -name "*.flac" -mtime +90 -delete
   
   # Clean up processed M3U files
   find /volume1/audio/input/processed -name "*.m3u" -mtime +30 -delete
   ```

### Multiple Instances

**To run multiple capture services:**
1. Duplicate the stack
2. Change:
   - Stack name: `flaccapture-2`
   - Container name: `flaccapture2`
   - Volume paths: `/volume1/audio2/input`, etc.

```yaml
services:
  flaccapture2:
    container_name: flaccapture2
    volumes:
      - /volume1/audio2/input:/app/input
      - /volume1/audio2/output:/app/output
```

## Portainer-Specific Features

### Webhooks

**Auto-redeploy on code changes:**
1. **Containers** ? **flaccapture** ? **Duplicate/Edit**
2. Scroll to **Webhook**
3. Enable webhook
4. Copy webhook URL
5. Configure in your CI/CD or Git repository

### Templates

**Save as template for reuse:**
1. **Settings** ? **App Templates**
2. **Add template**
3. Define template from your working stack
4. Use for future deployments

### Backup & Restore

**Export stack configuration:**
1. **Stacks** ? **flaccapture**
2. Click **Editor**
3. Copy YAML content
4. Save externally

**Import on new NAS:**
1. Create new stack
2. Paste YAML content
3. Deploy

## Security Best Practices

### Access Control

**Portainer authentication:**
- Use strong admin password
- Consider enabling 2FA (Portainer Business Edition)
- Create separate users for different access levels

**Network isolation:**
- Use Synology firewall rules
- Restrict Portainer access to local network
- Consider VPN for remote access

### Container Security

**Read-only volumes:**
```yaml
volumes:
  - /volume1/audio/output:/app/output:ro  # Read-only
```

**Drop capabilities:**
```yaml
cap_drop:
  - ALL
cap_add:
  - NET_BIND_SERVICE
```

## Performance Optimization

### Storage Performance

**Use SSD for output:**
- Move `/volume1/audio/output` to SSD volume if available
- Keep input on HDD (lower I/O)

**Enable caching:**
```yaml
volumes:
  - /volume1/audio/output:/app/output:cached
```

### Network Optimization

**Quality of Service (QoS):**
1. **Network** ? **QoS**
2. Add rule for Docker traffic
3. Set appropriate priority

## Comparison: Portainer vs Command Line

| Feature | Portainer | Command Line |
|---------|-----------|--------------|
| **Ease of Use** | ????? Visual | ?? Requires SSH |
| **Monitoring** | ????? Built-in graphs | ?? Manual commands |
| **Log Viewing** | ???? Web interface | ??? Terminal output |
| **Updates** | ???? Click to rebuild | ??? Run commands |
| **Automation** | ???? Webhooks | ????? Scripts |
| **Speed** | ??? Web overhead | ????? Direct |

**Recommendation:** Portainer for most users, command line for advanced automation.

## Next Steps

After successful deployment:

1. ? **Test with sample M3U** - Verify full workflow
2. ? **Set up monitoring** - Check logs regularly
3. ? **Configure backup** - Export stack configuration
4. ? **Schedule maintenance** - Clean up old files
5. ? **Document customizations** - Note any changes made

## Support Resources

- **Portainer Docs**: https://docs.portainer.io/
- **FlacCapture Docs**: See `docs/` directory
- **Synology Forums**: https://community.synology.com/
- **Docker Docs**: https://docs.docker.com/

## Frequently Asked Questions

**Q: Can I use Portainer CE (free) or do I need Business?**
A: Portainer CE is sufficient. Business adds advanced features but isn't required.

**Q: Can I manage multiple NAS devices from one Portainer?**
A: Yes! Add multiple "environments" in Portainer to manage different devices.

**Q: How do I access Portainer remotely?**
A: Use Synology QuickConnect, VPN, or expose via reverse proxy (advanced).

**Q: Can I use Portainer on DSM 6.x?**
A: Yes, but upgrade to DSM 7+ recommended for best compatibility.

**Q: Does this work with other NAS brands?**
A: Yes! Portainer works on any system running Docker (QNAP, Unraid, etc.)

## Conclusion

Portainer provides an excellent web-based interface for managing your FlacCapture container on Synology NAS. It's easier than command-line Docker for most users, while still providing full control and monitoring capabilities.

**Happy capturing!** ??

---

For more information:
- [Docker Deployment (Command Line)](DOCKER_DEPLOYMENT.md)
- [Container Quick Start](CONTAINER_QUICKSTART.md)
- [Main Documentation](INDEX.md)
