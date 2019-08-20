using Hapcon.Messages;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using static Hapcon.HapconController;

namespace Hapcon
{
    public class HapconSerialPort : IDisposable
    {
        #region Events
        // 데이터를 성공적으로 파싱하였을 때 발생하는 이벤트
        public delegate void DataReceivedEventHandler(object sender, Message msg);
        public event DataReceivedEventHandler DataReceivedEvent;

        public event ConnectedEventHandler ConnectedEvent;
        public event ErrorEventHandler ErrorEvent;
        public event DisconnectedEventHanlder DisconnectedEvent;
        #endregion

        #region Fields
        private SerialPort InnerSerialPort;

        private const string CommandPrefix = "AT";
        private const string CarriageReturn = "\r\n";
        private const char NewLine = '\n';
        private const string CONNECT = "+CONNECT="; // 연결  ex) AT+CONNECT=MAC주소
        private const string RESET = "Z"; //  장비 초기화 
        private const string UART = "+UART=115200"; 

        private StringBuilder Buffer = new StringBuilder();

        private bool IsDisposed;

        public bool IsOpen { get; private set; }

        private int BaudRate;
        private Parity Parity;
        private int DataBits;
        private StopBits StopBits;
        private string PortName;

        public PortType PortType { get; private set; }

        private TimeSpan Timeout = TimeSpan.FromMilliseconds(1000);
        private TimeSpan DataReceivedTime;
        private Stopwatch DataReceiveTimer;
        private DispatcherTimer DataIntervalTimer;
        #endregion

        #region Constructor
        internal HapconSerialPort(PortType type, string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            PortType = type;
            BaudRate = baudRate;
            Parity = parity;
            DataBits = dataBits;
            StopBits = stopBits;
            PortName = portName;

            DataReceiveTimer = new Stopwatch();
            DataIntervalTimer = new DispatcherTimer();
            DataIntervalTimer.Interval = Timeout;
            DataIntervalTimer.Tick += DataIntervalTimer_Tick;
        }

        ~HapconSerialPort()
        {
            Dispose(false);
        }
        #endregion

        #region Methods

        //SerialPort Open 
        public void Open()
        {
            if (IsOpen) { return; }
            if (IsDisposed) { throw new Exception("Dispose된 객체"); }

            try
            {
                Close();

                InnerSerialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
                InnerSerialPort.Open();
                InnerSerialPort.DataReceived += InnerSerialPort_DataReceived;

                IsOpen = true;
                ConnectedEvent?.Invoke(this, PortType);
                //InvokeCommand("000", "050", 2, 9, 50);

                DataReceiveTimer.Start();
                DataIntervalTimer.Start();
            }
            catch (Exception e)
            {
                InnerSerialPort = null;
                IsOpen = false;
                ErrorEvent?.Invoke(this, e.Message);
                MessageBox.Show(e.Message);
            }
        }

        //Serial Port Close
        public void Close()
        {
            if (!IsOpen) { return; }
            if (IsDisposed) { throw new Exception("Dispose된 객체"); }
            IsOpen = false;

            DataReceiveTimer.Stop();
            DataIntervalTimer.Stop();
            DataReceivedTime = TimeSpan.FromSeconds(0);
            InnerSerialPort.Close();
            InnerSerialPort.DataReceived -= InnerSerialPort_DataReceived;

            InnerSerialPort = null;
        }

        //P2D InvokeCommand
        public void InvokeCommand(string wheel, string button, int cmd, int vib, int interval)
        {
            var q = $"<{wheel},{button},{vib.ToString("000")},{interval.ToString("000")}>\n\r";
            try
            {
                InnerSerialPort.Write($"<{wheel},{button},{cmd},{vib.ToString("000")},{interval.ToString("000")}>\n\r");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        #endregion

        #region Event handler

        //Serial DATA Process
        private void InnerSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            char[] charArray;

            // 1. SerialPort에 들어온 데이터 전부를 Buffer에 담음.
            Buffer.Append(InnerSerialPort.ReadExisting());

            int startTokenIndex = -1;
            if (Buffer.Length > 0 && Buffer[0] != '<')
            {
                for (int i = 0; i < Buffer.Length; i++)
                {
                    if (Buffer[i] == '<')
                    {
                        startTokenIndex = i;
                        break;
                    }
                }

                if (startTokenIndex != -1)
                {
                    Buffer.Remove(0, startTokenIndex);
                }
            }

            for (int i = 0; i < Buffer.Length; i++)
            {
                // 2. 버퍼에서 '\n' 검색
                if (Buffer[i] == NewLine)
                {
                    // 3. '\n'이 검색되었으면 검색된 Index만큼 배열 생성
                    charArray = new char[i - 1];
                    // 4. 생성한 배열에 0 부터 Index까지 복사
                    Buffer.CopyTo(0, charArray, 0, i - 1);
                    string news = new string(charArray);

                    // 5. 복사한 문자열을 정규식으로 우리가 원하는 형태의 데이터인지 검사
                    // 7. 메세지 파싱
                    if (Message.TryParse(news, out Message msg))
                    {
                        // 8. 이벤트 호출
                        DataReceivedEvent?.Invoke(this, msg);
                        DataReceivedTime = DataReceiveTimer.Elapsed;
                    }
                    else
                    {
                        //
                    }

                    // 10. 위에서 처리한 문자열만큼 제거 후 0부터 다시 시작
                    Buffer.Remove(0, i + 1);
                    i = 0;
                }

            }
        }

        // 2번째 방안
        //private void InnerSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    while (InnerSerialPort.BytesToRead > 0)
        //    {
        //        char c = (char)InnerSerialPort.ReadChar();
        //        if (c == '\n' || c == '\r') { continue; }

        //        if (c == '>')
        //        {
        //            Buffer.Append(c);
        //            string news = Buffer.ToString();

        //            if (Message.TryParse(news, out Message msg))
        //            {
        //                // 8. 이벤트 호출
        //                DataReceivedEvent?.Invoke(msg);

        //            }

        //            Buffer.Clear();
        //        }
        //        else
        //        {
        //            Buffer.Append(c);
        //        }

        //    }
        //}

        //Data Interval 
        private void DataIntervalTimer_Tick(object sender, EventArgs e)
        {
            if (DataReceiveTimer.Elapsed - DataReceivedTime > Timeout)
            {
                DisconnectedEvent?.Invoke(this, PortType);
                Close();
            }
        }
        #endregion

        #region IDisposable interface member
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (IsDisposed) { return; }

            if (disposing)
            {
                DataIntervalTimer.Stop();
                DataIntervalTimer.Tick -= DataIntervalTimer_Tick;

                if (InnerSerialPort != null)
                {
                    InnerSerialPort.DataReceived -= InnerSerialPort_DataReceived; InnerSerialPort.Close();
                    InnerSerialPort.Dispose();
                    InnerSerialPort = null;
                }

                if (DataIntervalTimer != null)
                {
                    DataIntervalTimer.Stop();
                    DataIntervalTimer = null;
                }
            }

            IsDisposed = true;
        }
        #endregion
    }
}