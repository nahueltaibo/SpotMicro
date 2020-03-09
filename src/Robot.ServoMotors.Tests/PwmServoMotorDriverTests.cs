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
        [DataTestMethod]
        [DataRow(1, 0, 500_000, 1_000_000)]
        [DataRow(1000, 0, 500, 1000)]
        public void ServoCalculatesDutyCycleCorrectly(int frequency, int minPulseWidth, int zeroPulseWidth, int maxPulseWidth)
        {
            var pca9685Mock = new Mock<Pca9685>();
            var loggerMock = new Mock<ILogger<PwmServoMotorDriver>>();
            var pwmChannel = new Mock<PwmChannel>();
            pwmChannel.SetupGet(m => m.Frequency).Returns(frequency);

            var settings = new PwmServoMotorDriverSettings
            {
                ServoId = 1,
                MaximumAngle = 2 * Math.PI,
                MinimumPulseWidthMicroseconds = minPulseWidth,
                ZeroPulseWidthMicroseconds = zeroPulseWidth,
                MaximumPulseWidthMicroseconds = maxPulseWidth,
                MinimumAngleLimit = -Math.PI,
                MaximumAngleLimit = Math.PI
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

        [TestMethod]
        public void ServoLimitsOutputAccordingToSettings()
        {
            var pca9685Mock = new Mock<Pca9685>();
            var loggerMock = new Mock<ILogger<PwmServoMotorDriver>>();
            var pwmChannel = new Mock<PwmChannel>();
            pwmChannel.SetupGet(m => m.Frequency).Returns(1); // Frequency set to 1, so we a cycle every 1 000 000 microseconds

            var settings = new PwmServoMotorDriverSettings
            {
                ServoId = 1,
                MaximumAngle = 2 * Math.PI,
                MinimumPulseWidthMicroseconds = 0, //Start position is at zero
                ZeroPulseWidthMicroseconds = 500_000, // Center position is at half second
                MaximumPulseWidthMicroseconds = 1_000_000, //Full position is at a full second
                MinimumAngleLimit = -Math.PI / 2,    // Servo supports 2*PI, but we limit it to half of that
                MaximumAngleLimit = Math.PI / 2     // Servo supports 2*PI, but we limit it to half of that
            };

            var driver = new PwmServoMotorDriver(pwmChannel.Object, settings, loggerMock.Object);

            // Set the servo to its zero radians position
            driver.SetAngle(0);
            pwmChannel.VerifySet(m => m.DutyCycle = 0.5);

            // Set the servo to its minimum allowed angle
            driver.SetAngle(Math.PI / 2);
            // Ensure the servo is were it should when we give the maximum allowed angle
            pwmChannel.VerifySet(m => m.DutyCycle = 0.75);

            // Set the servo to its maximum allowed angle
            driver.SetAngle(Math.PI);
            // Servo should respect the maximum limit
            pwmChannel.VerifySet(m => m.DutyCycle = 0.75);

            // Set the servo to its minimum allowed angle
            driver.SetAngle(-Math.PI/2);
            // Ensure the servo is were it should when we give the maximum allowed angle
            pwmChannel.VerifySet(m => m.DutyCycle = 0.25);

            // Set the servo to its maximum allowed angle
            driver.SetAngle(-Math.PI);
            // Servo should respect the minimum limit
            pwmChannel.VerifySet(m => m.DutyCycle = 0.25);
        }
    }
}
