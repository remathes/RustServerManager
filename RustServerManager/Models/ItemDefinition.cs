using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RustServerManager.Models
{
    public class ItemDefinition
    {
        public string Shortname { get; set; }
        public string DisplayName { get; set; }
        public int MaxStack { get; set; }
    }
}
