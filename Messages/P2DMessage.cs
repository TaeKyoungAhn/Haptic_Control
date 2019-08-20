namespace Hapcon.Messages
{
    public class P2DMessage : Message
    {
        #region MRWheel : double
        private double _MRWheel;
        public double MRWheel
        {
            get => _MRWheel;
            set => Set(ref _MRWheel, value);
        }
        #endregion

        #region MRButton : double
        private double _MRButton;
        public double MRButton
        {
            get => _MRButton;
            set => Set(ref _MRButton, value);
        }
        #endregion

        #region LRACommand : double
        private double _LRACommand;
        public double LRACommand
        {
            get => _LRACommand;
            set => Set(ref _LRACommand, value);
        }
        #endregion

        #region LRA1 : double
        private double _LRA1;
        public double LRA1
        {
            get => _LRA1;
            set => Set(ref _LRA1, value);
        }
        #endregion

        #region LRA2 : double
        private double _LRA2;
        public double LRA2
        {
            get => _LRA2;
            set => Set(ref _LRA2, value);
        }
        #endregion


        //P2D Split String 
        internal P2DMessage(string msg)
        {
            Type = MessageType.P2D;

            string[] split = msg.Split(',');

            MRWheel = double.Parse(split[0]);
            MRButton = double.Parse(split[1]);
            LRACommand = double.Parse(split[2]);
            LRA1 = double.Parse(split[3]);
            LRA2 = double.Parse(split[4]);
        }

        //P2D Serial Protocol
        public override string ToString()
        {
            return $"MRWheel = {MRWheel}, MRButton = {MRButton},LRACommand = {LRACommand} ,LRA1 = {LRA1}, LRA2 = {LRA2}";
        }
    }
}
