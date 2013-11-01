using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teamkeep.Models
{
    public class OpenAuthFail
    {
        public Exception Exception { get; set; }
        public DotNetOpenAuth.OpenId.RelyingParty.AuthenticationStatus Status { get; set; }
    }
}