using Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;

    public UnitOfWork(DbContext context) {
        _context = context;
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public void SaveChanges() => _context.SaveChanges();
}