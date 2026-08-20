using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Entities;
using Hekucoreapp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hekucoreapp.Infrastructure.Repositories;

public class GeographyRepository : IGeographyRepository
{
    private readonly HekucoreappDbContext _context;

    public GeographyRepository(HekucoreappDbContext context)
    {
        _context = context;
    }

    public async Task<IList<Country>> GetCountriesAsync() =>
        await _context.Countries
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<IList<State>> GetStatesByCountryAsync(int countryId) =>
        await _context.States
            .Where(s => s.CountryId == countryId)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public async Task<IList<City>> GetCitiesByStateAsync(int stateId) =>
        await _context.Cities
            .Where(c => c.StateId == stateId)
            .OrderBy(c => c.Name)
            .ToListAsync();
}