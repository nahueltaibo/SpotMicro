using Iot.Device.Pwm;

namespace Spot.Drivers
{
    public class ServoDriver : IServo
    {
        private Pca9685 pca9685;
        private int v;

        public ServoDriver(Pca9685 pca9685, int v)
        {
            this.pca9685 = pca9685;
            this.v = v;
        }
    }
}
