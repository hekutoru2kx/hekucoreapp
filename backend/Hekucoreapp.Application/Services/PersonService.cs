using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Interfaces;
using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Application.Services;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _repository;

    public PersonService(IPersonRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<PersonResult>> GetPersonsAsync(PersonListQuery query) =>
        await _repository.GetPersonsAsync(query);

    public async Task<PersonResult?> GetPersonByIdAsync(int personId) =>
        await _repository.GetPersonByIdAsync(personId);

    public async Task<PersonResult> CreatePersonAsync(UpsertPersonRequest request) =>
        await _repository.CreatePersonAsync(request);

    public async Task UpdatePersonAsync(int personId, UpsertPersonRequest request) =>
        await _repository.UpdatePersonAsync(personId, request);
}