using System.Collections.Generic;

namespace VolumeBuilderPoc
{
    public class Volume
    {
        private readonly List<Stamp> _stamps = new List<Stamp>();
        public Volume(int id)
        {
            Id = id;
        }
        public int Id { get;}
        public int EventCount { get; set; }
        public List<Stamp> Stamps => _stamps;
    }
}
