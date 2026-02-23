using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Me.Shiokawaii.IO
{
    public interface ISerializable
    {
        public abstract Task SerializeAsync(SerialStream stream, CancellationToken cancellationToken = default);
        public abstract Task DeserializeAsync(SerialStream stream, CancellationToken cancellationToken = default);
    }
}