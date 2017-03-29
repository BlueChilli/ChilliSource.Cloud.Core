using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ChilliSource.Cloud.Web.Api.Internal
{
    internal class FormData
    {
        private List<ValueFile> _Files;
        private List<ValueString> _Fields;
        private static Regex _matcher = new Regex(@"(\[.*?\])(\[*?.*?\]*?)$", RegexOptions.Compiled);
        public string Convert(string fieldName)
        {
            if (_matcher.IsMatch(fieldName))
            {
                var matches = _matcher.Split(fieldName).Where(m => !String.IsNullOrWhiteSpace(m)).ToList();
                var propertyName = String.Empty;

                if (matches.Count() == 3)
                {
                    propertyName = String.Format("{0}{1}", matches[0], matches[1]);

                    var property = matches[2].Substring(1 , matches[2].Length - 2);

                    if (_matcher.IsMatch(property))
                    {
                        return String.Format("{0}.{1}", propertyName, Convert(property));
                    }

                    return String.Format("{0}.{1}", propertyName, property);

                }

            }
           
            return fieldName;
        }
        public List<ValueFile> Files
        {
            get
            {
                if(_Files == null)
                    _Files = new List<ValueFile>();
                return _Files;
            }
            set
            {
                _Files = value;
            }
        }

        public List<ValueString> Fields
        {
            get
            {
                if(_Fields == null)
                    _Fields = new List<ValueString>();
                return _Fields;
            }
            set
            {
                _Fields = value;
            }
        }

        public void Add(string name, string value)
        {
            Fields.Add(new ValueString() { Name = Convert(name), Value = value});
        }

        public void Add(string name, HttpPostedFileBase value)
        {
            Files.Add(new ValueFile() { Name = Convert(name), Value = value });
        }

        public bool TryGetValue(string name, out string value)
        {
            var field = Fields.FirstOrDefault(m => String.Equals(m.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (field != null)
            {
                value = field.Value;
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGetValue(string name, out HttpPostedFileBase value)
        {
            var field = Files.FirstOrDefault(m => String.Equals(m.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (field != null)
            {
                value = field.Value;
                return true;
            }
            value = null;
            return false;
        }

        public class ValueString
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class ValueFile
        {
            public string Name { get; set; }
            public HttpPostedFileBase Value { get; set; }
        }
    }
}
