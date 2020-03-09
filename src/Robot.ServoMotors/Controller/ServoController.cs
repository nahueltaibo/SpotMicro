using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Robot.MessageBus;
using Robot.Messages;
using Robot.ServoMotors;
using System;
using System.Collections.Generic;
using System.Linq;
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
            foreach (var servo in _servos)
            {
                try
                {
                    servo.Dispose();
                }
                catch (Exception ex)
                {
                    _log.LogError($"Error disposing servo with ID {servo.Id}", ex);
                }

                _log.LogDebug($"{nameof(ServoController)} Stopped");
            }
            await Task.CompletedTask;
        }

        private void OnServoChanged(IMessage message)
        {
            try
            {
                var servoMessage = (ServoMessage)message;
                _log.LogDebug($"ServoMessage: id={servoMessage.Id}; pulse={servoMessage.PulseWidth}; angle={servoMessage.Angle}");

                var servo = _servos.FirstOrDefault(s => s.Id == servoMessage.Id);

                if (servo == null)
                {
                    _log.LogError($"Invalid servo id ({servoMessage.Id})");
                    return;
                }

                if (servoMessage.PulseWidth.HasValue)
                {
                    servo.SetPulseWidth(servoMessage.PulseWidth.Value);
                }
                else if (servoMessage.Angle.HasValue)
                {
                    servo.SetAngle(servoMessage.Angle.Value);
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Error processing ServoMessage {JsonConvert.SerializeObject(message)}", ex);
            }
        }
    }
}
