﻿using DSharpPlus.Entities;
using DSharpPlus;
using Stone_Red_Utilities.ColorConsole;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace DiscordCLI
{
    internal class Commands
    {
        private readonly DiscordClient client;

        private readonly OutputManager outputManager;

        public Commands(DiscordClient dicordClient, OutputManager outputMan)
        {
            client = dicordClient;
            outputManager = outputMan;
        }

        protected async Task ListGuilds(string args)
        {
            int index = 1;
            foreach (DiscordGuild guild in client.Guilds.Values)
            {
                Console.WriteLine($"{index}. {guild.Name}");
                index++;
            }

            await Task.CompletedTask;
        }

        protected async Task ListDms(string args)
        {
            int index = 1;
            foreach (DiscordDmChannel dmChannel in client.PrivateChannels)
            {
                Console.WriteLine($"{index}. {string.Join(", ", dmChannel.Recipients.Select(x => x.Username))}");
                index++;
            }
            await Task.CompletedTask;
        }

        protected async Task ListGuildChannels(string args)
        {
            IReadOnlyCollection<DiscordChannel> textChannels;
            DiscordGuild guild;

            if (args != null)
            {
                if (int.TryParse(args, out int ind))
                {
                    guild = client.Guilds?.Values.ElementAtOrDefault(ind - 1);
                }
                else
                {
                    guild = client.Guilds?.FirstOrDefault(x => x.Value.Name == args).Value;
                }
                GlobalInformation.currentTextChannel = null;
            }
            else
            {
                guild = GlobalInformation.currentGuild;
            }

            if (guild is null)
            {
                ConsoleExt.WriteLine("Guild not found!", ConsoleColor.Red);
                GlobalInformation.currentTextChannel = null;
                return;
            }

            GlobalInformation.currentGuild = guild;
            textChannels = guild.Channels;

            int index = 1;
            foreach (DiscordChannel channel in textChannels)
            {
                switch (channel.Type)
                {
                    case ChannelType.Category:
                        //Console.WriteLine($"[{channel.Name}]");
                        break;

                    case ChannelType.Text:
                        Console.WriteLine($"    {index}. {channel.Name}");
                        index++;
                        break;
                }
            }

            await Task.CompletedTask;
        }

        protected async Task EnterChannel(string args)
        {
            DiscordChannel textChannel;

            if (GlobalInformation.currentGuild is null)
            {
                ConsoleExt.WriteLine("You are not in a guild!", ConsoleColor.Red);
                return;
            }

            if (int.TryParse(args, out int ind))
            {
                textChannel = GlobalInformation.currentGuild?.Channels.Where(x => x.Type == ChannelType.Text).ElementAtOrDefault(ind - 1);
            }
            else
            {
                textChannel = GlobalInformation.currentGuild?.Channels.Where(x => x.Type == ChannelType.Text).FirstOrDefault(x => x.Name == args);
            }

            GlobalInformation.currentTextChannel = textChannel;

            if (textChannel is null)
            {
                ConsoleExt.WriteLine("Channel not found!", ConsoleColor.Red);
                return;
            }

            try
            {
                foreach (DiscordMessage message in (textChannel.GetMessagesAsync(10).Result).Reverse())
                {
                    await outputManager.WriteMessage(message, textChannel, GlobalInformation.currentGuild, false);
                }
            }
            catch (Exception ex)
            {
                ConsoleExt.WriteLine(ex.Message, ConsoleColor.Red);
                GlobalInformation.currentTextChannel = null;
            }

            await Task.CompletedTask;
        }

        protected async Task EnterDmChannel(string args)
        {
            DiscordDmChannel dmChannel;

            GlobalInformation.currentGuild = null;

            if (int.TryParse(args, out int ind))
            {
                dmChannel = client.PrivateChannels.ElementAtOrDefault(ind - 1);
            }
            else
            {
                dmChannel = client.PrivateChannels.FirstOrDefault(x => x.Name == args);
            }

            GlobalInformation.currentTextChannel = dmChannel;

            if (dmChannel is null)
            {
                ConsoleExt.WriteLine("Channel not found!", ConsoleColor.Red);
                return;
            }

            try
            {
                foreach (DiscordMessage message in (await dmChannel.GetMessagesAsync(10)).Reverse())
                {
                    await outputManager.WriteMessage(message, dmChannel, GlobalInformation.currentGuild, false);
                }
            }
            catch (Exception ex)
            {
                ConsoleExt.WriteLine(ex, ConsoleColor.Red);
                GlobalInformation.currentTextChannel = null;
            }
        }

        protected async Task DeleteToken(string args)
        {
            File.Delete(Program.tokenPath);
            Environment.Exit(0);

            await Task.CompletedTask;
        }
    }
}