namespace ContactList.Features.Contact
{
    using AutoMapper;
    using Model;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Contact, ContactIndex.ViewModel>();

            CreateMap<Contact, EditContact.Command>();
            CreateMap<EditContact.Command, Contact>();

            CreateMap<AddContact.Command, Contact>()
                .ForMember(x => x.Id, options => options.Ignore());
        }
    }
}