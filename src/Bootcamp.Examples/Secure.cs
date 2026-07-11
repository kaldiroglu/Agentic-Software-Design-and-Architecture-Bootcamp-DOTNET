// ◀ Slides: Deck 06 Secure Coding — injection, error leak, secrets
namespace dev.kaldiroglu.bootcamp.secure;

// Topic 06 — Secure Coding: SQL injection, error leaks, secrets.

// ---------- SQL injection ----------

/// SMELL — string concatenation lets the user rewrite the query.
public sealed class VulnerableUserDao
{
    public string BuildQuery(string name) =>
        $"SELECT * FROM users WHERE name = '{name}'";
}

/// A fixed SQL template plus bound parameters — user text never enters the SQL.
public record ParameterizedQuery(string Sql, IReadOnlyList<string> Parameters);

/// FIXED — a fixed template with the name bound as a parameter.
public sealed class SafeUserDao
{
    private const string Sql = "SELECT * FROM users WHERE name = ?";
    public ParameterizedQuery BuildQuery(string name) => new(Sql, new[] { name });
}

// ---------- Error leaks ----------

public sealed class ErrorResponder
{
    /// SMELL — leaks internal detail to the caller.
    public string SmellUserMessage(Exception e) => $"Error: {e.Message}";

    /// FIXED — the caller sees nothing sensitive.
    public string SafeUserMessage(Exception e) => "Something went wrong. Please try again.";

    /// FIXED — the real detail goes to the server log only.
    public string InternalLog(Exception e) => $"[ERROR] {e.GetType().Name}: {e.Message}";
}

// ---------- Secrets ----------

public sealed class Secrets(IReadOnlyDictionary<string, string> config)
{
    /// SMELL — never do this: a real key committed to source.
    public const string HardCodedApiKey = "sk_live_do_not_commit_me";

    /// FIXED — read the secret from injected configuration.
    public string ApiKey()
    {
        if (!config.TryGetValue("API_KEY", out var key) || string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("API_KEY is not configured");
        }
        return key;
    }
}
