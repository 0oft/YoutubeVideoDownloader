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
        private IVideoStreamInfo SelectVideoStream(StreamManifest streamManifest)
        {
            IVideoStreamInfo stream = null;
            var index = Config.Resolutions.IndexOf(Config.SelectedResolution);
            while (stream == null)
            {
                stream = streamManifest.GetVideoStreams().FirstOrDefault(x => x.VideoResolution.Height == Config.Resolutions[index].height || x.VideoResolution.Width == Config.Resolutions[index].width || x.VideoResolution.Height == Config.Resolutions[index].width || x.VideoResolution.Width == Config.Resolutions[index].height);
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
            MuxedPath = Config.SelectedMediaType == MediaType.Music ? MuxedPath = Path.Combine(Config.DownloadsFolderPath, VideoName + "." + AudioStreamInfo.Container.Name) : MuxedPath = Path.Combine(Config.DownloadsFolderPath, VideoName + "." + VideoStreamInfo.Container.Name);
            ID = System.Guid.NewGuid().ToString();
        }

        private string videoName { get; set; }
        public string ID { get; }
        public IVideoStreamInfo VideoStreamInfo { get; }
        public IStreamInfo AudioStreamInfo { get; }
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
