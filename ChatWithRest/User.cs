using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chat
{
    class User
    {
        public string UserName { get; } = "";

        public User(string username)
        {
            UserName = username;
        }
    }
}
