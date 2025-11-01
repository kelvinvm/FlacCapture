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

### Overview of Update Process

FlacCapture updates fall into two categories:
1. **Configuration changes** - Environment variables, volume paths (no rebuild needed)
2. **Code changes** - Application updates, bug fixes (rebuild required)

### Method 1: Rebuild via Portainer (Stack-based Deployment)

**Best for:** Updates when using Git repository or uploaded compose file with build context

1. **Go to Stacks** ? Select **flaccapture**
2. Click **Editor** to modify if needed
3. Click **Update the stack**
4. Enable **Re-pull image and redeploy**
5. Click **Update**

**Note:** This only works if you deployed using Git repository method. For uploaded stacks, see Method 3.

### Method 2: Update Configuration Only (No Rebuild)

**Best for:** Changing environment variables, adjusting settings

**Via Portainer UI:**
1. **Containers** ? **flaccapture** ? **Duplicate/Edit**
2. Scroll to **Env** section
3. Modify environment variables
4. Click **Deploy the container**

**Via Stack Editor:**
1. **Stacks** ? **flaccapture** ? **Editor**
2. Modify environment variables in YAML
3. Click **Update the stack**
4. Container will restart with new settings

### Method 3: Update After Code Changes (Recommended)

**When you need to rebuild after source code changes:**

#### Step-by-Step Process:

**1. Update source code on Synology:**

```bash
# SSH into your Synology
ssh admin@your-nas-ip
cd /volume1/docker/FlacCapture

# Option A: Pull from Git (if using version control)
git pull origin main

# Option B: Upload new files via SCP
# From your local machine:
scp -r FlacCapture/ admin@your-nas-ip:/volume1/docker/
```

**2. Rebuild the Docker image:**

```bash
# Still in SSH session
cd /volume1/docker/FlacCapture

# Rebuild with no cache to ensure fresh build
sudo docker build -t flaccapture:latest --no-cache .

# Verify new image was created
sudo docker images flaccapture
```

**3. Restart container via Portainer:**

**Option A: Quick Restart (if using pre-built image)**
1. **Containers** ? **flaccapture**
2. Click **Recreate**
3. Enable **Pull latest image**
4. Click **Recreate**

**Option B: Via Stack Update**
1. **Stacks** ? **flaccapture**
2. Click **Stop stack**
3. Wait for container to stop
4. Click **Start stack**

**Option C: Manual Stop/Start**
1. **Containers** ? **flaccapture** ? **Stop**
2. Wait for container to stop completely
3. Click **Start**

### Method 4: Complete Rebuild (Nuclear Option)

**When something is broken and you need a fresh start:**

```bash
# SSH into Synology
ssh admin@your-nas-ip
cd /volume1/docker/FlacCapture

# Stop and remove everything
sudo docker-compose down -v  # WARNING: -v removes volumes!

# Or to keep data:
sudo docker-compose down

# Rebuild from scratch
sudo docker-compose build --no-cache

# Start services
sudo docker-compose up -d
```

**In Portainer:**
1. **Stacks** ? **flaccapture** ? **Stop stack**
2. **Remove stack** (keeps volumes by default)
3. Create new stack with same configuration
4. Deploy

### Updating Workflow Examples

#### Example 1: Bug Fix Release

**Scenario:** New version fixes a FLAC encoding issue

```bash
# 1. SSH into NAS
ssh admin@your-nas-ip
cd /volume1/docker/FlacCapture

# 2. Get latest code
git pull origin main
# Or: git fetch && git checkout v1.1.0

# 3. Rebuild image
sudo docker build -t flaccapture:latest --no-cache .

# 4. Exit SSH, go to Portainer
# Containers ? flaccapture ? Recreate
```

**Expected downtime:** 2-3 minutes

#### Example 2: Adding New Feature

**Scenario:** New feature requires environment variable

```bash
# 1. Update code (same as Example 1)
git pull origin main

# 2. Rebuild image
sudo docker build -t flaccapture:latest --no-cache .
```

**In Portainer:**
1. **Stacks** ? **flaccapture** ? **Editor**
2. Add new environment variable:
   ```yaml
   environment:
     - NEW_FEATURE_ENABLED=true
   ```
3. Click **Update the stack**

#### Example 3: Hotfix Deployment

**Scenario:** Critical bug needs immediate fix

```bash
# 1. Quick code update
ssh admin@your-nas-ip
cd /volume1/docker/FlacCapture
git pull origin hotfix/critical-bug

# 2. Fast rebuild
sudo docker build -t flaccapture:latest .

# 3. Force restart (fast path)
sudo docker restart flaccapture
```

**Expected downtime:** 10-15 seconds

#### Example 4: Scheduled Maintenance Update

**Scenario:** Monthly update during off-hours

```bash
# 1. SSH and prepare
ssh admin@your-nas-ip
cd /volume1/docker/FlacCapture

# 2. Backup current state
sudo docker commit flaccapture flaccapture:backup-$(date +%Y%m%d)

# 3. Update code
git pull origin main

# 4. Rebuild
sudo docker build -t flaccapture:latest --no-cache .

# 5. Stop old, start new
sudo docker stop flaccapture
sudo docker rm flaccapture
sudo docker-compose up -d

# 6. Verify in Portainer
# Check logs for successful startup
```

### Version Control Integration

**Tagging releases for easy rollback:**

```bash
# After successful deployment
cd /volume1/docker/FlacCapture
git tag -a v1.0.0 -m "Stable release 1.0.0"
sudo docker tag flaccapture:latest flaccapture:v1.0.0

# Later, rollback if needed:
git checkout v1.0.0
sudo docker build -t flaccapture:latest .
sudo docker restart flaccapture
```

### Automatic Updates via Webhooks

**Set up auto-rebuild on Git push:**

**1. In Portainer:**
1. **Containers** ? **flaccapture** ? **Duplicate/Edit**
2. Scroll to **Webhook**
3. Enable webhook
4. Copy webhook URL

**2. In GitHub:**
1. Repository **Settings** ? **Webhooks**
2. Add webhook
3. Paste Portainer webhook URL
4. Select trigger: **Just the push event**
5. Save

**3. Test:**
```bash
# Push code change
git commit -m "Test auto-deploy"
git push

# Portainer automatically rebuilds and redeploys!
```

### Monitoring Updates

**Check update status:**

**In Portainer:**
1. **Containers** ? **flaccapture** ? **Logs**
2. Look for startup messages:
   ```
   [INFO] FLAC Capture Service - Container Mode
   [INFO] Version: 1.1.0
   ```

**Via SSH:**
```bash
# Check image creation date
sudo docker images flaccapture

# Check container uptime
sudo docker ps | grep flaccapture

# View recent logs
sudo docker logs --tail 50 flaccapture
```

### Rollback Procedure

**If update causes problems:**

**Method 1: Revert to previous image**
```bash
ssh admin@your-nas-ip

# List available images
sudo docker images flaccapture

# Use older image
sudo docker stop flaccapture
sudo docker run -d \
  --name flaccapture \
  --volumes-from flaccapture \
  flaccapture:backup-20251101

# Or restore from tag
sudo docker tag flaccapture:v1.0.0 flaccapture:latest
sudo docker restart flaccapture
```

**Method 2: Revert code via Git**
```bash
cd /volume1/docker/FlacCapture
git log --oneline  # Find previous commit
git checkout <commit-hash>
sudo docker build -t flaccapture:latest .
sudo docker restart flaccapture
```

### Update Checklist

**Before updating:**
- [ ] Read release notes
- [ ] Backup current configuration (export stack from Portainer)
- [ ] Note current version number
- [ ] Check disk space (need ~500MB for new image)
- [ ] Schedule during low-usage time
- [ ] Notify users if applicable

**During update:**
- [ ] Stop container gracefully
- [ ] Verify source code is updated
- [ ] Rebuild Docker image
- [ ] Check build for errors
- [ ] Start container
- [ ] Wait for full startup (30 seconds)

**After update:**
- [ ] Check logs for errors
- [ ] Verify container is running
- [ ] Test with sample M3U file
- [ ] Monitor for 10-15 minutes
- [ ] Document any issues
- [ ] Update documentation if needed

### Troubleshooting Updates

**Build fails:**
```bash
# Clean Docker build cache
sudo docker builder prune -a

# Retry build
sudo docker build -t flaccapture:latest --no-cache .
```

**Container won't start after update:**
```bash
# Check logs
sudo docker logs flaccapture

# Verify volumes still exist
ls -la /volume1/audio/input
ls -la /volume1/audio/output

# Reset to known-good state
sudo docker stop flaccapture
sudo docker rm flaccapture
sudo docker run -d \
  --name flaccapture \
  -v /volume1/audio/input:/app/input \
  -v /volume1/audio/output:/app/output \
  flaccapture:v1.0.0
```

**Configuration changes don't apply:**
```bash
# Force recreate container
sudo docker-compose down
sudo docker-compose up -d --force-recreate
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

### WASAPI Error on Linux (FIXED)

**Error message:**
```
[ERROR] NotSupportedException: This functionality is only supported on Windows Vista or newer
at NAudio.CoreAudioApi.MMDeviceEnumerator..ctor()
```

**Cause:** WASAPI is Windows-only, doesn't work on Synology/Linux

**Solution:** The application now automatically uses **DirectStreamCapture** (Linux mode) instead:

**1. Update to latest code:**
```bash
ssh admin@your-nas-ip
cd /volume1/docker/FlacCapture
git pull origin main
```

**2. Rebuild container:**
```bash
sudo docker build -t flaccapture:latest --no-cache .
```

**3. Restart in Portainer:**
- Containers ? flaccapture ? Recreate

**Verify fix in logs:**
```
Initializing direct stream capture (Linux mode)...
[INFO] Downloading stream...
[INFO] Downloaded successfully
```

**See also:** [Linux Mode Explained](LINUX_MODE_EXPLAINED.md) for full details

**Benefits of Linux mode:**
- ? 10-30x faster (downloads at full speed, not real-time)
- ? Lower CPU/memory usage
- ? Same audio quality (bit-perfect)
- ? More reliable

### Can't Access Logs
