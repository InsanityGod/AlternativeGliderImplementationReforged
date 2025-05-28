using InsanityLib.Attributes.Auto.Config;
using System.ComponentModel;

namespace AlternativeGliderImplementationReforged.Config
{
    public class AltGliderServerConfig
    {
        [AutoConfig("AlternativeGliderImplementationReforged/ServerConfig.json", ServerSync = true)]
        public static AltGliderServerConfig Instance { get; private set; }

        /// <summary>
        /// If enalbled diving is done through sneaking instead of walking forward
        /// </summary>
        [Category("Controls")]
        [DefaultValue(false)]
        public bool AlternativeDiveControls { get; set; }

        /// <summary>
        /// If enalbled braking is done through jump instead of walking backwards
        /// </summary>
        [Category("Controls")]
        [DefaultValue(false)]
        public bool AlternativeBrakeControls { get; set; }

        /// <summary>
        /// Wether the player is allowed to glide while climbing
        /// </summary>
        [Category("Controls")]
        [DefaultValue(true)]
        public bool AllowGlideWhileClimbing { get; set; } = true;

        /// <summary>
        /// Lift generated relative to the speed reduction caused by braking
        /// Original mod value: 0.035
        /// </summary>
        [DefaultValue(0.045d)]
        public double BrakeLiftAcc { get; set; } = 0.045d;

        /// <summary>
        /// This limits the lift in relation to the horizontal speed in the form of:
        /// lift * Math.Min(horizontalMotion / BrakeHMotionLimit, 1)
        /// Original mod value: 0.02
        /// </summary>
        [DefaultValue(0.015d)]
        public double BrakeHMotionLimit { get; set; } = 0.015d;
    }
}