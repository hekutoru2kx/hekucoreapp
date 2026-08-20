using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Models;
using Hekucoreapp.Domain.Interfaces;

namespace Hekucoreapp.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly EmailTemplates _emailTemplates;

    public UserService(IUserRepository userRepository, IEmailService emailService, EmailTemplates emailTemplates)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _emailTemplates = emailTemplates;
    }

    public async Task UpdateLanguageAsync(string userId, string language)
    {
        await _userRepository.UpdateLanguageAsync(userId, language);
    }

    public async Task<UserProfileResult> GetProfileAsync(string userId)
    {
        return await _userRepository.GetProfileAsync(userId);
    }
    public async Task UpdateProfileAsync(UpdateProfileRequest request) 
    {
        await _userRepository.UpdateProfileAsync(request);
    }
    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        await _userRepository.ChangePasswordAsync(userId, currentPassword, newPassword);

        var profile = await _userRepository.GetProfileAsync(userId);
        var (subject, body) = _emailTemplates.PasswordChanged(profile.UserName);
        await _emailService.SendAsync(profile.Email, subject, body);
    }

    public async Task<PersonResult?> GetPersonAsync(string userId) =>
        await _userRepository.GetPersonAsync(userId);

    public async Task UpsertPersonAsync(string userId, UpsertPersonRequest request) =>
        await _userRepository.UpsertPersonAsync(userId, request);

    public async Task<PersonMatchResult> CheckExistingPersonAsync(string callerId, string? documentType, string? documentId, string? email) =>
        await _userRepository.CheckExistingPersonAsync(callerId, documentType, documentId, email);
}