using AlternativeGliderImplementationReforged.Config;
using Cairo;
using Vintagestory.API.Client;

namespace AlternativeGliderImplementationReforged.Code.GUI
{
    public class AltGliderElement : HudElement
    {
        public const string dialogName = "altglider";
        public const string barKey = "altgliderbar";

        public const float barX = 0;
        public const float barY = -256;
        public const float barHeight = 10;

        public readonly Color Color = new(1, 1, 1);

        protected AltGliderStatbar bar;

        private readonly long listenerId;

        public AltGliderElement(ICoreClientAPI capi) : base(capi)
        {
            // Create bar.
            ElementBounds dialogBounds = new()
            {
                Alignment = EnumDialogArea.CenterBottom,
                BothSizing = ElementSizing.Fixed,
                fixedWidth = AltGliderClientConfig.Instance.BarWidth,
                fixedHeight = barHeight
            };

            ElementBounds barBounds = ElementBounds.Fixed(barX, barY, AltGliderClientConfig.Instance.BarWidth, barHeight);

            Composers[dialogName] = capi.Gui
                    .CreateCompo(dialogName, dialogBounds)
                    .AddInteractiveElement(new AltGliderStatbar(capi, barBounds, Color, false), barKey) //TODO Maybe convert this to use base game StatsBar class
                    .Compose();

            bar = (AltGliderStatbar)Composers[dialogName].GetElement(barKey);

            listenerId = capi.World.RegisterGameTickListener(UpdateValue, 50);

            TryOpen();
        }

        public void UpdateValue(float deltaTime)
        {
            if (!bar.Visible) return;

            bar.SetValue((float)bar.Controls.GlideSpeed);
        }

        public override bool ShouldReceiveKeyboardEvents() => false;

        public override void OnMouseDown(MouseEvent args)
        {
            // Cannot be clicked.
        }

        protected override void OnFocusChanged(bool on)
        {
            // Cannot be focused.
        }

        public override void Dispose()
        {
            base.Dispose();
            capi.World.UnregisterGameTickListener(listenerId);
        }
    }
}