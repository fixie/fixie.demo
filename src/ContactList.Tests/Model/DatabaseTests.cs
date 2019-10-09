namespace ContactList.Tests.Model
{
    using System;
    using System.Linq;
    using ContactList.Model;
    using Shouldly;
    using static Testing;

    public class DatabaseTests
    {
        public void ShouldRollBackOnFailure()
        {
            var contact = new Contact
            {
                Email = "email@example.com",
                Name = "First Last",
                PhoneNumber = "555-123-4567"
            };

            var countBefore = Count<Contact>();

            Action failingTransaction = () =>
            {
                Transaction(database =>
                {
                    database.Contact.Add(contact);
                    database.SaveChanges();

                    var intermediateCount = database.Contact.Count();
                    intermediateCount.ShouldBe(countBefore + 1);

                    throw new Exception("This is expected to cause a rollback.");
                });
            };

            failingTransaction.Throws<Exception>()
                .Message.ShouldBe("This is expected to cause a rollback.");

            var countAfter = Count<Contact>();

            countAfter.ShouldBe(countBefore);

            var loaded = Query<Contact>(contact.Id);
            loaded.ShouldBeNull();
        }
    }
}