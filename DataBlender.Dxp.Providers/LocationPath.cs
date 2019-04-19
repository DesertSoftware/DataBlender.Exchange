using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataBlender.Dxp.Providers
{
    public class LocationPath : IEquatable<LocationPath>
    {
        public string CompanyName { get; set; }
        public string RegionName { get; set; }
        public string SiteName { get; set; }

        public bool Equals(LocationPath other) {
            if (Object.ReferenceEquals(other, null)) return false;
            if (Object.ReferenceEquals(this, other)) return true;

            return
                (CompanyName ?? "").Equals(other.CompanyName ?? "", StringComparison.CurrentCultureIgnoreCase) &&
                (RegionName ?? "").Equals(other.RegionName ?? "", StringComparison.CurrentCultureIgnoreCase) &&
                (SiteName ?? "").Equals(other.SiteName ?? "", StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode() {
            int hashCompanyName = CompanyName == null ? 0 : CompanyName.GetHashCode();
            int hashRegionName = RegionName == null ? 0 : RegionName.GetHashCode();
            int hashSiteName = SiteName == null ? 0 : SiteName.GetHashCode();

            return hashCompanyName ^ hashRegionName ^ hashSiteName;
        }
    }
}
