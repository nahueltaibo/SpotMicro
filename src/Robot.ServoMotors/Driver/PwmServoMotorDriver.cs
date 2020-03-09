using Iot.Device.Pwm;
using Microsoft.Extensions.Logging;
using System;
using System.Device.Pwm;

namespace Robot.ServoMotors
{
    public class PwmServoMotorDriver : IServoMotor
    {
        private readonly PwmServoMotorDriverSettings _settings;
        private readonly ILogger<PwmServoMotorDriver> _logger;
        private readonly PwmChannel _pwmChannel;
        private double _radiansToMicroseconds;
        private int _minimumRangeLimit;
        private int _maximumRangeLimit;
        private bool _isServoStarted;

        public int Id => _settings.ServoId;

        public PwmServoMotorDriver(Pca9685 pca9685, int channelId, PwmServoMotorDriverSettings settings, ILogger<PwmServoMotorDriver> logger)
        : this(pca9685.CreatePwmChannel(channelId),
              settings,
              logger)
        {
        }

        public PwmServoMotorDriver(PwmChannel pwmChannel, PwmServoMotorDriverSettings settings, ILogger<PwmServoMotorDriver> logger)
        {
            if (settings.MaximumAngle > 2 * Math.PI)
            {
                throw new ArgumentException(nameof(settings.MaximumAngle), "Maximum angle cant be higher than a full turn (2*PI)");
            }

            if (settings.MaximumAngleLimit - settings.MinimumAngleLimit <= 0)
            {
                throw new ArgumentException($"Configured servo limits wont allow the servo to move. (min:{settings.MinimumAngleLimit}, max:{settings.MaximumAngle})");
            }

            if (settings.MaximumAngleLimit - settings.MinimumAngleLimit > settings.MaximumAngle)
            {
                throw new ArgumentException("Servo limits can go beyond servo's Maximum angle");
            }

            _pwmChannel = pwmChannel;
            _settings = settings;
            _logger = logger;

            Calibrate(settings.MaximumAngle, settings.MinimumPulseWidthMicroseconds, settings.MaximumPulseWidthMicroseconds, settings.ZeroPulseWidthMicroseconds);
        }

        public void SetAngle(double radians)
        {
            SetPulseWidth(RadiansToMicroseconds(radians));
        }

        public void SetPulseWidth(int microseconds)
        {
            // Clamp the value to be sure we are not trying to push it outside of the servo limits
            microseconds = Math.Clamp(microseconds, _minimumRangeLimit, _maximumRangeLimit);

            var dutyCycle = MicrosecondsToDutyCycle(microseconds);

            _pwmChannel.DutyCycle = dutyCycle;
            if (!_isServoStarted)
            {
                _pwmChannel.Start();
                _isServoStarted = true;
                _logger.LogInformation($"Servo {_settings.ServoId} started");
            }
        }

        private void Calibrate(double maximumAngle, int pulseWidthAtAngleMinimum, int pulseWidthAtAngleMaximum, int zeroPulseWidthMicroseconds)
        {
            _radiansToMicroseconds = ((pulseWidthAtAngleMaximum - pulseWidthAtAngleMinimum) / maximumAngle);

            _minimumRangeLimit = RadiansToMicroseconds(_settings.MinimumAngleLimit);
            _maximumRangeLimit = RadiansToMicroseconds(_settings.MaximumAngleLimit);
        }

        private int RadiansToMicroseconds(double radians)
        {
            return (int)(_radiansToMicroseconds * radians) + _settings.ZeroPulseWidthMicroseconds;
        }

        /// <summary>
        /// Converts from microseconds to the duty cycle (between 0 and 1), considering the frequency of the PWM channel
        /// </summary>
        /// <returns>The duty cycle for the current PWM channel.</returns>
        private double MicrosecondsToDutyCycle(int microseconds)
        {
            double dutyCycle = (double)microseconds / 1_000_000 * _pwmChannel.Frequency;

            return dutyCycle;
        }

        ~PwmServoMotorDriver()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pwmChannel.Stop();
                _logger.LogInformation($"Servo {_settings.ServoId} stopped");
            }
        }
    }
}
