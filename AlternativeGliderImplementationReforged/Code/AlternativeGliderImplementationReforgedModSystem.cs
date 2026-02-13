using AlternativeGliderImplementationReforged.Code.GUI;
using AlternativeGliderImplementationReforged.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace AlternativeGliderImplementationReforged.Code;

public partial class AlternativeGliderImplementationReforgedModSystem : ModSystem
{
    public AltGliderElement GliderBarElement { get; private set; }

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
        AutoSetup(api);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Event.PlayerEntitySpawn += WaitForPlayerEntity;
    }

    private void WaitForPlayerEntity(IClientPlayer player)
    {
        if (_api is ICoreClientAPI capi && player == capi.World.Player)
        {
            //Load GUI when player entity is available
            LoadGui(capi);
            capi.Event.PlayerEntitySpawn -= WaitForPlayerEntity;
        }
    }

    public void LoadGui(ICoreClientAPI capi)
    {
        if (AltGliderClientConfig.Instance.ShowBar)
        {
            GliderBarElement = new AltGliderElement(capi);
        }
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);
        AutoAssetsLoaded(api);
    }

    public override void Dispose()
    {
        base.Dispose();
        if(_api is ICoreClientAPI capi) capi.Event.PlayerEntitySpawn -= WaitForPlayerEntity;
        GliderBarElement?.Dispose();
        AutoDispose();
    }
}