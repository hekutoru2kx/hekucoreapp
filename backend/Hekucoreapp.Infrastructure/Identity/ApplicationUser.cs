using Hekucoreapp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hekucoreapp.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    [Column("person_id")]
    public int? PersonId { get; set; }

    [Column("preferred_language")]
    public string PreferredLanguage { get; set; } = "en";

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("must_change_password")]
    public bool MustChangePassword { get; set; } = false;

    [Column("preferred_theme")]
    public string PreferredTheme { get; set; } = "azure";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Person? Person { get; set; }
}