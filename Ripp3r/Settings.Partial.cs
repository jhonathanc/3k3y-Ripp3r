using System;

namespace Ripp3r.Properties
{
    partial class Settings
    {
        public long CalculatedSize
        {
            get { return (long) (zipPartSize*(Math.Pow(1024, zipPartUnit))); }
        }
    }
}
