using System;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;

namespace MyTodoList.Service.Filters
{
    public class IdentityTokenAuthentication : ActionFilterAttribute, IAuthenticationFilter
    {
#pragma warning disable 1998
        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
#pragma warning restore 1998
        {
            // Look for access_token in Authorization header in the request.
            HttpRequestMessage request = context.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;

            // If there are no credentials, do nothing.
            if (authorization == null)
            {
                context.ErrorResult = new AuthenticationBearerJwtFailureResult(HttpStatusCode.Unauthorized, "Authorization header required", request);
                return;
            }

            // Scheme must be bearer.
            if (authorization.Scheme != "Bearer" ||
                String.IsNullOrEmpty(authorization.Parameter))
            {
                context.ErrorResult = new AuthenticationBearerJwtFailureResult(HttpStatusCode.Unauthorized, "Authorization header must contain value with must Bearer scheme", request);
                return;
            }

            try
            {
                // Use validator configuration from the web.config
                IdentityTokenValidatorSection config =
                    (IdentityTokenValidatorSection)ConfigurationManager.GetSection("identityTokenValidator");
                var validationParameters = new TokenValidationParameters
                {

                    CertificateValidator = X509CertificateValidator.None,
                    ValidAudience = config.ClientId,
                    ValidIssuer = config.Issuer,
                    IssuerSigningKeyResolver =
                        (token, securityToken, keyIdentifier, tokenValidationParameters) =>
                            new X509SecurityKey(config.IssuerSigningCertificate)
                };

                // Validate the token and set the context principal
                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken validatedSecurityToken;
                var principal = new ClaimsPrincipal(tokenHandler.ValidateToken(authorization.Parameter, validationParameters, out validatedSecurityToken));

                // Make sure that the token has an id claim
                if (!principal.HasClaim(claim => claim.Type == ClaimTypes.NameIdentifier))
                {
                    context.ErrorResult = new AuthenticationBearerJwtFailureResult(HttpStatusCode.Unauthorized, "Token must contain id claim", request);
                    return;
                }

                context.Principal = principal;
            }
            catch (FormatException e)
            {
                context.ErrorResult = new AuthenticationBearerJwtFailureResult(HttpStatusCode.InternalServerError,
                    "Error in base64 encoding of x509certs in web.config: " + e.Message, request);
            }
            catch (SecurityTokenValidationException e)
            {
                context.ErrorResult = new AuthenticationBearerJwtFailureResult(HttpStatusCode.Unauthorized, e.Message, request);
            }
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        private class AuthenticationBearerJwtFailureResult : IHttpActionResult
        {
            public AuthenticationBearerJwtFailureResult(HttpStatusCode statusCode, string message, HttpRequestMessage request)
            {
                StatusCode = statusCode;
                Message = message;
                Request = request;
            }

            private HttpStatusCode StatusCode { get; set; }
            private string Message { get; set; }

            private HttpRequestMessage Request { get; set; }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(Execute());
            }

            private HttpResponseMessage Execute()
            {
                HttpResponseMessage response = new HttpResponseMessage(StatusCode);
                response.RequestMessage = Request;
                response.Content = new StringContent(Message);
                return response;
            }
        }
    }
}