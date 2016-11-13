using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading;

namespace Discord.Addons.InteractiveCommands
{
    public class InteractiveModuleBase : ModuleBase
    {
        /// <summary>
        /// Waits for the user to send a message.
        /// </summary>
        /// <param name="user">Which user to wait for a message from.</param>
        /// <param name="channel">Which channel the message should be sent in. (If null, will accept a response from any channel).</param>
        /// <param name="timeout">How long to wait for a message before timing out. This value will default to 15 seconds.</param>
        /// <param name="preconditions">Any preconditions to run to determine if a response is valid.</param>
        /// <returns>The response.</returns>
        /// <remarks>When you use this in a command, the command's RunMode MUST be set to 'async'. Otherwise, the gateway thread will be blocked, and this will never return.</remarks>
        /// <exception cref="NotSupportedException">This addon must be ran with a DiscordSocketClient.</exception>
        public async Task<IUserMessage> WaitForMessage(IUser user, IMessageChannel channel = null, TimeSpan? timeout = null, params ResponsePrecondition[] preconditions)
        {
            var client = Context.Client as DiscordSocketClient;
            if (client == null) throw new NotSupportedException("This addon must be ran with a DiscordSocketClient.");
            if (timeout == null) timeout = TimeSpan.FromSeconds(15);

            var block = new SemaphoreSlim(0, 1);
            IUserMessage response = null;

            Func<IMessage, Task> isValid = async (messageParameter) =>
            {
                var message = messageParameter as IUserMessage;
                if (message == null) return;
                if (message.Author.Id != user.Id) return;
                if (channel != null && message.Channel != channel) return;

                var context = new ResponseContext(client, message);

                foreach (var precondition in preconditions)
                {
                    var result = await precondition.CheckPermissions(context);
                    if (!result.IsSuccess) return;
                }

                response = message;
                block.Release();
            };

            client.MessageReceived += isValid;
            await block.WaitAsync();
            client.MessageReceived -= isValid;

            return response;
        }
    }
}
