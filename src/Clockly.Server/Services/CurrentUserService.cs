namespace Clockly.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{

    public Task<string?> GetUserId()
    {
        try
        {

            var userId = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(m => m.Type == "sub")?.Value;
            
            return Task.FromResult(userId);
        }
        catch
        {
            return Task.FromResult<string?>(string.Empty);
        }
    }

    public Task<string?> GetUserName()
    {
        try
        {
            var userName = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(m => m.Type == "preferred_username")?.Value;

            return Task.FromResult(userName);
        }
        catch
        {
            return Task.FromResult<string?>(string.Empty);
        }
    }
}
