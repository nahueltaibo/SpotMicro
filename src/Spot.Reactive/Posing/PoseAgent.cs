using Microsoft.Extensions.Logging;
using Robot.MessageBus;
using Robot.MessageBus.Messages;
using Robot.Messages;
using Robot.Model.RemoteControl;
using Spot.Model.Posing;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spot.Reactive.Posing
{
    public class PoseAgent : IPoseAgent
    {
        private readonly IMessageBroker _messageBroker;
        private readonly ILogger<PoseAgent> _logger;

        public PoseAgent(IMessageBroker messageBroker, ILogger<PoseAgent> logger)
        {
            _messageBroker = messageBroker;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _messageBroker.Subscribe<RemoteControlMessage>(OnRemoteControlMessageReceived);

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private void OnRemoteControlMessageReceived(IMessage message)
        {
            var rcMessage = (RemoteControlMessage)message;

            if (rcMessage.Key == (int)RemoteControlKey.X && rcMessage.Value == 1)
            {
                _messageBroker.Publish(new PoseMessage
                {
                    Name = PoseNames.Sit
                });
            }
            else if (rcMessage.Key == (int)RemoteControlKey.A && rcMessage.Value == 1)
            {
                _messageBroker.Publish(new PoseMessage
                {
                    Name = PoseNames.Stand
                });
            }
            else if (rcMessage.Key == (int)RemoteControlKey.B && rcMessage.Value == 1)
            {
                _messageBroker.Publish(new PoseMessage
                {
                    Name = PoseNames.Sleep
                });
            }
        }
    }
}
