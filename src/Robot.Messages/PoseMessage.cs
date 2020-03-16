using Robot.MessageBus;

namespace Robot.Messages
{
    [Topic("motion/pose")]
    public class PoseMessage : IMessage
    {
        public string Name { get; set; }
    }
}
