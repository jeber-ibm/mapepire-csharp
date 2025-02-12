using System.Text.Json.Serialization;

namespace io.github.mapepire_ibmi.types {



/**
 * Represents the parameter result for a query.
 */
public class ConnectOptions {
 
    [JsonPropertyName("id")]
    public  String Id { get; set; }

    [JsonPropertyName("type")]
    public  String Type { get; set; }

    [JsonPropertyName("technique")]
    public  String Technique { get; set; }

    [JsonPropertyName("application")]
    public  String Applicatoin { get; set; }

public ConnectOptions(String id, String type, String technique, String application) {
    this.Id = id; 
    this.Type = type;
    this.Technique = technique;
    this.Applicatoin = application; 
}


}
}