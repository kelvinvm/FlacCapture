# M3U File Processing Lifecycle

## Overview

FlacCapture automatically moves M3U files after processing to prevent reprocessing, regardless of whether the processing succeeded or failed.

## Directory Structure

```
/app/input/      (or /volume1/audio/input on Synology)
??? *.m3u     ? Drop new M3U files here
??? processed/          ? Successfully processed M3U files moved here
?   ??? show_20251101_143022.m3u
??? failed/      ? Failed M3U files moved here
    ??? broken_20251101_144530.m3u
```

## Processing Flow

```
???????????????????????????????????????????????????????
? 1. User drops M3U file in input directory   ?
???????????????????????????????????????????????????????
?
???????????????????????????????????????????????????????
? 2. FileWatcher detects new file            ?
?    [INFO] New file detected: show.m3u   ?
???????????????????????????????????????????????????????
         ?
???????????????????????????????????????????????????????
? 3. Processing starts        ?
?    - Downloads streams       ?
?    - Captures audio    ?
?    - Converts to FLAC        ?
???????????????????????????????????????????????????????
   ?
      ???????????????
      ?          ?
????????????? ?????????????
?  SUCCESS  ? ?  FAILURE  ?
????????????? ?????????????
      ?         ?
      ?             ?
   processed/  failed/
```

## Success Path

### When Processing Succeeds

**Conditions:**
- All streams downloaded successfully
- Audio capture completed
- FLAC conversion succeeded (if enabled)

**Actions:**
1. Output FLAC file saved to `/app/output/`
2. M3U file moved to `/app/input/processed/`
3. Log message: `M3U file moved to: processed/show.m3u`

**Example Log:**
```
[INFO] Processing: show.m3u
[INFO] Found 3 stream URL(s)
[INFO] Starting capture for: show
[INFO] [1/3] Downloading: http://...
[INFO] Downloaded successfully
[INFO] [2/3] Downloading: http://...
[INFO] Downloaded successfully
[INFO] [3/3] Downloading: http://...
[INFO] Downloaded successfully
[INFO] Combining 3 stream(s) into WAV file...
[INFO] Capture completed: show_20251101_143022.wav
[INFO] FLAC conversion successful: show_20251101_143022.flac
[INFO] M3U file moved to: processed/show.m3u
```

## Failure Path

### When Processing Fails

**Possible Failure Reasons:**
- Stream URL is invalid or unreachable
- Network connection lost during download
- Audio file format not supported
- Disk space insufficient
- File read/write permissions issue

**Actions:**
1. Error logged with details
2. M3U file moved to `/app/input/failed/`
3. Log message: `M3U file moved to: failed/show.m3u (processing failed)`
4. **File will NOT be retried automatically**

**Example Log:**
```
[INFO] Processing: broken.m3u
[INFO] Found 1 stream URL(s)
[INFO] Starting capture for: broken
[INFO] [1/1] Downloading: http://invalid-url.com/stream.mp3
[ERROR] Failed to capture streams from: /app/input/broken.m3u
[ERROR] Exception: HttpRequestException: The remote server returned an error: (404) Not Found
[WARN] M3U file moved to: failed/broken.m3u (processing failed)
```

## File Naming Conflicts

### Duplicate Filenames

If a file with the same name already exists in `processed/` or `failed/`, a timestamp is added:

**Original:**
```
input/show.m3u
```

**After first processing:**
```
processed/show.m3u
```

**After second file with same name:**
```
processed/show_20251101_143530_456.m3u
```

**Timestamp format:** `yyyyMMdd_HHmmss_fff` (includes milliseconds)

## Preventing Reprocessing

### Why Files Are Moved

1. **Avoid infinite loops** - Failed files won't be retried endlessly
2. **Clear history** - Easy to see what was processed vs what failed
3. **Manual retry** - You can move files back to retry if desired
4. **Performance** - Scanner doesn't waste time checking already-processed files

### Internal Tracking

The service maintains an in-memory set of processed files:

```csharp
private readonly HashSet<string> _processedFiles;
```

**Tracked states:**
- Files currently being processed (prevents duplicates)
- Files successfully processed
- Files that failed processing

**On restart:** Container forgets the in-memory set, but files have been moved to subdirectories, so they won't be reprocessed.

## Manual Retry

### Retrying Failed Files

If you want to retry a failed M3U file:

**Option 1: Via File Station**
1. Navigate to `/volume1/audio/input/failed/`
2. Move the M3U file back to `/volume1/audio/input/`
3. Container will detect and process it again

**Option 2: Via SSH**
```bash
# Move specific file back to input
mv /volume1/audio/input/failed/show.m3u /volume1/audio/input/

# Or move all failed files back
mv /volume1/audio/input/failed/*.m3u /volume1/audio/input/
```

**Option 3: Via Docker Console (Portainer)**
```bash
# Open console in Portainer
# Containers ? flaccapture ? Console

mv /app/input/failed/show.m3u /app/input/
```

### Fixing Issues Before Retry

**Common fixes:**

1. **Bad URL** - Edit the M3U file to fix the URL
2. **Network issue** - Wait for connection to stabilize
3. **Disk space** - Free up space in output directory
4. **Permissions** - Fix folder permissions

**Example edit:**
```bash
# Edit the M3U file
nano /volume1/audio/input/failed/show.m3u

# Fix the URL
# Before: http://old-server.com/stream.mp3
# After:  http://new-server.com/stream.mp3

# Move back to input
mv /volume1/audio/input/failed/show.m3u /volume1/audio/input/
```

## Maintenance

### Cleaning Up Old Files

**Processed files (success):**
```bash
# Delete successfully processed M3U files older than 30 days
find /volume1/audio/input/processed -name "*.m3u" -mtime +30 -delete
```

**Failed files (for review):**
```bash
# List failed files to review
ls -lh /volume1/audio/input/failed/

# Delete after reviewing (be careful!)
# rm /volume1/audio/input/failed/old_broken.m3u
```

### Automated Cleanup (Synology Task Scheduler)

**Create scheduled task:**
1. **Control Panel** ? **Task Scheduler**
2. **Create** ? **Scheduled Task** ? **User-defined script**
3. **Schedule:** Monthly
4. **Script:**

```bash
#!/bin/bash
# Clean up old processed M3U files (keep last 90 days)
find /volume1/audio/input/processed -name "*.m3u" -mtime +90 -delete

# Archive failed files older than 30 days
mkdir -p /volume1/audio/input/archive
find /volume1/audio/input/failed -name "*.m3u" -mtime +30 -exec mv {} /volume1/audio/input/archive/ \;

# Log cleanup
echo "$(date): Cleaned up old M3U files" >> /volume1/audio/logs/cleanup.log
```

## Monitoring

### Check Processing Status

**View processed files:**
```bash
# Via SSH or Portainer console
ls -lh /app/input/processed/
```

**View failed files:**
```bash
ls -lh /app/input/failed/
```

**Count files by status:**
```bash
echo "Pending: $(ls /app/input/*.m3u 2>/dev/null | wc -l)"
echo "Processed: $(ls /app/input/processed/*.m3u 2>/dev/null | wc -l)"
echo "Failed: $(ls /app/input/failed/*.m3u 2>/dev/null | wc -l)"
```

### Via Portainer

**Check directories:**
1. **Containers** ? **flaccapture** ? **Console**
2. Run:
   ```bash
   ls -la /app/input/
   ls -la /app/input/processed/
   ls -la /app/input/failed/
   ```

## Configuration

### No Environment Variables Needed

The processed/failed subdirectories are created automatically. No configuration required.

**Automatic behavior:**
- ? Creates `processed/` on first successful processing
- ? Creates `failed/` on first failure
- ? Handles filename conflicts automatically
- ? Works with any input directory structure

## Edge Cases

### What If Move Fails?

If the M3U file cannot be moved (permissions, disk full, etc.):

```
[WARN] Could not move M3U file to processed folder
```

**Fallback:** File is marked as processed in-memory to prevent immediate retry

**On container restart:** File will be reprocessed (since in-memory state is lost)

**Solution:** Fix permissions/disk space issues

### Empty M3U Files

Files with no valid URLs are marked as failed:

```
[WARN] No valid URLs found in: empty.m3u
[WARN] M3U file moved to: failed/empty.m3u (processing failed)
```

### Partial Success

If some streams succeed but others fail:

**Behavior:** Entire job marked as FAILED
**Reason:** Incomplete capture, missing segments
**M3U moved to:** `failed/` subdirectory

**Future enhancement:** Could be changed to allow partial success

## Best Practices

1. **Review failed directory regularly** - Identify recurring issues
2. **Keep failed files for 30 days** - In case you need to debug
3. **Clean processed files monthly** - Prevent directory bloat
4. **Fix M3U files before retry** - Don't retry without fixing the issue
5. **Monitor disk space** - Both input and output directories

## Troubleshooting

### Files Not Moving

**Check permissions:**
```bash
ls -ld /app/input
ls -ld /app/input/processed
ls -ld /app/input/failed
```

**Fix permissions:**
```bash
chmod 755 /app/input
chmod 755 /app/input/processed
chmod 755 /app/input/failed
```

### Duplicate Processing

**Symptom:** Same file processed multiple times

**Causes:**
- Container restarted during processing
- File copied back to input while being processed

**Solution:** Wait for processing to complete before moving files

### Files Stuck in Input

**Check logs for errors:**
```bash
docker logs flaccapture | grep ERROR
```

**Common reasons:**
- Invalid M3U format (not recognized as *.m3u)
- File permissions prevent reading
- Service not running

## Summary

? **Success** ? M3U moved to `input/processed/`
? **Failure** ? M3U moved to `input/failed/`
?? **Retry** ? Move file back to `input/`
?? **Cleanup** ? Periodically delete old files

**No file is ever processed twice** unless you explicitly move it back to the input directory!

---

**See also:**
- [Container Quick Start](CONTAINER_QUICKSTART.md) - Usage guide
- [Portainer Deployment](PORTAINER_DEPLOYMENT.md) - Management UI
- [Linux Mode Explained](LINUX_MODE_EXPLAINED.md) - How capture works
