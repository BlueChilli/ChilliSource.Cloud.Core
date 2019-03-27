using ChilliSource.Cloud.Core;
using Humanizer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class ShortGuidTests
    {
        [Fact]
        public void ToShortGuid_GivenGuid_RetunsShortGuidRepresentationOfGuid()
        {
            var guid = Guid.Parse("7B9DA377-CD4B-431E-BE8E-16693D1B613C");

            var sg = guid.ToShortGuid();
            Assert.Equal("d6Ode0vNHkO-jhZpPRthPA", sg.Value);
            Assert.Equal(sg.Guid, guid);

            Assert.Equal(ShortGuid.Empty.Guid, Guid.Empty);

            var s = ShortGuid.Encode(guid.ToString());
            var g = ShortGuid.Decode(s);
            Assert.Equal(g, guid);

            Assert.True(sg == new ShortGuid(s));

            Assert.Null(ShortGuid.Decode("7-0l0eWeHRnv93947qn9bvSD"));
            Assert.Equal(ShortGuid.Empty.Value, new ShortGuid("7-0l0eWeHRnv93947qn9bvSD").Value);
        }

        [Fact]
        public void TypeConverterConvertFromNullable()
        {
            const string stringValue = "8-xFfr7wrEeLFxKW1W15Cw";
            Guid guid = new Guid("7E45ECF3-F0BE-47AC-8B17-1296D56D790B");

            var typeConverter = TypeDescriptor.GetConverter(typeof(ShortGuid?));
            var convertedShortGuidNull = (ShortGuid?)typeConverter.ConvertFrom(null);
            var convertedShortGuidEmpty = (ShortGuid?)typeConverter.ConvertFrom(String.Empty);
            var convertedShortGuid = (ShortGuid?)typeConverter.ConvertFrom(stringValue);
            var convertedFromGuid = (ShortGuid?)typeConverter.ConvertFrom(guid);
            var convertedFromGuidNullable = (ShortGuid?)typeConverter.ConvertFrom((Guid?)guid);

            Assert.True(typeConverter.CanConvertFrom(typeof(String)));
            Assert.True(typeConverter.CanConvertFrom(typeof(Guid)));

            Assert.Null(convertedShortGuidNull);
            Assert.Null(convertedShortGuidEmpty);
            Assert.Equal(guid, convertedShortGuid.Value.Guid);
            Assert.Equal(guid, convertedFromGuid.Value.Guid);
            Assert.Equal(guid, convertedFromGuidNullable.Value.Guid);
        }

        [Fact]
        public void TypeConverterConvertToNullable()
        {
            var guid = new Guid("7E45ECF3-F0BE-47AC-8B17-1296D56D790B");
            ShortGuid? shortGuid = new ShortGuid(guid);
            const string stringValue = "8-xFfr7wrEeLFxKW1W15Cw";

            var typeConverter = TypeDescriptor.GetConverter(typeof(ShortGuid?));

            Assert.True(typeConverter.CanConvertTo(typeof(String)));
            Assert.True(typeConverter.CanConvertTo(typeof(Guid)));
            Assert.True(typeConverter.CanConvertTo(typeof(Guid?)));

            var convertedToStringNull = typeConverter.ConvertToString(null);
            var convertedToStringEmpty = typeConverter.ConvertToString((ShortGuid?)ShortGuid.Empty);
            var convertedToString = typeConverter.ConvertToString(shortGuid);
            var convertedToGuid = typeConverter.ConvertTo(shortGuid, typeof(Guid?));

            Assert.Equal(String.Empty, convertedToStringNull);
            Assert.Equal(ShortGuid.Empty.Value, convertedToStringEmpty);
            Assert.Equal(stringValue, convertedToString);
            Assert.Equal(guid, convertedToGuid);
        }

        [Fact]
        public void TypeConverterConvertFromEmpty()
        {
            var typeConverter = TypeDescriptor.GetConverter(typeof(ShortGuid));
            var convertedShortGuidNull = typeConverter.ConvertFrom(null);
            var convertedShortGuidEmpty = typeConverter.ConvertFrom(String.Empty);

            Assert.Equal(ShortGuid.Empty, convertedShortGuidNull);
            Assert.Equal(ShortGuid.Empty, convertedShortGuidEmpty);
        }

        [Fact]
        public void TypeConverterConvertFrom()
        {
            var guid = new Guid("7E45ECF3-F0BE-47AC-8B17-1296D56D790B");
            var shortGuid = new ShortGuid(guid);
            const string stringValue = "8-xFfr7wrEeLFxKW1W15Cw";

            var typeConverter = TypeDescriptor.GetConverter(typeof(ShortGuid));

            var convertedShortGuidString = typeConverter.ConvertFrom(stringValue);
            var convertedShortGuidGuid = typeConverter.ConvertFrom(guid);

            Assert.True(typeConverter.CanConvertFrom(typeof(String)));
            Assert.True(typeConverter.CanConvertFrom(typeof(Guid)));

            Assert.Equal(shortGuid, convertedShortGuidString);
            Assert.Equal(shortGuid, convertedShortGuidGuid);
        }

        [Fact]
        public void TypeConverterConvertToEmpty()
        {
            var typeConverter = TypeDescriptor.GetConverter(typeof(ShortGuid));
            var convertedToStringDefault = typeConverter.ConvertToString(default(ShortGuid));

            var convertedToStringNull = typeConverter.ConvertToString(new ShortGuid((string)null));
            var convertedToStringEmpty = typeConverter.ConvertToString(new ShortGuid(String.Empty));

            Assert.Equal(ShortGuid.Empty.Value, convertedToStringDefault);
            Assert.Equal(ShortGuid.Empty.Value, convertedToStringNull);
            Assert.Equal(ShortGuid.Empty.Value, convertedToStringEmpty);
        }

        [Fact]
        public void TypeConverterConvertTo()
        {
            var guid = new Guid("7E45ECF3-F0BE-47AC-8B17-1296D56D790B");
            var shortGuid = new ShortGuid(guid);
            const string stringValue = "8-xFfr7wrEeLFxKW1W15Cw";

            var typeConverter = TypeDescriptor.GetConverter(typeof(ShortGuid));

            var convertedToString = typeConverter.ConvertToString(shortGuid);
            var convertedToGuid = typeConverter.ConvertTo(shortGuid, typeof(Guid));

            Assert.True(typeConverter.CanConvertTo(typeof(String)));
            Assert.True(typeConverter.CanConvertTo(typeof(Guid)));

            Assert.Equal(stringValue, convertedToString);
            Assert.Equal(guid, convertedToGuid);
        }
    }
}
