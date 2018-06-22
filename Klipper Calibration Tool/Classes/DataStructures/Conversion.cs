namespace Klipper_Calibration_Tool.Classes.DataStructures
{
    class Conversion
    {
        public static double Scale(double value, double frommin, double frommax, double tomin, double tomax)
        {
            double fromrange = frommax - frommin;
            double torange = tomax - tomin;

            return (value - frommin) * torange / fromrange + tomin;
        }
    }
}
