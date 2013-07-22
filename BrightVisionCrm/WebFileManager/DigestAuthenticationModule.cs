using System;
using System.Web;
using System.Text;
using System.Security.Principal;
using System.Security;
using System.Web.Security;

namespace WebFileManager
{
    /// <summary>
    /// ASP.NET module which implements 'Digest' authentication protocol.
    /// Implementation itself is in the <see cref="DigestAuthenticationProvider"/> class.
    /// This is just an adapter class so digest authentication is exposed as ASP.NET module.
    /// </summary>
    public class DigestAuthenticationModule : AuthenticationModuleBase
    {
        /// <summary>
        /// Gets (and initializes as needed) <see cref="DigestAuthenticationProvider"/>.
        /// </summary>
        private DigestAuthenticationProvider Provider
        {
            get
            {
                string key = "%DigestProvider%";
                if (HttpContext.Current.Items[key] == null)
                {
                    HttpContext.Current.Items[key] = new DigestAuthenticationProvider(getPasswordAndRoles);
                }

                return (DigestAuthenticationProvider)HttpContext.Current.Items[key];
            }
        }

        /// <summary>
        /// Performs request authentication.
        /// </summary>
        /// <param name="request">Instance of <see cref="HttpRequest"/>.</param>
        /// <returns>Instance of <see cref="IPrincipal"/>, or <c>null</c> if user was not authenticated.</returns>
        protected override IPrincipal AuthenticateRequest(HttpRequest request)
        {
            try
            {
                return Provider.AuthenticateRequest(request.Headers, request.HttpMethod);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("Failed to authenticate user", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets challenge string.
        /// </summary>
        /// <returns>Challenge string.</returns>
        protected override string GetChallenge()
        {
            return Provider.GetChallenge();
        }

        /// <summary>
        /// Checks whether authorization header is present.
        /// </summary>
        /// <param name="request">Instance of <see cref="HttpRequest"/>.</param>
        /// <returns>'true' if there's basic authentication header.</returns>
        protected override bool IsAuthorizationPresent(HttpRequest request)
        {
            return Provider.IsAuthorizationPresent(request.Headers);
        }

        /// <summary>
        /// Retrieves user's password.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <returns>Users password.</returns>
        private DigestAuthenticationProvider.PasswordAndRoles getPasswordAndRoles(string userName)
        {
            var user = Membership.GetUser(userName);
            if (user != null)
            {
                return new DigestAuthenticationProvider.PasswordAndRoles(user.GetPassword(), null);
            }

            return null;
        }
    }
}
