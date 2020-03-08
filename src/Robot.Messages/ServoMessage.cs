using Robot.MessageBus;

namespace Robot.Messages
{
    [Topic("motion/servo")]
    public class ServoMessage : IMessage
    {
        /// <summary>
        /// Servo Id that the message is about
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Pulse width in microseconds
        /// </summary>
        public int? PulseWidth { get; set; }
        /// <summary>
        /// Angle in Radians
        /// </summary>
        public double? Angle { get; set; }
    }
}
