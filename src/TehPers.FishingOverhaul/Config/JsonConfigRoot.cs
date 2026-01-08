using Newtonsoft.Json;

namespace TehPers.FishingOverhaul.Config
{
    /// <summary>
    /// A configuration.
    /// </summary>
    public abstract record JsonConfigRoot
    {
        private protected const string jsonSchemaRootUrl =
            "https://raw.githubusercontent.com/Hiztaar/TehsStardewValleyMods_1.6Revived/blob/main/docs/TehPers.FishingOverhaul/schemas/";

        /// <summary>
        /// Optional '$schema' URL. This is ignored and exists entirely for convenience.
        /// </summary>
        [JsonProperty("$schema", DefaultValueHandling = DefaultValueHandling.Ignore)]
        protected abstract string? Schema { get; }
    }
}
