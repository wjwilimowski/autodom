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

        [Fact]
        public void AllBillsHaveBeenProcessed_ReturnsNone()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void SomeNewBillsWithOldDate_ReturnsNewBills_UpdatesRecordWithNewBills()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void BillsWithANewDate_AllReturned_AllRecorded()
        {
            throw new NotImplementedException();
        }
    }
}