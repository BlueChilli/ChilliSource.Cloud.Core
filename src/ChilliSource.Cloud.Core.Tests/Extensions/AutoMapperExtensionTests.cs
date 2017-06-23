using AutoMapper;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Humanizer;
using System;
using System.Collections.Generic;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class AutoMapperExtensionTests
    {
        [Fact]
        public void IgnoreIfSourceIsNullOrZero_DoesNotMapField_WhenHasNullOrZeroOrEquivalentValue()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.CreateMap<Apple, Orange>()
                   .ForAllMembers(c => c.IgnoreIfSourceIsNullOrZero())              
            );
            var mapper = config.CreateMapper();

            var orange = new Orange { A = new DateTime(2001, 1, 1), B = "Orange", C = 1, D = 2.0, E = TestEnum.Something };
            var apple1 = new Apple { A = new DateTime(2001, 12, 12), B = "Apple1", C = 10, D = 100.0, E = TestEnum.SomethingElse };
            var apple2 = new Apple { A = DateTime.MinValue, B = null, C = null, D = 0, E = TestEnum.None };

            var orange1 = mapper.Map<Apple, Orange>(apple1, orange.ToJson().FromJson<Orange>());
            Assert.Equal(orange1.ToJson(), apple1.ToJson());

            var orange2 = mapper.Map<Apple, Orange>(apple2, orange.ToJson().FromJson<Orange>());
            Assert.Equal(orange2.ToJson(), orange.ToJson());
        }


        public class Apple
        {
            public DateTime A { get; set; }

            public string B { get; set; }

            public int? C { get; set; }

            public double D { get; set; }

            public TestEnum E { get; set; }
        }

        public class Orange
        {
            public DateTime A { get; set; }

            public string B { get; set; }

            public int? C { get; set; }

            public double D { get; set; }

            public TestEnum E { get; set; }
        }

        public enum TestEnum
        {
            None = 0,
            Something,
            SomethingElse
        }
    }
}
