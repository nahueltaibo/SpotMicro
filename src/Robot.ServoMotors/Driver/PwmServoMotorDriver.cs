using Iot.Device.Pwm;
using Iot.Device.ServoMotor;
using Microsoft.Extensions.Logging;
using System;

namespace Robot.ServoMotors
{
    public class PwmServoMotorDriver : IServoMotor
    {
        private readonly Pca9685 _pca9685;
        private readonly PwmServoMotorDriverSettings _settings;
        private readonly ILogger<PwmServoMotorDriver> _logger;
        private readonly ServoMotor _servo;
        private double _radiansToMicroseconds;
        private int _minimumRangeLimit;
        private int _maximumRangeLimit;

        public PwmServoMotorDriver(Pca9685 pca9685, PwmServoMotorDriverSettings settings, ILogger<PwmServoMotorDriver> logger)
        {
            _pca9685 = pca9685;
            _settings = settings;
            _logger = logger;

            Calibrate(settings.MaximumAngle, settings.MinimumPulseWidthMicroseconds, settings.MaximumPulseWidthMicroseconds);

            var pwmChannel = _pca9685.CreatePwmChannel(_settings.ChannelId);

            _servo = new ServoMotor(
                pwmChannel,
                settings.MaximumAngle,
                settings.MinimumPulseWidthMicroseconds,
                settings.MaximumPulseWidthMicroseconds);
        }

        private void Calibrate(double maximumAngle, int pulseWidthAtAngleZero, int pulseWidthAtAngleMaximum)
        {
            _radiansToMicroseconds = (pulseWidthAtAngleMaximum - pulseWidthAtAngleZero) / maximumAngle;

            _minimumRangeLimit = RadiansToMicroseconds(_settings.MinimumAngleLimit);
            _maximumRangeLimit = RadiansToMicroseconds(_settings.MaximumAngleLimit);
        }

        private int RadiansToMicroseconds(double radians)
        {
            return (int)(_radiansToMicroseconds * radians);
        }

        public int PulseWidgh
        {
            set
            {
                // Clamping the value to be sure we dont go passing the constraints settings
                _servo.WritePulseWidth(Math.Clamp(value, _minimumRangeLimit, _maximumRangeLimit));
            }
        }

        public double Angle
        {
            set => PulseWidgh = RadiansToMicroseconds(value);
        }
    }
}
