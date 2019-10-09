namespace ContactList.Features.Contact
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using AutoMapper;
    using FluentValidation;
    using MediatR;
    using Model;

    public class AddContact
    {
        public class Command : IRequest<Response>
        {
            public string Email { get; set; }

            public string Name { get; set; }

            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
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

        public class Response
        {
            public Guid ContactId { get; set; }
        }

        public class CommandHandler : RequestHandler<Command, Response>
        {
            readonly Database _database;
            readonly IMapper _mapper;

            public CommandHandler(Database database, IMapper mapper)
            {
                _database = database;
                _mapper = mapper;
            }

            protected override Response HandleCore(Command message)
            {
                var contact = _mapper.Map<Contact>(message);

                _database.Contact.Add(contact);

                return new Response
                {
                    ContactId = contact.Id
                };
            }
        }
    }
}