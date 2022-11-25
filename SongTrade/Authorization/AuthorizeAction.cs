using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;
using SongTrade.Utility;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SongTrade.Authorization
{
    public class AuthorizeAction : IAuthorizationFilter
    {
        //private readonly IConfiguration _config;
        readonly Claim _claim;
        /*private readonly string _claimType;
        private readonly string _claimValue;*/

        public AuthorizeAction(Claim claim)
        {
            _claim = claim; 
            //_config = config;
            /*_claimType = claimType;
            _claimValue = claimValue;*/
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Session.GetString(StaticDetails.UserToken);
            if (ValidateCurrentToken(token))
            {
                var role = GetClaim(token, "Role");
                if (role == _claim.Value)
                    return;
            }
            context.Result = new UnauthorizedResult();
            return;
        }

        private bool ValidateCurrentToken(string token)
        {
            /*var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config.GetSection("Jwt:Token").Value));*/ //key from appsettings.json
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("658C4CD6F8CA3AE8CD7D4D3A29D2697E92E9AA9FAB0D0B1851DE5FFAB2D8F22E"));

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private string GetClaim(string token, string claimType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            var ClaimValue = securityToken.Claims.First(claim => claim.Type == claimType).Value;
            return ClaimValue;
        }
    }
}
