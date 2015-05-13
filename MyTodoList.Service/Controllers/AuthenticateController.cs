using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using MyTodoList.Service.Filters;
using MyTodoList.Shared.Models;
using Newtonsoft.Json;

namespace MyTodoList.Service.Controllers
{
    public class AuthenticateController : ApiController
    {
        public async Task<IHttpActionResult> GetCode([FromUri] string code)
        {
            var config = (IdentityTokenValidatorSection)ConfigurationManager.GetSection("identityTokenValidator");

            using (var httpClient = new HttpClient())
            {
                try
                {
                    // Call the Token endpoint using the authorization code
                    var tokenRequest = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("client_id", config.ClientId),
                        new KeyValuePair<string, string>("client_secret", config.ClientSecret),
                        new KeyValuePair<string, string>("redirect_uri", config.RedirectUri),
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    });

                    var result = await httpClient.PostAsync(config.TokenEndpoint, tokenRequest);
                    if (!result.IsSuccessStatusCode)
                    {
                        // Something is probably wrong with the OpenIdConnect configuration badly configured on the server
                        InternalServerError(new Exception("Failure when calling Token endpoint" + config.TokenEndpoint));
                    }
                    var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>( await result.Content.ReadAsStringAsync());

                    // Call the UserInfo endpoint (we need the user's display name
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse["access_token"]);
                    result = await httpClient.GetAsync(config.UserinfoEndpoint);
                    if (!result.IsSuccessStatusCode)
                    {
                        // Something is probably wrong with the OpenIdConnect configuration badly configured on the server
                        InternalServerError(new Exception("Failure when calling Userinfo endpoint" + config.UserinfoEndpoint ));
                    }

                    var userinfoResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(await result.Content.ReadAsStringAsync());
                    
                    // Pass back the identity token and the display name of the user.
                    return Ok(new AuthenticationResponse()
                    {
                        IdentityToken = tokenResponse["id_token"],
                        DisplayName = userinfoResponse["name"]
                    });

                }
                catch (Exception e)
                {
                    return InternalServerError(e);
                }
            }
        }
    }
}
