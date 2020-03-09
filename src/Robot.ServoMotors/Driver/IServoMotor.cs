using System;

namespace Robot.ServoMotors
{
    public interface IServoMotor : IDisposable
    {
        /// <summary>
        /// The unique ID of the servo
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Length of the pulse in microseconds
        /// </summary>
        void SetPulseWidth(int microseconds);

        /// <summary>
        /// Angle in Radians of the servo
        /// </summary>
        void SetAngle(double radians);
    }
}