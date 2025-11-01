using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Lame;

namespace FlacCapture;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("FLAC Capture Utility - Bit-Perfect WASAPI Recording");
        Console.WriteLine("====================================================\n");

        string m3uPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assests", "show.m3u");
        string outputWav = $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
        string outputFlac = Path.ChangeExtension(outputWav, ".flac");

        if (!File.Exists(m3uPath))
        {
            Console.WriteLine($"Error: M3U file not found at {m3uPath}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        var captureService = new WasapiFlacCapture(playbackVolume: 0.7f); // 70% monitoring volume
        
        try
        {
            Console.WriteLine($"Reading playlist: {m3uPath}");
            var urls = await File.ReadAllLinesAsync(m3uPath);
            
            // Filter out empty lines and comments
            var validUrls = urls.Where(url => !string.IsNullOrWhiteSpace(url) && !url.StartsWith("#")).ToArray();

            Console.WriteLine($"Found {validUrls.Length} stream(s)");
            Console.WriteLine($"Output file: {outputWav}\n");

            await captureService.CaptureStreamToFile(validUrls, outputWav);
     
            Console.WriteLine($"\nCapture completed successfully!");
            Console.WriteLine($"WAV output saved to: {Path.GetFullPath(outputWav)}");
   
            // Offer FLAC conversion
            Console.Write("\nConvert to FLAC format? (y/n): ");
            var response = Console.ReadLine()?.Trim().ToLower();
            
            if (response == "y" || response == "yes")
            {
                if (FlacConverter.ConvertToFlac(outputWav, outputFlac))
                {
                    Console.WriteLine($"\nFinal output: {Path.GetFullPath(outputFlac)}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            captureService.Dispose();
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
