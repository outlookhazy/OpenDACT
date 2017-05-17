using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using System.Collections.Concurrent;

namespace OpenDACT.Class_Files
{
    static class Threading
    {
        public static bool _continue = true;
        public static bool isCalibrating = true;

        static ConcurrentQueue<string> readLineData = new ConcurrentQueue<string>();

        public static void Read()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");

            while (_continue)
            {
                try
                {
                    string message = Connection._serialPort.ReadLine();

                    UserInterface.printerLog.Log(message);

                    //DecisionHandler.handleInput(message);
                    readLineData.Enqueue(message);
                }
                catch (TimeoutException) { }
            }//end while
        }//end void

        public static void HandleRead()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");

            while (_continue)
            {
                try
                {                   
                    while (isCalibrating && readLineData.Any())
                    {
                        //wait for ok to perform calculation?
                        UserVariables.isInitiated = true;
                        string line;
                        while (!readLineData.TryDequeue(out line)) { /* spin */ }        

                        DecisionHandler.HandleInput(line);                        
                    }//end while
                }
                catch (Exception) { }
            }//end while continue
        }

    }
}
