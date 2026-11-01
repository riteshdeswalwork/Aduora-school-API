using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sms.api.Helpers
{
    /// <summary>
    /// Accepts the same TimeSpan strings as the default System.Text.Json converter ("c" format,
    /// e.g. "14:30:00") but also accepts "HH:mm" (no seconds) and "H:mm" — the format produced by
    /// an HTML &lt;input type="time"&gt; element, which is what every Add/Edit form in the admin
    /// panel sends for exam/period/bus-route/bus-stop times.
    ///
    /// Without this, the default converter throws a JsonException for "14:30", which ASP.NET Core's
    /// [ApiController] model binding turns into a ModelState error and the generic
    /// "One or more validation errors occurred." 400 response — with no indication that the real
    /// problem was a time field, because the request never reaches the controller action.
    ///
    /// Registered once, globally, in Program.cs; applies to both <see cref="TimeSpan"/> and
    /// <see cref="Nullable{TimeSpan}"/> properties.
    /// </summary>
    public class LenientTimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return default;

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result))
                return result;

            throw new JsonException($"The JSON value '{value}' could not be converted to a time (expected formats like \"14:30\" or \"14:30:00\").");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("c", CultureInfo.InvariantCulture));
        }
    }
}
