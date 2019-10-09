namespace ContactList.Infrastructure
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using Model;

    public class UnitOfWork : IActionFilter
    {
        readonly Database _database;

        public UnitOfWork(Database database)
        {
            _database = database;
        }

        public void OnActionExecuting(ActionExecutingContext context)
            => _database.BeginTransaction();

        public void OnActionExecuted(ActionExecutedContext context)
            => _database.CloseTransaction(context.Exception);
    }
}