using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Hapcon.Messages
{
    public abstract class Message : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged interface member
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void Set<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public MessageType Type;

        private static Regex P2D = new Regex(@"(([-+]?\d*(\.?\d*),){3})([-+]?\d*(\.?\d*){1})", RegexOptions.IgnorePatternWhitespace);
        private static Regex D2P = new Regex(@"(([-+]?\d*(\.?\d*),){5})([-+]?\d*(\.?\d*){1})", RegexOptions.IgnorePatternWhitespace);

        //Message Match 
        public static Message Parse(string msg)
        {
            if (P2D.IsMatch(msg))
            {
                return new P2DMessage(msg);
            }
            if (D2P.IsMatch(msg))
            {
                return new D2PMessage(msg);
            }

            throw new Exception("Unknown string");
        }

        //Message Type Check
        public static bool TryParse(string msg, out Message result)
        {
            result = null;

            var q1 = P2D.IsMatch(msg);
            var q2 = D2P.IsMatch(msg);

            Match match = D2P.Match(msg);
            if (!string.IsNullOrEmpty(match.Value))
            {
                result = new D2PMessage(match.Value);
                return true;
            }
            else
            {
                match = P2D.Match(msg);

                if (!string.IsNullOrEmpty(match.Value))
                {
                    result = new P2DMessage(match.Value);
                    return true;
                }
            }

            return false;
        }
    }
}
