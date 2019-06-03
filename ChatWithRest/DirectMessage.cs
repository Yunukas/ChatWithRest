using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWithRest
{
    public class DirectMessage
    {
        public string Model { get; set; }
        public int Pk { get; set; }
        public FieldsClass Fields { get; set; }

        public class FieldsClass
        {
            public string Sender { get; set; }
            public string Message { get; set; }
            public string User1 { get; set; }
            public string User2 { get; set; }
            public DateTime Updated_At { get; set; }
            public DateTime Created_At { get; set; }
        }

    }
}
