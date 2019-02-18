#if NET_4X
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public class DecimalPrecisionAttribute : Attribute
    {
        byte _precision;
        private byte _scale;

        public DecimalPrecisionAttribute(byte precision, byte scale)
        {
            _precision = precision;
            _scale = scale;
        }

        public byte Precision { get { return _precision; } }
        public byte Scale { get { return _scale; } }
    }

    public class DecimalPrecisionAttributeConvention : PrimitivePropertyAttributeConfigurationConvention<DecimalPrecisionAttribute>
    {
        public override void Apply(ConventionPrimitivePropertyConfiguration configuration, DecimalPrecisionAttribute attribute)
        {
            if (attribute.Precision < 1 || attribute.Precision > 38)
            {
                throw new InvalidOperationException("Precision must be between 1 and 38.");
            }

            if (attribute.Scale > attribute.Precision)
            {
                throw new InvalidOperationException("Scale must be between 0 and the Precision value.");
            }

            configuration.HasPrecision(attribute.Precision, attribute.Scale);

        }
    }
}
#endif
