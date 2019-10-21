namespace ContactList.Tests.Features.Contact
{
    using System;
    using System.Threading.Tasks;
    using ContactList.Features.Contact;
    using ContactList.Model;
    using static Testing;

    public class EditContactTests
    {
        public async Task ShouldGetCurrentContactDataById()
        {
            var contactToEdit = new AddContact.Command
            {
                Email = "jsmith@example.com",
                Name = "John Smith",
                PhoneNumber = "555-123-0001"
            };

            var anotherContact = new AddContact.Command
            {
                Email = "another.contact@example.com",
                Name = "Another Contact"
            };

            var response = await Send(contactToEdit);
            await Send(anotherContact);

            var selectedContactId = response.ContactId;

            var result = await Send(new EditContact.Query { Id = selectedContactId });

            result.ShouldMatch(new EditContact.Command
            {
                Id = selectedContactId,
                Email = "jsmith@example.com",
                Name = "John Smith",
                PhoneNumber = "555-123-0001"
            });
        }

        public void ShouldRequireMinimumFields()
        {
            new EditContact.Command()
                .ShouldNotValidate(
                    "'Email' must not be empty.",
                    "'Name' must not be empty.");
        }

        public void ShouldRequireValidEmailAddress()
        {
            var command = new EditContact.Command
            {
                Id = Guid.NewGuid(),
                Name = "Patrick Jones",
                PhoneNumber = "555-123-0002"
            };

            command.ShouldNotValidate("'Email' must not be empty.");

            command.Email = "test@example.com";
            command.ShouldValidate();

            command.Email = "test at example dot com";
            command.ShouldNotValidate("'Email' is not a valid email address.");
        }

        public async Task ShouldSaveChangesToContactData()
        {
            var contactToEdit = new AddContact.Command
            {
                Email = "jsmith@example.com",
                Name = "John Smith",
                PhoneNumber = "555-123-0001"
            };

            var anotherContact = new AddContact.Command
            {
                Email = "another.contact@example.com",
                Name = "Another Contact"
            };

            var response = await Send(contactToEdit);
            await Send(anotherContact);

            var selectedContactId = response.ContactId;

            await Send(new EditContact.Command
            {
                Id = selectedContactId,
                Email = "pjones@example.com",
                Name = "Patrick Jones",
                PhoneNumber = "555-123-0002"
            });

            var actual = Query<Contact>(selectedContactId);

            actual.ShouldMatch(new Contact
            {
                Id = selectedContactId,
                Email = "pjones@example.com",
                Name = "Patrick Jones",
                PhoneNumber = "555-123-0002"
            });
        }
    }
}