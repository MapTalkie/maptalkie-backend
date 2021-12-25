using System.ComponentModel.DataAnnotations;

namespace MapTalkie.Configuration;

public class RabbitMQConfiguration
{
    public string? Username { get; set; } = string.Empty;
    public string? Password { get; set; } = string.Empty;
    [Required] public string Host { get; set; } = string.Empty;
}