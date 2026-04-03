using System.IO;

namespace IDEK.Tools.DataStructures
{
    public interface ISerializable
    {
        void Serialize(Stream stream);
        void Deserialize(Stream stream);
    }
}
