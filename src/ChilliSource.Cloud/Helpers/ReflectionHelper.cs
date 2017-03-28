using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud
{
    public static class ReflectionHelper
    {
        #region GetPropertyValue
        public static object GetPropertyValue(PropertyInfo property, object propertyContainer)
        {
            if (!property.CanRead) return null;
            if (propertyContainer == null) return null;
            if (property.GetIndexParameters().Length > 0) return null;
            if (property.ReflectedType.Name != propertyContainer.GetType().Name)
                return null;
            return property.GetValue(propertyContainer, null);
        }

        public static T GetPropertyValue<T>(PropertyInfo property, object propertyContainer)
        {
            return (T)GetPropertyValue(property, propertyContainer);
        }

        public static object GetPropertyValue(string propertyName, object propertyContainer)
        {
            //when property name is Class1.Class2.PropertyName etc. extract the value using ExtractValue
            if (propertyName.IndexOf(".") > -1)
            {
                return WalkModel(propertyContainer, propertyName);
            }

            return GetPropertyValue(propertyContainer.GetType().GetProperty(propertyName), propertyContainer);
        }

        public static T GetPropertyValue<T>(string propertyName, object propertyContainer)
        {
            return (T)GetPropertyValue(propertyName, propertyContainer);
        }

        public static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> e)
        {
            var member = (MemberExpression)e.Body;
            return member.Member.Name;
        }

        public static MemberInfo GetProperty<T, TProperty>(Expression<Func<T, TProperty>> e)
        {
            var member = (MemberExpression)e.Body;
            return member.Member;
        }

        public static PropertyInfo AsPropertyInfo(this LambdaExpression expression)
        {
            PropertyInfo info = null;
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                info = ((MemberExpression)expression.Body).Member as PropertyInfo;
            }

            return info;
        }

        public static string GetExpressionText(this LambdaExpression expression)
        {
            var nameParts = new Stack<string>();
            var part = expression.Body;

            while (part != null)
            {
                if (part.NodeType != ExpressionType.MemberAccess)
                {
                    break;
                }

                var memberExpressionPart = (MemberExpression)part;
                nameParts.Push("." + memberExpressionPart.Member.Name);
                part = memberExpressionPart.Expression;

            }


            return nameParts.Count > 0 ? nameParts.Aggregate((left, right) => left + right).TrimStart('.') : String.Empty;
        }
        #endregion

        #region Set property value
        public static void SetPropertyValue(string propertyName, object value, object propertyContainer)
        {
            //when property name is Class1.Class2.PropertyName etc. extract the value using ExtractValue
            if (propertyName.IndexOf(".") > -1)
            {
                propertyContainer = ReflectionHelper.WalkModel(propertyContainer, propertyName);
            }

            SetPropertyValue(propertyContainer.GetType().GetProperty(propertyName), propertyContainer, value);
        }

        public static void SetPropertyValue(PropertyInfo property, object propertyContainer, object value)
        {
            if (!property.CanWrite) return;
            if (propertyContainer == null) return;
            //Not sure if needed.
            //if (property.GetIndexParameters().Length > 0) return;
            //if (property.ReflectedType.Name != propertyContainer.GetType().Name) return; 

            if (property.PropertyType.IsEnum)
            {
                if (value is string)
                {
                    value = Enum.Parse(property.PropertyType, value as string, ignoreCase: true);
                }
            }
            else
            {
                Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                value = (value == null) ? null : Convert.ChangeType(value, t);
            }
            property.SetValue(propertyContainer, value);
        }
        #endregion

        #region Walk Model
        //Extract the value of property in a model found at the end of a propertyPath
        //propertyPath = "MyFirstProp.MySecondProp.MyThirdProp.MyPropValueToExtract";
        public static T WalkModel<T>(object model, string propertyPath)
        {
            return (T)WalkModel(model, propertyPath);
        }

        //Return the last property at the end of a propertyPath
        //propertyPath = "MyFirstProp.MySecondProp.MyThirdProp.MyPropValueToExtract";        
        public static object WalkModel(object model, string propertyPath)
        {
            PropertyInfo property = null;
            object value = model;
            string[] properties = propertyPath.Split('.');
            foreach (string propertyName in properties)
            {
                if (property == null)
                {
                    property = model.GetType().GetProperty(propertyName);
                }
                else
                {
                    property = property.PropertyType.GetProperty(propertyName);
                }
                value = GetPropertyValue(property, value);
                if (value == null) break;
            }
            return value;
        }

        public static PropertyInfo WalkModel(PropertyInfo property, string propertyPath)
        {
            string[] properties = propertyPath.Split('.');
            for (int i = 0; i < properties.Length; i++)
            {
                if (String.IsNullOrEmpty(properties[i]))
                {
                    i++;
                    property = property.DeclaringType.GetProperty(properties[i]);
                }
                else
                {
                    property = property.PropertyType.GetProperty(properties[i]);
                }
            }
            return property;
        }     
        #endregion
    }
}
