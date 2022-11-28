// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using YoutubeVideoDownloader;

try
{
    ObservableCollection<ProgressClass> list = new();
    list.CollectionChanged += List_CollectionChanged;

    void List_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Console.Clear();
        list.ToList().ForEach(x => Console.WriteLine(x.Title.PadRight(20) + " downloading " + x.Progress + "% ..."));
    }

    var youtube = new YoutubeClient();
    Config.SelectedResolution = SelectAnResolution();
    Config.SelectedMediaType = SelectMediaType();
    Console.Clear();
    Console.WriteLine("Give a Youtube Link:");
    var mainLink = Console.ReadLine();
    Console.Clear();
    Utility model = new Utility();
    if (await model.Start(mainLink))
    {
        await Parallel.ForEachAsync(model.Videos, async (video, cancellationToken) =>
        {
            IProgress<double> downloadProgress = new Progress<double>(report =>
            {
                lock (list)
                {
                    var item = list.FirstOrDefault(x => x.Title == video.VideoName);
                    if (item != null)
                    {
                        if (item.Progress == (int)(report * 100))
                            return;
                        else
                        {
                            list.Insert(list.IndexOf(item), new ProgressClass { Title = video.VideoName, Progress = (int)(report * 100), ID = video.ID });
                            list.Remove(item);
                        }
                    }
                    else
                        list.Add(new ProgressClass { Title = video.VideoName, Progress = (int)(report * 100), ID = video.ID });

                }
            });
            if (Config.SelectedMediaType == MediaType.Video)
            {
                await youtube.Videos.DownloadAsync(new IStreamInfo[] { video.VideoStreamInfo, video.AudioStreamInfo }, new ConversionRequestBuilder(video.MuxedPath).SetFFmpegPath(Config.FfmpegDir).Build(), downloadProgress);
                //await youtube.Videos.Streams.DownloadAsync(video.VideoStreamInfo, video.VideoPath, downloadProgress);
                //await youtube.Videos.Streams.DownloadAsync(video.AudioStreamInfo, video.AudioPath);
                //await MergeVideoAndAudio(video.VideoPath, video.AudioPath, video.MuxedPath);
            }
            else
            {
                await youtube.Videos.Streams.DownloadAsync(video.AudioStreamInfo, video.MuxedPath, downloadProgress);
            }
        });
    }
}
catch (Exception ex)
{
    await File.WriteAllTextAsync(JsonConvert.SerializeObject(ex, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), Path.Combine(Config.AppDataLogDir, DateTime.Now.ToString("ss-mm-HH-dd-MM-yyyyy")));
    Environment.Exit(0);
}


MediaType SelectMediaType()
{
    Console.Clear();
    Console.WriteLine("Select a Type:");

    Console.WriteLine("1 - Video");
    Console.WriteLine("2 - Music");

    MediaType selection = 0;
    if (MediaType.TryParse(Console.ReadLine(), out selection))
        if (selection != 0)
            return selection;
        else
            return SelectMediaType();
    else
        return SelectMediaType();
}
int SelectAnResolution()
{
    Console.Clear();
    Console.WriteLine("Select a Resolution:");

    for (int i = 0; i < Config.Resolutions.Count; i++)
    {
        Console.WriteLine(i + 1 + "- " + Config.Resolutions[i] + "p");
    }

    int selection;
    if (int.TryParse(Console.ReadLine(), out selection))
        if (selection >= 1 && selection <= 9)
            return Config.Resolutions[selection - 1];
        else
            return SelectAnResolution();
    else
        return SelectAnResolution();
}
