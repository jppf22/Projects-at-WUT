using Voyagers.Data;

namespace Voyagers.Services
{
    public class FK_USERID_Validator
    {
        private readonly ApplicationDbContext _identityContext;

        public FK_USERID_Validator(ApplicationDbContext identityContext)
        {
            _identityContext = identityContext;
        }

        public bool IsValidUserId(string userId)
        {
            // return _identityContext.Users.Any(u => u.Id == userId);
            return _identityContext.Users.Any(u => u.Id == userId);
        }
    }
}
