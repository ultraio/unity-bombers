using System.Collections.Generic;

namespace Ultraio
{
    public class Permission
    {
        public List<string> global;
    }

    public class Referral
    {
        public string code;
    }

    public class UserInfo
    {
        public string sub;
        public bool email_verified;
        public string blockchain_id;
        public string countryCode;
        public string name;
        public string preferred_username;
        public string given_name;
        public string family_name;
        public string email;
        public string username;
    }


}