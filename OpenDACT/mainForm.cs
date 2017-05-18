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

namespace OpenDACT.Class_Files
{
    public partial class mainForm : Form
    {
        public mainForm()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");

            InitializeComponent();

            // Basic set of standard baud rates.
            baudRateCombo.Items.Add("250000");
            baudRateCombo.Items.Add("115200");
            baudRateCombo.Items.Add("57600");
            baudRateCombo.Items.Add("38400");
            baudRateCombo.Items.Add("19200");
            baudRateCombo.Items.Add("9600");
            baudRateCombo.Text = "250000";  // This is the default for most RAMBo controllers.

            this.comboBoxZMin.Items.AddRange(new object[] {
            OpenDACT.Class_Files.Printer.ProbeType.FSR,
            OpenDACT.Class_Files.Printer.ProbeType.ZProbe});
            this.comboBoxZMin.SelectedItem = OpenDACT.Class_Files.Printer.ProbeType.ZProbe;

            advancedPanel.Visible = false;
            printerLogPanel.Visible = false;

            Connection.Init();

            // Build the combobox of available ports.
            portsCombo.DataSource = new BindingSource(new List<string>(SerialPort.GetPortNames()), null);
            
            UserVariables.isInitiated = true;
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            Connection.Connect();
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            Connection.Disconnect();
        }

        private void CalibrateButton_Click(object sender, EventArgs e)
        {
            if (Connection.serialManager.CurrentState == ConnectionState.Connected)
            {
                GCode.checkHeights = true;
                EEPROMFunctions.ReadEEPROM();
                EEPROMFunctions.EEPROMReadOnly = false;
                Calibration.calibrationState = true;
                Calibration.calibrationSelection = Calibration.CalibrationType.NORMAL;
                Heights.checkHeightsOnly = false;
                Printer.isCalibrating = true;
            }
            else
            {
                UserInterface.consoleLog.Log("Not connected");
            }
        }
        
        private void QuickCalibrate_Click(object sender, EventArgs e)
        {
            if (Connection.serialManager.CurrentState == ConnectionState.Connected)
            {
                GCode.checkHeights = true;
                EEPROMFunctions.ReadEEPROM();
                EEPROMFunctions.EEPROMReadOnly = false;
                Calibration.calibrationState = true;
                Calibration.calibrationSelection = Calibration.CalibrationType.QUICK;
                Heights.checkHeightsOnly = false;
                Printer.isCalibrating = true;
            }
            else
            {
                UserInterface.consoleLog.Log("Not connected");
            }
        }

        private void ResetPrinter_Click(object sender, EventArgs e)
        {
                GCode.TrySend(GCode.Command.RESET);
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
                SendGCodeText();
        }

        private void SendGCodeText() 
            {
            if (GCode.TrySend(GCodeBox.Text.ToString().ToUpper())) {                
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

        public void SetHeightsInvoke()
        {
            float X = Heights.X;
            float XOpp = Heights.XOpp;
            float Y = Heights.Y;
            float YOpp = Heights.YOpp;
            float Z = Heights.Z;
            float ZOpp = Heights.ZOpp;

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

        public void SetEEPROMGUIList()
        {
            Invoke((MethodInvoker)delegate
            {
                this.stepsPerMMText.Text = EEPROM.stepsPerMM.ToString();
                this.zMaxLengthText.Text = EEPROM.zMaxLength.ToString();
                this.zProbeText.Text = EEPROM.zProbeHeight.ToString();
                this.zProbeSpeedText.Text = EEPROM.zProbeSpeed.ToString();
                this.diagonalRod.Text = EEPROM.diagonalRod.ToString();
                this.HRadiusText.Text = EEPROM.HRadius.ToString();
                this.offsetXText.Text = EEPROM.offsetX.ToString();
                this.offsetYText.Text = EEPROM.offsetY.ToString();
                this.offsetZText.Text = EEPROM.offsetZ.ToString();
                this.AText.Text = EEPROM.A.ToString();
                this.BText.Text = EEPROM.B.ToString();
                this.CText.Text = EEPROM.C.ToString();
                this.DAText.Text = EEPROM.DA.ToString();
                this.DBText.Text = EEPROM.DB.ToString();
                this.DCText.Text = EEPROM.DC.ToString();
            });
        }

        private void SendEEPROMButton_Click(object sender, EventArgs e)
        {
            EEPROM.stepsPerMM.Value = Convert.ToInt32(this.Invoke((Func<double>)delegate { Double.TryParse(this.stepsPerMMText.Text, out double value); return value; }));
            EEPROM.zMaxLength.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.zMaxLengthText.Text, out double value); return value; }));
            EEPROM.zProbeHeight.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.zProbeText.Text, out double value); return value; }));
            EEPROM.zProbeSpeed.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.zProbeSpeedText.Text, out double value); return value; }));
            EEPROM.diagonalRod.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.diagonalRod.Text, out double value); return value; }));
            EEPROM.HRadius.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.HRadiusText.Text, out double value); return value; }));
            EEPROM.offsetX.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.offsetXText.Text, out double value); return value; }));
            EEPROM.offsetY.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.offsetYText.Text, out double value); return value; }));
            EEPROM.offsetZ.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.offsetZText.Text, out double value); return value; }));
            EEPROM.A.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.AText.Text, out double value); return value; }));
            EEPROM.B.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.BText.Text, out double value); return value; }));
            EEPROM.C.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.CText.Text, out double value); return value; }));
            EEPROM.DA.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.DAText.Text, out double value); return value; }));
            EEPROM.DB.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.DBText.Text, out double value); return value; }));
            EEPROM.DC.Value = Convert.ToSingle(this.Invoke((Func<double>)delegate { Double.TryParse(this.DCText.Text, out double value); return value; }));

            EEPROMFunctions.SendEEPROM();
        }

        private void ReadEEPROM_Click(object sender, EventArgs e)
        {
            if (Connection.serialManager.CurrentState == ConnectionState.Connected)
            {
                EEPROMFunctions.tempEEPROMSet = false;
                EEPROMFunctions.ReadEEPROM();
                EEPROMFunctions.EEPROMReadOnly = true;
                Heights.checkHeightsOnly = false;
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
            if (EEPROMFunctions.tempEEPROMSet == false)
            {
                EEPROMFunctions.ReadEEPROM();
            }

            GCode.checkHeights = true;
            EEPROMFunctions.EEPROMReadOnly = false;
            Calibration.calibrationState = true;
            Calibration.calibrationSelection = Calibration.CalibrationType.NORMAL;
            Heights.checkHeightsOnly = true;
            Heights.heightsSet = false;
        }

        private void StopBut_Click(object sender, EventArgs e)
        {
            if (Connection.serialManager.CurrentState == ConnectionState.Connected)
            {
                Connection.serialManager.ClearOutBuffer();
                GCode.TrySend(GCode.Command.RESET);
                Connection.Disconnect();
                Printer.isCalibrating = false;
                Connection.Connect();
            }
        }

        private void ManualCalibrateBut_Click(object sender, EventArgs e)
        {
            try
            {
                Calibration.calibrationState = true;

                Program.mainFormTest.SetUserVariables();

                Heights.X = Convert.ToSingle(xManual.Text);
                Heights.XOpp = Convert.ToSingle(xOppManual.Text);
                Heights.Y = Convert.ToSingle(yManual.Text);
                Heights.YOpp = Convert.ToSingle(yOppManual.Text);
                Heights.Z = Convert.ToSingle(zManual.Text);
                Heights.ZOpp = Convert.ToSingle(zOppManual.Text);

                EEPROM.stepsPerMM.Value = Convert.ToSingle(spmMan.Text);
                EEPROM.tempSPM = Convert.ToSingle(spmMan.Text);
                EEPROM.zMaxLength.Value = Convert.ToSingle(zMaxMan.Text);
                EEPROM.zProbeHeight.Value = Convert.ToSingle(zProHeiMan.Text);
                EEPROM.zProbeSpeed.Value = Convert.ToSingle(zProSpeMan.Text);
                EEPROM.HRadius.Value = Convert.ToSingle(horRadMan.Text);
                EEPROM.diagonalRod.Value = Convert.ToSingle(diaRodMan.Text);
                EEPROM.offsetX.Value = Convert.ToSingle(towOffXMan.Text);
                EEPROM.offsetY.Value = Convert.ToSingle(towOffYMan.Text);
                EEPROM.offsetZ.Value = Convert.ToSingle(towOffZMan.Text);
                EEPROM.A.Value = Convert.ToSingle(alpRotAMan.Text);
                EEPROM.B.Value = Convert.ToSingle(alpRotBMan.Text);
                EEPROM.C.Value = Convert.ToSingle(alpRotCMan.Text);
                EEPROM.DA.Value = Convert.ToSingle(delRadAMan.Text);
                EEPROM.DB.Value = Convert.ToSingle(delRadBMan.Text);
                EEPROM.DC.Value = Convert.ToSingle(delRadCMan.Text);

                Calibration.BasicCalibration();

                //set eeprom vals in manual calibration
                this.spmMan.Text = EEPROM.stepsPerMM.ToString();
                this.zMaxMan.Text = EEPROM.zMaxLength.ToString();
                this.zProHeiMan.Text = EEPROM.zProbeHeight.ToString();
                this.zProSpeMan.Text = EEPROM.zProbeSpeed.ToString();
                this.diaRodMan.Text = EEPROM.diagonalRod.ToString();
                this.horRadMan.Text = EEPROM.HRadius.ToString();
                this.towOffXMan.Text = EEPROM.offsetX.ToString();
                this.towOffYMan.Text = EEPROM.offsetY.ToString();
                this.towOffZMan.Text = EEPROM.offsetZ.ToString();
                this.alpRotAMan.Text = EEPROM.A.ToString();
                this.alpRotBMan.Text = EEPROM.B.ToString();
                this.alpRotCMan.Text = EEPROM.C.ToString();
                this.delRadAMan.Text = EEPROM.DA.ToString();
                this.delRadBMan.Text = EEPROM.DB.ToString();
                this.delRadCMan.Text = EEPROM.DC.ToString();

                //set expected height map
                this.xExp.Text = Heights.X.ToString();
                this.xOppExp.Text = Heights.XOpp.ToString();
                this.yExp.Text = Heights.Y.ToString();
                this.yOppExp.Text = Heights.YOpp.ToString();
                this.zExp.Text = Heights.Z.ToString();
                this.zOppExp.Text = Heights.ZOpp.ToString();


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
            if(Connection.serialManager != null)
            {
                Connection.serialManager.Dispose();
            }
        }
    }
}
