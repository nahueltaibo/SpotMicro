namespace Spot.Drivers.Servos
{ 
    public interface IServo
    {
        int PulseWidgh { set; }

        double Angle { set; }
    }
}