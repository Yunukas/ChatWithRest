using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWithRest
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
