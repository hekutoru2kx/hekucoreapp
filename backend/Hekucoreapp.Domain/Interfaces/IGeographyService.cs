using Hekucoreapp.Domain.Entities;

namespace Hekucoreapp.Domain.Interfaces;

public interface IGeographyService
{
    Task<IList<Country>> GetCountriesAsync();
    Task<IList<State>> GetStatesByCountryAsync(int countryId);
    Task<IList<City>> GetCitiesByStateAsync(int stateId);
}