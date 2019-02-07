using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Serialization;
using ReOsuStoryboardPlayer.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayer.Core.UnitTest.Serialization
{
    public class AssertStoryboardObjectEqualityComparer : IEqualityComparer<StoryboardObject>
    {
        public bool Equals(StoryboardObject x, StoryboardObject y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(StoryboardObject obj)
        {
            throw new NotImplementedException();
        }
    }

    public class AssertCommandEqualityComparer : IEqualityComparer<StoryboardObject>
    {
        public bool Equals(StoryboardObject x, StoryboardObject y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(StoryboardObject obj)
        {
            throw new NotImplementedException();
        }
    }

    [TestClass]
    public class WriteReadTest
    {
        static string[] test_cases = new[]
        {
            @".\TestData\Hatsuki Yura - Fuuga (Lan wings).osb"
        };

        List<StoryboardObject> objectsA;
        IEnumerable<StoryboardObject> objectsB;
        MemoryStream stream=new MemoryStream();

        [TestMethod]
        public void MainReadWriteTest()
        {
            foreach (var file_path in test_cases)
            {
                GenerateOsbin(file_path);

                stream.Position=0;

                Parser();

                Judge();
            }
        }

        private void Judge()
        {
            foreach (var objB in objectsB)
            {
                //fast find
                var objA = objectsA.FirstOrDefault(x=>x.FromOsbFile==objB.FromOsbFile&&x.FileLine==objB.FileLine);

                //compare
                Assert.IsTrue(objB.Equals(objA));

                objectsA.Remove(objA);
            }
            
            Assert.IsFalse(objectsA.Any());
        }

        private void Parser()
        {
            objectsB = StoryboardSerializationHelper.Deserialize(stream).ToList();
        }

        private void GenerateOsbin(string file_path)
        {
            objectsA=StoryboardParserHelper.GetStoryboardObjects(file_path);
            
            StoryboardSerializationHelper.Serialize(0,objectsA, stream);
        }
    }
}
