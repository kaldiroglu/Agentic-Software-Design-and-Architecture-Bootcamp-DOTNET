// ◀ Slides: Deck 02 Fundamentals — "A Password Value Object" (+ presenter note)
using System.Security.Cryptography;

namespace dev.kaldiroglu.bootcamp.fundamentals.password;

// Everything about a password lives in this one cohesive package: the value object,
// its durable hashed form, and the hasher port + adapter.

/// FIXED — a highly cohesive value object: validation, random creation, comparison
/// and masking for a password all live here, and nothing unrelated leaks in.
public sealed class Password
{
    private const int MinLength = 12;
    private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lower = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Symbols = "!@#$%^&*()-_=+";
    private static readonly Random Rng = new();   // demo only; use RandomNumberGenerator in production
    private readonly string _value;

    private Password(string value) => _value = value;

    // create & validate — one place owns the rules
    public static Password Of(string raw) =>
        IsValid(raw) ? new Password(raw)
                     : throw new ArgumentException("Password does not meet the policy");

    public static bool IsValid(string? raw) =>
        raw is not null
        && raw.Length >= MinLength
        && raw.Any(char.IsUpper)
        && raw.Any(char.IsLower)
        && raw.Any(char.IsDigit)
        && raw.Any(ch => !char.IsLetterOrDigit(ch));

    // generate a strong random password (reuses Of()/IsValid)
    public static Password Random(int length)
    {
        if (length < MinLength) throw new ArgumentException($"length must be at least {MinLength}");
        var chars = new List<char> { Pick(Upper), Pick(Lower), Pick(Digits), Pick(Symbols) };
        var pool = Upper + Lower + Digits + Symbols;
        while (chars.Count < length) chars.Add(Pick(pool));
        for (var i = chars.Count - 1; i > 0; i--)   // shuffle
        {
            var j = Rng.Next(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return Of(new string(chars.ToArray()));
    }

    // compare safely — constant-time, no early exit on first difference
    public bool Matches(string? candidate)
    {
        if (candidate is null || candidate.Length != _value.Length) return false;
        var diff = 0;
        for (var i = 0; i < _value.Length; i++) diff |= _value[i] ^ candidate[i];
        return diff == 0;
    }

    // format & mask — never leak the raw value
    public string Masked() => new('•', _value.Length);

    public string MaskedShowingLast(int shown)
    {
        if (shown <= 0) return Masked();
        var hidden = Math.Max(0, _value.Length - shown);
        return new string('•', hidden) + _value[hidden..];
    }

    public override string ToString() => Masked();   // safe by default
    public override bool Equals(object? other) => other is Password p && Matches(p._value);
    public override int GetHashCode() => _value.GetHashCode();

    // internal: only same-assembly collaborators (e.g. a PasswordHasher) may read the
    // raw value, and only to hash or verify it. It never leaks to the outside world.
    internal string Value => _value;

    private static char Pick(string pool) => pool[Rng.Next(pool.Length)];
}

/// The DURABLE form of a password — what actually gets persisted. A distinct value
/// object from Password because it has a different lifecycle (a Password lives for
/// milliseconds; this outlives every session). The encoded string carries everything
/// needed to verify later: algorithm parameters, salt, and the derived key.
public sealed record HashedPassword(string Encoded);

/// The answer to "why isn't hashing on Password?" — hashing lives HERE, in a port,
/// not on the value object. It changes for different reasons (algorithm, work factor)
/// and needs infrastructure, so it stays out of the pure domain type (DIP).
public interface IPasswordHasher
{
    HashedPassword Hash(Password password);
    bool Verify(Password candidate, HashedPassword stored);
}

/// FIXED — the infrastructure adapter that implements IPasswordHasher. All the
/// volatile, dependency-heavy detail (PBKDF2-HMAC-SHA256, a per-password salt, a
/// tunable work factor) lives here; swap it for Argon2 and Password never changes.
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 210_000;   // tune upward as hardware improves
    private const int KeyBytes = 32;
    private const int SaltBytes = 16;

    public HashedPassword Hash(Password password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var key = Rfc2898DeriveBytes.Pbkdf2(password.Value, salt, Iterations, HashAlgorithmName.SHA256, KeyBytes);
        return new HashedPassword($"{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(key)}");
    }

    public bool Verify(Password candidate, HashedPassword stored)
    {
        var parts = stored.Encoded.Split(':');
        var iterations = int.Parse(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(candidate.Value, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(expected, actual);   // constant-time
    }
}

/// FIXED — the presentation task, split out of Password. As display formats multiply
/// (log redaction, UI hints, ...) they live here instead of bloating the value object.
public sealed class PasswordFormatter
{
    /// Log-safe: reveals only the length, never the value.
    public string RedactedForLog(Password password) => $"[redacted: {password.Value.Length} chars]";

    /// Show only the first `shown` characters (a UI hint); mask the rest.
    public string ShowingFirst(Password password, int shown)
    {
        var value = password.Value;
        var visible = Math.Max(0, Math.Min(shown, value.Length));
        return value[..visible] + new string('•', value.Length - visible);
    }
}

/// FIXED — the construction task, split out of Password. Named build strategies
/// (temporary, reset token, from raw input) live here, each reusing Password's rules.
public sealed class PasswordFactory
{
    private const int TemporaryLength = 16;
    private const int ResetTokenLength = 24;

    public Password Temporary() => Password.Random(TemporaryLength);
    public Password ResetToken() => Password.Random(ResetTokenLength);
    public Password FromRaw(string raw) => Password.Of(raw);
}
