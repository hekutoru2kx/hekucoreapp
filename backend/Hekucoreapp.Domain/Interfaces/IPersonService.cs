using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Domain.Interfaces;

public interface IPersonService
{
    Task<PagedResult<PersonResult>> GetPersonsAsync(PersonListQuery query);
    Task<PersonResult?> GetPersonByIdAsync(int personId);
    Task<PersonResult> CreatePersonAsync(UpsertPersonRequest request);
    Task UpdatePersonAsync(int personId, UpsertPersonRequest request);
}