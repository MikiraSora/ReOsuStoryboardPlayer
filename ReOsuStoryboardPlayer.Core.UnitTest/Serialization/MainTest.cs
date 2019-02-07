using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Serialization;
using ReOsuStoryboardPlayer.Core.Serialization.DeserializationFactory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayer.Core.UnitTest.Serialization
{
    [TestClass]
    public class MainTest
    {
        [TestMethod]
        public void CommandTest()
        {
            var command = new FadeCommand();
            command.StartTime=1000;
            command.EndTime=2000;
            command.StartValue=1;
            command.EndValue=0;
            command.Easing=EasingTypes.None;

            MemoryStream stream = new MemoryStream();

            StringCacheTable cache = new StringCacheTable();

            using (var writer=new BinaryWriter(stream))
            {
                command.OnSerialize(writer, cache);
            }

            var bytes = stream.ToArray();
            stream.Dispose();

            stream=new MemoryStream(bytes);

            using (var reader=new BinaryReader(stream))
            {
                var fade = CommandDeserializtionFactory.Create(reader, cache);

                Assert.AreEqual(command.StartTime, fade.StartTime);
                Assert.AreEqual(command.EndTime, fade.EndTime);
                Assert.AreEqual(command.Event, fade.Event);

                var x=fade as FadeCommand;
                Assert.IsNotNull(x);

                Assert.AreEqual(command.StartValue, x.StartValue);
                Assert.AreEqual(command.EndValue, x.EndValue);
                Assert.AreEqual(command.Easing, x.Easing);
            }
        }
    }
}
