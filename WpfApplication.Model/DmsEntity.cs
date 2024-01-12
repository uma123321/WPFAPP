using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WpfApplication.Model
{
    public class DmsEntity

    {

        public string EntityName { get; set; }

        public string Operation { get; set; }
        public List<DmsAttribute> Attributes { get; set; } = new List<DmsAttribute>();


    }

}
