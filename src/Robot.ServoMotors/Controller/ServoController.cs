using Microsoft.Extensions.Logging;
using Robot.MessageBus;
using Robot.Messages;
using Robot.ServoMotors;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Spot.Controllers
{
    public class ServoController : IServoController
    {
        private readonly IList<IServoMotor> _servos;
        private readonly IMessageBroker _messageBroker;
        private readonly ILogger<ServoController> _log;

        public ServoController(IList<IServoMotor> servos, IMessageBroker messageBroker, ILogger<ServoController> logger)
        {
            _servos = servos;
            _messageBroker = messageBroker;
            _log = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _messageBroker.Subscribe<ServoMessage>(OnServoChanged);

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private void OnServoChanged(IMessage message)
        {
            var servoMessage = (ServoMessage)message;
            _log.LogDebug($"ServoMessage: id={servoMessage.Id}; pulse={servoMessage.PulseWidth}; angle={servoMessage.Angle}");

            if (servoMessage.Id < 0 || servoMessage.Id > _servos.Count - 1)
            {
                _log.LogError($"Invalid servo id ({servoMessage.Id})");
                return;
            }

            var servo = _servos[servoMessage.Id];

            if (servoMessage.PulseWidth.HasValue)
            {
                servo.PulseWidgh = servoMessage.PulseWidth.Value;
            }
            else if (servoMessage.Angle.HasValue)
            {
                servo.Angle = servoMessage.Angle.Value;
            }
        }
    }
}
