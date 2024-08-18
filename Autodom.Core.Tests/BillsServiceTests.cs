using FluentAssertions;

namespace Autodom.Core.Tests
{
    public class BillsServiceTests
    {
        private readonly BillsService _sut = new();

        [Fact]
        public void ReturnsEmptyWhenNoInput()
        {
            var (bills, record) = _sut.FindUnprocessedBills(new List<BillDto>(), new());

            bills.Should().BeEmpty();
        }
    }
}