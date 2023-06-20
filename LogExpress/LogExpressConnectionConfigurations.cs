using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogExpress
{
    public class LogExpressConnectionConfigurations
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string LogFullTime { get; set; }= DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        public string Parameters { get; set; }
        public string HttpMethod { get; set; }
        public string EndPointName { get; set; }
        public string Response { get; set; }
       
    }
}
