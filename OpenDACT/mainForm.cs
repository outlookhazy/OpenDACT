using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;
using System.Globalization;
using OpenDACT.Class_Files.Workflow_Classes;
using System.Diagnostics;
using static OpenDACT.Class_Files.Printer;

namespace OpenDACT.Class_Files
{
    public partial class mainForm : Form
    {
        public static SerialManager serialManager;
        public Workflow masterWorkflow;        

        public mainForm()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");

            InitializeComponent();

            serialManager = new SerialManager();
            serialManager.SerialConnectionChanged += SerialManager_SerialConnectionChanged;
            serialManager.NewSerialLine += SerialManager_NewSerialLine;

            masterWorkflow.ID = "MasterWorkflow";

            // Basic set of standard baud rates.
            baudRateCombo.Items.Add("250000");
            baudRateCombo.Items.Add("115200");
            baudRateCombo.Items.Add("57600");
            baudRateCombo.Items.Add("38400");
            baudRateCombo.Items.Add("19200");
            baudRateCombo.Items.Add("9600");
            baudRateCombo.Text = "250000";  // This is the default for most RAMBo controllers.

            this.comboBoxZMin.Items.AddRange(new object[] {
            ProbeType.FSR,
            ProbeType.ZProbe});
            this.comboBoxZMin.SelectedItem = ProbeType.ZProbe;

            advancedPanel.Visible = false;
            printerLogPanel.Visible = false;

            // Build the combobox of available ports.
            portsCombo.DataSource = new BindingSource(new List<string>(SerialPort.GetPortNames()), null);
            if(portsCombo.Items.Count != 0)
                portsCombo.SelectedIndex = portsCombo.Items.Count - 1;
            
            UserVariables.isInitiated = true;
        }

        void ActivateWorkflow(Workflow workflow)
        {
            this.masterWorkflow.Abort();
            this.masterWorkflow.AddWorkflowItem(workflow);
            this.masterWorkflow.Start();
        }

        private void SerialManager_SerialConnectionChanged(object sender, ConnectionState newState)
        {
            UserInterface.consoleLog.Log(String.Format("Serial {0}", newState));
        }

        private void SerialManager_NewSerialLine(object sender, string data)
        {
            UserInterface.printerLog.Log(String.Format("Received: {0}", data), LogConsole.LogLevel.DEBUG);
            masterWorkflow.RouteMessage(data);
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (serialManager.CurrentState == ConnectionState.CONNECTED)
            {
                UserInterface.consoleLog.Log("Already Connected");
            }
            else
            {                
                this.Connect();
            }
        }

        private void Connect()
        {
            try
            {
                string PortName = Program.mainFormTest.portsCombo.Text;
                int BaudRate = int.Parse(Program.mainFormTest.baudRateCombo.Text, CultureInfo.InvariantCulture);
                UserInterface.consoleLog.Log(Program.mainFormTest.portsCombo.Text);

                if (PortName != "" && BaudRate != 0)
                {
                    UserInterface.consoleLog.Log("Connecting");
                    serialManager.Connect(PortName, BaudRate);
                }
                else
                {
                    UserInterface.consoleLog.Log("Please fill all text boxes above");
                }
            }
            catch (Exception e1)
            {
                UserInterface.consoleLog.Log("Connection Failed " + e1.Message);
                serialManager.Disconnect();
            }
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            this.Disconnect();            
        }

        private void Disconnect()
        {
            if (serialManager.CurrentState == ConnectionState.CONNECTED)
            {
                serialManager.Disconnect();
            }
            else
            {
                UserInterface.consoleLog.Log("Not Connected");
            }
        }

        private void CalibrateButton_Click(object sender, EventArgs e)
        {
            if (serialManager.CurrentState == ConnectionState.CONNECTED)
            {
                //WorkflowManager.WorkflowQueue.AddLast(new ReadEEPROMWF());
                //ActivateWorkflow(new MeasureHeightsWF(serialManager));
                //WorkflowManager.WorkflowQueue.AddLast(WorkflowManager.WorkflowType.CALIBRATE);
            }
            else
            {
                UserInterface.consoleLog.Log("Not connected");
            }
        }
        
        private void QuickCalibrate_Click(object sender, EventArgs e)
        {
            if (serialManager.CurrentState == ConnectionState.CONNECTED)
            {
                ActivateWorkflow(new FastCalibrationWF());
            }
            else
            {
                UserInterface.consoleLog.Log("Not connected");
            }
        }

        private void ResetPrinter_Click(object sender, EventArgs e)
        {            
            serialManager.ClearOutBuffer();
            serialManager.WriteLine(GCode.Command.ESTOP);
        }

        public void AppendMainConsole(string value)
        {
            Invoke((MethodInvoker)delegate { consoleMain.AppendText(value + "\n"); });
            Invoke((MethodInvoker)delegate { consoleMain.ScrollToCaret(); });
        }

        public void AppendPrinterConsole(string value)
        {
            Invoke((MethodInvoker)delegate { consolePrinter.AppendText(value + "\n"); });
            Invoke((MethodInvoker)delegate { consolePrinter.ScrollToCaret(); });
        }        

        private void OpenAdvanced_Click(object sender, EventArgs e)
        {
            if (advancedPanel.Visible == false)
            {
                advancedPanel.Visible = true;
                printerLogPanel.Visible = true;
            }
            else
            {
                advancedPanel.Visible = false;
                printerLogPanel.Visible = false;
            }
        }

        private void SendGCode_Click(object sender, EventArgs e)
        {
            SendGCodeText();
        }

        private void GCodeBox_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter)
            {
                SendGCodeText();
                GCodeBox.SelectAll();
            }
        }

        private void SendGCodeText() 
            {
            if (serialManager.WriteLine(GCodeBox.Text.ToString().ToUpper())) {                
                UserInterface.consoleLog.Log("Sent: " + GCodeBox.Text.ToString().ToUpper());
            }            
        }

        public void SetAccuracyPoint(float x, float y)
        {
            Invoke((MethodInvoker)delegate
            {
                accuracyTime.Refresh();
                accuracyTime.Series["Accuracy"].Points.AddXY(x, y);
            });
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Version: ??\n\nCreated by Steven T. Rowland\n\nWith help from Gene Buckle and Michael Hackney\n\nMajor Rewrite by Outlookhazy");
        }
        private void ContactButton_Click(object sender, EventArgs e)
        {
            /*
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "mailto:steventrowland@gmail.com";
            proc.Start();
            */
        }

        private void DonateButton_Click(object sender, EventArgs e)
        {
            /*
            string url = "";

            string business = "steventrowland@gmail.com";
            string description = "Donation";
            string country = "US";
            string currency = "USD";

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            System.Diagnostics.Process.Start(url);
            */
        }

        public void SetHeightsInvoke( HeightMap Heights)
        {
            float X = Heights[Position.X].Z;
            float XOpp = Heights[Position.XOPP].Z;
            float Y = Heights[Position.Y].Z;
            float YOpp = Heights[Position.YOPP].Z;
            float Z = Heights[Position.Z].Z;
            float ZOpp = Heights[Position.ZOPP].Z;

            //set base heights for advanced calibration comparison
            if (Calibration.iterationNum == 0)
            {
                Invoke((MethodInvoker)delegate {
                    this.iXtext.Text = Math.Round(X, 3).ToString();
                    this.iXOpptext.Text = Math.Round(XOpp, 3).ToString();
                    this.iYtext.Text = Math.Round(Y, 3).ToString();
                    this.iYOpptext.Text = Math.Round(YOpp, 3).ToString();
                    this.iZtext.Text = Math.Round(Z, 3).ToString();
                    this.iZOpptext.Text = Math.Round(ZOpp, 3).ToString();
                });

                Calibration.iterationNum++;

                Invoke((MethodInvoker)delegate { this.XText.Text = Math.Round(X, 3).ToString();
                    this.XOppText.Text = Math.Round(XOpp, 3).ToString();
                    this.YText.Text = Math.Round(Y, 3).ToString();
                    this.YOppText.Text = Math.Round(YOpp, 3).ToString();
                    this.ZText.Text = Math.Round(Z, 3).ToString();
                    this.ZOppText.Text = Math.Round(ZOpp, 3).ToString();
                });
            }
            else
            {
                Invoke((MethodInvoker)delegate { this.XText.Text = Math.Round(X, 3).ToString();
                    this.XOppText.Text = Math.Round(XOpp, 3).ToString();
                    this.YText.Text = Math.Round(Y, 3).ToString();
                    this.YOppText.Text = Math.Round(YOpp, 3).ToString();
                    this.ZText.Text = Math.Round(Z, 3).ToString();
                    this.ZOppText.Text = Math.Round(ZOpp, 3).ToString();
                });
            }
        }

        public void SetEEPROMGUIList(EEPROM values)
        {
            Invoke((MethodInvoker)delegate
            {
                this.stepsPerMMText.Text = values[EEPROM_POSITION.StepsPerMM].Value.ToString();
                this.zMaxLengthText.Text = values[EEPROM_POSITION.zMaxLength].Value.ToString();
                this.zProbeText.Text = values[EEPROM_POSITION.zProbeHeight].Value.ToString();
                this.zProbeSpeedText.Text = values[EEPROM_POSITION.zProbeSpeed].Value.ToString();
                this.diagonalRod.Text = values[EEPROM_POSITION.diagonalRod].Value.ToString();
                this.HRadiusText.Text = values[EEPROM_POSITION.HRadius].Value.ToString();
                this.offsetXText.Text = values[EEPROM_POSITION.offsetX].Value.ToString();
                this.offsetYText.Text = values[EEPROM_POSITION.offsetY].Value.ToString();
                this.offsetZText.Text = values[EEPROM_POSITION.offsetZ].Value.ToString();
                this.AText.Text = values[EEPROM_POSITION.A].Value.ToString();
                this.BText.Text = values[EEPROM_POSITION.B].Value.ToString();
                this.CText.Text = values[EEPROM_POSITION.C].Value.ToString();
                this.DAText.Text = values[EEPROM_POSITION.DA].Value.ToString();
                this.DBText.Text = values[EEPROM_POSITION.DB].Value.ToString();
                this.DCText.Text = values[EEPROM_POSITION.DC].Value.ToString();
            });
        }

        private void SendEEPROMButton_Click(object sender, EventArgs e)
        {
            EEPROM uiValues = new EEPROM();
            uiValues[EEPROM_POSITION.StepsPerMM].Value = Int32.Parse(this.stepsPerMMText.Text);
            uiValues[EEPROM_POSITION.zMaxLength].Value = float.Parse(this.zMaxLengthText.Text);
            uiValues[EEPROM_POSITION.zProbeHeight].Value = float.Parse(this.zProbeText.Text);
            uiValues[EEPROM_POSITION.zProbeSpeed].Value = float.Parse(this.zProbeSpeedText.Text);
            uiValues[EEPROM_POSITION.diagonalRod].Value = float.Parse(this.diagonalRod.Text);
            uiValues[EEPROM_POSITION.HRadius].Value = float.Parse(this.HRadiusText.Text);
            uiValues[EEPROM_POSITION.offsetX].Value = float.Parse(this.offsetXText.Text);
            uiValues[EEPROM_POSITION.offsetY].Value = float.Parse(this.offsetYText.Text);
            uiValues[EEPROM_POSITION.offsetZ].Value = float.Parse(this.offsetZText.Text);
            uiValues[EEPROM_POSITION.A].Value = float.Parse(this.AText.Text);
            uiValues[EEPROM_POSITION.B].Value = float.Parse(this.BText.Text);
            uiValues[EEPROM_POSITION.C].Value = float.Parse(this.CText.Text);
            uiValues[EEPROM_POSITION.DA].Value = float.Parse(this.DAText.Text);
            uiValues[EEPROM_POSITION.DB].Value = float.Parse(this.DBText.Text);
            uiValues[EEPROM_POSITION.DC].Value = float.Parse(this.DCText.Text);

            ActivateWorkflow(new ApplySettingsWF(serialManager,uiValues));
        }

        private void ReadEEPROM_Click(object sender, EventArgs e)
        {
            if (serialManager.CurrentState == ConnectionState.CONNECTED)
            {
                Debug.WriteLine("Added Read EEPROM WF Item");
                ActivateWorkflow(new ReadEEPROMWF(serialManager));                
            }
            else
            {
                UserInterface.consoleLog.Log("Not Connected");
            }
        }

        public void SetButtonValues()
        {
            Invoke((MethodInvoker)delegate
            {
                this.textAccuracy.Text = UserVariables.calculationAccuracy.ToString();
                this.textAccuracy2.Text = UserVariables.accuracy.ToString();
                this.textHRadRatio.Text = UserVariables.HRadRatio.ToString();
                this.textDRadRatio.Text = UserVariables.DRadRatio.ToString();

                this.heuristicComboBox.Text = UserVariables.advancedCalibration.ToString();

                this.textPauseTimeSet.Text = UserVariables.pauseTimeSet.ToString();
                this.textMaxIterations.Text = UserVariables.maxIterations.ToString();
                this.textProbingSpeed.Text = UserVariables.probingSpeed.ToString();
                this.textFSRPO.Text = UserVariables.FSROffset.ToString();
                this.textDeltaOpp.Text = UserVariables.deltaOpp.ToString();
                this.textDeltaTower.Text = UserVariables.deltaTower.ToString();
                this.diagonalRodLengthText.Text = UserVariables.diagonalRodLength.ToString();
                this.alphaText.Text = UserVariables.alphaRotationPercentage.ToString();
                this.textPlateDiameter.Text = UserVariables.plateDiameter.ToString();
                this.textProbingHeight.Text = UserVariables.probingHeight.ToString();

                //XYZ Offset percs
                this.textOffsetPerc.Text = UserVariables.offsetCorrection.ToString();
                this.textMainOppPerc.Text = UserVariables.mainOppPerc.ToString();
                this.textTowPerc.Text = UserVariables.towPerc.ToString();
                this.textOppPerc.Text = UserVariables.oppPerc.ToString();
            });
        }
        private Printer.ProbeType GetZMin()
        {
            return comboBoxZMin.InvokeRequired ? (Printer.ProbeType)comboBoxZMin.Invoke(new Func<Printer.ProbeType>(GetZMin)) : (Printer.ProbeType)Enum.Parse(typeof(Printer.ProbeType), comboBoxZMin.SelectedItem.ToString());
        }

        private string GetHeuristic()
        {
            return heuristicComboBox.InvokeRequired ? (string)heuristicComboBox.Invoke(new Func<string>(GetHeuristic)) : heuristicComboBox.Text;
        }

        public void SetUserVariables()
        {
            UserVariables.calculationAccuracy = Convert.ToSingle(this.textAccuracy.Text);
            UserVariables.accuracy = Convert.ToSingle(this.textAccuracy2.Text);
            UserVariables.HRadRatio = Convert.ToSingle(this.textHRadRatio.Text);
            UserVariables.DRadRatio = Convert.ToSingle(this.textDRadRatio.Text);

            UserVariables.probeChoice = GetZMin();
            UserVariables.advancedCalibration = Convert.ToBoolean(GetHeuristic());

            UserVariables.pauseTimeSet = Convert.ToInt32(this.textPauseTimeSet.Text);
            UserVariables.maxIterations = Convert.ToInt32(this.textMaxIterations.Text);
            UserVariables.probingSpeed = Convert.ToSingle(this.textProbingSpeed.Text);
            UserVariables.FSROffset = Convert.ToSingle(this.textFSRPO.Text);
            UserVariables.deltaOpp = Convert.ToSingle(this.textDeltaOpp.Text);
            UserVariables.deltaTower = Convert.ToSingle(this.textDeltaTower.Text);
            UserVariables.diagonalRodLength = Convert.ToSingle(this.diagonalRodLengthText.Text);
            UserVariables.alphaRotationPercentage = Convert.ToSingle(this.alphaText.Text);
            UserVariables.plateDiameter = Convert.ToSingle(this.textPlateDiameter.Text);
            UserVariables.probingHeight = Convert.ToSingle(this.textProbingHeight.Text);

            //XYZ Offset percs
            UserVariables.offsetCorrection = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(textOffsetPerc.Text, out double value); return value; }));
            UserVariables.mainOppPerc = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(textMainOppPerc.Text, out double value); return value; }));
            UserVariables.towPerc = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(textTowPerc.Text, out double value); return value; }));
            UserVariables.oppPerc = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(textOppPerc.Text, out double value); return value; }));

            UserVariables.xySpeed = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(xySpeedTxt.Text, out double value); return value; }));
        }

        private void CheckHeights_Click(object sender, EventArgs e)
        {

            //WorkflowManager.ActivateWorkflow(new ZProbeMeasureWF());
        }

        private void StopBut_Click(object sender, EventArgs e)
        {
            if (serialManager.CurrentState == ConnectionState.CONNECTED)
            {
                serialManager.ClearOutBuffer();
                serialManager.WriteLine(GCode.Command.ESTOP);
                Disconnect();
            }
        }

        private void ManualCalibrateBut_Click(object sender, EventArgs e)
        {
            try
            {
                Calibration.calibrationState = true;

                Program.mainFormTest.SetUserVariables();

                HeightMap Heights = new HeightMap();

                Heights[Position.X].Z = Convert.ToSingle(xManual.Text);
                Heights[Position.XOPP].Z = Convert.ToSingle(xOppManual.Text);
                Heights[Position.Y].Z = Convert.ToSingle(yManual.Text);
                Heights[Position.YOPP].Z = Convert.ToSingle(yOppManual.Text);
                Heights[Position.Z].Z = Convert.ToSingle(zManual.Text);
                Heights[Position.ZOPP].Z = Convert.ToSingle(zOppManual.Text);

                EEPROM manual = new EEPROM();
                manual[EEPROM_POSITION.StepsPerMM].Value = Convert.ToSingle(spmMan.Text);
                manual.tempSPM = Convert.ToSingle(spmMan.Text);

                manual[EEPROM_POSITION.zMaxLength].Value = Convert.ToSingle(zMaxMan.Text);
                manual[EEPROM_POSITION.zProbeHeight].Value = Convert.ToSingle(zProHeiMan.Text);
                manual[EEPROM_POSITION.zProbeSpeed].Value = Convert.ToSingle(zProSpeMan.Text);
                manual[EEPROM_POSITION.HRadius].Value = Convert.ToSingle(horRadMan.Text);
                manual[EEPROM_POSITION.diagonalRod].Value = Convert.ToSingle(diaRodMan.Text);
                manual[EEPROM_POSITION.offsetX].Value = Convert.ToSingle(towOffXMan.Text);
                manual[EEPROM_POSITION.offsetY].Value = Convert.ToSingle(towOffYMan.Text);
                manual[EEPROM_POSITION.offsetZ].Value = Convert.ToSingle(towOffZMan.Text);
                manual[EEPROM_POSITION.A].Value = Convert.ToSingle(alpRotAMan.Text);
                manual[EEPROM_POSITION.B].Value = Convert.ToSingle(alpRotBMan.Text);
                manual[EEPROM_POSITION.C].Value = Convert.ToSingle(alpRotCMan.Text);
                manual[EEPROM_POSITION.DA].Value = Convert.ToSingle(delRadAMan.Text);
                manual[EEPROM_POSITION.DB].Value = Convert.ToSingle(delRadBMan.Text);
                manual[EEPROM_POSITION.DC].Value = Convert.ToSingle(delRadCMan.Text);

                Calibration.BasicCalibration();

                //set eeprom vals in manual calibration
                this.spmMan.Text = manual[EEPROM_POSITION.StepsPerMM].Value.ToString();
                this.zMaxMan.Text = manual[EEPROM_POSITION.zMaxLength].Value.ToString();
                this.zProHeiMan.Text = manual[EEPROM_POSITION.zProbeHeight].Value.ToString();
                this.zProSpeMan.Text = manual[EEPROM_POSITION.zProbeSpeed].Value.ToString();
                this.diaRodMan.Text = manual[EEPROM_POSITION.diagonalRod].Value.ToString();
                this.horRadMan.Text = manual[EEPROM_POSITION.HRadius].Value.ToString();
                this.towOffXMan.Text = manual[EEPROM_POSITION.offsetX].Value.ToString();
                this.towOffYMan.Text = manual[EEPROM_POSITION.offsetY].Value.ToString();
                this.towOffZMan.Text = manual[EEPROM_POSITION.offsetZ].Value.ToString();
                this.alpRotAMan.Text = manual[EEPROM_POSITION.A].Value.ToString();
                this.alpRotBMan.Text = manual[EEPROM_POSITION.B].Value.ToString();
                this.alpRotCMan.Text = manual[EEPROM_POSITION.C].Value.ToString();
                this.delRadAMan.Text = manual[EEPROM_POSITION.DA].Value.ToString();
                this.delRadBMan.Text = manual[EEPROM_POSITION.DB].Value.ToString();
                this.delRadCMan.Text = manual[EEPROM_POSITION.DC].Value.ToString();

                //set expected height map
                this.xExp.Text = Heights[Position.X].Z.ToString();
                this.xOppExp.Text = Heights[Position.XOPP].Z.ToString();
                this.yExp.Text = Heights[Position.Y].Z.ToString();
                this.yOppExp.Text = Heights[Position.YOPP].Z.ToString();
                this.zExp.Text = Heights[Position.Z].Z.ToString();
                this.zOppExp.Text = Heights[Position.ZOPP].Z.ToString();


                Calibration.calibrationState = false;
            }
            catch (Exception ex)
            {
                UserInterface.consoleLog.Log(ex.ToString());
            }
        }

        private void MainForm_Load(object sender, EventArgs e) {
            UserInterface.Init();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Disconnect();
            if(serialManager != null)
            {
                serialManager.Dispose();
            }
        }
    }
}
