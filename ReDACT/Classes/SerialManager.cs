using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDACT.Class_Files
{
    public class SerialManager : IDisposable
    {
        private SerialPort _port;
        private string _readBuffer;
        private Object _bufferlock;

        private Queue<String> _readQueue;
        private Object _queuelock;

        public ConnectionState CurrentState { get; private set; }

        public event SerialConnectionChangedEventHandler SerialConnectionChanged;
        public event NewSerialLineEventHandler NewSerialLine;

        public bool closePending = false;

        public SerialManager() {
            this._readBuffer = "";
            this._bufferlock = new object();

            this._readQueue = new Queue<string>();
            this._queuelock = new object();
            this.CurrentState = ConnectionState.DISCONNECTED;
        }

        public bool Connect(string Port, int Baud) {
            if (this.CurrentState == ConnectionState.DISCONNECTED)
            {
                this.closePending = false;
                try
                {
                    this._port = new SerialPort(Port, Baud)
                    {
                        ReadTimeout = 500,
                        WriteTimeout = 500
                    };
                    this._port.ErrorReceived += _port_ErrorReceived;
                    this._port.DataReceived += _port_DataReceived;
                    this._port.Open();
                    this.CurrentState = ConnectionState.Connected;
                    this.OnConnectionStateChanged(this.CurrentState);
                    return true;

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                }
            }

            return false;
        }

        public bool Disconnect()
        {
            closePending = true;
            if(this._port != null)
            {
                this._port.ErrorReceived -= this._port_ErrorReceived;
                this._port.DataReceived -= this._port_DataReceived;
                if (this._port.IsOpen)
                {                  
                    //this._port.ReadExisting();
                    this._port.Close();
                    this._port = null; //dispose of local reference
                }                
                this.CurrentState = ConnectionState.DISCONNECTED;
                this.OnConnectionStateChanged(ConnectionState.DISCONNECTED);
                return true;
            }
            return false;
        }

        public bool WriteLine(string text)
        {
            if (this.CurrentState == ConnectionState.Connected)
            {
                this._port.WriteLine(text);
                return true;
            }else
            {
                return false;
            }
        }

        private void _port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            this.Disconnect();
        }

        private void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (this._bufferlock) {
                try
                {
                    if (!this.closePending)
                        this._readBuffer += _port.ReadExisting();
                } catch (Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                    //ignore case where port is closed before data can be read
                }
            }
            ProcessBufferContents();
            ProcessQueueContents();
        }

        private void ProcessBufferContents() {
            lock (this._bufferlock)
            {
                while (this._readBuffer.Contains('\n')) {

                    //does the buffer end in newline
                    bool enqueueLast = (this._readBuffer.LastIndexOf('n') == (this._readBuffer.Length - 1));

                    //split buffer based on newline
                    string[] commandArray = this._readBuffer.Split('\n');


                    lock (_queuelock)
                    {
                        //iterate through all but final element (if exist)
                        for (int i = 0; i < commandArray.Length - 1; i++)
                        {
                            this._readQueue.Enqueue(commandArray[i]);
                        }

                        //special handling for final element - assumes at least one element produced by split
                        if (enqueueLast)
                        {
                            this._readQueue.Enqueue(commandArray[commandArray.Length - 1]);
                            this._readBuffer = "";
                        }
                        else
                        {
                            this._readBuffer = commandArray[commandArray.Length - 1];
                        }
                    }

                }
            }
        }

        private void ProcessQueueContents()
        {
            lock (_queuelock)
            {
                while(_readQueue.Count > 0)
                {
                    this.OnNewLine(_readQueue.Dequeue());
                }
            }
        }

        protected virtual void OnNewLine(string e)
        {            
            NewSerialLine?.Invoke(this, String.Copy(e));
        }

        protected virtual void OnConnectionStateChanged(ConnectionState e)
        {
            SerialConnectionChanged?.Invoke(this, e);
        }

        public void Dispose()
        {
            try
            {
                this.Disconnect();
            } catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }

    public enum ConnectionState
    {
        Connected,
        DISCONNECTED
    }

    public delegate void NewSerialLineEventHandler(object sender, string data);
    public delegate void SerialConnectionChangedEventHandler(object sender, ConnectionState newState);
}
