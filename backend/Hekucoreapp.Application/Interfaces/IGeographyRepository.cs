using Hekucoreapp.Domain.Entities;

namespace Hekucoreapp.Application.Interfaces;

public interface IGeographyRepository
{
    Task<IList<Country>> GetCountriesAsync();
    Task<IList<State>> GetStatesByCountryAsync(int countryId);
    Task<IList<City>> GetCitiesByStateAsync(int stateId);
}