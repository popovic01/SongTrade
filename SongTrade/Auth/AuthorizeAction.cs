using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;
using SongTrade.Auth;
using SongTrade.Utility;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SongTrade.Authorization
{
    public class AuthorizeAction : IAuthorizationFilter
    {
        readonly Claim _claim;
        private readonly IAuthService _auth;

        public AuthorizeAction(Claim claim, IAuthService auth)
        {
            _claim = claim; 
            _auth = auth;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Session.GetString(StaticDetails.UserToken);
            if (_auth.ValidateCurrentToken(token))
            {
                var role = _auth.GetClaim(token, "Role");
                if (role == _claim.Value)
                    return;
            }
            context.Result = new UnauthorizedResult();
            return;
        }
    }
}
