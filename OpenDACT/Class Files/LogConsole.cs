using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenDACT.Class_Files {
    
    public class LogConsole {
        private RichTextBox ConsoleUI;
        public LogLevel ConsoleLogLevel = LogLevel.NORMAL;

        public LogConsole(RichTextBox targetUIElement) {
            this.ConsoleUI = targetUIElement;
        }

        public void Log(string message, LogLevel logLevel = LogLevel.NORMAL) {
            if (!UserVariables.isInitiated)
                return;

            if (logLevel == LogLevel.NORMAL || (logLevel == LogLevel.DEBUG && this.ConsoleLogLevel == LogLevel.DEBUG))
                this.ConsoleUI.Invoke(new Action(
                    () => {
                        this.ConsoleUI.AppendText(message + "\n");
                        this.ConsoleUI.ScrollToCaret();
                        }
                    ));
        }

        public enum LogLevel {
            NORMAL,
            DEBUG
        }
    }
}
