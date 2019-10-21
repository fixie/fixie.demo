namespace ContactList.Features.Contact
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using AutoMapper;
    using FluentValidation;
    using MediatR;
    using Model;

    public class EditContact
    {
        public class Query : IRequest<Command>
        {
            public Guid Id { get; set; }
        }

        public class QueryHandler : RequestHandler<Query, Command>
        {
            readonly Database _database;
            readonly IMapper _mapper;

            public QueryHandler(Database database, IMapper mapper)
            {
                _database = database;
                _mapper = mapper;
            }

            protected override Command Handle(Query message)
            {
                var contact = _database.Contact.Find(message.Id);

                return _mapper.Map<Command>(contact);
            }
        }

        public class Command : IRequest
        {
            public Guid Id { get; set; }

            public string? Email { get; set; }

            public string? Name { get; set; }

            [Display(Name = "Phone Number")]
            public string? PhoneNumber { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Email).NotEmpty().EmailAddress().Length(1, 255);
                RuleFor(x => x.Name).NotEmpty().Length(1, 100);
                RuleFor(x => x.PhoneNumber).Length(1, 50);
            }
        }

        public class CommandHandler : RequestHandler<Command>
        {
            readonly Database _database;
            readonly IMapper _mapper;

            public CommandHandler(Database database, IMapper mapper)
            {
                _database = database;
                _mapper = mapper;
            }

            protected override void Handle(Command message)
            {
                var contact = _database.Contact.Find(message.Id);

                _mapper.Map(message, contact);

                _database.Update(contact);
            }
        }
    }
}