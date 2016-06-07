using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

//using System.IO;
//using System.Reflection;
//using System.Threading;
//using System.Threading.Tasks;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Services;
//using Google.Apis.Upload;
//using Google.Apis.Util.Store;
//using Google.Apis.YouTube.v3;
//using Google.Apis.YouTube.v3.Data;
//using System.Threading;

namespace SayingsBot
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>


        public static Logger RNGLogger;
        public static RNGWindow mainWindow;
        public static HarbBot HarbBot;
        public static NetComm.Host Server;
        //static DBHandler RNGDB;
        


        [STAThread] static void Main()
        {
#if DEBUG
            Server = new NetComm.Host(8524);
#else
            Server = new NetComm.Host(8523);
#endif

            RNGLogger = new Logger();
            RNGLogger.addLog("Main()", 0, "Logger object created");
#if OFFLINE
            RNGLogger.addLog("Main()", 0, "Working in offline mode, no IRC connection will be made!");
#endif

#if !OFFLINE
            HarbBot = new HarbBot(RNGLogger, Server);
#endif

            //RNGDB = new DBHandler("rngppbot.sqlite", RNGLogger);
            /*
            try
            {
                new Program().Run().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }*/

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainWindow = new RNGWindow(RNGLogger, HarbBot);
            try
            {
                Application.Run(mainWindow);
            } catch (Exception EEE) {
                HarbBot.appendFile(HarbBot.progressLogPATH, EEE.ToString());
            }
        }

        /*private async Task Run()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for full read/write access to the
                    // authenticated user's account.
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });
            var lb = new LiveBroadcast();
            var tset = new LiveBroadcastsResource(youtubeService);
            tset.Bind(lb.Id, "");
            var ls = new LiveStream();
            var lcm = new LiveChatMessagesResource(youtubeService);
            var listed = lcm.List(lb.Snippet.LiveChatId, "snippet").Execute().Items;
            int max = listed.Count;
            string[][] messges = new string[][]{};
            List<string> seenMessages = new List<string>();
            for (int i = 0; i < max; i++)
            {
                if (!seenMessages.Contains(listed.ElementAt(i).Id)) { 
                    messges[i][0] = listed.ElementAt(i).AuthorDetails.DisplayName;
                    if (listed.ElementAt(i).AuthorDetails.IsChatModerator == true)
                        { messges[i][1] = "true"; } else { messges[i][1] = "false"; }
                    if (listed.ElementAt(i).AuthorDetails.IsChatSponsor == true)
                        { messges[i][2] = "true"; } else { messges[i][2] = "false"; }
                    if (listed.ElementAt(i).AuthorDetails.IsChatOwner == true)
                        { messges[i][3] = "true"; } else { messges[i][3] = "false"; }
                    messges[i][4] = listed.ElementAt(i).Snippet.DisplayMessage;
                    Console.WriteLine("Youtube - User:" + messges[i][0] + " Message:" + messges[i][4]);
                    commandChecker(messges[i]);
                    seenMessages.Add(listed.ElementAt(i).Id);
                }
            }
            
            /*
            // Create a new, private playlist in the authorized user's channel.
            var newPlaylist = new Playlist();
            newPlaylist.Snippet = new PlaylistSnippet();
            newPlaylist.Snippet.Title = "Test Playlist";
            newPlaylist.Snippet.Description = "A playlist created with the YouTube API v3";
            newPlaylist.Status = new PlaylistStatus();
            newPlaylist.Status.PrivacyStatus = "public";
            newPlaylist = await youtubeService.Playlists.Insert(newPlaylist, "snippet,status").ExecuteAsync();
            // Add a video to the newly created playlist.
            var newPlaylistItem = new PlaylistItem();
            newPlaylistItem.Snippet = new PlaylistItemSnippet();
            newPlaylistItem.Snippet.PlaylistId = newPlaylist.Id;
            newPlaylistItem.Snippet.ResourceId = new ResourceId();
            newPlaylistItem.Snippet.ResourceId.Kind = "youtube#video";
            newPlaylistItem.Snippet.ResourceId.VideoId = "GNRMeaz6QRI";
            newPlaylistItem = await youtubeService.PlaylistItems.Insert(newPlaylistItem, "snippet").ExecuteAsync();
            Console.WriteLine("Playlist item id {0} was added to playlist id {1}.", newPlaylistItem.Id, newPlaylist.Id);
             */
       /* }

        void commandChecker(string[] data)
        {
            HarbBot.checkCommand(HarbBot.channels, data[0], data[4]);
        }*/
    }
}
