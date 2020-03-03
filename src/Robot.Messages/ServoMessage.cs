using MessageBus;

namespace Robot.Messages
{
    [Topic("motion/servo")]
    public class ServoMessage : IMessage
    {
        public int Id { get; set; }
        public int? PulseWidth { get; set; }
        public double? Angle { get; set; }
    }
}
