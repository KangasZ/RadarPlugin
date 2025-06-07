using System.Text.Json.Serialization;
using RadarPlugin.Enums;

namespace RadarPlugin.Models;

public class AggroInfo
{
    [JsonPropertyName("Id")]
    public uint NameId { get; set; }

    [JsonPropertyName("AggroType")]
    public AggroType AggroType { get; set; }
}
