using Newtonsoft.Json;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.SequenceItem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photon.NINA.Skyflats {

    /// <summary>
    /// This Class shows the basic principle on how to add a new Sequence Trigger to the N.I.N.A. sequencer via the plugin interface
    /// For ease of use this class inherits the abstract SequenceTrigger which already handles most of the running logic, like logging, exception handling etc.
    /// A complete custom implementation by just implementing ISequenceTrigger is possible too
    /// The following MetaData can be set to drive the initial values
    /// --> Name - The name that will be displayed for the item
    /// --> Description - a brief summary of what the item is doing. It will be displayed as a tooltip on mouseover in the application
    /// --> Icon - a string to the key value of a Geometry inside N.I.N.A.'s geometry resources
    ///
    /// If the item has some preconditions that should be validated, it shall also extend the IValidatable interface and add the validation logic accordingly.
    /// </summary>
    [ExportMetadata("Name", "Loop while SQM")]
    [ExportMetadata("Description", "Loop while the SQM is brighter/darker than the defined value")]
    [ExportMetadata("Icon", "Plugin_Skyflats_SVG")]
    [ExportMetadata("Category", "SkyFlats")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class SkyflatsCondition : SequenceCondition {

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

        [ImportingConstructor]
        public SkyflatsCondition(IWeatherDataMediator weatherDataMediator) {
            this.weatherDataMediator = weatherDataMediator;
            sqmThreshold = 8.0;
            SQMoperators = new ObservableCollection<string> { "brighter than", "darker than" };
            SQMOperator = "darker than";
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
        /// Once this check returns false, the condition will cause its parent instruction set to skip the rest and proceed with the next set
        /// </summary>
        /// <param name="previousItem"></param>
        /// <param name="nextItem"></param>
        /// <returns></returns>
        public override bool Check(ISequenceItem previousItem, ISequenceItem nextItem) {
            if (SQMOperator == "darker than")
                return weatherDataMediator.GetInfo().SkyQuality > SQMThreshold;
            else
                return weatherDataMediator.GetInfo().SkyQuality < SQMThreshold;
        }

        public override object Clone() {
            return new SkyflatsCondition(weatherDataMediator) {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description
            };
        }

        /// <summary>
        /// This string will be used for logging
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(SkyflatsCondition)}, SQMThreshold: {SQMThreshold}";
        }
    }
}