using System;
using System.Collections.Generic;
using System.Text;

namespace Droneboi_Server
{
    public class Database
    {
        public class Ban
        {
            public string username;
            public string userId;
            public string reason;
        }
        public List<Ban> ban_list;
        public Database()
        {
            ban_list = new List<Ban>();
        }
    }
}
