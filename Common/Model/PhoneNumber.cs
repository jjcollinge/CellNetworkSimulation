using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class PhoneNumber : IComparable<PhoneNumber>, IEquatable<PhoneNumber>
    {
        [DataMember]
        private long _phoneNumber;

        public PhoneNumber(long phoneNumber)
        {
            ValidatePhoneNumber(phoneNumber);
        }

        private void ValidatePhoneNumber(long phoneNumber)
        {
            var length = Math.Floor(Math.Log10(phoneNumber) + 1);

            if (length != 11)
            {
                throw new ArgumentException("A phone number can only be 11 numbers long and cannot start with a 0");
            }

            _phoneNumber = phoneNumber;
        }

        public int CompareTo(PhoneNumber other)
        {
            if (other == null)
                return 1;

            if (this._phoneNumber == other._phoneNumber)
                return 0; 
            else if (this._phoneNumber < other._phoneNumber)
                return -1;
            else
                return 1;
        }

        public static bool operator <(PhoneNumber a, PhoneNumber b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >(PhoneNumber a, PhoneNumber b)
        {
            return a.CompareTo(b) > 0;
        }

        public bool Equals(PhoneNumber other)
        {
            return CompareTo(other) == 0 ? true: false;
        }

        public override string ToString()
        {
            return _phoneNumber.ToString();
        }
    }
}
