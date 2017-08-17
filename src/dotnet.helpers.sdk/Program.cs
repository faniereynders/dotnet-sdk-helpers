using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace dotnet.helpers.sdk
{
    class Program
    {
        private static bool busy;
        private static string busyText;
#if DEBUG
        private static bool showHelp = true;
#endif
        static void Main(string[] args)
        {
#if DEBUG           
            while (true)
            {
#endif
                var app = new CommandLineApplication();

                app.Name = ".net sdk";

                app.HelpOption("-?|-h|--help");
                app.Command("use", UseCommand);
                app.Command("list", ListCommand);
                app.Command("releases", ReleasesCommand);
                app.Command("install", InstallReleaseCommand);
#if DEBUG
                if (showHelp)
                {
                    app.ShowHelp();
                    showHelp = false;
                }
                
                Console.Write("> ");
                args = Console.ReadLine().Split(" ").Where(a => a != "dotnet" && a != ".net" && a != "sdk").ToArray();
#endif
                try
                {
                    if (args.Length > 0)
                    {
                        app.Execute(args);
                    }
                    else
                    {
                        Console.WriteLine(".NET Core SDK Helper");
                        app.ShowHelp();
                    }
                    Console.WriteLine();
                }
                catch (CommandParsingException ex)
                {

                    Console.WriteLine(ex.Message);
                }
#if DEBUG
            }
#endif
        }

        static Action<CommandLineApplication> UseCommand = (command) =>
        {
            command.Description = "Switch to a specific version of the SDK";
            command.HelpOption("-?|-h|--help");

            var versionArgument = command.Argument("[version]", "Specific version of the .NET Core SDK");

            command.Arguments.Add(new CommandArgument { Name = "latest", Description = "Latest version of the .NET Core SDK" });
            command.OnExecute(() =>
            {
                if (!string.IsNullOrEmpty(versionArgument.Value))
                {
                    if (versionArgument.Value.Equals("latest", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (File.Exists(@"global.json"))
                        {
                            File.Delete(@"global.json");
                        }
                        if (File.Exists(@"..\global.json"))
                        {
                            Console.WriteLine("There's a global.json in your parent directory. Do you want to delete it? (n/y)");
                            var choice = Console.ReadLine();
                            if (choice.Equals("y", StringComparison.InvariantCultureIgnoreCase))
                            {
                                File.Delete(@"..\global.json");
                            }
                        }
                    }
                    else
                    {
                        var versionExists = GetInstalledSdks().Select(d => d.Name).Any(d => d.Equals(versionArgument.Value));
                        if (versionExists)
                        {
                            var globalJson = new { sdk = new { version = versionArgument.Value } };
                            File.WriteAllText(@".\global.json", JsonConvert.SerializeObject(globalJson, Formatting.Indented));
                        }
                        else
                        {
                            Console.WriteLine($"No such version available locally: {versionArgument.Value} (use 'list' for all local versions)");
                            return 1;
                        }
                    }
                    var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = "--version",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };

                    proc.Start();
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string line = proc.StandardOutput.ReadLine();
                        if (line != null)
                        {
                            Console.WriteLine($".NET Core SDK current version is now {line}");
                        }
                    }
                }
                else
                {
                    command.ShowHelp("use");
                }

                return 1;
            });
        };
        static Action<CommandLineApplication> ListCommand = (command) =>
        {
            command.Description = "Lists all installed .NET Core SDKs";
            command.OnExecute(() =>
            {
                Console.WriteLine("The installed .NET Core SDKs are:");
                foreach (var directory in GetInstalledSdks())
                {
                    Console.WriteLine($"{directory.Name}");
                }

                return 1;
            });
        };
        static Action<CommandLineApplication> ReleasesCommand = (command) =>
        {
            command.Description = "Lists all available releases of .NET Core SDKs";

            command.OnExecute(async () =>
            {
                Console.WriteLine("Releases available for the .NET Core SDK are: (* = installed)");
                Console.WriteLine();
                var releases = await GetSDKReleases();
                var installedSdks = GetInstalledSdks();
                foreach (var release in releases)
                {
                    var isInstalled = installedSdks.Any(folder =>
                    folder.Name.Equals(release["version-sdk"], StringComparison.CurrentCultureIgnoreCase)) ? "*" : string.Empty;
                    Console.WriteLine($"{release["date"],-10} {release["version-sdk"] + isInstalled,-30}");

                }
                return 1;
            });
        };
        static Action<CommandLineApplication> InstallReleaseCommand = (command) =>
        {
            command.Description = "Download & installs the provided release version";
            command.HelpOption("-?|-h|--help");

            var versionArgument = command.Argument("[version]", "Specific version of the .NET Core SDK");

            command.Arguments.Add(new CommandArgument { Name = "latest", Description = "Latest version of the .NET Core SDK" });

            var platformOption = command.Option("-r|--runtime", "Target platform version", CommandOptionType.SingleValue);

            command.OnExecute(async () =>
            {
                if (!string.IsNullOrEmpty(versionArgument.Value))
                {
                    var platform = GetOSPlatform();

                    if (platformOption.HasValue())
                    {
                        platform = platformOption.Value();
                    }
                    var platformKey = $"sdk-{platform}";

                    var sdks = await GetSDKReleases();

                    if (sdks.Any(r => r["version-sdk"] == versionArgument.Value))
                    {
                        IDictionary<string, string> release;
                        if (versionArgument.Value.Equals("latest", StringComparison.InvariantCultureIgnoreCase))
                        {
                            release = sdks.First();
                        }
                        else
                        {
                            release = sdks.First(r => r["version-sdk"] == versionArgument.Value);
                        }
                        var spin = new Animations();
                        Console.WriteLine($"Getting .NET Core SDK version {release["version-sdk"]} for platform {platform}...");

                        var client = new HttpClient();
                        var fileName = release[platformKey];//.Replace(".zip", ".exe");
                        var url = $"{release["blob-sdk"]}{fileName}";

                        GetRelease(url, $"{GetDotNetPath()}");
                        var row = Console.CursorTop + 2;

                        while (busy)
                        {
                            spin.Turn(busyText, row);
                            spin.Ready();
                        }

                        Console.CursorTop--;
                        Console.SetCursorPosition(0, row);
                        Console.WriteLine("Completed!                                                       ");
                    }
                    else
                    {
                        Console.WriteLine($"The version {versionArgument.Value} does not exists. (use 'releases' to see what is available)");
                    }
                }
                else
                {
                    command.ShowHelp("use");
                }
                return 1;
            });
        };
        
        private static IEnumerable<DirectoryInfo> GetInstalledSdks()
        {
            var dotnetDirectory = new DirectoryInfo($@"{GetDotNetPath()}\sdk");
            return dotnetDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly).Where(d => d.Name.Contains(".")).ToList();
        }

        private static string GetDotNetPath()
        {
            return $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\dotnet";
        }

        private static async Task<IDictionary<string, string>[]> GetSDKReleases()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("https://raw.githubusercontent.com/dotnet/core/master/release-notes/releases.json");

                var content = await response.Content.ReadAsStringAsync();
                var releases = JsonConvert.DeserializeObject<IDictionary<string, string>[]>(content);

                return releases.GroupBy(r => r["version-sdk"]).Select(s => s.First()).ToArray();
            }
        }
        private static string GetOSPlatform()
        {
            string arc = RuntimeInformation.OSArchitecture.ToString().ToLower();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"win-{arc}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return $"linux-{arc}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return $"mac-{arc}";
            }

            return string.Empty;
        }

        private static async Task GetRelease(string url, string dest)
        {
            try
            {
                busy = true;
                busyText = "Preparing download...";
                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var tempFile = Path.GetTempFileName();
                            var expectedLength = (decimal)response.Content.Headers.ContentLength;
                            using (var ms = new FileStream(tempFile, FileMode.Create))
                            {
                                UpdateDownloadProgress(tempFile, expectedLength);
                                await stream.CopyToAsync(ms);

                                using (var archive = new ZipArchive(ms, ZipArchiveMode.Read))
                                {
                                    var result = archive.Entries.AsEnumerable();
#if DEBUG
                                    result = result.Where(e => !e.Name.StartsWith("dotnet"));
#endif
                                    result = result.ToList();

                                    var assetCount = (double)result.Count();
                                    var assetId = 1d;
                                    foreach (var entry in result)
                                    {
                                        busyText = $"Installing ({Math.Round((assetId / assetCount) * 100)}%)...";

                                        var file = new FileInfo(Path.Combine(dest, entry.FullName));
                                        if (!file.Directory.Exists)
                                        {
                                            file.Directory.Create();
                                        }
                                        entry.ExtractToFile(Path.Combine(dest, entry.FullName), true);
                                        assetId++;
                                    }
                                }
                            }
                            File.Delete(tempFile);
                        }
                    }
                }
                busy = false;
            }
            catch (Exception ex)
            {
                busy = false;
                throw;
            }
        }

        private static void UpdateDownloadProgress(string tempFile, decimal length)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var info = new FileInfo(tempFile);
                    busyText = $"Downloading ({Math.Round(info.Length / length * 100)}%)... ";
                    Thread.Sleep(500);
                    if (info.Length >= length)
                    {
                        return;
                    }
                }
            });
        }
    }
}
