using Hapcon.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace Hapcon
{
    public class HapconController
    {
        #region Singleton pattern
        private static object LockObject = new object();
        private static HapconController _Instance;
        public static HapconController Instance
        {
            get
            {
                lock (LockObject)
                {
                    return _Instance ?? (_Instance = new HapconController());
                }
            }
        }
        #endregion

        #region Event

        //Serial Data Event
        public delegate void DataReceivedEventHandler(object sender, Message msg);
        public event DataReceivedEventHandler DataReceivedEvent;

        //Connect Event
        public delegate void ConnectedEventHandler(object sender, PortType portType);
        public event ConnectedEventHandler ConnectedEvent;

        //Error Event
        public delegate void ErrorEventHandler(object sender, string message);
        public event ErrorEventHandler ErrorEvent;

        //Disconnect event
        public delegate void DisconnectedEventHanlder(object sender, PortType portType);
        public event DisconnectedEventHanlder DisconnectedEvent;
        #endregion

        #region Fields
        #region Constant
        //Connecting Protocol
        public const int HapconBaudRate = 115200;
        public const int HapconDataBits = 8;
        public const Parity HapconParity = Parity.None;
        public const StopBits HapconStopBits = StopBits.One;
        #endregion

        private readonly TimeSpan FindTimeout = TimeSpan.FromSeconds(2);

        private HapconSerialPort HapconPort1;
        private HapconSerialPort HapconPort2;

        // 연결된 포트별로 사용되고 있는 PortName 저장
        private Dictionary<PortType, string> CachedPortName = new Dictionary<PortType, string>();
        private BackgroundWorker ConnectWorker;
        #endregion

        #region Properties
        public bool IsPort1Open { get => HapconPort1 == null ? false : HapconPort1.IsOpen; }
        public bool IsPort2Open { get => HapconPort2 == null ? false : HapconPort2.IsOpen; }
        #endregion

        #region Constructor
        private HapconController() { }
        #endregion

        #region Methods

        //SerialPort Open
        public void Open(PortType port)
        {
            if (ConnectWorker != null && ConnectWorker.IsBusy) { return; }

            ConnectWorker = new BackgroundWorker();
            ConnectWorker.DoWork += ConnectWorker_DoWork;
            ConnectWorker.RunWorkerCompleted += ConnectWorker_RunWorkerCompleted;

            ConnectWorker.RunWorkerAsync(port);
        }

        
        public async Task<bool> OpenAsync(PortType portType)
        {
            bool result = false;

            await Task.Run(() =>
            {
                string portName = GetPort(portType);

                if (!string.IsNullOrEmpty(portName))
                {
                    result = true;

                    switch (portType)
                    {
                        case PortType.Port1:
                            HapconPort1 = new HapconSerialPort(portType, portName, HapconBaudRate, HapconParity, HapconDataBits, HapconStopBits);
                            HapconPort1.DataReceivedEvent += HapconPort1_DataReceivedEvent;
                            HapconPort1.DisconnectedEvent += HapconPort1_DisconnectedEvent;
                            HapconPort1.Open();
                            break;
                        case PortType.Port2:
                            HapconPort2 = new HapconSerialPort(portType, portName, HapconBaudRate, HapconParity, HapconDataBits, HapconStopBits);
                            HapconPort2.DataReceivedEvent += HapconPort2_DataReceivedEvent;
                            HapconPort2.DisconnectedEvent += HapconPort2_DisconnectedEvent;
                            HapconPort2.Open();
                            break;
                    }

                    ConnectedEvent?.Invoke(this, portType);
                }
            });

            return result;
        }

        //serial Port Close 
        public void Close(PortType port)
        {
            switch (port)
            {
                case PortType.Port1:
                    if (HapconPort1 != null && HapconPort1.IsOpen)
                    {
                        HapconPort1.DataReceivedEvent -= HapconPort1_DataReceivedEvent;
                        HapconPort1.DisconnectedEvent -= HapconPort1_DisconnectedEvent;
                        HapconPort1.Close();

                        HapconPort1.Dispose();

                        CachedPortName.Remove(port);
                    }
                    break;
                case PortType.Port2:
                    if (HapconPort2 != null && HapconPort2.IsOpen)
                    {
                        HapconPort2.DataReceivedEvent -= HapconPort2_DataReceivedEvent;
                        HapconPort2.DisconnectedEvent -= HapconPort2_DisconnectedEvent;
                        HapconPort2.Close();

                        HapconPort2.Dispose();

                        CachedPortName.Remove(port);
                    }
                    break;
            }
        }

        private string GetPort(PortType portType)
        {
            // 연결된 포트번호 
            List<string> ports = CachedPortName.Values.ToList();
            // 연결가능한 포트리스트
            string[] availablePorts = SerialPort.GetPortNames();

            for (int i = 0; i < availablePorts.Length; i++)
            {
                string portName = availablePorts[i];
                SerialPort port = null;

                // 현재 포트가 이미 연결된 포트에 포함되면 Skip
                if (ports.Contains(portName)) { continue; }

                try
                {
                    port = new SerialPort(portName, HapconBaudRate, HapconParity, HapconDataBits, HapconStopBits);
                    port.Open();

                    // 포트에 연결했을 때 데이터가 넘어오는지 확인하기 위한 변수
                    string test = string.Empty;


                    // Timeout 적용을 위한 타이머
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    // Timeout까지 무한 루프
                    // 연결 후 데이터가 바로 안들어 올 수 있으니 무한루프로 계속 확인
                    while (true)
                    {
                        // 연결 후 읽을 수 있는 데이터 가져옴
                        test = port.ReadExisting();


                        // 받은 데이터가 Message형태인지 확인필요
                        // 메세지가 맞다면 이라는 조건 추가 해야 함 
                        // 아래의 조건은 이상한 포트에 연결되었는데 계가 데이터 전달 해 줘도 찾은걸로 판단함 


                        // 받은 데이터가 있으면
                        if (!string.IsNullOrEmpty(test))
                        {
                            // 포트 순서, 포트명 저장
                            CachedPortName.Add(portType, portName);
                            return portName;
                        }

                        // Timeout때 까지 데이터를 받지 못했으면 break 다음 포트로 계속
                        if (sw.Elapsed > FindTimeout)
                        {
                            break; // 여기서 break는 위의 while문을 끊음. for문은 안끊음.
                        }
                    }

                    sw.Stop();
                }
                catch { }
                finally
                {
                    // 찾았든 못찾았든 테스트하기 위한 port초기화.
                    port.Close();
                    port.Dispose();
                }
            }

            return null;
        }
        #endregion

        #region Event handler
        private void ConnectWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            PortType portType = (PortType)e.Argument;
            // Port Name Search
            string portName = GetPort(portType);

            if (string.IsNullOrEmpty(portName))
            {
                e.Cancel = true;
                return;
            }

            object[] result = new object[2];
            result[0] = portType;
            result[1] = portName;

            e.Result = result;
        }

        private void ConnectWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null && e.Result != null && e.Result is object[] result)
            {
                PortType portType = (PortType)result[0];
                string portName = (string)result[1];

                switch (portType)
                {
                    case PortType.Port1:
                        HapconPort1 = new HapconSerialPort(portType, portName, HapconBaudRate, HapconParity, HapconDataBits, HapconStopBits);
                        HapconPort1.DataReceivedEvent += HapconPort1_DataReceivedEvent;
                        HapconPort1.DisconnectedEvent += HapconPort1_DisconnectedEvent;
                        HapconPort1.Open();
                        break;
                    case PortType.Port2:
                        HapconPort2 = new HapconSerialPort(portType, portName, HapconBaudRate, HapconParity, HapconDataBits, HapconStopBits);
                        HapconPort2.DataReceivedEvent += HapconPort2_DataReceivedEvent;
                        HapconPort2.DisconnectedEvent += HapconPort2_DisconnectedEvent;
                        HapconPort2.Open();
                        break;
                }

                ConnectedEvent?.Invoke(this, portType);
            }
            else
            {
                ErrorEvent?.Invoke(this, "연결 실패");
            }

            ConnectWorker.Dispose();
            ConnectWorker.DoWork -= ConnectWorker_DoWork;
            ConnectWorker.RunWorkerCompleted -= ConnectWorker_RunWorkerCompleted;
            ConnectWorker = null;
        }

        // Serialport 1 Discoonect Event
        private void HapconPort1_DisconnectedEvent(object sender, PortType portType)
        {
            CachedPortName.Remove(PortType.Port1);
            DisconnectedEvent?.Invoke(this, PortType.Port1);
        }
        // Serialport 2 Disconnect Event
        private void HapconPort2_DisconnectedEvent(object sender, PortType portType)
        {
            CachedPortName.Remove(PortType.Port2);
            DisconnectedEvent?.Invoke(this, PortType.Port2);
        }
        // Serialport 1 Datareceive Event
        private void HapconPort1_DataReceivedEvent(object sender, Message msg)
        {
            DataReceivedEvent?.Invoke(sender, msg);
            Debug.WriteLine($"[{msg.Type}] {msg}");
        }
        // Serialport 2 Datareceive Event
        private void HapconPort2_DataReceivedEvent(object sender, Message msg)
        {
            DataReceivedEvent?.Invoke(sender, msg);
            Debug.WriteLine($"[{msg.Type}] {msg}");
        }
        #endregion
    }
}