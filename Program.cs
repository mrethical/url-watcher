using CommandLine;
using HtmlAgilityPack;
using Standart.Hash.xxHash;

class Program
{
    public class Options
    {
        [Value(index: 0, Required = true, MetaName = "url", HelpText = "Url to watch.")]
        public string url { get; set; }
        
        [Option('i', "interval", Required = false, HelpText = "Ping interval (milliseconds).")]
        public int? interval { get; set; }
        
        [Option('x', "xpath", Required = false, HelpText = "XPath selector to track only a section of the page.")]
        public string? xpath { get; set; }
        
        [Option('v', "verbose", Required = false, HelpText = "Verbose logging.")]
        public bool verbose { get; set; }
    }
    
    public static int Main(string[] args)
    {
        string? url = null;
        string? xpath = null;
        int interval = 50;
        bool verbose = false;

        Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(options => {
                url = options.url;
                xpath = options.xpath;
                interval = options.interval ??= interval;
                verbose = options.verbose;
            });

        if (String.IsNullOrEmpty(url)) {
            return 1;
        }

        ulong? currentHash = null;
        var timer = new System.Timers.Timer(interval);
        timer.Elapsed += (sender, evnt) => {
            HttpClient client = new HttpClient();
            Task<string> responseTask = Task.Run(() => client.GetStringAsync(url));
            string response = responseTask.Result;

            if (! String.IsNullOrEmpty(xpath)) {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(response);

                response = htmlDocument.DocumentNode.SelectSingleNode(xpath).OuterHtml;
            }

            ulong? newHash = xxHash64.ComputeHash(response);

            if (! currentHash.HasValue && newHash.HasValue) {
                currentHash = newHash;
                Console.WriteLine(String.Format("[{0}] Welcome.", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")));
                return;
            }

            if (! currentHash.Equals(newHash)) {
                currentHash = newHash;
                Console.WriteLine(String.Format("[{0}] New content detected.", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")));
            }
            else if (verbose) {
                Console.WriteLine(String.Format("[{0}] No changes detected.", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")));
            }
        };
        timer.Start();
        while (true) {
            Thread.Sleep(1000);
        }
    }
}
