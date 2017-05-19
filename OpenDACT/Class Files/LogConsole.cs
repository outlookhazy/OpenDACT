using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenDACT.Class_Files {
    
    public class LogConsole {
        private RichTextBox ConsoleUI;
        public LogLevel ConsoleLogLevel = LogLevel.NORMAL;

        public LogConsole(RichTextBox targetUIElement, LogLevel consoleLogLevel = LogLevel.NORMAL) {
            this.ConsoleUI = targetUIElement;
            this.ConsoleLogLevel = consoleLogLevel;
        }

        public void Log(string message, LogLevel logLevel = LogLevel.NORMAL) {
            if (!UserVariables.isInitiated)
                return;

            if (logLevel == LogLevel.NORMAL || this.ConsoleLogLevel == LogLevel.DEBUG)
                try
                {
                    this.ConsoleUI.Invoke(new Action(
                    () => {

                            this.ConsoleUI.AppendText(message + "\n");
                            this.ConsoleUI.ScrollToCaret();
                        
                        }
                    ));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    Debug.WriteLine(message + "\n");
                }
        }

        public enum LogLevel {
            NORMAL,
            DEBUG
        }
    }
}
