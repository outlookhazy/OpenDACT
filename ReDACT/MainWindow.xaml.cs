using OpenDACT.Class_Files;
using OpenDACT.Class_Files.Workflow_Classes;
using ReDACT.Classes;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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

namespace ReDACT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialManager serialManager;
        public MainWindow()
        {
            InitializeComponent();
            comboBoxSerial.ItemsSource = SerialPort.GetPortNames();
            if(comboBoxSerial.Items.Count > 0)
                comboBoxSerial.SelectedIndex = comboBoxSerial.Items.Count - 1;

            comboBoxBaud.Items.Add("250000");
            comboBoxBaud.Items.Add("115200");
            comboBoxBaud.Items.Add("57600");
            comboBoxBaud.Items.Add("38400");
            comboBoxBaud.Items.Add("19200");
            comboBoxBaud.Items.Add("9600");
            comboBoxBaud.Text = "250000";

            this.serialManager = new SerialManager();
            this.serialManager.SerialConnectionChanged += SerialManager_SerialConnectionChanged;
            this.serialManager.NewSerialLine += SerialManager_NewSerialLine;
        }

        private void SerialManager_NewSerialLine(object sender, string data)
        {
            WorkflowManager.DelegateInput(data);
        }

        private void SerialManager_SerialConnectionChanged(object sender, ConnectionState newState)
        {
            switch (newState)
            {
                case ConnectionState.Connected:
                    this.buttonConnect.Dispatcher.BeginInvoke(new Action(() => {
                        this.buttonConnect.Content = "Disconnect";
                        }));
                    WorkflowManager.ActivateWorkflow(new ReadEEPROMWF(this.serialManager));
                    break;
                case ConnectionState.DISCONNECTED:
                    this.buttonConnect.Dispatcher.BeginInvoke(new Action(() => {
                        this.buttonConnect.Content = "Connect";
                    }));
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DeltaParameters delta = new DeltaParameters(DeltaParameters.Firmware.Repetier, 80, 269, 133.98, 288.907, 0, 0, 0, 0, 0, 0);
            double[,] testpoints = new double[,] {
                {0,         100,    1 },
                {86.6,      50,     2 },
                { 86.6,     -50,    3 },
                {0,         -100,   4 },
                {-86.6,     -50,    5 },
                { -86.6,    50,     6 },
                {0,         0,      0 }
            };
            TestData data = new TestData(7, Escher.NumFactors.SIX, 100, testpoints, true);
            Escher.Calc(delta, data);
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (serialManager.CurrentState == ConnectionState.Connected)
                serialManager.Disconnect();
            else
                serialManager.Connect(this.comboBoxSerial.SelectedItem.ToString(),  int.Parse(this.comboBoxBaud.Text));
        }
    }
}
