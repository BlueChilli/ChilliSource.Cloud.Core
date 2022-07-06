using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class GoogleAddressTests
    {
        private const string Json = "{\"address\":\"17 Germaine Ave, Mount Riverview NSW 2774, Australia\",\"formatted_address\":\"17 Germaine Ave, Mount Riverview NSW 2774, Australia\",\"geometry\":{\"location\":{\"lat\":-33.7373426,\"lng\":150.6261958},\"latitude\":-33.7373426,\"longitude\":150.6261958},\"addressParts\":[{\"types\":[\"street_number\"],\"long_name\":\"17\",\"short_name\":\"17\",\"isEmpty\":false},{\"types\":[\"route\"],\"long_name\":\"Germaine Avenue\",\"short_name\":\"Germaine Ave\",\"isEmpty\":false},{\"types\":[\"locality\",\"political\"],\"long_name\":\"Mount Riverview\",\"short_name\":\"Mount Riverview\",\"isEmpty\":false},{\"types\":[\"administrative_area_level_2\",\"political\"],\"long_name\":\"Blue Mountains City Council\",\"short_name\":\"Blue Mountains\",\"isEmpty\":false},{\"types\":[\"administrative_area_level_1\",\"political\"],\"long_name\":\"New South Wales\",\"short_name\":\"NSW\",\"isEmpty\":false},{\"types\":[\"country\",\"political\"],\"long_name\":\"Australia\",\"short_name\":\"AU\",\"isEmpty\":false},{\"types\":[\"postal_code\"],\"long_name\":\"2774\",\"short_name\":\"2774\",\"isEmpty\":false}],\"address_components\":[{\"types\":[\"street_number\"],\"long_name\":\"17\",\"short_name\":\"17\",\"isEmpty\":false},{\"types\":[\"route\"],\"long_name\":\"Germaine Avenue\",\"short_name\":\"Germaine Ave\",\"isEmpty\":false},{\"types\":[\"locality\",\"political\"],\"long_name\":\"Mount Riverview\",\"short_name\":\"Mount Riverview\",\"isEmpty\":false},{\"types\":[\"administrative_area_level_2\",\"political\"],\"long_name\":\"Blue Mountains City Council\",\"short_name\":\"Blue Mountains\",\"isEmpty\":false},{\"types\":[\"administrative_area_level_1\",\"political\"],\"long_name\":\"New South Wales\",\"short_name\":\"NSW\",\"isEmpty\":false},{\"types\":[\"country\",\"political\"],\"long_name\":\"Australia\",\"short_name\":\"AU\",\"isEmpty\":false},{\"types\":[\"postal_code\"],\"long_name\":\"2774\",\"short_name\":\"2774\",\"isEmpty\":false}]}";

        [Fact]
        public void AddressIsCorrect()
        {
            var address = new GoogleAddress(Json);

            Assert.Equal("17 Germaine Ave, Mount Riverview NSW 2774, Australia", address.Address);
            Assert.Equal("17 Germaine Ave", address.Street);
        }

        [Fact]
        public void CustomAddressIsCorrect()
        {
            var address = new GoogleAddress(Json);
            address.SetOptions(new GoogleAddress.CustomOptions { UseLongStreetType = true });

            Assert.Equal("17 Germaine Avenue", address.Street);
        }

    }
}
