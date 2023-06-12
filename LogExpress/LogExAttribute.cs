using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogExpress
{
    public class LogExAttribute : Attribute
    {

        public string UserId { get; set; } = "0";
        public string Time { get; } = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

    }
}
