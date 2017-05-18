using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using System.Globalization;

namespace OpenDACT.Class_Files
{
    static class Connection
    {
        public static SerialManager serialManager;

        public static void Init()
        {
            Connection.serialManager = new SerialManager();
            Connection.serialManager.SerialConnectionChanged += SerialManager_SerialConnectionChanged;
            Connection.serialManager.NewSerialLine += SerialManager_NewSerialLine;
        }

        public static void Connect()
        {
            if (serialManager.CurrentState == ConnectionState.Connected) {
                UserInterface.consoleLog.Log("Already Connected");
            } else {
                try {                   
                    string PortName = Program.mainFormTest.portsCombo.Text;
                    int BaudRate = int.Parse(Program.mainFormTest.baudRateCombo.Text, CultureInfo.InvariantCulture);
                    
                    // Open the serial port and start reading on a reader thread.
                    // _continue is a flag used to terminate the app.

                    UserInterface.consoleLog.Log(Program.mainFormTest.portsCombo.Text);

                    if (PortName != "" && BaudRate != 0)
                    {
                        UserInterface.consoleLog.Log("Connecting");
                        Connection.serialManager.Connect(PortName, BaudRate);                       
                    }
                    else
                    {
                        UserInterface.consoleLog.Log("Please fill all text boxes above");
                    }
                }
                catch (Exception e1)
                {
                    UserInterface.consoleLog.Log(e1.Message);
                    serialManager.Disconnect();
                }
            }
        }

        private static void SerialManager_NewSerialLine(object sender, string data)
        {
            UserInterface.printerLog.Log(String.Format("Received: {0}", data), LogConsole.LogLevel.DEBUG);
            DecisionHandler.HandleInput(data);
        }

        private static void SerialManager_SerialConnectionChanged(object sender, ConnectionState newState)
        {
            UserInterface.consoleLog.Log(String.Format("Serial {0}",newState));
        }

        public static void Disconnect()
        {
            if (serialManager.CurrentState == ConnectionState.Connected)
            {
                serialManager.Disconnect();
            }
            else
            {
                UserInterface.consoleLog.Log("Not Connected");
            }
        }
    }
}
