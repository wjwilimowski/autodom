using FluentAssertions;

namespace Autodom.Core.Tests
{
    public class BillsServiceTests
    {
        private readonly BillsService _sut = new();

        [Fact]
        public void ReturnsEmptyWhenNoInput()
        {
            var result = _sut.FindUnprocessedBills(new List<BillDto>(), new());

            result.Should().BeEmpty();
        }
    }
}