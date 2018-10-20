using System.Collections.Generic;
using System.Linq;

namespace VolumeBuilderPoc
{
    public class Stamp
    {
        private readonly List<int> _partCounts = new List<int>();

        public Stamp(string id)
        {
            Id = id;
        }
        public string Id { get;  }
        public List<int> PartCounts => _partCounts;
        public int Count => _partCounts.Sum(c => c);
    }
}
