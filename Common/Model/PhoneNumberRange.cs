using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class PhoneNumberRange
    {
        [DataMember]
        public long Max { get; private set; }
        [DataMember]
        public long Min { get; private set; }
        [DataMember]
        private long _currentOffset = 0;

        public PhoneNumberRange(long min, long max)
        {
            ValidatePhoneNumbers(min, max);
            Min = min;
            Max = max;
        }

        private void ValidatePhoneNumbers(long min, long max)
        {
            new PhoneNumber(min);
            new PhoneNumber(max);
            // Exception thrown if invalid
        }

        public PhoneNumber AllocatePhoneNumber()
        {
            PhoneNumber phoneNumber = null;
            if(Min + _currentOffset < Max)
            {
                //_currentOffset = Interlocked.Increment(ref _currentOffset);
                phoneNumber = new PhoneNumber(Min + _currentOffset++);
            }
            return phoneNumber;
        }
    }
}
