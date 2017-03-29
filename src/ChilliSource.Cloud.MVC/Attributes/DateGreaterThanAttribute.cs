using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.ComponentModel;
using ChilliSource.Cloud.Core;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Validates that a DateTime field must have a value greater than another DateTime field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DateGreaterThanAttribute : ValidationAttribute, IClientValidatable
    {
        /// <summary>
        /// (Optional) Set this if you want to use the unaltered DisplayName attribute value in the error message
        /// </summary>
        public string MyProperty { get; private set; }
        /// <summary>
        /// Other DateTime field name.
        /// </summary>
        public string OtherProperty { get; private set; }

        private string _OtherPropertyDisplayName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherProperty"></param>
        /// <param name="myProperty">Set this if you want to use the unaltered DisplayName attribute value in the error message</param>
        public DateGreaterThanAttribute(string otherProperty, string myProperty = null)
        {
            OtherProperty = otherProperty;
            MyProperty = myProperty;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format("{0} must be greater than {1}", name, _OtherPropertyDisplayName);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var otherProperty = validationContext.ObjectInstance.GetType().GetProperty(this.OtherProperty);

            if (otherProperty == null) 
                return new ValidationResult(String.Format("unknown property {0}", this.OtherProperty));

            var displayAttribute = otherProperty.GetCustomAttribute<DisplayNameAttribute>(true);

            _OtherPropertyDisplayName = displayAttribute != null ? displayAttribute.DisplayName : OtherProperty.ToSentenceCase(true);

            if (value == null || !(value is DateTime))
                return ValidationResult.Success;

            var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

            if (otherValue == null || !(otherValue is DateTime))
                return ValidationResult.Success;

            var to = (DateTime) value;
            var from = (DateTime) otherValue;

            var myProperty = this.MyProperty == null ? null : validationContext.ObjectInstance.GetType().GetProperty(this.MyProperty);
            var myDisplayAttribute = myProperty == null ? null : myProperty.GetCustomAttribute<DisplayNameAttribute>(true);
            return to < from ? new ValidationResult(FormatErrorMessage(myDisplayAttribute == null ? validationContext.DisplayName.ToSentenceCase(true) : myDisplayAttribute.DisplayName)) : ValidationResult.Success;
        }


        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var otherProperty = metadata.ContainerType.GetProperty(this.OtherProperty);
            var displayAttribute = otherProperty.GetAttribute<DisplayAttribute>(true);

            _OtherPropertyDisplayName = displayAttribute != null ? displayAttribute.GetName() : OtherProperty.ToSentenceCase(true);          
            var rule = new ModelClientValidationRule()
                           {
                               ValidationType = "greaterthan",
                               ErrorMessage = FormatErrorMessage(metadata.DisplayName)
                           };

            rule.ValidationParameters["other"] = OtherProperty;         
            yield return rule;
        }


    }
}