using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionALabb4
{
    class Picture
    {
        [JsonProperty(PropertyName = "id")]
        public string _id { get; set; }
        public string PictureURL { get; set; }
    }
}
