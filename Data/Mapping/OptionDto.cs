using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandSyncFile.Data.Mapping
{
    public class OptionDtoResponse
    {
        public List<OptionDto> results { get; set; }
    }
    
    public class OptionDto
    {
        public string value { get; set; }
    }
}
