using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Fitbit
{
    public static class StringEnum
    {
        public static string GetStringValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            //Check first in our cached results...

            //Look for our 'StringValueAttribute' 

            //in the field's custom attributes

            //this is the way we did it in previous non PCL library:
            //FieldInfo fi = type.GetField(value.ToString());

            //http://stackoverflow.com/questions/19512274/is-getfields-supported-in-a-pcl
            //this is the attempted PCL way:

            var runtimeFields = type.GetRuntimeFields().Where(x => (x.IsPublic || x.IsStatic) && x.Name == value.ToString());

            FieldInfo fi = runtimeFields.FirstOrDefault();

            StringValue[] attrs =
               fi.GetCustomAttributes(typeof(StringValue),
                                       false) as StringValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }

            return output;
        }

    }


    public class StringValue : System.Attribute
    {
        private string _value;

        public StringValue(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }

    }

}
