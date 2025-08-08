using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

public class OdooMany2OneField
{
    [JsonIgnore]
    [Ignore]
    private JToken _raw { get; set; }

    [JsonIgnore]
    public JToken RawValue
    {
        get => _raw;
        set => _raw = value;
    }

    [Column("value")] // Nombre del campo en SQLite (puedes cambiarlo)
    public int? Value
    {
        get
        {
            if (_raw == null || _raw.Type == JTokenType.Null)
                return null;

            if (_raw.Type == JTokenType.Array)
                return _raw[0]?.Value<int>();

            return _raw.Value<int>();
        }
        set
        {
            _raw = value != null ? new JValue(value.Value) : null;
        }
    }

    public override string ToString() => Value?.ToString() ?? "null";
}
