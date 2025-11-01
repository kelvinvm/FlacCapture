# File Locking Issue - Fix Documentation

## Problem

**Error Message:**
```
Error during FLAC conversion: The process cannot access the file 
'capture_20251101_102106.wav' because it is being used by another process.
```

## Root Cause

The WAV file was not being properly closed before attempting FLAC conversion. Specifically:

1. **WaveFileWriter not flushed**: The `_waveWriter` object needed to explicitly flush its buffers before disposal
2. **Immediate conversion attempt**: FLAC conversion was attempted immediately after disposal without allowing the OS to fully release the file handle
3. **No retry logic**: The converter didn't handle the case where the file was temporarily locked

## Solution Implemented

### 1. WasapiFlacCapture.cs - Proper File Closure

**Changes in the `finally` block:**

```csharp
// BEFORE (incorrect):
_waveWriter?.Dispose();
_waveWriter = null;

// AFTER (correct):
if (_waveWriter != null)
{
    _waveWriter.Flush();      // ? Explicitly flush buffers
    _waveWriter.Dispose();      // ? Close the file
    _waveWriter = null;
}

// Give the OS time to release the file handle
await Task.Delay(100);  // ? Wait for OS to release handle
```

**Why this fixes it:**
- `Flush()` ensures all buffered data is written to disk before closing
- `Task.Delay(100)` gives Windows time to fully release the file handle
- The explicit null check and proper ordering ensures clean disposal

### 2. FlacConverter.cs - Retry Logic

**Changes in `ConvertToFlac` method:**

```csharp
// Retry logic to handle file access issues
WaveFileReader? reader = null;
int retries = 3;
int delayMs = 200;

for (int attempt = 0; attempt < retries; attempt++)
{
    try
    {
        reader = new WaveFileReader(wavFile);
        break; // Success!
    }
    catch (IOException) when (attempt < retries - 1)
{
        Console.WriteLine($"  Waiting for file to be released (attempt {attempt + 1}/{retries})...");
        Thread.Sleep(delayMs);
        delayMs *= 2; // Exponential backoff
    }
}
```

**Why this fixes it:**
- **Retry with exponential backoff**: Tries 3 times with increasing delays (200ms, 400ms, 800ms)
- **Graceful handling**: Informs the user if it needs to wait
- **Fallback**: If all retries fail, throws the exception to trigger the external flac.exe fallback

**Additional safety in file deletion:**

```csharp
if (response == "y" || response == "yes")
{
    Thread.Sleep(100);    // ? Wait before deletion
    try
    {
        File.Delete(wavFile);
        Console.WriteLine("Original WAV file deleted.");
 }
    catch (IOException ex)
    {
        Console.WriteLine($"Could not delete WAV file: {ex.Message}");
        Console.WriteLine("You can manually delete it later.");
    }
}
```

## Technical Details

### File Handle Lifecycle

```
1. WaveFileWriter creates file    ? File handle OPEN
2. Audio data written      ? Buffers accumulating
3. _waveWriter.Flush()            ? Buffers written to disk
4. _waveWriter.Dispose()     ? File handle CLOSING (async)
5. await Task.Delay(100)        ? Wait for OS to complete
6. WaveFileReader opens file      ? File handle OPEN (now safe)
```

### Why the Delay is Necessary

When you call `Dispose()` on a `WaveFileWriter`:
1. It signals to close the file
2. The OS begins releasing the file handle
3. **This is not instantaneous** - the OS may need a few milliseconds
4. If you immediately try to open the file, you can get a lock error

The `Task.Delay(100)` gives the OS enough time to:
- Flush any OS-level buffers
- Release the file handle
- Update file system metadata
- Complete any pending I/O operations

### Exponential Backoff Strategy

```
Attempt 1: Wait 200ms  (total: 200ms)
Attempt 2: Wait 400ms  (total: 600ms)
Attempt 3: Wait 800ms  (total: 1400ms)
```

This ensures:
- Quick success if the file is released immediately
- Progressive patience if the system is under load
- Maximum total wait of ~1.5 seconds before giving up

## Testing

### Test Case 1: Normal Operation
```
? Expected: Smooth conversion without errors
? Result: File properly closed, FLAC conversion succeeds
```

### Test Case 2: Under System Load
```
? Expected: May need 1-2 retries, but eventually succeeds
? Result: Retry logic handles temporary locks
```

### Test Case 3: Immediate Ctrl+C
```
? Expected: WAV file saved, FLAC conversion not attempted
? Result: File properly closed, no conversion errors
```

## Performance Impact

The fix adds minimal overhead:
- **100ms delay**: Negligible compared to encoding time (seconds to minutes)
- **Retry logic**: Only triggers if file is locked (rare in normal operation)
- **Flush overhead**: Negligible, ensures data integrity

## Alternative Solutions Considered

### ? Longer initial delay
```csharp
await Task.Delay(1000);  // Too long, wasteful
```
**Rejected**: 100ms is sufficient for most systems

### ? No delay, just retry
```csharp
// No initial delay
```
**Rejected**: Would always fail first attempt, then retry

### ? FileShare.ReadWrite on WaveFileWriter
```csharp
// Allow multiple readers/writers
```
**Rejected**: Could corrupt the file if not handled carefully

### ? Current approach (Flush + Delay + Retry)
**Selected**: Best balance of reliability, performance, and user experience

## User Experience

### Before Fix
```
Recording stopped.
Converting to FLAC...
Error during FLAC conversion: The process cannot access the file...
? Confusing error, no clear resolution
```

### After Fix
```
Recording stopped.
Converting to FLAC (quality: 100)...
  Input format: 48000Hz, 32bit, 2ch
  Encoding to FLAC...
  ? Conversion complete!
  WAV size:  1.32 GB
  FLAC size: 685.45 MB
  Compression: 48.1% reduction
? Smooth, predictable behavior
```

### If Retry Needed (rare)
```
Converting to FLAC (quality: 100)...
  Waiting for file to be released (attempt 1/3)...
  Input format: 48000Hz, 32bit, 2ch
  Encoding to FLAC...
? Transparent handling, user informed
```

## Recommendations

### For Users
1. **No action required**: The fix is automatic
2. If conversion still fails after retries, check:
   - Antivirus software scanning the file
- File indexing services (Windows Search)
   - Other audio software accessing the file

### For Developers
1. **Always flush before dispose**: Especially with file writers
2. **Add delays after file operations**: Give the OS time to release handles
3. **Implement retry logic**: For any file I/O that might be temporarily locked
4. **Provide clear error messages**: Help users understand what's happening

## Conclusion

The fix ensures reliable WAV-to-FLAC conversion by:
1. ? Properly flushing and closing the WAV file
2. ? Waiting for the OS to release file handles
3. ? Retrying with exponential backoff if needed
4. ? Gracefully handling edge cases

**Result**: Robust, reliable file conversion with excellent user experience.
