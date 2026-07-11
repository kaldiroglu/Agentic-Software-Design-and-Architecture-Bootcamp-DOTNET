using dev.kaldiroglu.bootcamp.project;
using Xunit;

namespace Bootcamp.Examples.Tests;

public class ProjectTests
{
    private static readonly DateOnly Due = new(2026, 8, 1);

    private static (LoanService svc, ILoanRepository loans) Wire()
    {
        var catalogue = new Dictionary<string, Book>
        {
            ["isbn-dune"] = new("isbn-dune", "Dune", 2),
            ["isbn-solo"] = new("isbn-solo", "Solo Copy", 1),
        };
        var loans = new InMemoryLoanRepository();
        return (new LoanService(catalogue, loans), loans);
    }

    [Fact]
    public void BorrowingRecordsAnActiveLoan()
    {
        var (svc, loans) = Wire();
        svc.Borrow("m1", "isbn-dune", Due);
        Assert.Equal(1, loans.ActiveCountForMember("m1"));
        Assert.Equal(1, loans.CopiesOnLoan("isbn-dune"));
    }

    [Fact]
    public void CannotBorrowAnUnknownBook()
    {
        var (svc, _) = Wire();
        Assert.Throws<BookNotFoundException>(() => svc.Borrow("m1", "isbn-ghost", Due));
    }

    [Fact]
    public void CannotBorrowWhenNoCopiesRemain()
    {
        var (svc, _) = Wire();
        svc.Borrow("m1", "isbn-solo", Due);
        Assert.Throws<NoCopiesAvailableException>(() => svc.Borrow("m2", "isbn-solo", Due));
    }

    [Fact]
    public void EnforcesTheFiveLoanLimit()
    {
        var catalogue = new Dictionary<string, Book>();
        for (var i = 0; i < 6; i++)
        {
            catalogue["b" + i] = new("b" + i, "Book " + i, 5);
        }
        var svc = new LoanService(catalogue, new InMemoryLoanRepository());
        for (var i = 0; i < 5; i++)
        {
            svc.Borrow("m1", "b" + i, Due);
        }
        Assert.Throws<LoanLimitExceededException>(() => svc.Borrow("m1", "b5", Due));
    }

    [Fact]
    public void ReturningFreesTheCopyForSomeoneElse()
    {
        var (svc, loans) = Wire();
        svc.Borrow("m1", "isbn-solo", Due);
        svc.ReturnBook("m1", "isbn-solo");
        svc.Borrow("m2", "isbn-solo", Due);
        Assert.Equal(1, loans.CopiesOnLoan("isbn-solo"));
        Assert.Equal(0, loans.ActiveCountForMember("m1"));
    }
}
