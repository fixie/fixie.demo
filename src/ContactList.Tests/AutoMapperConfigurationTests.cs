namespace ContactList.Tests
{
    using AutoMapper;
    using static Testing;

    class AutoMapperConfigurationTests
    {
        public void ShouldBeValid()
        {
            Scoped<IMapper>(mapper =>
            {
                mapper.ConfigurationProvider.AssertConfigurationIsValid();
            });
        }
    }
}
