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

  Messages
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
    
  </code><pre>
