using Hekucoreapp.Application.DTOs;
using Hekucoreapp.Domain.Enums.Permissions;
using Hekucoreapp.Domain.Interfaces;
using Hekucoreapp.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hekucoreapp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PersonController : ControllerBase
{
    private readonly IPersonService _personService;

    public PersonController(IPersonService personService)
    {
        _personService = personService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPersons(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? sortBy = "LastName",
        [FromQuery] string sortDirection = "asc",
        [FromQuery] string? search = null,
        [FromQuery] int? countryId = null)
    {
        var result = await _personService.GetPersonsAsync(new PersonListQuery
        {
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection,
            Search = search,
            CountryId = countryId
        });

        return Ok(new
        {
            items = result.Items.Select(MapToDto),
            totalCount = result.TotalCount,
            page = result.Page,
            pageSize = result.PageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPerson(int id)
    {
        var person = await _personService.GetPersonByIdAsync(id);
        if (person == null) return NotFound();
        return Ok(MapToDto(person));
    }

    [HttpPost]
    [Authorize(Policy = nameof(PersonsPermission) + "." + nameof(PersonsPermission.Create))]
    public async Task<IActionResult> CreatePerson(UpsertPersonDto dto)
    {
        try
        {
            var result = await _personService.CreatePersonAsync(MapToRequest(dto));
            return Ok(MapToDto(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = nameof(PersonsPermission) + "." + nameof(PersonsPermission.Update))]
    public async Task<IActionResult> UpdatePerson(int id, UpsertPersonDto dto)
    {
        try
        {
            await _personService.UpdatePersonAsync(id, MapToRequest(dto));
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static PersonDto MapToDto(PersonResult person) => new()
    {
        Id = person.Id,
        FirstName = person.FirstName,
        LastName = person.LastName,
        Birthday = person.Birthday,
        DocumentType = person.DocumentType,
        DocumentId = person.DocumentId,
        Phone = person.Phone,
        PhoneExtension = person.PhoneExtension,
        Email = person.Email,
        Address = person.Address,
        PostalCode = person.PostalCode,
        Gender = person.Gender,
        CountryId = person.CountryId,
        StateId = person.StateId,
        CityId = person.CityId,
        CountryName = person.CountryName,
        StateName = person.StateName,
        CityName = person.CityName,
        LinkedUserName = person.LinkedUserName
    };

    private static UpsertPersonRequest MapToRequest(UpsertPersonDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Birthday = dto.Birthday,
        DocumentType = dto.DocumentType,
        DocumentId = dto.DocumentId,
        Phone = dto.Phone,
        PhoneExtension = dto.PhoneExtension,
        Email = dto.Email,
        Address = dto.Address,
        PostalCode = dto.PostalCode,
        Gender = dto.Gender,
        CountryId = dto.CountryId,
        StateId = dto.StateId,
        CityId = dto.CityId
        
    };
}