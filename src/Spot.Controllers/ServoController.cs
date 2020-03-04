using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Robot.MessageBus;
using Microsoft.Extensions.Logging;
using Spot.Drivers;

namespace Spot.Controllers
{
    public class ServoController : IServoController
    {
        private List<IServo> _servos;
        private IMessageBroker _messageBroker;
        private ILogger<ServoController> _log;

        public ServoController(List<IServo> list, IMessageBroker messageBroker, ILogger<ServoController> logger)
        {
            _servos = list;
            _messageBroker = messageBroker;
            _log = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
