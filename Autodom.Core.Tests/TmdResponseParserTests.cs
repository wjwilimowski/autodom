using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodom.Core.Dtos;
using FluentAssertions;

namespace Autodom.Core.Tests
{
    public class TmdResponseParserTests
    {
        [Fact]
        public void AccountBalanceDto()
        {
            const string json =
            @"[
                [
                    ""o"",
                    -771.57,
                    0,
                    true,
                    [
                        [
                            ""k26102011692309123000000075"",
                            ""26102011692309123000000075"",
                            80512,
                            -771.57,
                            0,
                            ""2024-08-12T00:00:00"",
                            [
                                ""Rozrachunki""
                            ],
                            null
                        ]
                    ]
                ]
            ]";

            var result = TmdResponseParser.ParseAccountBalanceDto(json);

            result.Balance.Should().Be(-771.57m);
        }
    }
}
