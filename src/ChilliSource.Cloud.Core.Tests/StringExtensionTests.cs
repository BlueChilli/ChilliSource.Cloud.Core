using System;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class StringExtensionTests
    {
        private const string source = "Lorem ipsum dolor sit amet, potenti nec quam non ut in, suspendisse maecenas nisl commodo nec. Auctor mollis sollicitudin orci orci, leo donec condimentum elementum dui, suscipit elit. Accumsan massa id, ut vivamus. Accumsan gravida risus, pellentesque quisque malesuada, quam eget orci sollicitudin, pede pharetra. Dui felis viverra et pellentesque minima, sem arcu wisi, quasi leo vitae orci netus praesent, nunc vulputate consequatur molestie, lacus ipsum dui massa accumsan interdum. Semper dolorem. Justo sit justo. Eros penatibus, dictum pellentesque, eget dolor tortor, pede sodales adipiscing. Purus eget, eveniet eu id non in nonummy, est nunc sed hac est turpis ut. Lectus commodo donec nulla parturient morbi morbi, interdum fermentum ac taciti, commodo in neque porta per aliquet, pellentesque consequat at primis vitae, dolor vitae./r/nUt nullam penatibus et blandit mattis euismod, cupiditate lacinia non et ullamcorper blandit morbi, eros wisi tincidunt velit. Dapibus dui libero, incidunt integer. Lacus metus bibendum sit adipiscing eget, vitae pede venenatis magna, tincidunt consectetuer bibendum, aliquam suspendisse libero quam, non massa mauris lorem in. Rhoncus lacus lobortis dui, dignissim nec est ligula lacinia, et ligula metus. Mauris dictum, adipiscing a nonummy, purus et auctor eu at est dolor. Commodo lobortis duis libero, tempor ac nibh metus turpis donec integer. Elit non arcu ut, dapibus sem tristique felis consequat platea sapien, ligula sociis tempus posuere dignissim odio, ornare ab nibh quis odio ut lacus.";

        #region Security

        [Fact]
        public void SaltedHash_ReturnsSameHash_ForValue()
        {
            var password1 = "password1";
            var password2 = "QWERTY";

            Assert.Equal("", password1.SaltedHash("123456"));
            Assert.Equal("", password2.SaltedHash("ABCDEF"));

        }

        [Fact]
        public void AesEncrypt_ReturnsSameValue_WhenDecrypted()
        {
            Assert.Equal(source, source.AesEncrypt("123456", "ABCDEF").AesDecrypt("123456", "ABCDEF"));
        }

        #endregion

        #region Convert To & From
        [Fact]
        public void ToNullable_ReturnsNullableType_OfConvertedValue()
        {
            Assert.ThrowsAny<Exception>(() => "NotWork".ToNullable<int>().Value);
            Assert.Equal("16".ToNullable<int>().Value, 16);
            Assert.Equal("true".ToNullable<bool>().Value, true);
            Assert.Equal("123.456".ToNullable<decimal>().Value, 123.456M);
        }

        [Fact]
        public void To_ReturnsType_OfConvertedValue()
        {
            Assert.Equal("16".To<int>(), 16);
            Assert.Equal("true".To<bool>(), true);
            Assert.Equal("123.456".To<decimal>(), 123.456M);
        }
        #endregion

    }
}
