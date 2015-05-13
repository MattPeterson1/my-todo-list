using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MyTodoList.Service.Filters;
using MyTodoList.Shared.Models;

namespace MyTodoList.Service.Controllers
{
    public class OpenIdConfigurationController : ApiController
    {
        public IHttpActionResult GetOpenIdConfiguration(HttpRequestMessage requestMessage)
        {
            var config = (IdentityTokenValidatorSection)ConfigurationManager.GetSection("identityTokenValidator");

            var redirectUri = new UriBuilder(requestMessage.RequestUri.AbsoluteUri);
            redirectUri.Path = "/api/Authenticate";
            
            return Ok(new OpenIdConfiguration()
            {
                AuthorizationEndpoint = config.AuthorizationEndpoint,
                ClientId = config.ClientId,
                RedirectUri = config.RedirectUri
            });

        }
    }
}
