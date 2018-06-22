using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Klipper_Calibration_Tool.Classes;
using Klipper_Calibration_Tool.Classes.DataStructures;
using Klipper_Calibration_Tool.Classes.Escher;
using Klipper_Calibration_Tool.Classes.UI;
using Klipper_Calibration_Tool.Properties;
using Renci.SshNet;

namespace Klipper_Calibration_Tool
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private int _currentpoint;

        private const string Decimalformat = "F8";

        #region Properties
        //backing values
        private double _currentRadius;
        private double _currentRod;
        private double _currentEndStopX;
        private double _currentEndStopY;
        private double _currentEndStopZ;
        private double _currentAngleX;
        private double _currentAngleY;
        private double _currentAngleZ;

        public double CurrentRadius {
            get => _currentRadius;
            set => SetAndNotify(ref _currentRadius, value);
        }

        public double CurrentRod {
            get => _currentRod;
            set => SetAndNotify(ref _currentRod, value);
        }

        public double CurrentEndStopX {
            get => _currentEndStopX;
            set => SetAndNotify(ref _currentEndStopX, value);
        }

        public double CurrentEndStopY {
            get => _currentEndStopY;
            set => SetAndNotify(ref _currentEndStopY, value);
        }

        public double CurrentEndStopZ {
            get => _currentEndStopZ;
            set => SetAndNotify(ref _currentEndStopZ, value);
        }

        public double CurrentAngleX {
            get => _currentAngleX;
            set => SetAndNotify(ref _currentAngleX, value);
        }

        public double CurrentAngleY {
            get => _currentAngleY;
            set => SetAndNotify(ref _currentAngleY, value);
        }

        public double CurrentAngleZ {
            get => _currentAngleZ;
            set => SetAndNotify(ref _currentAngleZ, value);
        }

        public double ZProbeHeight {
            get => Settings.Default.ProbeZ;
            set {
                Settings.Default.ProbeZ = value;
                Settings.Default.Save();
            }
        }

        public double MoveHeight {
            get => Settings.Default.MoveHeight;
            set {
                Settings.Default.MoveHeight = value;
                Settings.Default.Save();
            }
        }

        public double ZProbeMoveHeight => ZProbeHeight + MoveHeight;

        public double PrintRadius {
            get => Settings.Default.PrintRadius;
            set {
                Settings.Default.PrintRadius = value;
                Settings.Default.Save();
            }
        }

        public BindPoint[] Point { get; } = {
            new BindPoint(),
            new BindPoint(),
            new BindPoint(),
            new BindPoint(),
            new BindPoint(),
            new BindPoint(),
            new BindPoint()
        };



        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            PseudoSerial.Init();
        }

        private void PseudoSerial_MessageReceived(string message)
        {
            if (!IsPositionMessage(message) || _currentpoint == -1) return;

            double z = ParseZ(message);
            Point[_currentpoint].Z = z;

            _currentpoint = -1;
        }

        private static bool IsPositionMessage(string message)
        {
            return message.Split(' ').Length == 8 && !message.Contains("!!");
        }

        private double ParseZ(string message)
        {
            string[] coords = message.Split(' ');
            string[] zblock = coords[2].Split(':');
            return double.Parse(zblock[1]);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double height = CurrentEndStopX;
            if (CurrentEndStopY < height)
                height = CurrentEndStopY;
            if (CurrentEndStopZ < height)
                height = CurrentEndStopZ;
            double offHx = CurrentEndStopX - height;
            double offHy = CurrentEndStopY - height;
            double offHz = CurrentEndStopZ - height;

            Debug.WriteLine($"Initial height:{height}");
            Debug.WriteLine($"Initial offsets: X:{offHx}, Y:{offHy}, Z:{offHz}");

            double offAx = CurrentAngleX - 210;
            double offAy = CurrentAngleY - 330;
            double offAz = CurrentAngleZ - 90;

            DParameters deltaP = new DParameters(CurrentRod, CurrentRadius, height, offHx, offHy, offHz, offAx, offAy, offAz);

            Parameters deltaT = new Parameters(DParameters.FirmwareType.Klipper, 7, 6, true)
            {
                XBedProbePoints = Point.Select(value => value.X).ToArray(),
                YBedProbePoints = Point.Select(value => value.Y).ToArray(),
                ZBedProbePoints = Point.Select(value => value.Z - ZProbeHeight).ToArray()
            }; //valid params 3 4 6 7


            CalibrationResult cr = Escher3D.Calc(ref deltaP, ref deltaT);
            Labelresult.Content = cr.ToString();

            double newheight = deltaP.HomedHeight;

            double newXStop = newheight + deltaP.Xstop;
            double newYStop = newheight + deltaP.Ystop;
            double newZStop = newheight + deltaP.Zstop;

            Debug.WriteLine("Calculated height:{0}", newheight);
            Debug.WriteLine($"Initial offsets: X:{offHx}, Y:{offHy}, Z:{offHz}");

            TextBoxCalculatedRadius.Text = deltaP.Radius.ToString(Decimalformat);
            TextBoxCalculatedRod.Text = deltaP.Diagonal.ToString(Decimalformat);

            TextBoxCalculatedEndX.Text = newXStop.ToString(Decimalformat);
            TextBoxCalculatedEndY.Text = newYStop.ToString(Decimalformat);
            TextBoxCalculatedEndZ.Text = newZStop.ToString(Decimalformat);

            TextBoxCalculatedAngleX.Text = (210 + deltaP.Xadj).ToString(Decimalformat);
            TextBoxCalculatedAngleY.Text = (330 + deltaP.Yadj).ToString(Decimalformat);
            TextBoxCalculatedAngleZ.Text = (90 + deltaP.Zadj).ToString(Decimalformat);

            ButtonApply.IsEnabled = true;
        }

        private void Apply()
        {
            string config = File.ReadAllText("printer.cfg.template");
            config = config.Replace("<TEMPLATE_RADIUS>", TextBoxCalculatedRadius.Text);
            config = config.Replace("<TEMPLATE_ROD>", TextBoxCalculatedRod.Text);
            config = config.Replace("<TEMPLATE_XEND>", TextBoxCalculatedEndX.Text);
            config = config.Replace("<TEMPLATE_YEND>", TextBoxCalculatedEndY.Text);
            config = config.Replace("<TEMPLATE_ZEND>", TextBoxCalculatedEndZ.Text);
            config = config.Replace("<TEMPLATE_XANGLE>", TextBoxCalculatedAngleX.Text);
            config = config.Replace("<TEMPLATE_YANGLE>", TextBoxCalculatedAngleY.Text);
            config = config.Replace("<TEMPLATE_ZANGLE>", TextBoxCalculatedAngleZ.Text);

            Stream configStream = GenerateStreamFromString(config);

            ScpClient client = new ScpClient("192.168.76.239", "pi", "raspberry");
            client.Connect();
            client.Upload(configStream, "/home/pi/printer.cfg");
            client.Disconnect();
            client.Dispose();
            configStream.Dispose();
            PseudoSerial.WriteLine("restart", 1);
        }

        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            Apply();

            Clear();

            ButtonApply.IsEnabled = false;
            ButtonHome.IsEnabled = false;
            Task.Delay(10000).ContinueWith(t =>
            {
                ButtonHome.Dispatcher.Invoke(() =>
                {
                    ButtonHome.IsEnabled = true;
                });
            });
        }

        private void Clear()
        {
            TextBoxCalculatedRadius.Text = "";
            TextBoxCalculatedRod.Text = "";
            TextBoxCalculatedEndX.Text = "";
            TextBoxCalculatedEndY.Text = "";
            TextBoxCalculatedEndZ.Text = "";
            TextBoxCalculatedAngleX.Text = "";
            TextBoxCalculatedAngleY.Text = "";
            TextBoxCalculatedAngleZ.Text = "";

            foreach (BindPoint p in Point)
            {
                p.Z = 0;
            }
        }

        private void ButtonPoint1_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 0;
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
        }

        private void ButtonPoint2_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 1;
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
        }

        private void ButtonPoint3_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 2;
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
        }

        private void ButtonPoint4_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 3;
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
        }

        private void ButtonPoint5_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 4;
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
        }

        private void ButtonPoint6_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 5;
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
        }
        private void ButtonPoint7_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 6;
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(Point[_currentpoint].X, Point[_currentpoint].Y, ZProbeMoveHeight);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PseudoSerial.WriteLine("g28", 1);
        }

        private void SendMove(double x, double y, double z, double f = 8000)
        {
            PseudoSerial.WriteLine($"G1 X{x} Y{y} Z{z} F{f}", 1);
        }

        private void TextBoxDeltaRadius_TextChanged(object sender, TextChangedEventArgs e)
        {
            double printradius = CurrentRadius;
            double[,] points = Parameters.CalcPoints(printradius, 6);
            for (int i = 0; i < 6; i++)
            {
                Point[i].X = points[i, 0];
                Point[i].Y = points[i, 1];
            }

            Point[6].X = 0;
            Point[6].Y = 0;

            Clear();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PseudoSerial.MessageReceived += PseudoSerial_MessageReceived;
            Clear();
        }
    }
}
