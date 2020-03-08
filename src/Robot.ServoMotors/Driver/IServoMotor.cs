namespace Robot.ServoMotors
{ 
    public interface IServoMotor
    {
        /// <summary>
        /// Length of the pulse in microseconds
        /// </summary>
        int PulseWidgh { set; }

        /// <summary>
        /// Angle in Radians of the servo
        /// </summary>
        double Angle { set; }
    }
}