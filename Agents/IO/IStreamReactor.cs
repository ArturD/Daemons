using System.IO;
using System.Linq;
using System.Text;
using Daemons.Reactors;

namespace Daemons.IO
{
    public interface IStreamReactor : IReactor 
    {
        Stream Stream { get; }
        void Bind(Stream stream);
    }
}
