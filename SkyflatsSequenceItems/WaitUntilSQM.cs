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
using System.Collections.ObjectModel;

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
    [ExportMetadata("Name", "Wait until SQM")]
    [ExportMetadata("Description", "Waits until the SQM is brighter/darker than the defined threshold")]
    [ExportMetadata("Icon", "Plugin_Skyflats_SVG")]
    [ExportMetadata("Category", "SkyFlats")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class WaitUntilSQM : SequenceItem {
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

        private IWeatherDataMediator weatherDataMediator;
        private ITelescopeMediator telescopeMediator;

        private IProfileService profileService;

        [ImportingConstructor]
        public WaitUntilSQM(IWeatherDataMediator weatherDataMediator, ITelescopeMediator telescopeMediator, IProfileService profileService) {
            this.telescopeMediator = telescopeMediator;
            this.profileService = profileService;
            this.weatherDataMediator = weatherDataMediator;
            SQMoperators = new ObservableCollection<string> { "brighter than", "darker than" };
            SQMOperator = "darker than";
        }

        public WaitUntilSQM(WaitUntilSQM copyMe) : this(copyMe.weatherDataMediator, copyMe.telescopeMediator, copyMe.profileService) {
            CopyMetaData(copyMe);
        }

        public ObservableCollection<string> SQMoperators { get; set; }

        private string _SQMoperator;

        [JsonProperty]
        public string SQMOperator {
            get => _SQMoperator;
            set {
                _SQMoperator = value;
                RaisePropertyChanged();
            }
        }

        private double sqmThreshold;

        [JsonProperty]
        public double SQMThreshold {
            get => sqmThreshold;
            set {
                sqmThreshold = value;
                RaisePropertyChanged();
            }
        }

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

            while (true) {
                if (token.IsCancellationRequested)
                    return;
                if (SQMOperator == "darker than") {
                    if (weatherDataMediator.GetInfo().SkyQuality > SQMThreshold)
                        return;
                } else {
                    if (weatherDataMediator.GetInfo().SkyQuality < SQMThreshold)
                        return;
                }
                await Task.Delay(1000, token);
            }
        }

        /// <summary>
        /// When items are put into the sequence via the factory, the factory will call the clone method. Make sure all the relevant fields are cloned with the object.
        /// </summary>
        /// <returns></returns>
        public override object Clone() {
            return new WaitUntilSQM(this) {
                SQMOperator = SQMOperator,
                SQMThreshold = SQMThreshold
            };
        }

        /// <summary>
        /// This string will be used for logging
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(WaitUntilSQM)}";
        }
    }
}