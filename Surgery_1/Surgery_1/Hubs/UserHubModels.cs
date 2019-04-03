using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surgery_1.Hubs
{
    public class UserHubModels
    {
        public string roleName { get; set; }
        public HashSet<string> ConnectionIds { get; set; }
    }
}
