using Klipper_Calibration_Tool.Classes.DataStructures;

namespace Klipper_Calibration_Tool.Classes.UI
{
    public class BindPoint : ViewModelBase
    {
        private double _x;
        private double _y;
        private double _z;

        public double X
        {
            get => _x;
            set => SetAndNotify(ref _x, value);
        }

        public double Y {
            get => _y;
            set => SetAndNotify(ref _y, value);
        }

        public double Z {
            get => _z;
            set => SetAndNotify(ref _z, value);
        }
    }
}
