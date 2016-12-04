using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Addons.InteractiveCommands
{
    public class InteractiveService
    {
        private readonly DiscordSocketClient _client;

        public InteractiveService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task<IUserMessage> WaitForMessage(IUser user, IMessageChannel channel = null, TimeSpan? timeout = null, params ResponsePrecondition[] preconditions)
        {
            if (timeout == null) timeout = TimeSpan.FromSeconds(15);

            var blockToken = new CancellationTokenSource();
            IUserMessage response = null;

            Func<IMessage, Task> isValid = async (messageParameter) =>
            {
                var message = messageParameter as IUserMessage;
                if (message == null) return;
                if (message.Author.Id != user.Id) return;
                if (channel != null && message.Channel != channel) return;

                var context = new ResponseContext(_client, message);

                foreach (var precondition in preconditions)
                {
                    var result = await precondition.CheckPermissions(context);
                    if (!result.IsSuccess) return;
                }

                response = message;
                blockToken.Cancel(true);
            };

            _client.MessageReceived += isValid;
            try
            {
                await Task.Delay(timeout.Value, blockToken.Token);
            }
            catch (TaskCanceledException)
            {
                return response;
            }
            finally
            {
                _client.MessageReceived -= isValid;
            }
            return null; // this should never happen
        }
    }
}
