namespace ContactList.Model
{
    using System;
    using System.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Serilog;

    public class Database : DbContext
    {
        IDbContextTransaction? _currentTransaction;

        public Database(DbContextOptions<Database> options)
            : base(options)
        {
        }

        public DbSet<Contact> Contact { get; set; } = default!;

        public void BeginTransaction()
        {
            if (_currentTransaction != null)
                return;

            _currentTransaction = Database.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void CloseTransaction()
        {
            CloseTransaction(exception: null);
        }

        public void CloseTransaction(Exception? exception)
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("A transaction cannot be closed, because no transaction has been started.");

            try
            {
                if (exception != null)
                {
                    _currentTransaction.Rollback();
                    return;
                }

                SaveChanges();

                _currentTransaction.Commit();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception thrown while attempting to close a transaction.");
                _currentTransaction.Rollback();
                throw;
            }
            finally
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }
}