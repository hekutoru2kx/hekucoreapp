using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Application.Resources;
using Hekucoreapp.Domain.Entities;
using Hekucoreapp.Domain.Models;
using Hekucoreapp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Hekucoreapp.Domain.Enums;

namespace Hekucoreapp.Infrastructure.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly HekucoreappDbContext _context;
    private readonly IStringLocalizer<Messages> _localizer;

    public PersonRepository(HekucoreappDbContext context, IStringLocalizer<Messages> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<PagedResult<PersonResult>> GetPersonsAsync(PersonListQuery query)
    {
        var personsQuery = _context.Persons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
                personsQuery = personsQuery.Where(p =>
                p.FirstName.ToLower().Contains(search) ||
                p.LastName.ToLower().Contains(search) ||
                (p.DocumentId != null && p.DocumentId.ToLower().Contains(search)));
        }

        if (query.CountryId.HasValue)
            personsQuery = personsQuery.Where(p => p.CountryId == query.CountryId.Value);

        personsQuery = query.SortBy?.ToLower() switch
        {
            "firstname" => query.SortDirection == "desc"
                ? personsQuery.OrderByDescending(p => p.FirstName)
                : personsQuery.OrderBy(p => p.FirstName),
            "email" => query.SortDirection == "desc"
                ? personsQuery.OrderByDescending(p => p.Email)
                : personsQuery.OrderBy(p => p.Email),
            "createdat" => query.SortDirection == "desc"
                ? personsQuery.OrderByDescending(p => p.CreatedAt)
                : personsQuery.OrderBy(p => p.CreatedAt),
            "countryid" => query.SortDirection == "desc"
                ? personsQuery.OrderByDescending(p => p.CountryId)
                : personsQuery.OrderBy(p => p.CountryId),
            _ => query.SortDirection == "desc"
                ? personsQuery.OrderByDescending(p => p.LastName)
                : personsQuery.OrderBy(p => p.LastName)
        };

        var totalCount = await personsQuery.CountAsync();

        var persons = await personsQuery
            .Include(p => p.Country)
            .Include(p => p.State)
            .Include(p => p.City)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var personIds = persons.Select(p => p.Id).ToList();
        var linkedUserNames = await _context.Users
            .Where(u => u.PersonId.HasValue && personIds.Contains(u.PersonId.Value))
            .ToDictionaryAsync(u => u.PersonId!.Value, u => u.UserName);

        return new PagedResult<PersonResult>
        {
            Items = persons.Select(p => MapToResult(p, linkedUserNames.GetValueOrDefault(p.Id))).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<PersonResult?> GetPersonByIdAsync(int personId)
    {
        var person = await _context.Persons.FindAsync(personId);
        if (person == null) return null;

        var linkedUserName = await _context.Users
            .Where(u => u.PersonId == personId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync();

        return MapToResult(person, linkedUserName);
    }

    public async Task<PersonResult> CreatePersonAsync(UpsertPersonRequest request)
    {
        var person = MapFromRequest(request);
        _context.Persons.Add(person);
        await _context.SaveChangesAsync();
        return MapToResult(person);
    }

    public async Task UpdatePersonAsync(int personId, UpsertPersonRequest request)
    {
        var person = await _context.Persons.FindAsync(personId)
            ?? throw new Exception(_localizer["PersonNotFound"]);

        UpdateFromRequest(person, request);
        await _context.SaveChangesAsync();
    }

    private static PersonResult MapToResult(Person person, string? linkedUserName = null) => new()
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
        CityId = person.CityId,
        CountryName = person.Country?.Name,
        StateName = person.State?.Name,
        CityName = person.City?.Name,
        LinkedUserName = linkedUserName
    };

    private static Person MapFromRequest(UpsertPersonRequest request)
    {
        var person = new Person();
        UpdateFromRequest(person, request);
        return person;
    }

    private static void UpdateFromRequest(Person person, UpsertPersonRequest request)
    {
        person.FirstName = request.FirstName;
        person.LastName = request.LastName;
        person.Birthday = request.Birthday;
        person.DocumentType = request.DocumentType != null
            ? Enum.Parse<DocumentType>(request.DocumentType) : null;
        person.DocumentId = string.IsNullOrEmpty(request.DocumentId) ? null : request.DocumentId;
        person.Phone = string.IsNullOrEmpty(request.Phone) ? null : request.Phone;
        person.PhoneExtension = string.IsNullOrEmpty(request.PhoneExtension) ? null : request.PhoneExtension;
        person.Email = string.IsNullOrEmpty(request.Email) ? null : request.Email;
        person.Address = string.IsNullOrEmpty(request.Address) ? null : request.Address;
        person.PostalCode = string.IsNullOrEmpty(request.PostalCode) ? null : request.PostalCode;
        person.Gender = request.Gender != null
            ? Enum.Parse<Gender>(request.Gender) : null;
        person.CountryId = request.CountryId;
        person.StateId = request.StateId;
        person.CityId = request.CityId;
    }
}