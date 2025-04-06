using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMA.Models.ExternalModels
{
    internal class KMModel
    {
        public int Val0 { get; set; }
        public string? Val1 { get; set; }
        public int Val2 { get; set; }


        public override string ToString()
        {
            var properties = this.GetType().GetProperties();

            var values = properties.Select(p => p.GetValue(this)?.ToString() ?? string.Empty);

            return string.Join(",", values);
        }
    }
}
