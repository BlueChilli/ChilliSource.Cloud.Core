using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Base class for CheckSum attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class CheckSumNumberAttribute : ValidationAttribute, IClientValidatable
    {
        /// <summary>
        /// Description of CheckSumType to be used in client validation rules.
        /// </summary>
        public string CheckSumType { get; private set; }

        protected CheckSumNumberAttribute(string checkSumType)
        {
            CheckSumType = checkSumType;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule()
                           {
                               ErrorMessage = !String.IsNullOrWhiteSpace(this.ErrorMessage) ? ErrorMessage : FormatErrorMessage(metadata.DisplayName),
                               ValidationType = "checksum"
                           };

            rule.ValidationParameters.Add("checksumtype", this.CheckSumType);

            yield return rule;
        }

        protected static string RemoveWhitespacesInBetween(string number)
        {
            return number.Replace(" ", "");
        }
    }
}