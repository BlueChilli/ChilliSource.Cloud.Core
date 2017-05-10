using System;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class ObjectExtensionTests
    {
        [Fact]
        public void ToNullable_ReturnsNullableTypeOfInstance()
        {
            var _int = 32;
            var _decimal = 64.0M;
            var _double = 128.0;

            Assert.Equal(_int.ToNullable<int>().Value, 32);
            Assert.Equal(_decimal.ToNullable<decimal>().Value, 64.0M);
            Assert.Equal(_double.ToNullable<double>().Value, 128.0);
        }

        private const string source = "Lorem ipsum dolor sit amet, potenti nec quam non ut in, suspendisse maecenas nisl commodo nec. Auctor mollis sollicitudin orci orci, leo donec condimentum elementum dui, suscipit elit. Accumsan massa id, ut vivamus. Accumsan gravida risus, pellentesque quisque malesuada, quam eget orci sollicitudin, pede pharetra. Dui felis viverra et pellentesque minima, sem arcu wisi, quasi leo vitae orci netus praesent, nunc vulputate consequatur molestie, lacus ipsum dui massa accumsan interdum. Semper dolorem. Justo sit justo. Eros penatibus, dictum pellentesque, eget dolor tortor, pede sodales adipiscing. Purus eget, eveniet eu id non in nonummy, est nunc sed hac est turpis ut. Lectus commodo donec nulla parturient morbi morbi, interdum fermentum ac taciti, commodo in neque porta per aliquet, pellentesque consequat at primis vitae, dolor vitae./r/nUt nullam penatibus et blandit mattis euismod, cupiditate lacinia non et ullamcorper blandit morbi, eros wisi tincidunt velit. Dapibus dui libero, incidunt integer. Lacus metus bibendum sit adipiscing eget, vitae pede venenatis magna, tincidunt consectetuer bibendum, aliquam suspendisse libero quam, non massa mauris lorem in. Rhoncus lacus lobortis dui, dignissim nec est ligula lacinia, et ligula metus. Mauris dictum, adipiscing a nonummy, purus et auctor eu at est dolor. Commodo lobortis duis libero, tempor ac nibh metus turpis donec integer. Elit non arcu ut, dapibus sem tristique felis consequat platea sapien, ligula sociis tempus posuere dignissim odio, ornare ab nibh quis odio ut lacus.";

        [Serializable]
        public class TestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public float Amount { get; set; }
        }

        [Fact]
        public void ToByteArray_AndBack_UsingTo_ShouldBeTheSameAsOriginalObject()
        {
            var result = source.ToByteArray().To<string>();
            Assert.Equal(result, source);

            var source2 = new TestClass { Id = 1, Name = "Jim Smith", Amount = 12345.6789F };
            var result2 = source2.ToByteArray().To<TestClass>();
            Assert.Equal(result2.Id, source2.Id);
            Assert.Equal(result2.Name, source2.Name);
            Assert.Equal(result2.Amount, source2.Amount);

        }


    }
}
