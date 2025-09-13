using Newtonsoft.Json;
using NINA.Astrometry;
using NINA.Core.Model;
using NINA.Profile.Interfaces;
using NINA.Core.Utility.Notification;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Profile;
using NINA.Sequencer.SequenceItem;
using Photon.NINA.Skyflats.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NINA.Core.Locale;
using NINA.Astrometry.Body;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Google.Protobuf.Reflection.ExtensionRangeOptions.Types;
using static NINA.Equipment.Equipment.MyGPS.PegasusAstro.UnityApi.DriverUranusReport;

namespace Photon.NINA.Skyflats {

    /// <summary>
    /// This Class shows the basic principle on how to add a new Sequence Instruction to the N.I.N.A. sequencer via the plugin interface
    /// For ease of use this class inherits the abstract SequenceItem which already handles most of the running logic, like logging, exception handling etc.
    /// A complete custom implementation by just implementing ISequenceItem is possible too
    /// The following MetaData can be set to drive the initial values
    /// --> Name - The name that will be displayed for the item
    /// --> Description - a brief summary of what the item is doing. It will be displayed as a tooltip on mouseover in the application
    /// --> Icon - a string to the key value of a Geometry inside N.I.N.A.'s geometry resources
    ///
    /// If the item has some preconditions that should be validated, it shall also extend the IValidatable interface and add the validation logic accordingly.
    /// </summary>
    [ExportMetadata("Name", "Slew to null point")]
    [ExportMetadata("Description", "This item will just show a notification and is just there to show how the plugin system works")]
    [ExportMetadata("Icon", "Plugin_Skyflats_SVG")]
    [ExportMetadata("Category", "SkyFlats")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class SlewToNullPoint : SequenceItem {

        /// <summary>
        /// The constructor marked with [ImportingConstructor] will be used to import and construct the object
        /// General device interfaces can be added to the constructor parameters and will be automatically injected on instantiation by the plugin loader
        /// </summary>
        /// <remarks>
        /// Available interfaces to be injected:
        ///     - IProfileService,
        ///     - ICameraMediator,
        ///     - ITelescopeMediator,
        ///     - IFocuserMediator,
        ///     - IFilterWheelMediator,
        ///     - IGuiderMediator,
        ///     - IRotatorMediator,
        ///     - IFlatDeviceMediator,
        ///     - IWeatherDataMediator,
        ///     - IImagingMediator,
        ///     - IApplicationStatusMediator,
        ///     - INighttimeCalculator,
        ///     - IPlanetariumFactory,
        ///     - IImageHistoryVM,
        ///     - IDeepSkyObjectSearchVM,
        ///     - IDomeMediator,
        ///     - IImageSaveMediator,
        ///     - ISwitchMediator,
        ///     - ISafetyMonitorMediator,
        ///     - IApplicationMediator
        ///     - IApplicationResourceDictionary
        ///     - IFramingAssistantVM
        ///     - IList<IDateTimeProvider>
        /// </remarks>
        ///
        private ITelescopeMediator telescopeMediator;

        private IProfileService profileService;

        [ImportingConstructor]
        public SlewToNullPoint(ITelescopeMediator telescopeMediator, IProfileService profileService) {
            this.telescopeMediator = telescopeMediator;
            this.profileService = profileService;
        }

        public SlewToNullPoint(SlewToNullPoint copyMe) : this(copyMe.telescopeMediator, copyMe.profileService) {
            CopyMetaData(copyMe);
        }

        /// <summary>
        /// An example property that can be set from the user interface via the Datatemplate specified in PluginTestItem.Template.xaml
        /// </summary>
        /// <remarks>
        /// If the property changes from the code itself, remember to call RaisePropertyChanged() on it for the User Interface to notice the change
        /// </remarks>
        [JsonProperty]
        public string Text { get; set; }

        /// <summary>
        /// The core logic when the sequence item is running resides here
        /// Add whatever action is necessary
        /// </summary>
        /// <param name="progress">The application status progress that can be sent back during execution</param>
        /// <param name="token">When a cancel signal is triggered from outside, this token can be used to register to it or check if it is cancelled</param>
        /// <returns></returns>
        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            if (telescopeMediator.GetInfo().AtPark) {
                Notification.ShowError(Loc.Instance["LblTelescopeParkedWarning"]);
                throw new SequenceEntityFailedException(Loc.Instance["LblTelescopeParkedWarning"]);
            }

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

            nullPoint.AzDegrees = (int)NormalizeAngle(az + 180); // point to the opposite direction of the sun
            nullPoint.AltDegrees = 75; // offset 15 degrees from zenith

            await telescopeMediator.SlewToTopocentricCoordinates(nullPoint.Coordinates, token);
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

        /// <summary>
        /// When items are put into the sequence via the factory, the factory will call the clone method. Make sure all the relevant fields are cloned with the object.
        /// </summary>
        /// <returns></returns>
        public override object Clone() {
            return new SlewToNullPoint(this);
        }

        /// <summary>
        /// This string will be used for logging
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(SlewToNullPoint)}, Text: {Text}";
        }
    }
}