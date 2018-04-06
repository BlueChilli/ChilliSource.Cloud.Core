using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using ChilliSource.Cloud.Core.LinqMapper;
using Xunit;
using System.Linq.Expressions;
using System.Diagnostics;

namespace ChilliSource.Cloud.Core.Tests.Infrastructure
{
    public class LinqMapperTests : IDisposable
    {
        private readonly StringBuilder Console = new StringBuilder();
        private readonly ITestOutputHelper _output;

        public void Dispose()
        {
            var outputStr = Console.ToString();
            if (outputStr.Length > 0)
            {
                _output.WriteLine(outputStr);
            }
        }

        public LinqMapperTests(ITestOutputHelper output)
        {
            _output = output;

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            LinqMapper.LinqMapper.Reset();
            LinqMapper.LinqMapper.AllowNullPropertyProjection(p => false);
        }

        [Fact]
        public void TestSimple()
        {
            LinqMapper.LinqMapper.CreateMap<Room, RoomProjection>();
            var map = LinqMapper.LinqMapper.GetMap<Room, RoomProjection>();

            Assert.Equal("src => new RoomProjection() {Name = src.Name}", map.ToString());
        }

        [Fact]
        public void TestSecondProjection()
        {
            LinqMapper.LinqMapper.CreateMap<Room, RoomProjection>();
            LinqMapper.LinqMapper.CreateMap<Booking, BookingProjection>();

            var map = LinqMapper.LinqMapper.GetMap<Booking, BookingProjection>();
            Assert.Equal("src => new BookingProjection() {Id = src.Id, StartTime = src.StartTime, EndTime = src.EndTime, Room = new RoomProjection() {Name = src.Room.Name}}", map.ToString());

            Exception eex = null;
            try
            {
                var bookingWithNullRoom = new Booking() { Room = null };

                var projection = map.Compile()(bookingWithNullRoom);
            }
            catch (Exception ex)
            {
                eex = ex;
            }

            if (eex == null)
                throw new ApplicationException("Exception was excepted");
        }

        [Fact]
        public void TestSecondProjectionAllowNullProjection()
        {
            LinqMapper.LinqMapper.AllowNullPropertyProjection(p => true);

            LinqMapper.LinqMapper.CreateMap<Room, RoomProjection>();
            LinqMapper.LinqMapper.CreateMap<Booking, BookingProjection>();

            var map = LinqMapper.LinqMapper.GetMap<Booking, BookingProjection>();
            Assert.Equal("src => new BookingProjection() {Id = src.Id, StartTime = src.StartTime, EndTime = src.EndTime, Room = IIF((src.Room == null), null, new RoomProjection() {Name = src.Room.Name})}", map.ToString());

            var bookingWithNullRoom = new Booking() { Room = null };

            var projection = map.Compile()(bookingWithNullRoom);
            Assert.True(projection.Room == null);
        }

        [Fact]
        public void TestSecondProjection_RegistrationOrder()
        {
            LinqMapper.LinqMapper.CreateMap<Booking, BookingProjection>();
            LinqMapper.LinqMapper.CreateMap<Room, RoomProjection>();

            var map = LinqMapper.LinqMapper.GetMap<Booking, BookingProjection>();
            Assert.Equal("src => new BookingProjection() {Id = src.Id, StartTime = src.StartTime, EndTime = src.EndTime, Room = new RoomProjection() {Name = src.Room.Name}}", map.ToString());
        }

        [Fact]
        public void TestSecondProjectionNotFound()
        {
            LinqMapper.LinqMapper.CreateMap<Booking, BookingProjection>();
            var map = LinqMapper.LinqMapper.GetMap<Booking, BookingProjection>();

            Assert.Equal("src => new BookingProjection() {Id = src.Id, StartTime = src.StartTime, EndTime = src.EndTime}", map.ToString());
        }

        [Fact]
        public void TestOverride()
        {
            LinqMapper.LinqMapper.CreateMap<Booking, BookingProjection>(src => new BookingProjection() { Id = 435 });
            var map = LinqMapper.LinqMapper.GetMap<Booking, BookingProjection>();

            Assert.Equal("src => new BookingProjection() {StartTime = src.StartTime, EndTime = src.EndTime, Id = 435}", map.ToString());
        }

        [Fact]
        public void TestInvokeMap()
        {
            LinqMapper.LinqMapper.CreateMap<Room, RoomProjection>();
            LinqMapper.LinqMapper.CreateMap<Booking, BookingProjection>(src => new BookingProjection() { Room = src.Room.InvokeMap<Room, RoomProjection>() });

            var map = LinqMapper.LinqMapper.GetMap<Booking, BookingProjection>();
            Assert.Equal("src => new BookingProjection() {Id = src.Id, StartTime = src.StartTime, EndTime = src.EndTime, Room = new RoomProjection() {Name = src.Room.Name}}", map.ToString());
        }

        [Fact]
        public void TestInvokeMapExplicitly()
        {
            Expression<Func<Room, RoomProjection>> roomMap = (Room r) => new RoomProjection() { Name = r.Name };
            LinqMapper.LinqMapper.CreateMap<Booking, BookingProjection>(src => new BookingProjection() { Room = src.Room.InvokeMap(roomMap) });

            var map = LinqMapper.LinqMapper.GetMap<Booking, BookingProjection>();
            Assert.Equal("src => new BookingProjection() {Id = src.Id, StartTime = src.StartTime, EndTime = src.EndTime, Room = new RoomProjection() {Name = src.Room.Name}}", map.ToString());
        }

        [Fact]
        public void TestExtend()
        {
            LinqMapper.LinqMapper.CreateMap<Member, MemberProjectionExtension>();
            var map = LinqMapper.LinqMapper.GetMap<Member, MemberProjectionExtension>();

            var extendedMap = LinqMapper.LinqMapper.ExtendMap(map,
                (Member m) => new MemberProjectionExtension() { Name = "blah", OtherInfo = "Confidential info" + m.Id });

            Assert.Equal("src => new MemberProjectionExtension() {Id = src.Id, Name = src.Name}", map.ToString());
            Assert.Equal("src => new MemberProjectionExtension() {Id = src.Id, Name = \"blah\", OtherInfo = (\"Confidential info\" + Convert(src.Id))}", extendedMap.ToString());
        }

        [Fact]
        public void TestCollectionProjection()
        {
            LinqMapper.LinqMapper.CreateMap<Member, MemberProjection>();
            LinqMapper.LinqMapper.CreateMap<Organisation, OrganisationProjection>();

            var map = LinqMapper.LinqMapper.GetMap<Organisation, OrganisationProjection>();

            Assert.Equal("src => new OrganisationProjection() {Name = src.Name, Members = src.Members.AsQueryable().Select(p_Members => new MemberProjection() {Id = p_Members.Id, Name = p_Members.Name}).ToList(), SpecialMembers = src.SpecialMembers.AsQueryable().Select(p_SpecialMembers => new MemberProjection() {Id = p_SpecialMembers.Id, Name = p_SpecialMembers.Name}).ToArray()}", map.ToString());
        }

        [Fact]
        public void SpeedTestMakeGenericMethod()
        {
            var methodInfo = typeof(LinqMapperTests).GetMethod("GenericMethodTX");
            var type1 = typeof(Member);
            var type2 = typeof(MemberProjection);

            var watch = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                var method = methodInfo.MakeGenericMethod(type1, type2);
            }
            watch.Stop();

            //Click through test result to see this
            Console.AppendLine($"SpeedTestMakeGenericMethod (ms): {watch.ElapsedMilliseconds}");
        }

        public static void GenericMethodTX<T, X>()
        {
        }

        [Fact]
        public void SpeedTestCollectionProjection()
        {
            LinqMapper.LinqMapper.CreateMap<Member, MemberProjection>();
            LinqMapper.LinqMapper.CreateMap<Organisation, OrganisationProjection>();

            var map = LinqMapper.LinqMapper.GetMap<Organisation, OrganisationProjection>();

            var watch = Stopwatch.StartNew();
            for (int i = 0; i < 5000; i++)
            {
                map = LinqMapper.LinqMapper.GetMap<Organisation, OrganisationProjection>();
            }

            watch.Stop();

            //Click through test result to see this
            Console.AppendLine($"SpeedTestCollectionProjection (ms): {watch.ElapsedMilliseconds}");
        }

        [Fact]
        public void TestMapperContext()
        {
            LinqMapper.LinqMapper.CreateMap<Member, MemberProjectionExtension>()
                .CreateRuntimeMap<TestContextClass>((TestContextClass ctx) =>
                {
                    return (Member m) => new MemberProjectionExtension() { Name = "blah", OtherInfo = "Context info: " + ctx.ContextValue };
                });

            var context = LinqMapper.LinqMapper.CreateContext();
            context.SetContext(new TestContextClass() { ContextValue = "abcd" });

            var map = LinqMapper.LinqMapper.GetMap<Member, MemberProjectionExtension>(context);
            var mapConstant = (map.Compile().Target as System.Runtime.CompilerServices.Closure).Constants[0];

            var assertValue = $"src => new MemberProjectionExtension() {{Id = src.Id, Name = \"blah\", OtherInfo = (\"Context info: \" + value({mapConstant.GetType().FullName}).ctx.ContextValue)}}";
            Assert.Equal(assertValue, map.ToString());
        }

        [Fact]
        public void TestSecondLevelExpand()
        {
            LinqMapper.LinqMapper.CreateMap<Member, MemberProjectionExtension>()
               .CreateRuntimeMap<TestContextClass>((TestContextClass ctx) =>
               {
                   return (Member m) => new MemberProjectionExtension() { Name = "blah", OtherInfo = "Context info: " + ctx.ContextValue };
               });

            LinqMapper.LinqMapper.CreateMap<Organisation, OrganisationProjectionExtension>((Organisation o) => new OrganisationProjectionExtension()
            {
                SpecialMembers = o.SpecialMembers.Select(m => m.InvokeMap<Member, MemberProjectionExtension>()).ToArray()
            })
            .CreateRuntimeMap((IObjectContext orgContext) =>
            {
                var memberMap = LinqMapper.LinqMapper.GetMap<Member, MemberProjectionExtension>(orgContext);
                var orgValue = orgContext.GetContext<TestContextOrgClass>().OrgValue;
                return (Organisation o) => new OrganisationProjectionExtension()
                {
                    Name = "Name " + orgValue,
                    Members = o.Members.Select(m => m.InvokeMap<Member, MemberProjectionExtension>(memberMap)).ToList()
                };
            });

            var context = LinqMapper.LinqMapper.CreateContext();
            context.SetContext(new TestContextClass() { ContextValue = "abcd" });
            context.SetContext(new TestContextOrgClass() { OrgValue = "OrgValue" });

            var map = LinqMapper.LinqMapper.GetMap<Organisation, OrganisationProjectionExtension>(context);
            var types = (map.Compile().Target as System.Runtime.CompilerServices.Closure).Constants.SelectMany(c => c.GetType() == typeof(object[]) ? (object[])c : new object[] { c })
                                .Select(c => c.GetType().FullName).Where(n => !n.Contains("DynamicMethod")).ToArray();
            var assertValue = $"o => new OrganisationProjectionExtension() {{Name = (\"Name \" + value({types[0]}).orgValue), Members = o.Members.Select(m => new MemberProjectionExtension() {{Id = m.Id, Name = \"blah\", OtherInfo = (\"Context info: \" + value({types[1]}).ctx.ContextValue)}}).ToList(), SpecialMembers = o.SpecialMembers.Select(m => new MemberProjectionExtension() {{Id = m.Id, Name = \"blah\", OtherInfo = (\"Context info: \" + value({types[1]}).ctx.ContextValue)}}).ToArray()}}";
            Assert.Equal(assertValue, map.ToString());
        }

        [Fact]
        public void SpeedTestSecondLevelExpand()
        {
            LinqMapper.LinqMapper.CreateMap<Member, MemberProjectionExtension>()
               .CreateRuntimeMap<TestContextClass>((TestContextClass ctx) =>
               {
                   return (Member m) => new MemberProjectionExtension() { Name = "blah", OtherInfo = "Context info: " + ctx.ContextValue };
               });

            LinqMapper.LinqMapper.CreateMap<Organisation, OrganisationProjectionExtension>((Organisation o) => new OrganisationProjectionExtension()
            {
                SpecialMembers = o.SpecialMembers.Select(m => m.InvokeMap<Member, MemberProjectionExtension>()).ToArray()
            })
            .CreateRuntimeMap((IObjectContext orgContext) =>
            {
                var memberMap = LinqMapper.LinqMapper.GetMap<Member, MemberProjectionExtension>(orgContext);
                var orgValue = orgContext.GetContext<TestContextOrgClass>().OrgValue;
                return (Organisation o) => new OrganisationProjectionExtension()
                {
                    Name = "Name " + orgValue,
                    Members = o.Members.Select(m => m.InvokeMap<Member, MemberProjectionExtension>(memberMap)).ToList()
                };
            });

            var context = LinqMapper.LinqMapper.CreateContext();
            context.SetContext(new TestContextClass() { ContextValue = "abcd" });
            context.SetContext(new TestContextOrgClass() { OrgValue = "OrgValue" });

            var map = LinqMapper.LinqMapper.GetMap<Organisation, OrganisationProjectionExtension>(context);

            var watch = Stopwatch.StartNew();
            for (int i = 0; i < 5000; i++)
            {
                context = LinqMapper.LinqMapper.CreateContext();
                context.SetContext(new TestContextClass() { ContextValue = "abcd" });
                context.SetContext(new TestContextOrgClass() { OrgValue = "OrgValue" });

                map = LinqMapper.LinqMapper.GetMap<Organisation, OrganisationProjectionExtension>(context);
            }

            watch.Stop();
            //Click through test result to see this
            Console.AppendLine($"SpeedTestSecondLevelExpand (ms): {watch.ElapsedMilliseconds}");
        }

        [Fact]
        public void TestIncludeBase()
        {
            LinqMapper.LinqMapper.CreateMap<Member, MemberProjection>((Member m) => new MemberProjection() { Id = 0, Name = m + "Custom" });
            LinqMapper.LinqMapper.CreateMap<Member, MemberProjectionExtension>((Member m) => new MemberProjectionExtension() { Id = 123, OtherInfo = m + "Other" })
                        .IncludeBase<Member, MemberProjection>();

            var map = LinqMapper.LinqMapper.GetMap<Member, MemberProjectionExtension>();
            var assertValue = "m => new MemberProjectionExtension() {Name = (m + \"Custom\"), Id = 123, OtherInfo = (m + \"Other\")}";

            Assert.Equal(assertValue, map.ToString());
        }


        [Fact]
        public void TestIgnoreMembers()
        {
            LinqMapper.LinqMapper.CreateMap<Room, RoomProjection>();
            LinqMapper.LinqMapper.CreateMap<Booking, BookingProjection>()
                .IgnoreMembers(d => d.StartTime, d => d.EndTime);

            var map = LinqMapper.LinqMapper.GetMap<Booking, BookingProjection>();
            Assert.Equal("src => new BookingProjection() {Id = src.Id, Room = new RoomProjection() {Name = src.Room.Name}}", map.ToString());
        }

        [Fact]
        public void TestIgnoreMembersByName()
        {
            LinqMapper.LinqMapper.CreateMap<Room, RoomProjection>();
            LinqMapper.LinqMapper.CreateMap<Booking, BookingProjection>()
                .IgnoreMembers("StartTime", "EndTime");

            var map = LinqMapper.LinqMapper.GetMap<Booking, BookingProjection>();
            Assert.Equal("src => new BookingProjection() {Id = src.Id, Room = new RoomProjection() {Name = src.Room.Name}}", map.ToString());
        }

        [Fact]
        public void TestRuntimeIgnore()
        {
            LinqMapper.LinqMapper.CreateMap<Room, RoomProjection>();
            LinqMapper.LinqMapper.CreateMap<Booking, BookingProjection>()
                .IgnoreRuntimeMembers<TestRuntimeIgnoreContext>(GetTestIgnoredProperties);

            var ignoreContext = new TestRuntimeIgnoreContext();
            var ctx = LinqMapper.LinqMapper.CreateContext();
            ctx.SetContext<TestRuntimeIgnoreContext>(ignoreContext);

            var map = LinqMapper.LinqMapper.GetMap<Booking, BookingProjection>(ctx);
            Assert.Equal("src => new BookingProjection() {Id = src.Id, StartTime = src.StartTime, EndTime = src.EndTime, Room = new RoomProjection() {Name = src.Room.Name}}", map.ToString());

            ignoreContext.IgnoreStartTime = true;
            map = LinqMapper.LinqMapper.GetMap<Booking, BookingProjection>(ctx);
            Assert.Equal("src => new BookingProjection() {Id = src.Id, EndTime = src.EndTime, Room = new RoomProjection() {Name = src.Room.Name}}", map.ToString());

            ignoreContext.IgnoreEndTime = true;
            map = LinqMapper.LinqMapper.GetMap<Booking, BookingProjection>(ctx);
            Assert.Equal("src => new BookingProjection() {Id = src.Id, Room = new RoomProjection() {Name = src.Room.Name}}", map.ToString());
        }

        [Fact]
        public void TestNullableProjection()
        {
            LinqMapper.LinqMapper.CreateMap<DataWithNullable, ProjectionWithoutNullable>();
            var map = LinqMapper.LinqMapper.GetMap<DataWithNullable, ProjectionWithoutNullable>();

            Assert.Equal("src => new ProjectionWithoutNullable() {Id = (src.Id ?? 0), Value = (src.Value ?? 01/01/0001 00:00:00)}", map.ToString());
        }

        [Fact]
        public void TestNullableProjectionReversed()
        {
            LinqMapper.LinqMapper.CreateMap<DataWithoutNullable, ProjectionWithNullable>();

            //Expression<Func<DataWithoutNullable, ProjectionWithNullable>> exp = src => new ProjectionWithNullable()
            //{
            //    Id = src.Id,
            //    Value = src.Value
            //};

            var map = LinqMapper.LinqMapper.GetMap<DataWithoutNullable, ProjectionWithNullable>();
            Assert.Equal("src => new ProjectionWithNullable() {Id = Convert(src.Id), Value = Convert(src.Value)}", map.ToString());
        }

        [Fact]
        public void TestBothNullableProjection()
        {
            LinqMapper.LinqMapper.CreateMap<DataWithNullable, ProjectionWithNullable>();

            var map = LinqMapper.LinqMapper.GetMap<DataWithNullable, ProjectionWithNullable>();
            Assert.Equal("src => new ProjectionWithNullable() {Id = src.Id, Value = src.Value}", map.ToString());
        }

        private IEnumerable<string> GetTestIgnoredProperties(TestRuntimeIgnoreContext ctx)
        {
            if (ctx.IgnoreStartTime) yield return "StartTime";
            if (ctx.IgnoreEndTime) yield return "EndTime";
        }

        [Fact]
        public void TestMultipleLevelsProjection()
        {
            LinqMapper.LinqMapper.CreateMap<TestData.Questionnaire, TestModel.QuestionnaireApiModel>();

            LinqMapper.LinqMapper.CreateMap<TestData.Tag, TestModel.TagApiModel>();

            LinqMapper.LinqMapper.CreateMap<TestData.PublishedLocation, TestModel.TagApiModel>().IncludeBase<TestData.Tag, TestModel.TagApiModel>();
            LinqMapper.LinqMapper.CreateMap<TestData.IntroType, TestModel.TagApiModel>().IncludeBase<TestData.Tag, TestModel.TagApiModel>();
            LinqMapper.LinqMapper.CreateMap<TestData.PostLength, TestModel.TagApiModel>().IncludeBase<TestData.Tag, TestModel.TagApiModel>();
            LinqMapper.LinqMapper.CreateMap<TestData.PostSubType, TestModel.SubTypeApiModel>()
                .IncludeBase<TestData.Tag, TestModel.TagApiModel>();

            LinqMapper.LinqMapper.CreateMap<TestData.EmotionalResponse, TestModel.TagApiModel>().IncludeBase<TestData.Tag, TestModel.TagApiModel>();
            LinqMapper.LinqMapper.CreateMap<TestData.BrandPositioning, TestModel.TagApiModel>().IncludeBase<TestData.Tag, TestModel.TagApiModel>();
            LinqMapper.LinqMapper.CreateMap<TestData.TimeSensitivity, TestModel.TagApiModel>().IncludeBase<TestData.Tag, TestModel.TagApiModel>();

            LinqMapper.LinqMapper.CreateMap<TestData.PostType, TestModel.PostTypeSummaryApiModel>();

            var postTypeData = new TestData.PostType[] { new TestData.PostType() };
            var postSubTypeData = new TestData.PostSubType[] { new TestData.PostSubType() { PostType = postTypeData[0] } };

            var data = new TestData.Questionnaire[]
            {
                new TestData.Questionnaire(){
                    PostSubType = postSubTypeData[0],
                    BrandPositioning = new TestData.BrandPositioning(),
                    EmotionalResponse = new TestData.EmotionalResponse(),
                    PostLength = new TestData.PostLength(),
                    PublishedLocation = new TestData.PublishedLocation(),
                    TimeSensitivity = new TestData.TimeSensitivity()
                }
            };

            var subTypeMap = LinqMapper.LinqMapper.GetMap<TestData.PostSubType, TestModel.SubTypeApiModel>();
            var projectedPostSubType = postSubTypeData.AsQueryable().Select(subTypeMap).FirstOrDefault();
            Assert.NotNull(projectedPostSubType);

            var map = LinqMapper.LinqMapper.GetMap<TestData.Questionnaire, TestModel.QuestionnaireApiModel>();
            var projected = data.AsQueryable().Select(map).FirstOrDefault();

            Assert.NotNull(projected);
        }
    }

    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RoomProjection
    {
        public string Name { get; set; }

        public string UnsettableName { get { return "fixed"; } } //should not be mapped.
    }

    public class Booking
    {
        public int Id { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; }
    }

    public class BookingProjection
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public RoomProjection Room { get; set; }
    }

    public class Organisation
    {
        public int Id { get; set; }
        public string Name { get; set; }

        SortedSet<Member> _sorted = null;
        public ICollection<Member> Members { get { return _sorted; } set { _sorted = new SortedSet<Member>(value); } }

        public List<Member> SpecialMembers { get; set; }
    }

    public class Member
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class OrganisationProjection
    {
        public string Name { get; set; }
        public List<MemberProjection> Members { get; set; }
        public MemberProjection[] SpecialMembers { get; set; }
    }

    public class MemberProjection
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class MemberProjectionExtension : MemberProjection
    {
        public string OtherInfo { get; set; }
    }

    public class OrganisationProjectionExtension
    {
        public string Name { get; set; }
        public List<MemberProjectionExtension> Members { get; set; }
        public MemberProjectionExtension[] SpecialMembers { get; set; }
    }

    public class TestContextClass
    {
        public string ContextValue { get; set; }
    }

    public class TestContextOrgClass
    {
        public string OrgValue { get; set; }
    }

    public class TestRuntimeIgnoreContext
    {
        public bool IgnoreStartTime { get; set; }
        public bool IgnoreEndTime { get; set; }
    }

    public class DataWithNullable
    {
        public int? Id { get; set; }
        public DateTime? Value { get; set; }
    }

    public class ProjectionWithoutNullable
    {
        public int Id { get; set; }
        public DateTime Value { get; set; }
    }

    public class DataWithoutNullable
    {
        public int Id { get; set; }
        public DateTime Value { get; set; }
    }

    public class ProjectionWithNullable
    {
        public int? Id { get; set; }
        public DateTime? Value { get; set; }
    }
}
