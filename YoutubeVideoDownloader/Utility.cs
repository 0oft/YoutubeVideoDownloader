using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace YoutubeVideoDownloader
{
    public class Utility
    {
        private YoutubeClient youtube = new YoutubeClient();
        public List<VideoInfo> Videos = new List<VideoInfo>();
        public async Task<bool> Start(string uri)
        {
            try
            {
                if (uri.Contains("playlist"))
                {
                    var videos = await youtube.Playlists.GetVideosAsync(uri);
                    foreach (var video in videos)
                    {
                        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Url);
                        Videos.Add(new VideoInfo(video.Title, SelectVideoStream(streamManifest), streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate()));
                    }
                    return true;
                }
                else
                {
                    var video = await youtube.Videos.GetAsync(uri);
                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(uri);
                    var videoStream = SelectVideoStream(streamManifest);
                    Videos.Add(new VideoInfo(video.Title, videoStream, streamManifest.GetAudioOnlyStreams().Where(x => x.Container == videoStream.Container).GetWithHighestBitrate()));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        private IVideoStreamInfo SelectVideoStream(StreamManifest streamManifest)
        {
            IVideoStreamInfo stream = null;
            var index = Config.Resolutions.IndexOf(Config.SelectedResolution);
            while (stream == null)
            {
                stream = streamManifest.GetVideoStreams().FirstOrDefault(x => x.VideoResolution.Height == Config.Resolutions[index]);
                index = index - 1;
            }
            return stream;
        }
    }

    public class VideoInfo
    {
        public VideoInfo(string _videoName, IVideoStreamInfo _videoStreamInfo, IStreamInfo _audioStreamInfo)
        {
            VideoName = _videoName;
            VideoStreamInfo = _videoStreamInfo;
            AudioStreamInfo = _audioStreamInfo;
            VideoPath = Path.Combine(Config.AppDataTempDir, System.Guid.NewGuid() + "." + VideoStreamInfo.Container.Name);
            AudioPath = Path.Combine(Config.AppDataTempDir, System.Guid.NewGuid() + "." + AudioStreamInfo.Container.Name);
            MuxedPath = Config.SelectedMediaType == MediaType.Music ? MuxedPath = Path.Combine(Config.AppDataFiles, VideoName + "." + AudioStreamInfo.Container.Name) : MuxedPath = Path.Combine(Config.AppDataFiles, VideoName + "." + VideoStreamInfo.Container.Name);
            ID = System.Guid.NewGuid().ToString();
        }

        private string videoName { get; set; }
        public string ID { get; }
        public IVideoStreamInfo VideoStreamInfo { get; }
        public IStreamInfo AudioStreamInfo { get; }
        public string AudioPath { get; }
        public string VideoPath { get; }
        public string MuxedPath { get; }
        public string VideoName
        {
            get
            {
                return videoName;
            }
            set
            {
                videoName = value.Replace("<", String.Empty)
                   .Replace(">", String.Empty)
                   .Replace(":", String.Empty)
                   .Replace("\"", String.Empty)
                   .Replace("/", String.Empty)
                   .Replace("\\", String.Empty)
                   .Replace("|", String.Empty)
                   .Replace("?", String.Empty)
                   .Replace("*", String.Empty);
            }
        }
    }
    public class ProgressClass
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public int Progress { get; set; }
    }
}
