namespace Robot.ServoMotors
{
    public interface IServoMotor
    {
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