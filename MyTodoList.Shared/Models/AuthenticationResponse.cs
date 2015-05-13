using System;
using System.Collections.Generic;
using System.Text;

namespace MyTodoList.Shared.Models
{
    public class AuthenticationResponse
    {
        public string IdentityToken { get; set; }
        public string DisplayName { get; set; }
    }
}
