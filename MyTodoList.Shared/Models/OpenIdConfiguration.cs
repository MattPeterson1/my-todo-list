using System;
using System.Collections.Generic;
using System.Text;

namespace MyTodoList.Shared.Models
{
    public class OpenIdConfiguration
    { 
        public string AuthorizationEndpoint { get; set; }
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
    }
}
