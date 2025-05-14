using AlternativeGliderImplementationReforged.Code.GUI;
using AlternativeGliderImplementationReforged.Config;
using InsanityLib.Attributes.Auto;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

[assembly: AutoPatcher("alternativegliderimplementationreforged")]

namespace AlternativeGliderImplementationReforged.Code
{
    public class AlternativeGliderImplementationReforgedModSystem : ModSystem
    {
        public AltGliderElement gliderBarElement { get; private set; }

        public override void StartClientSide(ICoreClientAPI api)
        {
            api.Event.PlayerEntitySpawn += player =>
            {
                if (player == api.World.Player)
                {
                    //Load GUI when player entity is available
                    LoadGui(api);
                }
            };
        }

        public void LoadGui(ICoreClientAPI capi)
        {
            if (AltGliderClientConfig.Instance.ShowBar)
            {
                gliderBarElement = new AltGliderElement(capi);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            gliderBarElement?.Dispose();
        }
    }
}