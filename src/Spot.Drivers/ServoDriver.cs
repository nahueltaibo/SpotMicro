using Iot.Device.Pwm;
using Iot.Device.ServoMotor;

namespace Spot.Drivers.Servos
{
    public class ServoDriver : IServo
    {
        private readonly Pca9685 _pca9685;
        private readonly ServoMotor _servo;

        public ServoDriver(Pca9685 pca9685, int channel)
        {
            _pca9685 = pca9685;

            var pwmChannel = _pca9685.CreatePwmChannel(channel);

            _servo = new ServoMotor(pwmChannel, 180, 500, 2500);
        }

        public int PulseWidgh
        {
            set => _servo.WritePulseWidth(value);
        }

        public double Angle
        {
            set => _servo.WriteAngle(value);
        }
    }
}
