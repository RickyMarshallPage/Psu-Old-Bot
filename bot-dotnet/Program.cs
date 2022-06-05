using System;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.IO;

using System.Text;
using System.Text.RegularExpressions;

using System.Collections;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft;

using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;

using Obfuscator;
using Obfuscator.Obfuscation;

namespace DiscordBot
{
    public class Program
    {
        static void Main(string[] Arguments) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public static ulong EPIC = 459569058526789632;
        public static bool RESTRICTED_MODE = false;
        public static string VERSION = "4.0.A";

        public static DiscordSocketClient Client;
        private static CommandService Commands;
        private static IServiceProvider Services;

        private static Encoding LuaEncoding = Encoding.GetEncoding(28591);

        private static string Token = "Your bot token";

        public static Emoji A = new Emoji("🇦");
        public static Emoji B = new Emoji("🇧");
        public static Emoji C = new Emoji("🇨");
        public static Emoji D = new Emoji("🇩");
        public static Emoji E = new Emoji("🇪");
        public static Emoji F = new Emoji("🇫");

        public async Task RunBotAsync()
        {
            Client = new DiscordSocketClient();

            Commands = new CommandService();

            Services = new ServiceCollection().AddSingleton(Client).AddSingleton(Commands).BuildServiceProvider();

            Client.Log += ClientLog;

            await RegisterCommandsAsync();

            await Client.LoginAsync(TokenType.Bot, Token);

            await Client.StartAsync();

            await Client.SetStatusAsync(UserStatus.Online);

            await Task.Delay(-(1));
        }

        private Task ClientLog(LogMessage Argument) { return Task.CompletedTask; }
        public async Task RegisterCommandsAsync() { Client.MessageReceived += HandleCommandAsync; await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services); }

        //////////////////////////////////////////////////

        public struct ObfuscationProcess { public ulong UserId; public IUserMessage IUserMessage; public Obfuscator.Obfuscator Obfuscator; public bool Obfuscating; public Process Process; public bool License; public bool Boost; };
        public struct UserDataTemplate { public long Time; public short Obfuscations; public long ObfuscationTime; };

        public static Dictionary<ulong, ObfuscationProcess> Processes = new Dictionary<ulong, ObfuscationProcess>();
        public static Dictionary<ulong, UserDataTemplate> UserData = new Dictionary<ulong, UserDataTemplate>();

        //////////////////////////////////////////////////
        
        public static void StopObfuscationProcess(ulong UserId)
        {
            if (Processes.ContainsKey(UserId))
            {
                ObfuscationProcess ObfuscationProcess = Processes[UserId];
                ObfuscationProcess.Obfuscating = false;

                if (ObfuscationProcess.Obfuscator != null)
                    ObfuscationProcess.Obfuscator.Obfuscating = false;

                if (Processes[UserId].Process != null)
                    Processes[UserId].Process.Kill();

                Processes.Remove(UserId);
            };
        }

        private async Task HandleCommandAsync(SocketMessage Argument)
        {
            if (!(Argument is SocketUserMessage)) return;
            SocketUserMessage Message = (Argument as SocketUserMessage);

            SocketCommandContext Context = new SocketCommandContext(Client, Message);

            if ((object.ReferenceEquals(Message.Author, null)) || (Message.Author.IsBot)) { return; };

            ulong UserId = Message.Author.Id;

            if (RESTRICTED_MODE && UserId != EPIC) return;

            int ArgumentPosition = 0;

            if (Message.HasStringPrefix("!", ref ArgumentPosition))
            {
                if (!UserData.ContainsKey(UserId))
                    UserData.Add(UserId, new UserDataTemplate { Time = 0 });

                if ((DateTime.Now.Ticks - UserData[UserId].Time) < 10000000)
                    return;

                UserDataTemplate UserDataTemplate = UserData[UserId];
                UserDataTemplate.Time = DateTime.Now.Ticks;

                Commands.ExecuteAsync(Context, ArgumentPosition, Services);
            }
            else if (Processes.ContainsKey(UserId))
            {
                ObfuscationProcess ObfuscationProcess = Processes[UserId];

                string VanityByteCodeMode = "Default";

                if (ObfuscationProcess.License)
                {
                    VanityByteCodeMode = Message.Content;

                    switch (Message.Content)
                    {
                        case ("Chinese"):
                        case ("Arabic"):
                        case ("Korean"):
                        case ("Emoji"):
                        case ("Greek"):
                        case ("Symbols #1"):
                        case ("Symbols #2"):
                        case ("Symbols #3"):
                            {
                                break;
                            };

                        default: { VanityByteCodeMode = "Default"; break; };
                    };
                };

                IUserMessage IUserMessage = ObfuscationProcess.IUserMessage;

                if (ObfuscationProcess.Obfuscating) { return; };
                ObfuscationProcess.Obfuscating = true;

                async void StartObfuscationProcess()
                {
                    void Error(string Message) { var Embed = new EmbedBuilder { Description = $"Error While Obfuscating: ``{Message}``" }; Embed.WithFooter(F => F.Text = $"{VERSION}").WithColor(Color.DarkGrey).WithCurrentTimestamp(); IUserMessage.ModifyAsync(M => M.Embed = Embed.Build()); StopObfuscationProcess(UserId); };

                    string Content = "";

                    if (Message.Attachments.Count == 0)
                    {
                        if ((Message.Content.Length > 7) && (Message.Content.Substring(0, 7).ToLower() == "```lua\n") && (Message.Content.EndsWith("```")))
                        {
                            Content = Message.Content.Substring(7);
                            Content = Content.Substring(0, Content.Length - 3);
                        }
                        else { Error("Invalid Response"); return; };
                    }
                    else if (Message.Attachments.Count > 0)
                    {
                        Attachment Attachment = Message.Attachments.First();

                        WebClient WebClient = new WebClient();

                        string URL = Attachment.Url;

                        byte[] Buffer = WebClient.DownloadData(URL);

                        Content = LuaEncoding.GetString(Buffer);
                    };

                    string Folder = $@"Storage\Obfuscation\{UserId.ToString()}";

                    if (Directory.Exists(Folder)) { Directory.Delete(Folder, true); };
                    Directory.CreateDirectory(Folder);

                    try
                    {
                        var Embed = new EmbedBuilder();
                        Embed.WithAuthor((new EmbedAuthorBuilder { Name = "Script Obfuscation" }));
                        Embed.WithFooter(F => F.Text = $"{VERSION}").WithColor(Color.DarkGrey).WithCurrentTimestamp();

                        string Input = Path.Combine(Folder, "Input.lua");

                        File.WriteAllText(Input, Content, LuaEncoding);
                        FileInfo FileInfo = new FileInfo(Input);

                        IMessage IMessage = IUserMessage.Channel.GetMessageAsync(IUserMessage.Id).Result;

                        ObfuscationSettings ObfuscationSettings;

                        if (ObfuscationProcess.License || ObfuscationProcess.Boost)
                        {
                            ObfuscationSettings = new ObfuscationSettings
                            {
                                EncryptAllStrings = (IMessage.Reactions.ContainsKey(A) && IMessage.Reactions[A].ReactionCount > 1),
                                DisableSuperOperators = (IMessage.Reactions.ContainsKey(B) && IMessage.Reactions[B].ReactionCount > 1),
                                ControlFlowObfuscation = true,
                                EnhancedConstantEncryption = false,
                                ConstantEncryption = false,
                                MaximumSecurityEnabled = (IMessage.Reactions.ContainsKey(C) && IMessage.Reactions[C].ReactionCount > 1),
                                EnhancedOutput = (IMessage.Reactions.ContainsKey(D) && IMessage.Reactions[D].ReactionCount > 1),
                                PremiumFormat = (IMessage.Reactions.ContainsKey(E) && IMessage.Reactions[E].ReactionCount > 1),
                                ByteCodeMode = VanityByteCodeMode
                            };
                        }
                        else
                        {
                            ObfuscationSettings = new ObfuscationSettings
                            {
                                EncryptAllStrings = false,
                                DisableSuperOperators = true,
                                ControlFlowObfuscation = false,
                                ConstantEncryption = false,
                                MaximumSecurityEnabled = false,
                                DisableAllMacros = true,
                                EnhancedOutput = false,
                                PremiumFormat = false,
                                ByteCodeMode = "Default"
                            };

                            if (FileInfo.Length > 250000) { Error("Non-Premium Users Cannot Obfuscate Files Larger Than 250Kb"); return; };
                        };

                        if (FileInfo.Length > 2500000) { Error("Cannot Obfuscate Files Larger Than 2.5Mb (Please Use The Web Panel or API For Large Files!)"); return; };

                        Console.WriteLine($"Obfuscating File From {Message.Author.Username}#{Message.Author.Discriminator.ToString()} ({Message.Author.Id}) @ {Message.Timestamp} ({FileInfo.Length} Bytes)");

                        if (!ObfuscationProcess.License)
                        {
                            Program.UserDataTemplate UserData = Program.UserData[UserId];
                            UserData.Obfuscations += 1;
                            Program.UserData[UserId] = UserData;
                        };

                        Stopwatch ElapsedTime = new Stopwatch();
                        ElapsedTime.Start();

                        bool Success = false;

                        ObfuscationSettings.DebugMode = RESTRICTED_MODE;

                        ObfuscationProcess.Obfuscator = new Obfuscator.Obfuscator(ObfuscationSettings, Folder);

                        async void Obfuscate()
                        {
                            Stopwatch Stopwatch = new Stopwatch(); Stopwatch.Start();

                            string ErrorMessage = "";

                            Embed.AddField("Obfuscation Status", ":arrows_counterclockwise: Compiling...");
                            await IUserMessage.ModifyAsync(M => M.Embed = Embed.Build());

                            Stopwatch.Restart(); try { Success = ObfuscationProcess.Obfuscator.Compile(out ErrorMessage); Stopwatch.Stop(); } catch (Exception Exception) { Console.WriteLine(Exception); Error("Error While Obfuscating File"); }; if (!Success) { Error(ErrorMessage); return; };
                            string CompileTime = ((float)Stopwatch.ElapsedMilliseconds / 1000).ToString();

                            Embed.Fields.Clear();
                            Embed.AddField("Obfuscation Status", $":ballot_box_with_check: Compiled in {CompileTime} Seconds\n:arrows_counterclockwise: Deserializing...");
                            IUserMessage.ModifyAsync(M => M.Embed = Embed.Build());

                            Stopwatch.Restart(); try { Success = ObfuscationProcess.Obfuscator.Deserialize(out ErrorMessage); Stopwatch.Stop(); } catch (Exception Exception) { Console.WriteLine(Exception); Error("Error While Obfuscating File"); }; if (!Success) { Error(ErrorMessage); return; };
                            string DeserializeTime = ((float)Stopwatch.ElapsedMilliseconds / 1000).ToString();

                            Embed.Fields.Clear();
                            Embed.AddField("Obfuscation Status", $":ballot_box_with_check: Compiled in {CompileTime} Seconds\n:ballot_box_with_check: Deserialized in {DeserializeTime} Seconds\n:arrows_counterclockwise: Obfuscating...");
                            IUserMessage.ModifyAsync(M => M.Embed = Embed.Build());

                            Stopwatch.Restart(); try { Success = ObfuscationProcess.Obfuscator.Obfuscate(out ErrorMessage); } catch (Exception Exception) { Console.WriteLine(Exception); Error("Error While Obfuscating File"); }; Stopwatch.Stop(); if (!Success) { Error(ErrorMessage); return; };
                            string ObfuscateTime = ((float)Stopwatch.ElapsedMilliseconds / 1000).ToString();

                            Embed.Fields.Clear();
                            Embed.AddField("Obfuscation Status", $":ballot_box_with_check: Compiled in {CompileTime} Seconds\n:ballot_box_with_check: Deserialized in {DeserializeTime} Seconds\n:ballot_box_with_check: Obfuscated in {ObfuscateTime} Seconds");
                            IUserMessage.ModifyAsync(M => M.Embed = Embed.Build());

                            string TimeTaken = ((float)ElapsedTime.ElapsedMilliseconds / 1000).ToString();

                            try { await IMessage.Channel.SendFileAsync(Path.Combine(Folder, "Output.lua"), $"{Message.Author.Mention}, Obfuscated in {TimeTaken} Seconds!"); } catch (Exception Exception) { Console.WriteLine(Exception); Error("Error While Uploading File"); return; };

                            ElapsedTime.Stop();

                            Directory.Delete(Folder, true);
                        }

                        Thread Thread = new Thread(Obfuscate); Thread.Start();
                        if (!Thread.Join(60000)) { try { Thread.Abort(); Error("Obfuscation Timeout"); } catch { }; };

                        ObfuscationProcess.Obfuscating = false;
                        StopObfuscationProcess(UserId);
                        if (Processes.ContainsKey(UserId)) { Processes.Remove(UserId); };
                    }
                    catch (Exception Exception) { Console.WriteLine(Exception); Error("Unknown Error #2"); StopObfuscationProcess(UserId); return; };
                }

                new Thread(StartObfuscationProcess).Start();
            };
        }
    };
};
