using Klipper_Calibration_Tool.Classes;
using ReDACT.Classes;
using ReDACT.Classes.Escher;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Klipper_Calibration_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int currentpoint = 0;

        static string decimalformat = "F8";

        public MainWindow()
        {
            InitializeComponent();
            PseudoSerial.Init();
            
        }

        private void LoadSettings()
        {
            LoadSetting(TextBoxInitialRadius, Properties.Settings.Default.Radius);
            LoadSetting(TextBoxInitialRod, Properties.Settings.Default.Rod);
            LoadSetting(TextBoxInitialXEnd, Properties.Settings.Default.EndX);
            LoadSetting(TextBoxInitialYEnd, Properties.Settings.Default.EndY);
            LoadSetting(TextBoxInitialZEnd, Properties.Settings.Default.EndZ);
            LoadSetting(TextBoxInitialXAngle, Properties.Settings.Default.AngleX);
            LoadSetting(TextBoxInitialYAngle, Properties.Settings.Default.AngleY);
            LoadSetting(TextBoxInitialZangle, Properties.Settings.Default.AngleZ);
            LoadSetting(textboxzprobeheight, Properties.Settings.Default.ProbeZ);
            LoadSetting(TextBoxMoveHeight, Properties.Settings.Default.MoveHeight);
            LoadSetting(TextBoxDeltaRadius, Properties.Settings.Default.PrintRadius);
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.Radius = TextBoxInitialRadius.Text;
            Properties.Settings.Default.Rod = TextBoxInitialRod.Text;
            Properties.Settings.Default.EndX = TextBoxInitialXEnd.Text;
            Properties.Settings.Default.EndY = TextBoxInitialYEnd.Text;
            Properties.Settings.Default.EndZ = TextBoxInitialZEnd.Text;
            Properties.Settings.Default.AngleX = TextBoxInitialXAngle.Text;
            Properties.Settings.Default.AngleY = TextBoxInitialYAngle.Text;
            Properties.Settings.Default.AngleZ = TextBoxInitialZangle.Text;
            Properties.Settings.Default.ProbeZ = textboxzprobeheight.Text;
            Properties.Settings.Default.MoveHeight = TextBoxMoveHeight.Text;
            Properties.Settings.Default.PrintRadius = TextBoxDeltaRadius.Text;

            Properties.Settings.Default.Save();
        }

        private void LoadSetting(TextBox box, string value)
        {
            if (value.Length != 0)
                box.Text = value;
        }

        private void PseudoSerial_MessageReceived(string message)
        {
            if (IsPositionMessage(message) && currentpoint != 0)
            {
                double z = ParseZ(message);
                switch (currentpoint)
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
                currentpoint = 0;
            }
        }

        private void InvokeTextUpdate(TextBox target, string text)
        {
            target.Dispatcher.Invoke(new Action(() =>
            {
                target.Text = text;
            }));
        }

        private bool IsPositionMessage(string message)
        {
            return (message.Split(' ').Length == 8) && !message.Contains("!!");
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
            double offHX = hx - height;
            double offHY = hy - height;
            double offHZ = hz - height;

            Debug.WriteLine(String.Format("Initial height:{0}", height));
            Debug.WriteLine(String.Format("Initial offsets: X:{0}, Y:{1}, Z:{2}", offHX, offHY, offHZ));

            double offAX = double.Parse(TextBoxInitialXAngle.Text) - 210;
            double offAY = double.Parse(TextBoxInitialYAngle.Text) - 330;
            double offAZ = double.Parse(TextBoxInitialZangle.Text) - 90;

            DParameters deltaP = new DParameters(rod, radius, height, offHX, offHY, offHZ, offAX, offAY, offAZ);

            TParameters deltaT = new TParameters(DParameters.Firmware.KLIPPER, 7, 6, true);//valid params 3 4 6 7
            deltaT.xBedProbePoints = new double[7]
            {
                double.Parse(TextBoxXPoint1.Text),
                double.Parse(TextBoxXPoint2.Text),
                double.Parse(TextBoxXPoint3.Text),
                double.Parse(TextBoxXPoint4.Text),
                double.Parse(TextBoxXPoint5.Text),
                double.Parse(TextBoxXPoint6.Text),
                double.Parse(TextBoxXPoint7.Text),
            };

            deltaT.yBedProbePoints = new double[7]
            {
                double.Parse(TextBoxYPoint1.Text),
                double.Parse(TextBoxYPoint2.Text),
                double.Parse(TextBoxYPoint3.Text),
                double.Parse(TextBoxYPoint4.Text),
                double.Parse(TextBoxYPoint5.Text),
                double.Parse(TextBoxYPoint6.Text),
                double.Parse(TextBoxYPoint7.Text),
            };


            double zeroheight = double.Parse(textboxzprobeheight.Text);
            deltaT.zBedProbePoints = new double[7]
            {
                double.Parse(TextBoxZPoint1.Text) - zeroheight,
                double.Parse(TextBoxZPoint2.Text) - zeroheight,
                double.Parse(TextBoxZPoint3.Text) - zeroheight,
                double.Parse(TextBoxZPoint4.Text) - zeroheight,
                double.Parse(TextBoxZPoint5.Text) - zeroheight,
                double.Parse(TextBoxZPoint6.Text) - zeroheight,
                double.Parse(TextBoxZPoint7.Text) - zeroheight,
            };

            CalibrationResult cr = Escher3D.calc(ref deltaP, ref deltaT);
            labelresult.Content = cr.ToString();

            double newheight = deltaP.homedHeight;

            double newXStop = newheight + deltaP.xstop;
            double newYStop = newheight + deltaP.ystop;
            double newZStop = newheight + deltaP.zstop;

            Debug.WriteLine(String.Format("Calculated height:{0}", newheight));
            Debug.WriteLine(String.Format("Initial offsets: X:{0}, Y:{1}, Z:{2}", offHX, offHY, offHZ));

            TextBoxCalculatedRadius.Text = deltaP.radius.ToString(decimalformat);
            TextBoxCalculatedRod.Text = deltaP.diagonal.ToString(decimalformat);

            TextBoxCalculatedEndX.Text = (newXStop).ToString(decimalformat);
            TextBoxCalculatedEndY.Text = (newYStop).ToString(decimalformat);
            TextBoxCalculatedEndZ.Text = (newZStop).ToString(decimalformat);

            TextBoxCalculatedAngleX.Text = (210 + deltaP.xadj).ToString(decimalformat);
            TextBoxCalculatedAngleY.Text = (330 + deltaP.yadj).ToString(decimalformat);
            TextBoxCalculatedAngleZ.Text = (90 + deltaP.zadj).ToString(decimalformat);

            buttonApply.IsEnabled = true;
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

            var client = new ScpClient("192.168.76.239", "pi", "raspberry");
            client.Connect();
            client.Upload(configStream, "/home/pi/printer.cfg");
            client.Disconnect();
            client.Dispose();
            configStream.Dispose();
            PseudoSerial.WriteLine("restart",1);
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
            buttonApply.IsEnabled = false;
            buttonHome.IsEnabled = false;
            Task.Delay(10000).ContinueWith(t =>
            {
                buttonHome.Dispatcher.Invoke(new Action(() =>
                {
                    buttonHome.IsEnabled = true;
                }));
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
            currentpoint = 1;
            SendMove(
                double.Parse(TextBoxXPoint1.Text),
                double.Parse(TextBoxYPoint1.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(
                double.Parse(TextBoxXPoint1.Text),
                double.Parse(TextBoxYPoint1.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));
        }

        private void ButtonPoint2_Click(object sender, RoutedEventArgs e)
        {
            currentpoint = 2;
            SendMove(
                double.Parse(TextBoxXPoint2.Text),
                double.Parse(TextBoxYPoint2.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(
                double.Parse(TextBoxXPoint2.Text),
                double.Parse(TextBoxYPoint2.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));
        }

        private void ButtonPoint3_Click(object sender, RoutedEventArgs e)
        {
            currentpoint = 3;
            SendMove(
                double.Parse(TextBoxXPoint3.Text),
                double.Parse(TextBoxYPoint3.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(
                double.Parse(TextBoxXPoint3.Text),
                double.Parse(TextBoxYPoint3.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));
        }

        private void ButtonPoint4_Click(object sender, RoutedEventArgs e)
        {
            currentpoint = 4;
            SendMove(
                double.Parse(TextBoxXPoint4.Text),
                double.Parse(TextBoxYPoint4.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(
                double.Parse(TextBoxXPoint4.Text),
                double.Parse(TextBoxYPoint4.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));
        }

        private void ButtonPoint5_Click(object sender, RoutedEventArgs e)
        {
            currentpoint = 5;
            SendMove(
                double.Parse(TextBoxXPoint5.Text),
                double.Parse(TextBoxYPoint5.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(
                double.Parse(TextBoxXPoint5.Text),
                double.Parse(TextBoxYPoint5.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));

        }

        private void ButtonPoint6_Click(object sender, RoutedEventArgs e)
        {
            currentpoint = 6;
            SendMove(
                double.Parse(TextBoxXPoint6.Text),
                double.Parse(TextBoxYPoint6.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));
            PseudoSerial.WriteLine("m400", 1);
            PseudoSerial.WriteLine("probe", 1);
            PseudoSerial.WriteLine("m114", 2);
            SendMove(
                double.Parse(TextBoxXPoint6.Text),
                double.Parse(TextBoxYPoint6.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));
        }
        private void ButtonPoint7_Click(object sender, RoutedEventArgs e)
        {
            currentpoint = 7;
            SendMove(
                double.Parse(TextBoxXPoint7.Text),
                double.Parse(TextBoxYPoint7.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));            
            PseudoSerial.WriteLine("m400",1);
            PseudoSerial.WriteLine("probe",1);
            PseudoSerial.WriteLine("m114",2);
            SendMove(
                double.Parse(TextBoxXPoint7.Text),
                double.Parse(TextBoxYPoint7.Text),
                 (double.Parse(textboxzprobeheight.Text) + (double.Parse(TextBoxMoveHeight.Text))));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PseudoSerial.WriteLine("g28",1);
        }

        private void SendMove(double X, double Y, double Z, double F = 8000)
        {
            PseudoSerial.WriteLine(String.Format("G1 X{0} Y{1} Z{2} F{3}", X, Y, Z, F),1);
        }

        private void TextBoxDeltaRadius_TextChanged(object sender, TextChangedEventArgs e)
        {
            double printradius = double.Parse(TextBoxDeltaRadius.Text);
            double[,] points = TParameters.calcPoints(printradius, 6);
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
