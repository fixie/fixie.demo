using AutoMapper;

namespace ContactList.Tests
{
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
