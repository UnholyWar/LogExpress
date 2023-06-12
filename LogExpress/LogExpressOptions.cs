using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogExpress
{
    public class LogExpressOptions
    {
        [Required]
        public string ConnectionString { get; set; }
        [Required]
        public string TableName { get; set; } = "MyLogExpress";
        
    }
}
