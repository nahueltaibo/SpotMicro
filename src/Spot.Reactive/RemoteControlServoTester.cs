using Microsoft.Extensions.Logging;
using Robot.MessageBus;
using Robot.MessageBus.Messages;
using Robot.Messages;
using Robot.Model.RemoteControl;
using System;
using System.Linq;
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

            if (rcMessage.Key == (int)RemoteControlKey.X && rcMessage.Value == 1)
            {
                foreach(var servoId in Enumerable.Range(1, 16))
                {
                    MoveServoToAngle(servoId, 0);
                }
            }
            else if (rcMessage.Key == (int)RemoteControlKey.A && rcMessage.Value == 1)
            {
                foreach (var servoId in Enumerable.Range(1, 16))
                {
                    MoveServoToAngle(servoId, -Math.PI / 8);
                }
            }
            else if (rcMessage.Key == (int)RemoteControlKey.B && rcMessage.Value == 1)
            {
                foreach (var servoId in Enumerable.Range(1, 16))
                {
                    MoveServoToAngle(servoId, Math.PI / 8);
                }
            }
        }

        private void MoveServoToAngle(int servoId, double radians)
        {
            _messageBroker.Publish(new ServoMessage
            {
                Id = servoId,
                Angle = radians
            });
        }
    }
}
