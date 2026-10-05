using Microsoft.EntityFrameworkCore;
using Webstaurant.IDS.Api.Domains.EDI.Models;
using Webstaurant.IDS.Api.Infrastructure.Data;

namespace Webstaurant.IDS.Api.Infrastructure.Repositories;

public interface IEdiRepository
{
    Task<IEnumerable<EdiTransaction>> GetAllTransactionsAsync();
    Task<EdiTransaction> SaveTransactionAsync(EdiTransaction transaction);
}

public class EdiRepository : IEdiRepository
{
    private readonly AppDbContext _context;

    public EdiRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EdiTransaction>> GetAllTransactionsAsync()
    {
        return await _context.EdiTransactions
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<EdiTransaction> SaveTransactionAsync(EdiTransaction transaction)
    {
        _context.EdiTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }
}
