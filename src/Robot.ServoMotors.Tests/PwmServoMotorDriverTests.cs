using Iot.Device.Pwm;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Device.Pwm;

namespace Robot.ServoMotors.Tests
{
    [TestClass]
    public class PwmServoMotorDriverTests
    {

        [TestMethod]
        public void ServoCorrectlyCalculatesDutyCycle()
        {
            var pca9685Mock = new Mock<Pca9685>();
            var loggerMock = new Mock<ILogger<PwmServoMotorDriver>>();
            var pwmChannel = new Mock<PwmChannel>();
            pwmChannel.SetupGet(m => m.Frequency).Returns(1000);

            var settings = new PwmServoMotorDriverSettings
            {
                ServoId = 1,
                MaximumAngle = 2 * Math.PI,
                MinimumPulseWidthMicroseconds = 0,
                ZeroPulseWidthMicroseconds = 500,
                MaximumPulseWidthMicroseconds = 1000,
            };

            var driver = new PwmServoMotorDriver(pwmChannel.Object, settings, loggerMock.Object);

            // Set the servo to its zero radians position
            driver.SetAngle(0);
            pwmChannel.VerifySet(m => m.DutyCycle = 0.5);

            // Set the servo to its minimum angle
            driver.SetAngle(-Math.PI);
            pwmChannel.VerifySet(m => m.DutyCycle = 0);

            // Set the servo to its maximum angle
            driver.SetAngle(Math.PI);
            pwmChannel.VerifySet(m => m.DutyCycle = 1);
        }
    }
}
