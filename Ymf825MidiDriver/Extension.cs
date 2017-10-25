using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Ymf825MidiDriver
{
    internal static class Extension
    {
        public static T DeepClone<T>(this T @object)
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, @object);
                ms.Position = 0;
                return (T)bf.Deserialize(ms);
            }
        }
    }
}
