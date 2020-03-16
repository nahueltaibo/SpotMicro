using Robot.MessageBus;
using System.Collections.Generic;

namespace Robot.Messages
{
    [Topic("motion/servos")]
    public class MultiServoMessage : IMessage
    {
        /// <summary>
        /// Servo Messages Collection
        /// </summary>
        public IList<ServoMessage> ServoMessages { get; set; }
    }
}
