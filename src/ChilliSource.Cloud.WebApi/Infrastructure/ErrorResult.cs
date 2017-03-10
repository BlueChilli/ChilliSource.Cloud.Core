using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.WebApi.Infrastructure
{
    public class ErrorResult
    {
        public ErrorResult()
        {
            Errors = new List<string>();
        }

        public List<string> Errors { get; set; }

        public string ErrorMessage
        {
            get { return string.Join("\n", Errors); }
        }
    }
}
