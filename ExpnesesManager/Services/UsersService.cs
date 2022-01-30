using System.Security.Claims;

namespace ExpnesesManager.Services
{

    public interface IUsersService
    {
        int GetUserId();
    }
    public class UsersService : IUsersService
    {
        private readonly HttpContext _httpContext;
        public UsersService(IHttpContextAccessor contextAccessor)
        {
            _httpContext = contextAccessor.HttpContext;       
        }

        public int GetUserId()
        {
            if (_httpContext.User.Identity.IsAuthenticated)
            {

                var idClaim = _httpContext.User.
                    Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                return int.Parse(idClaim.Value);


            } else
            {
                throw new ApplicationException("There is no user logged in");
            }
        }

    }
}
