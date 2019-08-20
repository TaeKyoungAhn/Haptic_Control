# Hapcon

Haptic Controller API
=====================
This project is an API for Haptic Control.
-------------------------------------------------------


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

  D2PMessage.cs
  <pre><code>
  
    //Device to PC Protocol 
        public override string ToString()
        {
            return $"Button = {Button}, FSR = {FSR}, Encoder = {Encoder}, Roll = {Roll}, Pitch = {Pitch}, Yaw = {Yaw}";
        }
        
  </code><pre>
  
  P2DMessage.cs
  <pre><code>
  
   //P2D Serial Protocol
        public override string ToString()
        {
            return $"MRWheel = {MRWheel}, MRButton = {MRButton},LRACommand = {LRACommand} ,LRA1 = {LRA1}, LRA2 = {LRA2}";
        }
        
  </code><pre>
  
  MessageType.cs
   <pre><code>
 
   public enum MessageType
    {
        D2P,
        P2D
    }
    
  </code><pre>
