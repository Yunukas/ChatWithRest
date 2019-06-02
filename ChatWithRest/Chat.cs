using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chat
{
    public class Chat
    {
        public string Model { get; set; }
        public int Pk { get; set; }
        public FieldsClass Fields { get; set; }

        public class FieldsClass
        {
            public string Message { get; set; }
            public string Username { get; set; }
            public DateTime Updated_At { get; set; }
            public DateTime Created_At { get; set; }


        }

    }
}
