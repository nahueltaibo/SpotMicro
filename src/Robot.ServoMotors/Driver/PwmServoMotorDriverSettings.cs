namespace Robot.ServoMotors
{
    public class PwmServoMotorDriverSettings
    {
        /// <summary>
        /// The Id used to identify and work with a specific servo
        /// </summary>
        public int ServoId { get; set; }

        /// <summary>
        /// The Channel Id in the PCA9685 the servo is connected to
        /// </summary>
        public int ChannelId { get; set; }

        /// <summary>
        /// The maximum angle in radians the servo motor can move represented as a value between 0 and 2*PI
        /// This is about the factory range of this servo, not your specific usage of it
        /// ie: The servo range is PI radians, or 2*PI radians
        /// </summary>
        public double MaximumAngle { get; set; }

        /// <summary>
        /// The minimum pulse width, in microseconds, that represent an angle for 0 degrees
        /// </summary>
        public int MinimumPulseWidthMicroseconds { get; set; }

        /// <summary>
        /// The pulse width, when the servo is at its zero Radiand angle
        /// Use this to calibrate the Sero of the servo
        /// </summary>
        public int ZeroPulseWidthMicroseconds { get; set; }

        /// <summary>
        /// The maximum pulse width, in microseconds, that represent an angle for maximum angle
        /// </summary>
        public int MaximumPulseWidthMicroseconds { get; set; }

        /// <summary>
        /// Limit the maximum angle a specific servo can reach
        /// Used for calibration to avoid destroying the hardware
        /// </summary>
        public double MinimumAngleLimit { get; set; }

        /// <summary>
        /// Limit the minimum angle a specific servo can reach.
        /// Used for calibration to avoid destroying the hardware
        /// </summary>
        public double MaximumAngleLimit { get; set; }
    }
}
