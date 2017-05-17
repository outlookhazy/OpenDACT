﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;

namespace OpenDACT.Class_Files
{
    static class Threading
    {
        public static bool _continue = true;
        public static bool isCalibrating = true;

        static List<string> readLineData = new List<string>();

        public static void Read()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");

            while (_continue)
            {
                try
                {
                    string message = Connection._serialPort.ReadLine();

                    UserInterface.LogPrinter(message);

                    //DecisionHandler.handleInput(message);
                    readLineData.Add(message);
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
                    bool hasData;
                    lock (readLineData)
                    {
                        hasData = readLineData.Any();
                    }

                    while (isCalibrating && hasData)
                    {
                        //wait for ok to perform calculation?
                        UserVariables.isInitiated = true;
                        bool canMove;

                        if (readLineData.First().Contains("wait"))
                        {
                            canMove = true;
                        }
                        else
                        {
                            canMove = false;
                        }

                        DecisionHandler.HandleInput(readLineData.First(), canMove);
                        readLineData.RemoveAt(0);
                    }//end while
                }
                catch (Exception) { }
            }//end while continue
        }

    }
}
