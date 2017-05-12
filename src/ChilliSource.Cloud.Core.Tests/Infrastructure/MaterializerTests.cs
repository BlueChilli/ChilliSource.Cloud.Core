using ChilliSource.Cloud.Core.LinqMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{    
    public class MaterializerTests
    {        
        public MaterializerTests()
        {
            LinqMapper.LinqMapper.Reset();
            LinqMapper.Materializer.Reset();

            LinqMapper.Materializer.TreeTraversal = LinqMapper.TreeTraversal.ChildrenFirst;
        }

        [Fact]
        public void TestTracker()
        {
            using (var tracker = new MaterializerTracker())
            {
                var object1 = new ProjectionClassA() { Name = "name1" };

                Assert.True(tracker.BeginTrackObject(object1));
                object1.Name = "other name 2";

                Assert.False(tracker.BeginTrackObject(object1));
            }
        }

        [Fact]
        public void SimpleTest()
        {
            LinqMapper.LinqMapper.CreateMap<ClassA, ProjectionClassA>();

            LinqMapper.Materializer.RegisterAfterMap<ProjectionClassA>((o) =>
            {
                o.Name = o.Name + "_afterMap";
            });

            ClassA[] objects = new ClassA[] { new ClassA() { Name = "my name" } };

            var result = objects.AsQueryable().Materialize<ClassA, ProjectionClassA>().FirstOrDefault();

            Assert.Equal(result.Name, "my name_afterMap");
        }

        [Fact]
        public void TestWithContext()
        {
            LinqMapper.LinqMapper.CreateMap<ClassA, ProjectionClassA>()
                .CreateRuntimeMap<ContextA>((ctx) => runtimeMapA(ctx));

            LinqMapper.Materializer.RegisterAfterMap<ProjectionClassA, ContextA>((o, ctx) =>
            {
                o.Name = o.Name + ctx.MaterializeValue;
            });

            ClassA[] objects = new ClassA[] { new ClassA() { Name = "my name" } };

            var result = objects.AsQueryable().Materialize<ClassA, ProjectionClassA>()
                            .Context(new ContextA() { LinqMapperValue = "B", MaterializeValue = "A" })
                            .ToList();

            Assert.Equal(result[0].Name, "my nameBA");
        }

        [Fact]
        public void TestHierarchyAfterMap()
        {
            LinqMapper.LinqMapper.CreateMap<ClassA, ProjectionClassA>();

            LinqMapper.Materializer.RegisterAfterMap<BaseClassA>((o) =>
            {
                o.Name = o.Name + "_base";
            });

            LinqMapper.Materializer.RegisterAfterMap<IClassA>((o) =>
            {
                o.Name = o.Name + "_interface";
            });

            LinqMapper.Materializer.RegisterAfterMap<ProjectionClassA>((o) =>
            {
                o.Name = o.Name + "_concrete";
            });

            ClassA[] objects = new ClassA[] { new ClassA() { Name = "my name" } };

            var result = objects.AsQueryable().Materialize<ClassA, ProjectionClassA>().FirstOrDefault();

            Assert.Equal(result.Name, "my name_interface_base_concrete");
        }

        [Fact]
        public void TestDictionary()
        {
            LinqMapper.LinqMapper.CreateMap<ClassA, ProjectionClassA>();

            LinqMapper.Materializer.RegisterAfterMap<KeyA>((o) =>
            {
                o.KeyName = o.KeyName + "_keyAfterMap";
            });

            LinqMapper.Materializer.RegisterAfterMap<ProjectionClassA>((o) =>
            {
                o.Name = o.Name + "_afterMap";
            });

            ClassA[] objects = new ClassA[] { new ClassA() { Name = "OriginalValue" } };

            var result = objects.AsQueryable().Materialize<ClassA, ProjectionClassA>()
                            .To(q => q.ToDictionary(a => new KeyA() { KeyName = "Key" + a.Name }));

            Assert.Equal(result.First().Key.KeyName, "KeyOriginalValue_keyAfterMap");
            Assert.Equal(result.First().Value.Name, "OriginalValue_afterMap");
        }

        [Fact]
        public void TestRecursion()
        {
            LinqMapper.Materializer.RegisterAfterMap<RecursiveNodeB>((b) =>
            {
                b.Text = b.Text + "_afterB";
            });

            var nodeA = new RecursiveNodeA() { Text = "node A", ChildrenA = new RecursiveNodeA[] { new RecursiveNodeA() { Text = "node A.1" } } };
            var nodeB = new RecursiveNodeB { Text = "node B", ChildrenA = new RecursiveNodeA[] { new RecursiveNodeA() { Text = "node BA.1" } } };

            nodeA.ChildrenB = new RecursiveNodeB[] { nodeB };

            LinqMapper.Materializer.ApplyAfterMap(nodeA, null);
        }

        [Fact]
        public void TestRecursion2()
        {
            var nodeA = new RecursiveNodeA() { Text = "node A", ChildrenA = new RecursiveNodeA[] { new RecursiveNodeA() { Text = "node A.1" } } };
            var nodeB = new RecursiveNodeB { Text = "node B", ChildrenA = new RecursiveNodeA[] { new RecursiveNodeA() { Text = "node BA.1" } } };

            nodeA.ChildrenB = new RecursiveNodeB[] { nodeB };

            LinqMapper.Materializer.ApplyAfterMap(nodeA, null);
            LinqMapper.Materializer.ApplyAfterMap(nodeA, null);
        }

        [Fact]
        public void TestRecursion3()
        {
            LinqMapper.Materializer.RegisterAfterMap<RecursiveNodeA>((a) =>
            {
                a.Text = a.Text + "_afterA";
            });

            LinqMapper.Materializer.RegisterAfterMap<RecursiveNodeB>((b) =>
            {
                b.Text = b.Text + "_afterB";
            });

            var nodeA = new RecursiveNodeA() { Text = "node A", ChildrenA = new RecursiveNodeA[] { new RecursiveNodeA() { Text = "node A.1" } } };
            //nodeB recursive 
            var nodeB = new RecursiveNodeB { Text = "node B", ChildrenA = new RecursiveNodeA[] { new RecursiveNodeA() { Text = "node BA.1" }, nodeA } };

            nodeA.ChildrenB = new RecursiveNodeB[] { nodeB };

            LinqMapper.Materializer.ApplyAfterMap(nodeA, null);
        }

        private Expression<Func<ClassA, ProjectionClassA>> runtimeMapA(ContextA ctx)
        {
            return (ClassA a) => new ProjectionClassA() { Name = a.Name + ctx.LinqMapperValue };
        }

        public class KeyA
        {
            public string KeyName { get; set; }

            public override bool Equals(object obj)
            {
                var cast = obj as KeyA;
                if (obj == null)
                    return false;

                return this.KeyName == cast.KeyName;
            }

            public override int GetHashCode()
            {
                return KeyName?.GetHashCode() ?? 0;
            }
        }

        public class ContextA
        {
            public string LinqMapperValue { get; set; }
            public string MaterializeValue { get; set; }
        }

        public class ClassA
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public interface IClassA
        {
            string Name { get; set; }
        }

        public class BaseClassA
        {
            public string Name { get; set; }
        }

        public class ProjectionClassA : BaseClassA, IClassA
        {
        }

        public class RecursiveNodeA
        {
            public string Text { get; set; }
            public RecursiveNodeA[] ChildrenA { get; set; }
            public RecursiveNodeB[] ChildrenB { get; set; }
        }

        public class RecursiveNodeB
        {
            public string Text { get; set; }
            public RecursiveNodeA[] ChildrenA { get; set; }
        }
    }
}
