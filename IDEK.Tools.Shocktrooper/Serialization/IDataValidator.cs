using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDEK.Tools.ShocktroopUtils.Serialization
{
    public interface IDataValidator<T>
    {
        bool IsDataValid(T data);
        public IEnumerable<string> HandledKeys { get; }
    }
}