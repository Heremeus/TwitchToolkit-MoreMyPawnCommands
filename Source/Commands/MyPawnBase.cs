using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchToolkit;
using TwitchToolkit.IRC;
using TwitchToolkit.PawnQueue;
using Verse;

namespace TwitchToolkitMoreMyPawnCommands.Commands
{
    public class MyPawnBase : CommandDriver
    {
        protected const int TWITCH_MSG_MAX_SIZE = 500;

        protected static Pawn GetPawnIfAllowed(IRCMessage message)
        {
            Viewer viewer = Viewers.GetViewer(message.User);
            GameComponentPawns component = Current.Game.GetComponent<GameComponentPawns>();

            if (!CommandsHandler.AllowCommand(message))
            {
                // No commands allowed in this channel
                return null;
            }

            Log.Message($"Parsing command {message.Message} from {message.User}");

            if (!component.HasUserBeenNamed(viewer.username))
            {
                Toolkit.client.SendMessage($"@{viewer.username} you are not in the colony.", CommandsHandler.SendToChatroom(message));
                return null;
            }

            return component.PawnAssignedToUser(viewer.username);
        }

        protected static void SendWrappedOutputText(string text, IRCMessage message)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            string[] originalLines = text.Split(new string[] { " " },
                StringSplitOptions.None);

            List<string> wrappedLines = new List<string>();

            StringBuilder actualLine = new StringBuilder();
            int actualLength = 0;

            foreach (var item in originalLines)
            {
                if (actualLength + item.Length + 1 > TWITCH_MSG_MAX_SIZE - 5)
                {
                    actualLine.Append("(...)");
                    wrappedLines.Add(actualLine.ToString());
                    actualLine = new StringBuilder();
                    actualLength = 0;
                }

                actualLine.Append(item + " ");
                actualLength += item.Length + 1;
            }

            if (actualLine.Length > 0)
                wrappedLines.Add(actualLine.ToString());
            
            if (wrappedLines.Count > 0)
            {
                foreach (string outputMessage in wrappedLines)
                {
                    Toolkit.client.SendMessage(outputMessage, CommandsHandler.SendToChatroom(message));
                }
            }
        }
    }
}
