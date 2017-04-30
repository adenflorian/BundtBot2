using System;
using System.Threading.Tasks;
using BundtCord.Discord;

namespace BundtBot
{
    public class TextCommand
    {
        public readonly string Name;
        public readonly Func<TextChannelMessage, ReceivedCommand, Task> Command;
        public readonly int MinimumArgCount;
        
        public TextCommand(string name, Func<TextChannelMessage, ReceivedCommand, Task> command, int minimumArgCount = 0)
        {
            Name = name;
            Command = command;
            MinimumArgCount = minimumArgCount;
        }
    }
}