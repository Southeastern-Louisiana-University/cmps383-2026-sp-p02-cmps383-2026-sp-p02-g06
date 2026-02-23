using Selu383.SP26.Api.Features.Users;

namespace Selu383.SP26.Api.Features.Locations;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int TableCount { get; set; }

    public int? ManagerId { get; set; } //
    public virtual User? Manager { get; set; }
}