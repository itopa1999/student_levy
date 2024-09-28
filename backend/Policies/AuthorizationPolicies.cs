using Microsoft.AspNetCore.Authorization;

public static class AuthorizationPolicies
{
    public static void AddPolicies(AuthorizationOptions options)
    {
        options.AddPolicy("IsStudent", policy =>
            policy.RequireClaim("IsStudent", "true"));
        
        options.AddPolicy("IsAdmin", policy =>
            policy.RequireClaim("IsAdmin", "true"));
    }
}
