// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using YoutubeVideoDownloader;

var response = "y";
while (response == "y")
{
    await Start();
    Console.Clear();
    Console.WriteLine("Do you want to download another videos?\n\rIf yes press \"y\" another will close the app.");
    response = Console.ReadLine().Trim();
}

Console.WriteLine("Press any key to exit");
Console.ReadLine();
Environment.Exit(0);

async Task Start()
{
    bool isErrorOccured = false;
    Utility model = new Utility();
    YoutubeClient youtube = new YoutubeClient(); ;
    ObservableCollection<ProgressClass> list = new ObservableCollection<ProgressClass>(); ;
    try
    {
        list.CollectionChanged += List_CollectionChanged;

        void List_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Console.Clear();
            list.ToList().ForEach(x => Console.WriteLine(x.Title.PadRight(20) + " downloading " + x.Progress + "% ..."));
        }

        Console.WriteLine("Files Creating...");
        await Config.CreateFiles();
        Config.SelectedResolution = SelectAnResolution();
        Config.SelectedMediaType = SelectMediaType();
        Console.Clear();
        Console.WriteLine("Give a Youtube Link:");
        var mainLink = Console.ReadLine();
        Console.Clear();
        Console.WriteLine("Getting Video Datas...");
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
                    await youtube.Videos.DownloadAsync(new IStreamInfo[] { video.VideoStreamInfo, video.AudioStreamInfo }, new ConversionRequestBuilder(video.MuxedPath).SetFFmpegPath(Config.FfmpegFilePath).Build(), downloadProgress);
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
        isErrorOccured = true;
        await File.WriteAllTextAsync(JsonConvert.SerializeObject(ex, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), Path.Combine(Config.AppDataLogDirPath, DateTime.Now.ToString("ss-mm-HH-dd-MM-yyyyy")));
        Console.WriteLine("Error occured. Help to fix it contact with app developer.");
        Console.ReadLine();
    }
    finally
    {
        if (!isErrorOccured)
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = Path.GetDirectoryName(model.Videos.FirstOrDefault().MuxedPath),
                UseShellExecute = true,
                Verb = "open"
            });
    }
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
(int width, int height) SelectAnResolution()
{
    Console.Clear();
    Console.WriteLine("Select a Resolution:");

    for (int i = 0; i < Config.Resolutions.Count; i++)
    {
        Console.WriteLine(i + 1 + "- " + Config.Resolutions[i].height + "p");
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
