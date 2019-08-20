namespace Hapcon.Messages
{
    //Device to PC
    public class D2PMessage : Message
    {
        
        #region Button : double
        private double _Button;
        public double Button
        {
            get => _Button;
            set => Set(ref _Button, value);
        }
        #endregion

        #region FSR : double
        private double _FSR;
        public double FSR
        {
            get => _FSR;
            set => Set(ref _FSR, value);
        }
        #endregion

        #region Encoder : double
        private double _Encoder;
        public double Encoder
        {
            get => _Encoder;
            set => Set(ref _Encoder, value);
        }
        #endregion

        #region Roll : double
        private double _Roll;
        public double Roll
        {
            get => _Roll;
            set => Set(ref _Roll, value);
        }
        #endregion

        #region Pitch : double
        private double _Pitch;
        public double Pitch
        {
            get => _Pitch;
            set => Set(ref _Pitch, value);
        }
        #endregion

        #region Yaw : double
        private double _Yaw;
        public double Yaw
        {
            get => _Yaw;
            set => Set(ref _Yaw, value);
        }
        #endregion

        //Split String
        public D2PMessage(string msg)
        {
            Type = MessageType.D2P;

            string[] split = msg.Split(',');

            Button = double.Parse(split[0]);
            FSR = double.Parse(split[1]);
            Encoder = double.Parse(split[2]);
            Roll = double.Parse(split[3]);
            Pitch = double.Parse(split[4]);
            Yaw = double.Parse(split[5]);
        }

        //Device to PC Protocol 
        public override string ToString()
        {
            return $"Button = {Button}, FSR = {FSR}, Encoder = {Encoder}, Roll = {Roll}, Pitch = {Pitch}, Yaw = {Yaw}";
        }
    }
}
