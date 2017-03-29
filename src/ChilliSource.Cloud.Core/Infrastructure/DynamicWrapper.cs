using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Basic implementation of a dynamic wrapper that allows late-bound member access.
    /// </summary>
    public class DynamicWrapper : DynamicObject
    {
        private readonly object _wrappedObject;
        private readonly Type _type;

        /// <summary>
        /// Wraps an object in a dynamic object
        /// </summary>
        /// <param name="obj">target object</param>
        /// <returns>A dynamic wrapper that allows late-bound member access</returns>
        public static dynamic Wrap(object obj)
        {
            if (obj == null)
                return null;

            return new DynamicWrapper(obj);
        }

        private DynamicWrapper(object obj)
        {
            _wrappedObject = obj;
            _type = obj.GetType();
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type == _type)
            {
                result = _wrappedObject;
                return true;
            }
            var converter = TypeDescriptor.GetConverter(_type);

            if (converter.CanConvertTo(binder.Type))
            {
                result = converter.ConvertTo(_wrappedObject, binder.Type);
                return true;
            }

            result = null;
            return false;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var members = _type.GetMember(binder.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (members.Length != 1)
                throw new ApplicationException($"Member [{binder.Name}] not found or duplicate detected");

            var member = members.First();
            object callResult = null;
            if (member is PropertyInfo)
            {
                callResult = (member as PropertyInfo).GetValue(_wrappedObject);
            }
            else
            {
                throw new ApplicationException($"Member type not supported yet: {member.GetType().FullName}");
            }

            result = DynamicWrapper.Wrap(callResult);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var members = _type.GetMember(binder.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (members.Length != 1)
                throw new ApplicationException($"Member [{binder.Name}] not found or duplicate detected");

            var member = members.First();
            object callResult = null;
            if (member is MethodInfo)
            {
                callResult = (member as MethodInfo).Invoke(_wrappedObject, args);
            }
            else
            {
                throw new ApplicationException($"Member type not supported yet: {member.GetType().FullName}");
            }

            result = DynamicWrapper.Wrap(callResult);
            return true;
        }

        public override string ToString()
        {
            return _wrappedObject.ToString();
        }
    }
}
