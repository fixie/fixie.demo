namespace ContactList.Tests.Model
{
    using System;
    using ContactList.Model;
    using Shouldly;
    using static Testing;

    public class ContactTests
    {
        public void ShouldPersist()
        {
            var contact = new Contact("email@example.com", "First Last", "555-123-4567");

            contact.Id.ShouldBe(Guid.Empty);

            Transaction(database => database.Contact.Add(contact));

            contact.Id.ShouldNotBe(Guid.Empty);

            var loaded = Query<Contact>(contact.Id);
            loaded.ShouldMatch(contact);
        }
    }
}