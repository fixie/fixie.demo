namespace ContactList.Model
{
    public class Contact : Entity
    {
        public Contact(string email, string name, string? phoneNumber = null)
        {
            Email = email;
            Name = name;
            PhoneNumber = phoneNumber;
        }

        public string Email { get; set; }
        public string Name { get; set; }
        public string? PhoneNumber { get; set; }
    }
}