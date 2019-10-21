namespace ContactList.Features.Contact
{
    using System;
    using MediatR;
    using Model;

    public class DeleteContact
    {
        public class Command : IRequest
        {
            public Guid Id { get; set; }
            public string? Name { get; set; }
        }

        public class CommandHandler : RequestHandler<Command>
        {
            readonly Database _database;

            public CommandHandler(Database database)
            {
                _database = database;
            }

            protected override void Handle(Command message)
            {
                var contact = _database.Contact.Find(message.Id);

                _database.Contact.Remove(contact);
            }
        }
    }
}