namespace ContactList.Tests.Features.Contact
{
    using System.Threading.Tasks;
    using ContactList.Features.Contact;
    using ContactList.Model;
    using Shouldly;
    using static Testing;

    public class DeleteContactTests
    {
        public async Task ShouldDeleteContactById()
        {
            var contactToDelete = new AddContact.Command
            {
                Email = "jsmith@example.com",
                Name = "John Smith",
                PhoneNumber = "555-123-0001"
            };

            var contactToPreserve = new AddContact.Command
            {
                Email = "another.contact@example.com",
                Name = "Another Contact"
            };

            var contactToDeleteId = (await Send(contactToDelete)).ContactId;
            var contactToPreserveId = (await Send(contactToPreserve)).ContactId;

            var countBefore = Count<Contact>();

            await Send(new DeleteContact.Command
            {
                Id = contactToDeleteId
            });

            var countAfter = Count<Contact>();
            countAfter.ShouldBe(countBefore - 1);

            var deletedContact = Query<Contact>(contactToDeleteId);
            deletedContact.ShouldBeNull();
            
            var remainingContact = Query<Contact>(contactToPreserveId);
            remainingContact.ShouldNotBeNull();
        }
    }
}