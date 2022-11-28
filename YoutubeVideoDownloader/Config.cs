using System.Diagnostics;

namespace YoutubeVideoDownloader
{
    public static class Config
    {
        static Config()
        {
            if (!Directory.Exists(AppDataDir))
                Directory.CreateDirectory(AppDataDir);
            if (!Directory.Exists(AppDataFiles))
                Directory.CreateDirectory(AppDataFiles);
            if (!Directory.Exists(AppDataTempDir))
                Directory.CreateDirectory(AppDataTempDir);
            if (!Directory.Exists(AppDataLogDir))
                Directory.CreateDirectory(AppDataLogDir);
            if (!Directory.Exists(AppDataFfmpegDir))
                Directory.CreateDirectory(AppDataFfmpegDir);
            if (!Directory.Exists(AppDataCompressedDir))
                Directory.CreateDirectory(AppDataCompressedDir);
            if (!File.Exists(FfmpegCompressedDir))
                File.WriteAllBytes(FfmpegCompressedDir, Properties.Resources.ffmpeg);
            if (!File.Exists(SevenZipDir))
                File.WriteAllBytes(SevenZipDir, Properties.Resources._7z);
            if (!File.Exists(FfmpegDir))
                UnzipFfmpeg();
        }
        public static List<int> Resolutions = new List<int>()
        {
        144,
        240,
        360,
        480,
        720,
        1080,
        1440,
        2160,
        4320
        };
        public static readonly string WatchLink = "https://www.youtube.com/watch?v=";
        public static readonly string AppDataDir = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YTVideoDownloader"));
        public static readonly string AppDataFiles = Path.Combine(AppDataDir, "Files");
        public static readonly string AppDataTempDir = Path.Combine(AppDataDir, "Temp");
        public static readonly string AppDataLogDir = Path.Combine(AppDataDir, "Logs");
        public static readonly string AppDataFfmpegDir = Path.Combine(AppDataDir, "ffmpeg");
        public static readonly string AppDataCompressedDir = Path.Combine(AppDataDir, "Compressed");
        public static readonly string FfmpegDir = Path.Combine(AppDataFfmpegDir, "ffmpeg.exe");
        public static readonly string FfmpegCompressedDir = Path.Combine(AppDataCompressedDir, "ffmpeg.7z");
        public static readonly string SevenZipDir = Path.Combine(AppDataCompressedDir, "7z.exe");


        private static void UnzipFfmpeg()
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = SevenZipDir;
            cmd.StartInfo.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", FfmpegCompressedDir, AppDataFfmpegDir);
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.Start();
            cmd.WaitForExit();
        }
        public static int SelectedResolution;
        public static MediaType SelectedMediaType;
    }
}
