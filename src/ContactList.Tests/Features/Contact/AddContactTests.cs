namespace ContactList.Tests.Features.Contact
{
    using System.Threading.Tasks;
    using ContactList.Features.Contact;
    using ContactList.Model;
    using static Testing;

    public class AddContactTests
    {
        public void ShouldRequireMinimumFields()
        {
            new AddContact.Command()
                .ShouldNotValidate(
                    "'Email' must not be empty.",
                    "'Name' must not be empty.");
        }

        public void ShouldRequireValidEmailAddress()
        {
            var command = new AddContact.Command
            {
                Name = "John Smith",
                PhoneNumber = "555-123-9999"
            };

            command.ShouldNotValidate("'Email' must not be empty.");

            command.Email = "test@example.com";
            command.ShouldValidate();

            command.Email = "test at example dot com";
            command.ShouldNotValidate("'Email' is not a valid email address.");
        }

        public async Task ShouldAddNewContact()
        {
            var response = await Send(new AddContact.Command
            {
                Email = "john@example.com",
                Name = "John Smith",
                PhoneNumber = "555-123-9999"
            });

            var actual = Query<Contact>(response.ContactId);

            actual.ShouldMatch(new Contact
            {
                Id = response.ContactId,
                Email = "john@example.com",
                Name = "John Smith",
                PhoneNumber = "555-123-9999"
            });
        }
    }
}