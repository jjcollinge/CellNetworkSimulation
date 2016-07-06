using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class EmailAddress
    {
        [IgnoreDataMember]
        private readonly String EMAIL_REGEX = "";
        [DataMember]
        private String _email;

        public EmailAddress(String email)
        {
            ValidateEmail(email);
            _email = email;
        }

        public EmailAddress(EmailAddress email)
        {
            _email = email.ToString();
        }

        private void ValidateEmail(string email)
        {
            if (!Regex.IsMatch(email, EMAIL_REGEX))
            {
                throw new ArgumentException("Invalid email provided");
            }
        }

        public override String ToString()
        {
            return _email;
        }

    }
}
