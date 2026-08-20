using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(GenerateTokenRequest request);
}