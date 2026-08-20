using Hekucoreapp.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Hekucoreapp.Application.Resources;
using Hekucoreapp.Domain.Models;
using Hekucoreapp.Domain.Entities;
using Hekucoreapp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Hekucoreapp.Domain.Enums;

namespace Hekucoreapp.Infrastructure.Identity;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<Messages> _localizer;

    private readonly HekucoreappDbContext _context;
    private readonly IUserRoleRepository _userRoleRepository;

    public UserRepository(UserManager<ApplicationUser> userManager, IStringLocalizer<Messages> localizer, HekucoreappDbContext context, IUserRoleRepository userRoleRepository)
    {
        _userManager = userManager;
        _localizer = localizer;
        _context = context;
        _userRoleRepository = userRoleRepository;
    }

    public async Task<string> CreateUserAsync(CreateUserRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        return user.Id;
    }

    public async Task<string?> ValidateUserAsync(string usernameOrEmail, string password)
    {
        var user = await _userManager.FindByEmailAsync(usernameOrEmail)
        ?? await _userManager.FindByNameAsync(usernameOrEmail);

        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            return null;

        return user.Id;
    }

    public async Task AssignRoleAsync(string email, string role)
    {
        var user = await _userManager.FindByEmailAsync(email)
            ?? throw new Exception(_localizer["UserNotFound"]);

        await _userRoleRepository.GrantRoleAsync(user.Id, role);
    }

    public async Task<IList<string>> GetRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception(_localizer["UserNotFound"]);
        return await _userRoleRepository.GetActiveRoleNamesAsync(user.Id);
    }

    public async Task UpdateLanguageAsync(string userId, string language)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception(_localizer["UserNotFound"]);

        user.PreferredLanguage = language;
        await _userManager.UpdateAsync(user);
    }

    public async Task<(string UserName, IList<string> Roles, bool MustChangePassword, string PreferredTheme)> GetUserInfoAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception(_localizer["UserNotFound"]);

        var roles = await _userRoleRepository.GetActiveRoleNamesAsync(userId);
        return (user.UserName, roles, user.MustChangePassword, user.PreferredTheme);
    }

    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception(_localizer["UserNotFound"]);

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        user.MustChangePassword = false;
        await _userManager.UpdateAsync(user);
    }

    public async Task<UserProfileResult> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception(_localizer["UserNotFound"]);

        var roles = await _userRoleRepository.GetActiveRoleNamesAsync(userId);

        return new UserProfileResult
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PreferredLanguage = user.PreferredLanguage,
            PreferredTheme = user.PreferredTheme,
            Roles = roles
        };
    }

    public async Task UpdateProfileAsync(UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new Exception(_localizer["UserNotFound"]);

        user.Email = request.Email;
        user.PreferredLanguage = request.PreferredLanguage;
        user.PreferredTheme = request.PreferredTheme;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<PersonResult?> GetPersonAsync(string userId)
{
    var user = await _userManager.Users
        .Include(u => u.Person)
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user?.Person == null) return null;

    var person = user.Person;
    return new PersonResult
    {
        Id = person.Id,
        FirstName = person.FirstName,
        LastName = person.LastName,
        Birthday = person.Birthday,
        DocumentType = person.DocumentType?.ToString(),
        DocumentId = person.DocumentId,
        Phone = person.Phone,
        PhoneExtension = person.PhoneExtension,
        Email = person.Email,
        Address = person.Address,
        PostalCode = person.PostalCode,
        Gender = person.Gender?.ToString(),
        CountryId = person.CountryId,
        StateId = person.StateId,
        CityId = person.CityId
    };
}

public async Task UpsertPersonAsync(string userId, UpsertPersonRequest request)
{
    var user = await _userManager.Users
        .Include(u => u.Person)
        .FirstOrDefaultAsync(u => u.Id == userId)
        ?? throw new Exception(_localizer["UserNotFound"]);

    if (user.Person == null)
    {
        Person? existingPerson = null;
        if (!string.IsNullOrEmpty(request.DocumentType) && !string.IsNullOrEmpty(request.DocumentId))
        {
            var documentType = Enum.Parse<DocumentType>(request.DocumentType);
            existingPerson = await _context.Persons.FirstOrDefaultAsync(p =>
                p.DocumentType == documentType && p.DocumentId == request.DocumentId);
        }

        if (existingPerson != null)
        {
            var alreadyLinked = await _context.Users.AnyAsync(u => u.PersonId == existingPerson.Id);
            var emailMatches = !string.IsNullOrEmpty(existingPerson.Email)
                && !string.IsNullOrEmpty(request.Email)
                && string.Equals(existingPerson.Email, request.Email, StringComparison.OrdinalIgnoreCase);

            if (alreadyLinked || !emailMatches)
                throw new Exception(_localizer["PersonAlreadyExistsCannotLink"]);

            user.PersonId = existingPerson.Id;
            await _userManager.UpdateAsync(user);
            return;
        }

        var person = new Person
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Birthday = request.Birthday,
            DocumentType = request.DocumentType != null ? Enum.Parse<DocumentType>(request.DocumentType) : null,
            DocumentId = string.IsNullOrEmpty(request.DocumentId) ? null : request.DocumentId,
            Phone = string.IsNullOrEmpty(request.Phone) ? null : request.Phone,
            PhoneExtension = string.IsNullOrEmpty(request.PhoneExtension) ? null : request.PhoneExtension,
            Email = string.IsNullOrEmpty(request.Email) ? null : request.Email,
            Address = string.IsNullOrEmpty(request.Address) ? null : request.Address,
            PostalCode = string.IsNullOrEmpty(request.PostalCode) ? null : request.PostalCode,
            Gender = request.Gender != null ? Enum.Parse<Gender>(request.Gender) : null,
            CountryId = request.CountryId,
            StateId = request.StateId,
            CityId = request.CityId
        };

        _context.Persons.Add(person);
        await _context.SaveChangesAsync();

        user.PersonId = person.Id;
        await _userManager.UpdateAsync(user);
    }
    else
    {
        var person = user.Person;
        person.FirstName = request.FirstName;
        person.LastName = request.LastName;
        person.Birthday = request.Birthday;
        person.DocumentType = request.DocumentType != null ? Enum.Parse<DocumentType>(request.DocumentType) : null;
        person.DocumentId = string.IsNullOrEmpty(request.DocumentId) ? null : request.DocumentId;
        person.Phone = string.IsNullOrEmpty(request.Phone) ? null : request.Phone;
        person.PhoneExtension = string.IsNullOrEmpty(request.PhoneExtension) ? null : request.PhoneExtension;
        person.Email = string.IsNullOrEmpty(request.Email) ? null : request.Email;
        person.Address = string.IsNullOrEmpty(request.Address) ? null : request.Address;
        person.PostalCode = string.IsNullOrEmpty(request.PostalCode) ? null : request.PostalCode;
        person.Gender = request.Gender != null ? Enum.Parse<Gender>(request.Gender) : null;
        person.CountryId = request.CountryId;
        person.StateId = request.StateId;
        person.CityId = request.CityId;

        await _context.SaveChangesAsync();
    }
}

// Read-only preview of what UpsertPersonAsync's matching branch would do — lets the
// profile form warn "this will link to an existing record" (or "contact an administrator")
// before the user submits, instead of only finding out from the save's error message.
public async Task<PersonMatchResult> CheckExistingPersonAsync(string callerId, string? documentType, string? documentId, string? email)
{
    if (string.IsNullOrEmpty(documentType) || string.IsNullOrEmpty(documentId))
        return new PersonMatchResult { MatchFound = false };

    var caller = await _userManager.FindByIdAsync(callerId)
        ?? throw new Exception(_localizer["UserNotFound"]);

    var parsedType = Enum.Parse<DocumentType>(documentType);
    var existingPerson = await _context.Persons.FirstOrDefaultAsync(p =>
        p.DocumentType == parsedType && p.DocumentId == documentId);

    // No match, or the match is the caller's own already-linked record — nothing to report,
    // since editing your own profile with your own unchanged document isn't "linking".
    if (existingPerson == null || existingPerson.Id == caller.PersonId)
        return new PersonMatchResult { MatchFound = false };

    var alreadyLinked = await _context.Users.AnyAsync(u => u.PersonId == existingPerson.Id);
    var emailMatches = !string.IsNullOrEmpty(existingPerson.Email)
        && !string.IsNullOrEmpty(email)
        && string.Equals(existingPerson.Email, email, StringComparison.OrdinalIgnoreCase);

    return new PersonMatchResult
    {
        MatchFound = true,
        Linkable = !alreadyLinked && emailMatches
    };
}

public async Task<(string UserId, bool IsNewUser)> FindOrCreateGoogleUserAsync(GoogleUserInfo googleUser)
{
    const string provider = "Google";

    var user = await _userManager.FindByLoginAsync(provider, googleUser.Subject);
    if (user != null) return (user.Id, false);

    user = await _userManager.FindByEmailAsync(googleUser.Email);
    if (user != null)
    {
        var linkResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, googleUser.Subject, provider));
        if (!linkResult.Succeeded)
            throw new Exception(string.Join(", ", linkResult.Errors.Select(e => e.Description)));

        return (user.Id, false);
    }

    var normalizedEmail = _userManager.NormalizeEmail(googleUser.Email);
    var wasDeleted = await _context.DeletedAccounts.AnyAsync(d => d.NormalizedEmail == normalizedEmail);
    if (wasDeleted)
        throw new Exception(_localizer["AccountWasDeleted"]);

    var baseUserName = googleUser.Email;
    var userName = baseUserName;
    var suffix = 1;
    while (await _userManager.FindByNameAsync(userName) != null)
        userName = $"{baseUserName}{suffix++}";

    var newUser = new ApplicationUser
    {
        UserName = userName,
        Email = googleUser.Email,
        EmailConfirmed = true
    };

    var createResult = await _userManager.CreateAsync(newUser);
    if (!createResult.Succeeded)
        throw new Exception(string.Join(", ", createResult.Errors.Select(e => e.Description)));

    var addLoginResult = await _userManager.AddLoginAsync(newUser, new UserLoginInfo(provider, googleUser.Subject, provider));
    if (!addLoginResult.Succeeded)
        throw new Exception(string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));

    return (newUser.Id, true);
}
}