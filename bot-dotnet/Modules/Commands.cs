using Discord;
using Discord.Commands;

using System;
using System.Linq;
using System.IO;

using System.Text;
using System.Text.RegularExpressions;

using System.Threading;
using System.Threading.Tasks;

using System.Collections;
using System.Collections.Generic;

using Discord.WebSocket;

using Newtonsoft.Json;

namespace DiscordBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private ulong GuildID = 929751465285812224;

        [Command("obfuscate")]
        public async Task Obfuscate()
        {
            ulong UserID = Context.Message.Author.Id;

            SocketGuildUser User = Program.Client.GetGuild(GuildID).GetUser(UserID);

            bool License = false;
            bool ServerBoost = false;

            foreach (SocketRole Role in User.Roles)
            {
                if (Role.Name == "Client")
                {
                    License = true;
                }
                else if (Role.Name == "Server Booster")
                {
                    ServerBoost = true;
                };
            };

            if (File.Exists("freepremium.txt"))
                License = true;

            Program.UserDataTemplate UserData = Program.UserData[UserID];

            Program.UserData[UserID] = UserData;

            async void CreateMessage()
            {
                var Embed = new EmbedBuilder();
                IUserMessage IUserMessage = null;

                Embed.WithAuthor((new EmbedAuthorBuilder { Name = "PSU Obfuscation" }));
                Embed.WithFooter(F => F.Text = $"PSU Obfuscator {Program.VERSION}").WithColor(Color.DarkGrey).WithCurrentTimestamp();

                if (License)
                {
                    Embed.WithDescription("Please upload a file or respond with formatted text to complete the process. **If you want this session to be private, please obfuscate in DMs!**");

                    Embed.WithTitle("Obfuscation Settings");

                    Embed.AddField(":regional_indicator_a: Encrypt All Strings", "Encrypts all strings in the file where they are defined. **This setting should NOT be used in large scripts and will cause a large performance loss and increase in obfuscation time.**");
                    Embed.AddField(":regional_indicator_b: Disable Super Operators", "Disables generation of Super Operators. This may fix some errors in scripts, but will also decrease performance.");
                    Embed.AddField(":regional_indicator_c: Maximum Security", "Uses more aggressive forms of security to prevent deobfuscation. This will GREATLY affect performance speed and is NOT reccommended on large scipts.");
                    Embed.AddField(":regional_indicator_d: Enhanced Output", "Generates the ULTIMATE output. Makes file size greatly larger but improves security.");
                    Embed.AddField(":regional_indicator_e: Premium Output", "Uses a differnt script structure than normal. Requires loadstring() or load(). This feature will also make PSU_MAX_SECURITY more secure against opcode hooking.");
                    Embed.AddField("Vanity Bytecode", "Replaces the normal Base36 formatted bytecode with a custom skin. Type the mode you would like to use with the file you upload. **Available Modes: Chinese, Arabic, Korean, Emoji, Greek, Symbols1, Symbols2, Symbols3, Default (Base36)**");

                    IUserMessage = await ReplyAsync($"{User.Mention}, Thank you for using PSU Obfuscation!", (false), Embed.Build());

                    if (Program.Processes.ContainsKey(UserID)) { Program.StopObfuscationProcess(UserID); };
                    Program.Processes[UserID] = (new Program.ObfuscationProcess { UserId = UserID, IUserMessage = IUserMessage, License = true });

                    async void AddReactions() { await IUserMessage.AddReactionAsync(Program.A); await IUserMessage.AddReactionAsync(Program.B); await IUserMessage.AddReactionAsync(Program.C); await IUserMessage.AddReactionAsync(Program.D); await IUserMessage.AddReactionAsync(Program.E); };
                    new Thread(AddReactions).Start();
                }
                else if  (License != true)
                {
                    Embed.WithDescription("Please upload a file or respond with formatted text to complete the process. **If you want this session to be private, please obfuscate in DMs!**");

                    Embed.WithTitle("Obfuscation Settings");

                    Embed.AddField(":regional_indicator_a: Encrypt All Strings", "Encrypts all strings in the file where they are defined. **This setting should NOT be used in large scripts and will cause a large performance loss and increase in obfuscation time.**");
                    Embed.AddField(":regional_indicator_b: Disable Super Operators", "Disables generation of Super Operators. This may fix some errors in scripts, but will also decrease performance.");

                    IUserMessage = await ReplyAsync($"{User.Mention}, Thank you for using PSU Obfuscation! You have **∞** obfuscations remaining today!", (false), Embed.Build());

                    if (Program.Processes.ContainsKey(UserID)) { Program.StopObfuscationProcess(UserID); };
                    Program.Processes[UserID] = (new Program.ObfuscationProcess { UserId = UserID, IUserMessage = IUserMessage, License = false, Boost = true });

                    async void AddReactions() { await IUserMessage.AddReactionAsync(Program.A); await IUserMessage.AddReactionAsync(Program.B); };
                    new Thread(AddReactions).Start();
                }
                else
                {
                    Embed.WithDescription("Please upload a file or respond with formatted text to complete the process. *If you want this session to be private, please obfuscate in DMs!*\n\n**You are using a non-licensed version of this product. Please upgrade to remove limitations and unlock all features: https://www.psu.dev/upgrade**");

                    IUserMessage = await ReplyAsync($"{User.Mention}, Thank you for using PSU Obfuscation! You have **∞** obfuscations remaining today!", (false), Embed.Build());

                    if (Program.Processes.ContainsKey(UserID)) { Program.StopObfuscationProcess(UserID); };
                    Program.Processes[UserID] = (new Program.ObfuscationProcess { UserId = UserID, IUserMessage = IUserMessage, License = false });
                };
            };

            new Thread(CreateMessage).Start();
        }
        /*
        [Command("Example")]
        public async Task example()
        {
            var Embed = new EmbedBuilder();
            IUserMessage IUserMessage = null;

            Embed.WithAuthor((new EmbedAuthorBuilder { Name = "PSU Obfuscation" }));
        
            Embed.WithFooter(F => F.Text = $"PSU Obfuscator {Program.VERSION}").WithColor(Color.Red).WithCurrentTimestamp();

            //Example
            
            Embed.WithDescription("Example");

            IUserMessage = await ReplyAsync($":ballot_box_with_check:", (false), Embed.Build());
        }
         */
        [Command("enablerestricted")]
        public async Task EnabledRestrictedMode()
        {
            var Embed = new EmbedBuilder();
            IUserMessage IUserMessage = null;

            Embed.WithAuthor((new EmbedAuthorBuilder { Name = "PSU Obfuscation" }));
            Embed.WithFooter(F => F.Text = $"PSU Obfuscator {Program.VERSION}").WithColor(Color.Red).WithCurrentTimestamp();

            Program.RESTRICTED_MODE = true;

            Embed.WithDescription("RestrictedMode::Enabled");

            IUserMessage = await ReplyAsync($":ballot_box_with_check: Only the owner can Obfuscate!", (false), Embed.Build());
        }
        [Command("disablerestricted")]
        public async Task DisabledRestrictedMode()
        {
            var Embed = new EmbedBuilder();
            IUserMessage IUserMessage = null;

            Embed.WithAuthor((new EmbedAuthorBuilder { Name = "PSU Obfuscation" }));
            Embed.WithFooter(F => F.Text = $"PSU Obfuscator {Program.VERSION}").WithColor(Color.Green).WithCurrentTimestamp();

            Program.RESTRICTED_MODE = false;

            Embed.WithDescription("RestrictedMode::Disabled");

            IUserMessage = await ReplyAsync($":ballot_box_with_check: buyers and nonbuyers can Obfuscate!", (false), Embed.Build());
        }
        [Command("ircChat")]
        public async Task ircChat()
        {
            var Embed = new EmbedBuilder();
            IUserMessage IUserMessage = null;

            Embed.WithAuthor((new EmbedAuthorBuilder { Name = "PSU Obfuscation" }));
            Embed.WithFooter(F => F.Text = $"PSU Obfuscator {Program.VERSION}").WithColor(Color.Green).WithCurrentTimestamp();

            Program.RESTRICTED_MODE = false;

            Embed.WithDescription("Message sent!");

            IUserMessage = await ReplyAsync($":ballot_box_with_check:", (false), Embed.Build());
        }
    };
};
