using System.ComponentModel.DataAnnotations.Schema;

namespace Hekucoreapp.Domain.Entities;

[Table("app_info")]
public class AppInfo
{
    [Column("id")]
    public int Id { get; set; }

    [Column("app_name")]
    public string AppName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("version")]
    public string Version { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}