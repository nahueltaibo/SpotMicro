using Microsoft.Extensions.Logging;
using Robot.MessageBus;
using Robot.MessageBus.Messages;
using Robot.Messages;
using Robot.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Spot.Reactive
{
    public class RemoteControlServoTester : IRemoteControlServoTester
    {
        private readonly IMessageBroker _messageBroker;
        private readonly ILogger<RemoteControlServoTester> _logger;

        public RemoteControlServoTester(IMessageBroker messageBroker, ILogger<RemoteControlServoTester> logger)
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

            if (rcMessage.Throttle.HasValue)
            {
                _messageBroker.Publish(new ServoMessage
                {
                    Id = 0,
                    PulseWidth = (int)ValueMapper.Map(rcMessage.Throttle.Value, -1, 1, 500, 2500)
                });
            }
        }
    }
}
