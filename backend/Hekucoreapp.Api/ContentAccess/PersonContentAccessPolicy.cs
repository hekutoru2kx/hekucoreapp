using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Constants;
using Hekucoreapp.Domain.Enums.Permissions;
using Hekucoreapp.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Hekucoreapp.Api.ContentAccess;

// Registered against ContentOwnerTypes.Person so the shared GET /api/content/{id}/file endpoint
// can authorize a read without knowing anything about Person. A caller can read their own
// profile picture, or anyone's with PersonsPermission.Read (the same policy PersonController's
// admin reads use).
public class PersonContentAccessPolicy : IContentAccessPolicy
{
    private readonly IUserService _userService;
    private readonly IAuthorizationService _authorizationService;

    public PersonContentAccessPolicy(IUserService userService, IAuthorizationService authorizationService)
    {
        _userService = userService;
        _authorizationService = authorizationService;
    }

    public string OwnerType => ContentOwnerTypes.Person;

    public async Task<bool> CanReadAsync(ClaimsPrincipal user, int ownerId)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            var person = await _userService.GetPersonAsync(userId);
            if (person != null && person.Id == ownerId) return true;
        }

        var authResult = await _authorizationService.AuthorizeAsync(user,
            nameof(PersonsPermission) + "." + nameof(PersonsPermission.Read));
        return authResult.Succeeded;
    }
}
