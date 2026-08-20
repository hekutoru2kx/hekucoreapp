using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Application.Interfaces;

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo> ValidateAsync(string idToken);
}
