using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeNChill.Models
{
    public class StaffDocument
    {
        public string FileName { get; set; } = string.Empty;

        public long Size { get; set; }

        public DateTimeOffset? LastModified { get; set; }
    }
}



