
using Microsoft.AspNetCore.Identity;

public static class IdentityResultExtensions
{
    public static void LogIdentityErrors<TLogger>(this IdentityResult result, TLogger logger) where TLogger : ILogger
    {
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                logger.LogError("Identity error ({Code}): {Description}", error.Code, error.Description);
            }
        }
    }
}