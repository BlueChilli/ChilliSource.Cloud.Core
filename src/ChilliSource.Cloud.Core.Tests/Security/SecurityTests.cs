using ChilliSource.Core.Extensions;
using System;
using System.IO;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class SecurityTests
    {
        private const string source = "Lorem ipsum dolor sit amet, potenti nec quam non ut in, suspendisse maecenas nisl commodo nec. Auctor mollis sollicitudin orci orci, leo donec condimentum elementum dui, suscipit elit. Accumsan massa id, ut vivamus. Accumsan gravida risus, pellentesque quisque malesuada, quam eget orci sollicitudin, pede pharetra. Dui felis viverra et pellentesque minima, sem arcu wisi, quasi leo vitae orci netus praesent, nunc vulputate consequatur molestie, lacus ipsum dui massa accumsan interdum. Semper dolorem. Justo sit justo. Eros penatibus, dictum pellentesque, eget dolor tortor, pede sodales adipiscing. Purus eget, eveniet eu id non in nonummy, est nunc sed hac est turpis ut. Lectus commodo donec nulla parturient morbi morbi, interdum fermentum ac taciti, commodo in neque porta per aliquet, pellentesque consequat at primis vitae, dolor vitae./r/nUt nullam penatibus et blandit mattis euismod, cupiditate lacinia non et ullamcorper blandit morbi, eros wisi tincidunt velit. Dapibus dui libero, incidunt integer. Lacus metus bibendum sit adipiscing eget, vitae pede venenatis magna, tincidunt consectetuer bibendum, aliquam suspendisse libero quam, non massa mauris lorem in. Rhoncus lacus lobortis dui, dignissim nec est ligula lacinia, et ligula metus. Mauris dictum, adipiscing a nonummy, purus et auctor eu at est dolor. Commodo lobortis duis libero, tempor ac nibh metus turpis donec integer. Elit non arcu ut, dapibus sem tristique felis consequat platea sapien, ligula sociis tempus posuere dignissim odio, ornare ab nibh quis odio ut lacus.";

        [Fact]
        public void GenerateSaltedHash_ReturnsSameHash_ForValue()
        {
            var password1 = "password1";
            var password2 = "QWERTY";

            Assert.Equal("CF-EB-3D-62-D1-1A-C9-40-E1-02-FA-EA-5C-E4-85-6F-8E-F9-3C-61-BA-B5-A4-9E-FB-BA-4B-64-F9-75-BA-3E", EncryptionHelper.GenerateSaltedHash(password1, "123456"));
            Assert.Equal("14-F3-50-09-43-02-44-DE-0B-C3-E6-3C-15-2C-CE-42-CC-22-45-1E-88-68-6B-02-C4-0C-0B-A7-E1-28-1E-01", EncryptionHelper.GenerateSaltedHash(password2, "ABCDEF"));

        }

        [Fact]
        public void EncryptStringAes_ReturnsSameValue_WhenDecrypted()
        {
            Assert.Equal(source, EncryptionHelper.DecryptStringAes(EncryptionHelper.EncryptStringAes(source, "123456", "ABCDEFGHIJK"), "123456", "ABCDEFGHIJK"));
        }

        [Fact]
        public void GetMd5Hash_ReturnsSameHash_ForValue()
        {
            Assert.Equal("5b514b79bc38a9eb7d2a86193456549c", EncryptionHelper.GetMd5Hash(source));

        }

        [Fact]
        public void Password_GenerateHumanReadable_ReturnsSomething()
        {
            var result = Password.GenerateHumanReadable(2, 2, 2);
            Assert.False(String.IsNullOrEmpty(result));

        }

        [Fact]
        public void RandomCodeGenerator_GenerateCode_ReturnsSomething()
        {
            var result1 = RandomCodeGenerator.GenerateCode(10, CharacterSet.Numbers);
            Assert.False(String.IsNullOrEmpty(result1));
            Assert.Equal(result1.ToNumeric(), result1);
            Assert.Equal(10, result1.Length);

        }

        [Fact]
        public void EncryptedStream_DecryptedStream_ReturnSameValue()
        {
            var stream = new MemoryStream(source.ToByteArray());
            var encryptedStream = EncryptedStream.Create(stream, "123456", "ABCDEFGHIJK");
            var encryptedData = encryptedStream.ReadToByteArray();           
            encryptedStream.Dispose();

            stream = new MemoryStream(encryptedData);
            var decryptedStream = DecryptedStream.Create(stream, "123456", "ABCDEFGHIJK", (int)stream.Length);
            var decryptedData = decryptedStream.ReadToByteArray();
            decryptedStream.Dispose();

            var result = decryptedData.To<string>();

            Assert.Equal(result, source);
        }

    }


}
