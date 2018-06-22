using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Klipper_Calibration_Tool.Classes;
using Klipper_Calibration_Tool.Classes.DataStructures;
using Klipper_Calibration_Tool.Classes.Escher;
using Klipper_Calibration_Tool.Properties;
using Renci.SshNet;

namespace Klipper_Calibration_Tool
{
    public partial class MainWindow
    {
        int _currentpoint;

        static string _decimalformat = "F8";

        public MainWindow()
        {
            InitializeComponent();
            PseudoSerial.Init();

        }

        private void LoadSettings()
        {
            LoadSetting(TextBoxInitialRadius, Settings.Default.Radius);
            LoadSetting(TextBoxInitialRod, Settings.Default.Rod);
            LoadSetting(TextBoxInitialXEnd, Settings.Default.EndX);
            LoadSetting(TextBoxInitialYEnd, Settings.Default.EndY);
            LoadSetting(TextBoxInitialZEnd, Settings.Default.EndZ);
            LoadSetting(TextBoxInitialXAngle, Settings.Default.AngleX);
            LoadSetting(TextBoxInitialYAngle, Settings.Default.AngleY);
            LoadSetting(TextBoxInitialZangle, Settings.Default.AngleZ);
            LoadSetting(Textboxzprobeheight, Settings.Default.ProbeZ);
            LoadSetting(TextBoxMoveHeight, Settings.Default.MoveHeight);
            LoadSetting(TextBoxDeltaRadius, Settings.Default.PrintRadius);
        }

        private void SaveSettings()
        {
            Settings.Default.Radius = TextBoxInitialRadius.Text;
            Settings.Default.Rod = TextBoxInitialRod.Text;
            Settings.Default.EndX = TextBoxInitialXEnd.Text;
            Settings.Default.EndY = TextBoxInitialYEnd.Text;
            Settings.Default.EndZ = TextBoxInitialZEnd.Text;
            Settings.Default.AngleX = TextBoxInitialXAngle.Text;
            Settings.Default.AngleY = TextBoxInitialYAngle.Text;
            Settings.Default.AngleZ = TextBoxInitialZangle.Text;
            Settings.Default.ProbeZ = Textboxzprobeheight.Text;
            Settings.Default.MoveHeight = TextBoxMoveHeight.Text;
            Settings.Default.PrintRadius = TextBoxDeltaRadius.Text;

            Settings.Default.Save();
        }

        private void LoadSetting(TextBox box, string value)
        {
            if (value.Length != 0)
                box.Text = value;
        }

        private void PseudoSerial_MessageReceived(string message)
        {
            if (IsPositionMessage(message) && _currentpoint != 0)
            {
                double z = ParseZ(message);
                switch (_currentpoint)
                {
                    case 1:
                        InvokeTextUpdate(TextBoxZPoint1, z.ToString("F3"));
                        break;
                    case 2:
                        InvokeTextUpdate(TextBoxZPoint2, z.ToString("F3"));
                        break;
                    case 3:
                        InvokeTextUpdate(TextBoxZPoint3, z.ToString("F3"));
                        break;
                    case 4:
                        InvokeTextUpdate(TextBoxZPoint4, z.ToString("F3"));
                        break;
                    case 5:
                        InvokeTextUpdate(TextBoxZPoint5, z.ToString("F3"));
                        break;
                    case 6:
                        InvokeTextUpdate(TextBoxZPoint6, z.ToString("F3"));
                        break;
                    case 7:
                        InvokeTextUpdate(TextBoxZPoint7, z.ToString("F3"));
                        break;
                }
                _currentpoint = 0;
            }
        }

        private void InvokeTextUpdate(TextBox target, string text)
        {
            target.Dispatcher.Invoke(() =>
            {
                target.Text = text;
            });
        }

        private bool IsPositionMessage(string message)
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
            double rod = double.Parse(TextBoxInitialRod.Text);
            double radius = double.Parse(TextBoxInitialRadius.Text);
            double hx = double.Parse(TextBoxInitialXEnd.Text);
            double hy = double.Parse(TextBoxInitialYEnd.Text);
            double hz = double.Parse(TextBoxInitialZEnd.Text);
            double height = hx;
            if (hy < height)
                height = hy;
            if (hz < height)
                height = hz;
            double offHx = hx - height;
            double offHy = hy - height;
            double offHz = hz - height;

            Debug.WriteLine("Initial height:{0}", height);
            Debug.WriteLine("Initial offsets: X:{0}, Y:{1}, Z:{2}", offHx, offHy, offHz);

            double offAx = double.Parse(TextBoxInitialXAngle.Text) - 210;
            double offAy = double.Parse(TextBoxInitialYAngle.Text) - 330;
            double offAz = double.Parse(TextBoxInitialZangle.Text) - 90;

            DParameters deltaP = new DParameters(rod, radius, height, offHx, offHy, offHz, offAx, offAy, offAz);

            Parameters deltaT = new Parameters(DParameters.FirmwareType.Klipper, 7, 6, true)
            {
                XBedProbePoints = new[]
            {
                double.Parse(TextBoxXPoint1.Text),
                double.Parse(TextBoxXPoint2.Text),
                double.Parse(TextBoxXPoint3.Text),
                double.Parse(TextBoxXPoint4.Text),
                double.Parse(TextBoxXPoint5.Text),
                double.Parse(TextBoxXPoint6.Text),
                double.Parse(TextBoxXPoint7.Text)
            },

                YBedProbePoints = new[]
            {
                double.Parse(TextBoxYPoint1.Text),
                double.Parse(TextBoxYPoint2.Text),
                double.Parse(TextBoxYPoint3.Text),
                double.Parse(TextBoxYPoint4.Text),
                double.Parse(TextBoxYPoint5.Text),
                double.Parse(TextBoxYPoint6.Text),
                double.Parse(TextBoxYPoint7.Text)
            }
            };//valid params 3 4 6 7


            double zeroheight = double.Parse(Textboxzprobeheight.Text);
            deltaT.ZBedProbePoints = new[]
            {
                double.Parse(TextBoxZPoint1.Text) - zeroheight,
                double.Parse(TextBoxZPoint2.Text) - zeroheight,
                double.Parse(TextBoxZPoint3.Text) - zeroheight,
                double.Parse(TextBoxZPoint4.Text) - zeroheight,
                double.Parse(TextBoxZPoint5.Text) - zeroheight,
                double.Parse(TextBoxZPoint6.Text) - zeroheight,
                double.Parse(TextBoxZPoint7.Text) - zeroheight
            };

            CalibrationResult cr = Escher3D.Calc(ref deltaP, ref deltaT);
            Labelresult.Content = cr.ToString();

            double newheight = deltaP.HomedHeight;

            double newXStop = newheight + deltaP.Xstop;
            double newYStop = newheight + deltaP.Ystop;
            double newZStop = newheight + deltaP.Zstop;

            Debug.WriteLine("Calculated height:{0}", newheight);
            Debug.WriteLine("Initial offsets: X:{0}, Y:{1}, Z:{2}", offHx, offHy, offHz);

            TextBoxCalculatedRadius.Text = deltaP.Radius.ToString(_decimalformat);
            TextBoxCalculatedRod.Text = deltaP.Diagonal.ToString(_decimalformat);

            TextBoxCalculatedEndX.Text = newXStop.ToString(_decimalformat);
            TextBoxCalculatedEndY.Text = newYStop.ToString(_decimalformat);
            TextBoxCalculatedEndZ.Text = newZStop.ToString(_decimalformat);

            TextBoxCalculatedAngleX.Text = (210 + deltaP.Xadj).ToString(_decimalformat);
            TextBoxCalculatedAngleY.Text = (330 + deltaP.Yadj).ToString(_decimalformat);
            TextBoxCalculatedAngleZ.Text = (90 + deltaP.Zadj).ToString(_decimalformat);

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

            TextBoxInitialRadius.Text = TextBoxCalculatedRadius.Text;

            TextBoxInitialRod.Text = TextBoxCalculatedRod.Text;

            TextBoxInitialXEnd.Text = TextBoxCalculatedEndX.Text;

            TextBoxInitialYEnd.Text = TextBoxCalculatedEndY.Text;

            TextBoxInitialZEnd.Text = TextBoxCalculatedEndZ.Text;

            TextBoxInitialXAngle.Text = TextBoxCalculatedAngleX.Text;

            TextBoxInitialYAngle.Text = TextBoxCalculatedAngleY.Text;

            TextBoxInitialZangle.Text = TextBoxCalculatedAngleZ.Text;

            Clear();

            SaveSettings();
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

            TextBoxZPoint1.Text = "";
            TextBoxZPoint2.Text = "";
            TextBoxZPoint3.Text = "";
            TextBoxZPoint4.Text = "";
            TextBoxZPoint5.Text = "";
            TextBoxZPoint6.Text = "";
            TextBoxZPoint7.Text = "";
        }

        private void ButtonPoint1_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 1;
            SendMove(
                double.Parse(TextBoxXPoint1.Text),
                double.Parse(TextBoxYPoint1.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(
                double.Parse(TextBoxXPoint1.Text),
                double.Parse(TextBoxYPoint1.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));
        }

        private void ButtonPoint2_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 2;
            SendMove(
                double.Parse(TextBoxXPoint2.Text),
                double.Parse(TextBoxYPoint2.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(
                double.Parse(TextBoxXPoint2.Text),
                double.Parse(TextBoxYPoint2.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));
        }

        private void ButtonPoint3_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 3;
            SendMove(
                double.Parse(TextBoxXPoint3.Text),
                double.Parse(TextBoxYPoint3.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(
                double.Parse(TextBoxXPoint3.Text),
                double.Parse(TextBoxYPoint3.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));
        }

        private void ButtonPoint4_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 4;
            SendMove(
                double.Parse(TextBoxXPoint4.Text),
                double.Parse(TextBoxYPoint4.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(
                double.Parse(TextBoxXPoint4.Text),
                double.Parse(TextBoxYPoint4.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));
        }

        private void ButtonPoint5_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 5;
            SendMove(
                double.Parse(TextBoxXPoint5.Text),
                double.Parse(TextBoxYPoint5.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(
                double.Parse(TextBoxXPoint5.Text),
                double.Parse(TextBoxYPoint5.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));

        }

        private void ButtonPoint6_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 6;
            SendMove(
                double.Parse(TextBoxXPoint6.Text),
                double.Parse(TextBoxYPoint6.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(
                double.Parse(TextBoxXPoint6.Text),
                double.Parse(TextBoxYPoint6.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));
        }
        private void ButtonPoint7_Click(object sender, RoutedEventArgs e)
        {
            _currentpoint = 7;
            SendMove(
                double.Parse(TextBoxXPoint7.Text),
                double.Parse(TextBoxYPoint7.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(
                double.Parse(TextBoxXPoint7.Text),
                double.Parse(TextBoxYPoint7.Text),
                 double.Parse(Textboxzprobeheight.Text) + double.Parse(TextBoxMoveHeight.Text));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PseudoSerial.WriteLine("g28", 1);
        }

        private void SendMove(double x, double y, double z, double f = 8000)
        {
            PseudoSerial.WriteLine(string.Format("G1 X{0} Y{1} Z{2} F{3}", x, y, z, f), 1);
        }

        private void TextBoxDeltaRadius_TextChanged(object sender, TextChangedEventArgs e)
        {
            double printradius = double.Parse(TextBoxDeltaRadius.Text);
            double[,] points = Parameters.CalcPoints(printradius, 6);
            TextBoxXPoint1.Text = points[0, 0].ToString("F3");
            TextBoxYPoint1.Text = points[0, 1].ToString("F3");
            TextBoxXPoint2.Text = points[1, 0].ToString("F3");
            TextBoxYPoint2.Text = points[1, 1].ToString("F3");
            TextBoxXPoint3.Text = points[2, 0].ToString("F3");
            TextBoxYPoint3.Text = points[2, 1].ToString("F3");
            TextBoxXPoint4.Text = points[3, 0].ToString("F3");
            TextBoxYPoint4.Text = points[3, 1].ToString("F3");
            TextBoxXPoint5.Text = points[4, 0].ToString("F3");
            TextBoxYPoint5.Text = points[4, 1].ToString("F3");
            TextBoxXPoint6.Text = points[5, 0].ToString("F3");
            TextBoxYPoint6.Text = points[5, 1].ToString("F3");
            TextBoxXPoint7.Text = "0.000";
            TextBoxYPoint7.Text = "0.000";

            Clear();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxDeltaRadius.TextChanged += TextBoxDeltaRadius_TextChanged;
            PseudoSerial.MessageReceived += PseudoSerial_MessageReceived;
            LoadSettings();
            Clear();
        }
    }
}
