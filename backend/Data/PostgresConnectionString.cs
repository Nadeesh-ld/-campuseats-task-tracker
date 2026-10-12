using Npgsql;

namespace CampusEats.Api.Data;

public static class PostgresConnectionString
{
    public static string Normalize(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            return value;
        }

        var credentials = uri.UserInfo.Split(':', 2);
        if (credentials.Length != 2)
            throw new ArgumentException("The PostgreSQL URL must include a username and password.");

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = Uri.UnescapeDataString(credentials[1])
        };

        foreach (var parameter in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = parameter.Split('=', 2);
            var name = Uri.UnescapeDataString(parts[0]);
            var parameterValue = parts.Length == 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;

            if (name.Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                builder["SSL Mode"] = NormalizeSslMode(parameterValue);
            else if (name.Equals("channel_binding", StringComparison.OrdinalIgnoreCase))
                builder["Channel Binding"] = parameterValue;
        }

        return builder.ConnectionString;
    }

    private static string NormalizeSslMode(string value) => value.ToLowerInvariant() switch
    {
        "verify-ca" => "VerifyCA",
        "verify-full" => "VerifyFull",
        "disable" => "Disable",
        "allow" => "Allow",
        "prefer" => "Prefer",
        "require" => "Require",
        _ => throw new ArgumentException("The PostgreSQL URL contains an unsupported sslmode value.")
    };
}
