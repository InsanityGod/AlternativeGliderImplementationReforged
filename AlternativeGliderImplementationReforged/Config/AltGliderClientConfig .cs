using InsanityLib.Attributes.Auto.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternativeGliderImplementationReforged.Config
{
    public class AltGliderClientConfig
    {
        [AutoConfig("AlternativeGliderImplementationReforged/ClientConfig.json", ServerSync = false)]
        public static AltGliderClientConfig Instance { get; private set; }

        /// <summary>
        /// Whether to display the glide power bar
        /// </summary>
        [DefaultValue(true)]
        public bool ShowBar {  get; set; } = true;

        /// <summary>
        /// The width of the glide power bar
        /// </summary>
        [DefaultValue(256f)]
        public float BarWidth { get; set; } = 256f;
    }
}
