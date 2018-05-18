using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElasticSearchDemo.Models
{
    public class Raw
    {
        [Text(Name = "Line-Count")]
        public string LineCount { set; get; }
        [Text(Name = "xmpTPg:NPages")]
        public string PageCount { set; get; }
        public string Date { set; get; }
    }
}