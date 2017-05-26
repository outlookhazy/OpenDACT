﻿using OpenDACT.Class_Files;
using OpenDACT.Class_Files.Workflow_Classes;
using ReDACT.Classes;
using ReDACT.Classes.Escher;
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
using static ReDACT.Classes.Escher.DParameters;

namespace ReDACT
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IWorkflowParent
    {
        



    public SerialManager SerialSource { get; set; }
        Workflow mainWorkflow;

        Chart Chart;

        public MainWindow()
        {
            InitializeComponent();
            comboBoxFirmware.ItemsSource = typeof(DParameters.Firmware).GetEnumValues();
            comboBoxFirmware.SelectedItem = DParameters.Firmware.REPETIER;

            comboBoxFactors.ItemsSource = typeof(Escher3D.NumFactors).GetEnumValues();
            comboBoxFactors.SelectedItem = Escher3D.NumFactors.SEVEN;

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

            this.SerialSource = new SerialManager();
            this.SerialSource.SerialConnectionChanged += SerialManager_SerialConnectionChanged;
            this.SerialSource.NewSerialInLine += SerialManager_NewSerialLine;
            this.SerialSource.NewSerialOutLine += SerialManager_NewSerialOutLine;

            mainWorkflow = new Workflow(this.SerialSource);

            Chart = new Chart(GraphDisplay);
        }

        private void SerialManager_NewSerialOutLine(object sender, string data)
        {
            textboxSerial.Dispatcher.BeginInvoke(new Action(() =>
            {
                textboxSerial.AppendText("=>" + data + "\n");
                if (textboxSerial.Text.Length > 10000)
                    textboxSerial.Text = textboxSerial.Text.Substring(1000);
                textboxSerial.ScrollToEnd();
            }));
        }

        private void SerialManager_NewSerialLine(object sender, string data)
        {
            textboxSerial.Dispatcher.BeginInvoke(new Action(() =>
            {
                textboxSerial.AppendText("<=" + data + "\n");
                if (textboxSerial.Text.Length > 10000)
                    textboxSerial.Text = textboxSerial.Text.Substring(1000);
                textboxSerial.ScrollToEnd();
            }));
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
            Chart.Clear();
            TParameters calibrationTestData = new TParameters((Firmware)comboBoxFirmware.SelectedItem, (int)sliderNumPoints.Value, (int)comboBoxFactors.SelectedItem, true);
            mainWorkflow.AddWorkflowItem(new EscherWF(calibrationTestData));
            mainWorkflow.Start(this);
            Continue(int.Parse(textBoxIterations.Text));
        }

        private bool Continue(int? newCount = null)
        {
            if (newCount != null)
                this.continuecount = (int)newCount;
            else
                this.continuecount--;

            labelIteration.Dispatcher.BeginInvoke(new Action(() => { labelIteration.Content = String.Format("({0})", this.continuecount); }));
            return this.continuecount > 0;
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (SerialSource.CurrentState == ConnectionState.Connected)
                SerialSource.Disconnect();
            else
                SerialSource.Connect(this.comboBoxSerial.SelectedItem.ToString(),  int.Parse(this.comboBoxBaud.Text));
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
            sliderNumPoints.Minimum = selected + 1;

            if (sliderNumPoints.Value < sliderNumPoints.Minimum)
                sliderNumPoints.Value = sliderNumPoints.Minimum;
        }

        int continuecount;
        public void ChildStateChanged(object child, WorkflowState newState)
        {
            if (newState != WorkflowState.FINISHED)
                return;

            Chart.AddSequential(EscherWF.Result.DeviationBefore);
            if (Continue())
            {
                Dispatcher.BeginInvoke(new Action(() => { 
                TParameters calibrationTestData = new TParameters((Firmware)comboBoxFirmware.SelectedItem, (int)sliderNumPoints.Value, (int)comboBoxFactors.SelectedItem, true);
                mainWorkflow.AddWorkflowItem(new EscherWF(calibrationTestData));
                mainWorkflow.Start(this);
                }));
            }
        }
    }
}
