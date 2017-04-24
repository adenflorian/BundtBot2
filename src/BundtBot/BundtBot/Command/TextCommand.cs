using System;
using BundtCord.Discord;

namespace BundtBot
{
    public class TextCommand
    {
        public readonly int MinimumArgCount;
        
        public string Name;
        public Action<TextChannelMessage, ReceivedCommand> Command;

        public TextCommand(string name, Action<TextChannelMessage, ReceivedCommand> command, int minimumArgCount = 0)
        {
            Name = name;
            Command = command;
            MinimumArgCount = minimumArgCount;
        }
    }
}