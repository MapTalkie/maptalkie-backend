using System;

namespace MapTalkie.Configuration;

public class CorsSettings
{
    public string[] Origins { get; set; } = Array.Empty<string>();
}