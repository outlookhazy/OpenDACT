using OpenDACT.Class_Files;
using OpenDACT.Class_Files.Workflow_Classes;
using OxyPlot;
using OxyPlot.Series;
using ReDACT.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static ReDACT.Classes.DeltaParameters;

namespace ReDACT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialManager serialManager;
        Workflow mainWorkflow;
        LineSeries ls;
        List<DataPoint> points = new List<DataPoint>();
        public MainWindow()
        {
            InitializeComponent();
            comboBoxFirmware.ItemsSource = typeof(DeltaParameters.Firmware).GetEnumValues();
            comboBoxFirmware.SelectedItem = DeltaParameters.Firmware.Repetier;

            comboBoxFactors.ItemsSource = typeof(Escher.NumFactors).GetEnumValues();
            comboBoxFactors.SelectedItem = Escher.NumFactors.SEVEN;

            mainWorkflow = new Workflow();

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

            oxyPlotView.Model = new OxyPlot.PlotModel();
            oxyPlotView.Model.Title = "Test";
            
        }



        private void SerialManager_NewSerialLine(object sender, string data)
        {
            mainWorkflow.RouteMessage(data);
        }

        private void SerialManager_SerialConnectionChanged(object sender, ConnectionState newState)
        {
            switch (newState)
            {
                case ConnectionState.Connected:
                    this.buttonConnect.Dispatcher.BeginInvoke(new Action(() => {
                        this.buttonConnect.Content = "Disconnect";
                        this.buttonCalibrate.IsEnabled = true;
                        }));                    
                    break;
                case ConnectionState.DISCONNECTED:
                    this.buttonConnect.Dispatcher.BeginInvoke(new Action(() => {
                        this.buttonConnect.Content = "Connect";
                        this.buttonCalibrate.IsEnabled = false;
                    }));
                    break;
            }
        }

        private void ButtonCalibrate_Click(object sender, RoutedEventArgs e)
        {
            TestData calibrationTestData = new TestData((Firmware)comboBoxFirmware.SelectedItem, (int)sliderNumPoints.Value, (Escher.NumFactors)comboBoxFactors.SelectedItem,true);
            mainWorkflow.AddWorkflowItem(new EscherWF(serialManager,calibrationTestData));
            mainWorkflow.Start();            
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (serialManager.CurrentState == ConnectionState.Connected)
                serialManager.Disconnect();
            else
                serialManager.Connect(this.comboBoxSerial.SelectedItem.ToString(),  int.Parse(this.comboBoxBaud.Text));
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (labelPointsSlider == null)
                return;
            labelPointsSlider.Dispatcher.BeginInvoke(new Action(() => { labelPointsSlider.Content = sliderNumPoints.Value.ToString(); }));
        }

        private void comboBoxFactors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selected = (int)comboBoxFactors.SelectedItem;
            if (selected > 3)
                sliderNumPoints.Minimum = selected;
            else
                sliderNumPoints.Minimum = 4;

            if (sliderNumPoints.Value < sliderNumPoints.Minimum)
                sliderNumPoints.Value = sliderNumPoints.Minimum;
        }
    }
}
