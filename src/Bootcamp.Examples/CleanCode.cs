// ◀ Slides: Deck 03 Clean Code — honest errors, guard clauses
namespace dev.kaldiroglu.bootcamp.cleancode;

// Topic 03 — Clean Code: honest errors and guard clauses.

public sealed class UserNotFoundException(string id)
    : Exception($"No user with id: {id}");

/// SMELL — swallows the miss and returns null; the failure is hidden.
public sealed class UserFinderSmell(IReadOnlyDictionary<string, string> users)
{
    public string? Find(string id)
    {
        try
        {
            return users.GetValueOrDefault(id);   // null when absent
        }
        catch (Exception)
        {
            return null;                          // even swallows real errors
        }
    }
}

/// FIXED — fails fast and clearly with a named exception.
public sealed class UserFinder(IReadOnlyDictionary<string, string> users)
{
    public string Find(string id)
    {
        if (!users.TryGetValue(id, out var name))
        {
            throw new UserNotFoundException(id);
        }
        return name;
    }
}

/// SMELL — deep nesting buries the happy path.
public sealed class EligibilitySmell
{
    public string Describe(int age, bool member, bool banned)
    {
        if (!banned)
        {
            if (member)
            {
                if (age >= 18) return "eligible";
                else return "too young";
            }
            else return "not a member";
        }
        else return "banned";
    }
}

/// FIXED — guard clauses handle each disqualifier up front.
public sealed class Eligibility
{
    public string Describe(int age, bool member, bool banned)
    {
        if (banned) return "banned";
        if (!member) return "not a member";
        if (age < 18) return "too young";
        return "eligible";
    }
}
