namespace ContactList.Features.Contact
{
    using System;
    using System.Linq;
    using AutoMapper;
    using MediatR;
    using Model;

    public class ContactIndex
    {
        public class Query : IRequest<ViewModel[]> { }

        public class ViewModel
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
        }

        public class QueryHandler : RequestHandler<Query, ViewModel[]>
        {
            readonly Database _database;
            readonly IMapper _mapper;

            public QueryHandler(Database database, IMapper mapper)
            {
                _database = database;
                _mapper = mapper;
            }

            protected override ViewModel[] HandleCore(Query request)
            {
                return _database.Contact
                    .OrderBy(x => x.Name)
                    .Select(_mapper.Map<ViewModel>)
                    .ToArray();
            }
        }
    }
}