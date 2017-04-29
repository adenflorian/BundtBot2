using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BundtBot.Extensions;
using BundtCommon.Extensions;
using BundtCord.Discord;

namespace BundtBot
{
    class CommandManager
    {
        public string CommandPrefix = "!";
        readonly Dictionary<string, TextCommand> CommandsDict = new Dictionary<string, TextCommand>();
        
        static readonly MyLogger _logger = new MyLogger(nameof(CommandManager));

        public void AddCommand(TextCommand newCommand)
        {
            CommandsDict.Add(newCommand.Name, newCommand);
        }

        public IEnumerable<TextCommand> GetCommands()
        {
            return CommandsDict.Values;
        }

        public async Task ProcessTextMessageAsync(TextChannelMessage message)
        {
            if (message.Content.DoesNotStartWith(CommandPrefix)) return;
            if (message.Content.Length < CommandPrefix.Length + 1) return;

            var commandString = StripPrefix(message.Content);

            var receivedCommand = new ReceivedCommand(commandString);

            if (CommandsDict.DoesNotContainKey(receivedCommand.Name)) throw new CommandException($"Command {receivedCommand.Name} not found, are you ok?");
            var matchingCommand = CommandsDict[receivedCommand.Name];

            if (receivedCommand.Args.Count < matchingCommand.MinimumArgCount)
            {
                throw new CommandException($"You gave me {receivedCommand.Args.Count} arguments, "
                    + $"but I need {matchingCommand.MinimumArgCount}");
            }

            try
            {
                await matchingCommand.Command.Invoke(message, receivedCommand);
            }
            catch (Exception ex)
            {
                await message.ReplyAsync($"Some guy named {ex.GetType()} punched me while I was following your {receivedCommand.Name} command...then he told me: '{ex.Message}");
                _logger.LogError(ex);
            }
        }

        string StripPrefix(string command)
        {
            return command.Substring(CommandPrefix.Length);
        }
    }
}