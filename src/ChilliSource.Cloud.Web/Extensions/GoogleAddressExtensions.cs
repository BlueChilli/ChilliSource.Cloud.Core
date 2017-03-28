
using ChilliSource.Cloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web
{
    public static class GoogleAddressExtensions
    {
        /// <summary>
        /// Converts this address into a MVC Route (Country, State, Suburb, Street, Lat and Lng)
        /// </summary>
        /// <returns>MVC Route dictionary</returns>
        public static RouteValueDictionary ToRoute(this GoogleAddress googleAddress)
        {
            var street = googleAddress.StreetParts.Name == null || googleAddress.StreetParts.Type == null ? "" : googleAddress.StreetParts.Name.ToSeoUrl() + "-" + googleAddress.StreetParts.Type.ToSeoUrl();
            var route = new RouteValueDictionary(new { Country = googleAddress.Country.ToSeoUrl(), State = googleAddress.State.ToSeoUrl(), Suburb = googleAddress.Suburb.ToSeoUrl(), Street = street });
            if (googleAddress.Location != null)
            {
                route.Add("Lat", googleAddress.Location.Latitude);
                route.Add("Lng", googleAddress.Location.Longitude);
            }
            return route;
        }
    }
}
