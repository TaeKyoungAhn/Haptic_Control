# Hapcon

Haptic Controller API
=====================

# [Environment]
<pre><code>
  Operating system :  Window 10
  Tool : Visual Studio 2017
  </code></pre>
  -----------------------------------------------
# [Nuget Pakage]

   MVVMMLight
   
   ----------------------------------------------
# [Method]

 Message
  <pre><code>
  
  D2PMessages.cs
        //Device to PC Protocol 
        public override string ToString()
        {
            return $"Button = {Button}, FSR = {FSR}, Encoder = {Encoder}, Roll = {Roll}, Pitch = {Pitch}, Yaw = {Yaw}";
        }
        
  
  P2DMessage.cs
  
        //P2D Serial Protocol
        public override string ToString()
        {
            return $"MRWheel = {MRWheel}, MRButton = {MRButton},LRACommand = {LRACommand} ,LRA1 = {LRA1}, LRA2 = {LRA2}";
        }
        
  MessageType.cs
  
        public enum MessageType
        {
           D2P,
           P2D
        }
    </pre></code>

  --------------------------------------------------------------------------------------
  HapconController.cs
  
    # Singleton Pattern
    
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
    
    
    # Event
    
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
        
    
    # Fields 
       
       //Connecting Protocol
        public const int HapconBaudRate = 115200;
        public const int HapconDataBits = 8;
        public const Parity HapconParity = Parity.None;
        public const StopBits HapconStopBits = StopBits.One;
        #endregion
    
    
    # Properties

        public bool IsPort1Open { get => HapconPort1 == null ? false : HapconPort1.IsOpen; }
        public bool IsPort2Open { get => HapconPort2 == null ? false : HapconPort2.IsOpen; }
       
    
    # Constructor
        private HapconController() { }
    
    # Methods
    
        //SerialPort Open
          public void Open(PortType port)
        //SerialPort OpenAsync
          public async Task<bool> OpenAsync(PortType portType)
        //SerialPort Close
           public void Close(PortType port)
        //Get SerialPort
           private string GetPort(PortType portType)
           
           
    # Event Handler
        //Get PortName
        private void ConnectWorker_DoWork(object sender, DoWorkEventArgs e)
        //ReConnectable Handler
        private void ConnectWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        // Serialport 1 Discoonect Event
        private void HapconPort1_DisconnectedEvent(object sender, PortType portType)
        // Serialport 2 Disconnect Event
        private void HapconPort2_DisconnectedEvent(object sender, PortType portType)
        // Serialport 1 Datareceive Event
        private void HapconPort1_DataReceivedEvent(object sender, Message msg)
        // Serialport 2 Datareceive Event
        private void HapconPort2_DataReceivedEvent(object sender, Message msg)
 
----------------------------------------------------------------------------------------------------------
  HapconSerialPort.cs
    
    #Events
    
        // Data Parsing Success Event
        public delegate void DataReceivedEventHandler(object sender, Message msg);
        public event DataReceivedEventHandler DataReceivedEvent;
        //other Event
        public event ConnectedEventHandler ConnectedEvent;
        public event ErrorEventHandler ErrorEvent;
        public event DisconnectedEventHanlder DisconnectedEvent
        
        
   #Fields
   
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
        
   #Constructor
   
       //SerialPort Connection setting
       internal HapconSerialPort(PortType type, string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
       
   #Methods     
   
       //SerialPort Open 
        public void Open()
       //Serial Port Close
        public void Close()
       //P2D InvokeCommand
        public void InvokeCommand(string wheel, string button, int cmd, int vib, int interval)
        
   #Event Handler
       //Serial DATA Process
        private void InnerSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
       //Data Interval 
        private void DataIntervalTimer_Tick(object sender, EventArgs e)
        
   #IDisposable Interface Member
       
        public void Dispose()
        protected void Dispose(bool disposing)
        
       
