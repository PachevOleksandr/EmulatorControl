using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmulatorControl.Helpers
{
    struct XmlSearchAttribute
    {
        public string Name;
        public string ValuePattern;

        public XmlSearchAttribute(string name, string valuePattern)
        {
            Name = name;
            ValuePattern = valuePattern;
        }

        public override string ToString()
        {
            return $"{Name}={ValuePattern}";
        }
    }
}
