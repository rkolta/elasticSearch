using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElasticSearchDemo.Models
{
    public class Doc
    {
        public string Content { set; get; }
        public File File { set; get; }
        public Path Path { set; get; }
        public Meta Meta { set; get; }
    }
}