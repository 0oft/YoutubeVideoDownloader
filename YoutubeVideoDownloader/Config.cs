using System.Diagnostics;

namespace YoutubeVideoDownloader
{
    public static class Config
    {
        public static async Task CreateFiles()
        {
            if (!Directory.Exists(AppDataDirPath))
                Directory.CreateDirectory(AppDataDirPath);
            if (!Directory.Exists(AppDataLogDirPath))
                Directory.CreateDirectory(AppDataLogDirPath);
            if (!Directory.Exists(AppDataFfmpegDirPath))
                Directory.CreateDirectory(AppDataFfmpegDirPath);
            if (!Directory.Exists(AppDataCompressedDirPath))
                Directory.CreateDirectory(AppDataCompressedDirPath);
            if (!File.Exists(FfmpegCompressedFilePath))
                await File.WriteAllBytesAsync(FfmpegCompressedFilePath, Properties.Resources.ffmpeg);
            if (!File.Exists(SevenZipFilePath))
                await File.WriteAllBytesAsync(SevenZipFilePath, Properties.Resources._7z);
            if (!File.Exists(FfmpegFilePath))
                await UnzipFfmpeg();
        }
        public static List<(int width, int height)> Resolutions = new List<(int width, int height)>() { (640, 480), (1280, 720), (1920, 1080), (2160, 1440), (3840, 2160), (7680, 4320) };
        public static readonly string AppDataDirPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YTVideoDownloader"));
        public static readonly string DownloadsFolderPath = System.Environment.ExpandEnvironmentVariables("%userprofile%/downloads/");
        public static readonly string AppDataLogDirPath = Path.Combine(AppDataDirPath, "Logs");
        public static readonly string AppDataFfmpegDirPath = Path.Combine(AppDataDirPath, "ffmpeg");
        public static readonly string AppDataCompressedDirPath = Path.Combine(AppDataDirPath, "Compressed");
        public static readonly string FfmpegFilePath = Path.Combine(AppDataFfmpegDirPath, "ffmpeg.exe");
        public static readonly string FfmpegCompressedFilePath = Path.Combine(AppDataCompressedDirPath, "ffmpeg.7z");
        public static readonly string SevenZipFilePath = Path.Combine(AppDataCompressedDirPath, "7z.exe");
        public static (int width, int height) SelectedResolution;
        public static MediaType SelectedMediaType;

        private static async Task UnzipFfmpeg()
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = SevenZipFilePath;
            cmd.StartInfo.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", FfmpegCompressedFilePath, AppDataFfmpegDirPath);
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.Start();
            await cmd.WaitForExitAsync();
        }
    }
}
