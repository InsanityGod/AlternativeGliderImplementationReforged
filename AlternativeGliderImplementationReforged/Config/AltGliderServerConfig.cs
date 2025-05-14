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
    }
}