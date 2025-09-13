using Newtonsoft.Json.Linq;
using NINA.Astrometry;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photon.NINA.Skyflats {

    internal class CalculateNullPoint {
        public InputTopocentricCoordinates NullPointCoordinates => Calculate();
        private IProfileService profileService;

        public CalculateNullPoint(IProfileService profileService) {
            this.profileService = profileService;
        }

        public InputTopocentricCoordinates Calculate() {
            InputTopocentricCoordinates nullPoint = null;

            nullPoint = new InputTopocentricCoordinates(Angle.ByDegree(profileService.ActiveProfile.AstrometrySettings.Latitude),
                        Angle.ByDegree(profileService.ActiveProfile.AstrometrySettings.Longitude),
                        profileService.ActiveProfile.AstrometrySettings.Elevation);

            // get sun azimuth

            double jd = AstroUtil.GetJulianDate(DateTime.Now);

            ObserverInfo observer = new ObserverInfo();
            observer.Latitude = profileService.ActiveProfile.AstrometrySettings.Latitude;
            observer.Longitude = profileService.ActiveProfile.AstrometrySettings.Longitude;
            observer.Elevation = profileService.ActiveProfile.AstrometrySettings.Elevation;

            NOVAS.SkyPosition sun = AstroUtil.GetSunPosition(DateTime.UtcNow, jd, observer);
            var siderealTime = AstroUtil.GetLocalSiderealTime(DateTime.Now, observer.Longitude);
            var hourAngle = AstroUtil.HoursToDegrees(AstroUtil.GetHourAngle(siderealTime, sun.RA));

            double alt = AstroUtil.GetAltitude(hourAngle, observer.Latitude, sun.Dec);
            double az = AstroUtil.GetAzimuth(hourAngle, alt, observer.Latitude, sun.Dec);

            double nullAz = NormalizeAngle(az + 180); // point to the opposite direction of the sun

            int azDegrees = (int)nullAz;
            double azMinFrac = (nullAz - azDegrees) * 60;
            int azMinutes = (int)azMinFrac;
            double azSeconds = (azMinFrac - azMinutes) * 60;

            nullPoint.AzDegrees = azDegrees;
            nullPoint.AzMinutes = azMinutes;
            nullPoint.AzSeconds = azSeconds;
            nullPoint.AltDegrees = 75; // offset 15 degrees from zenith

            return nullPoint;
            // await telescopeMediator.SlewToTopocentricCoordinates(nullPoint.Coordinates, token);
        }

        /// <summary>
        /// Normalize angle to range 0-360
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private double NormalizeAngle(double angle) {
            angle = angle % 360;
            if (angle < 0) angle += 360;
            return angle;
        }
    }
}