using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElasticSearchDemo.Models
{
    public class File
    {
        public string Extension { set; get; }
        public string Url { set; get; }
        [Text(Name = "filename")]
        public string FileName { set; get; }

    }
}