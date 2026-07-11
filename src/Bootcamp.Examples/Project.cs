// ◀ Slides: Deck 09 SW Design Project with AI — Library Loan service
namespace dev.kaldiroglu.bootcamp.project;

// Topic 09 — the Library Loan service.

public record Book(string Isbn, string Title, int TotalCopies)
{
    public int TotalCopies { get; init; } = TotalCopies >= 0
        ? TotalCopies
        : throw new ArgumentException("TotalCopies cannot be negative");
}

public record Loan(string Isbn, string MemberId, DateOnly DueDate);

/// The port the service depends on (DIP).
public interface ILoanRepository
{
    void Save(Loan loan);
    bool Remove(string isbn, string memberId);
    int ActiveCountForMember(string memberId);
    int CopiesOnLoan(string isbn);
}

public sealed class InMemoryLoanRepository : ILoanRepository
{
    private readonly List<Loan> _loans = new();

    public void Save(Loan loan) => _loans.Add(loan);

    public bool Remove(string isbn, string memberId) =>
        _loans.RemoveAll(l => l.Isbn == isbn && l.MemberId == memberId) > 0;

    public int ActiveCountForMember(string memberId) =>
        _loans.Count(l => l.MemberId == memberId);

    public int CopiesOnLoan(string isbn) =>
        _loans.Count(l => l.Isbn == isbn);
}

public sealed class BookNotFoundException(string isbn)
    : Exception($"No book with isbn: {isbn}");

public sealed class LoanLimitExceededException(string memberId, int limit)
    : Exception($"Member {memberId} already holds the maximum of {limit} loans");

public sealed class NoCopiesAvailableException(string isbn)
    : Exception($"No copies available for isbn: {isbn}");

/// The core use case: borrowing and returning, with the library's rules.
public sealed class LoanService(IReadOnlyDictionary<string, Book> catalogue, ILoanRepository loans)
{
    public const int MaxLoansPerMember = 5;

    public Loan Borrow(string memberId, string isbn, DateOnly dueDate)
    {
        if (!catalogue.TryGetValue(isbn, out var book))
        {
            throw new BookNotFoundException(isbn);
        }
        if (loans.ActiveCountForMember(memberId) >= MaxLoansPerMember)
        {
            throw new LoanLimitExceededException(memberId, MaxLoansPerMember);
        }
        if (loans.CopiesOnLoan(isbn) >= book.TotalCopies)
        {
            throw new NoCopiesAvailableException(isbn);
        }
        var loan = new Loan(isbn, memberId, dueDate);
        loans.Save(loan);
        return loan;
    }

    public void ReturnBook(string memberId, string isbn) => loans.Remove(isbn, memberId);
}
