using System;
using System.Collections.Generic;
using System.Linq;
using VolumeBuilderPoc;
using Xunit;

namespace VolumeBuilderPocTests
{
    public class VolumeBuilderTests
    {
        private VolumeBuilder _builder;

        public VolumeBuilderTests()
        {
            _builder = new VolumeBuilder(new Configuration());
        }

        [Theory, MemberData(nameof(TestData))]
        public void WriteVolume_NoStamps_ReturnsEmptyList(IList<Stamp>source)
        {
            var result = _builder.WriteVolume(source, 1, "testtable" );

            Assert.Equal(1, 0);
        }

        public static IEnumerable<object[]> TestData =>
            new List<object[]>
            {
                new object[]{null },
                new object[]{ new List<Stamp>() }
            };
    }
}
