using AlternativeGliderImplementationReforged.Code.HarmonyPatches;
using AlternativeGliderImplementationReforged.Config;
using Cairo;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace AlternativeGliderImplementationReforged.Code.GUI
{
    public class AltGliderStatbar : GuiElement
    {
        public EntityControls Controls { get; }

        public bool Visible => AltGliderClientConfig.Instance.ShowBar && Controls.Gliding;

        public Color Color { get; }

        protected bool rightToLeft = false;

        protected float value = 0;
        protected bool valuesSet;

        protected float lineInterval = 10;

        protected LoadedTexture baseTexture;
        protected LoadedTexture barTexture;

        private int valueHeight;

        public AltGliderStatbar(ICoreClientAPI capi, ElementBounds bounds, Color color, bool rightToLeft) : base(capi, bounds)
        {
            Controls = capi.World.Player.Entity.Controls;
            Color = color;

            barTexture = new LoadedTexture(capi);
            baseTexture = new LoadedTexture(capi);

            this.rightToLeft = rightToLeft;
        }

        public override void ComposeElements(Context ctxStatic, ImageSurface surface)
        {
            Bounds.CalcWorldBounds();

            surface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt + 1, Bounds.OuterHeightInt + 1);
            ctxStatic = new Context(surface);

            RoundRectangle(ctxStatic, 0, 0, Bounds.InnerWidth, Bounds.InnerHeight, 1);

            ctxStatic.SetSourceRGBA(0.15, 0.15, 0.15, 1);
            ctxStatic.Fill();
            EmbossRoundRectangleElement(ctxStatic, 0, 0, Bounds.InnerWidth, Bounds.InnerHeight, false, 3, 1);

            RecomposeOverlays();

            generateTexture(surface, ref baseTexture);
            surface.Dispose();
            ctxStatic.Dispose();
        }

        protected void RecomposeOverlays() => TyronThreadPool.QueueTask(ComposeValueOverlay);

        protected void ComposeValueOverlay()
        {
            Bounds.CalcWorldBounds();

            // Fix width formula.
            double widthRel = (double)((value - ControlChangePatches.speedMin) / (ControlChangePatches.speedMax - ControlChangePatches.speedMin));
            valueHeight = (int)Bounds.OuterHeight + 1;
            ImageSurface surface = new(Format.Argb32, Bounds.OuterWidthInt + 1, valueHeight);
            Context ctx = new(surface);

            if (widthRel > 0.01)
            {
                double width = Bounds.OuterWidth * widthRel;
                double x = rightToLeft
                        ? Bounds.OuterWidth - width
                        : 0;

                RoundRectangle(ctx, x, 0, width, Bounds.OuterHeight, 1);

                ctx.SetSourceColor(Color);
                ctx.FillPreserve();

                var offsetColor = new Color(Color.R * 0.4, Color.G * 0.4, Color.B * 0.4);
                ctx.SetSourceColor(offsetColor);

                ctx.LineWidth = scaled(3);
                ctx.StrokePreserve();
                surface.BlurFull(3);

                width = Bounds.InnerWidth * widthRel;
                x = rightToLeft
                        ? Bounds.InnerWidth - width
                        : 0;

                EmbossRoundRectangleElement(ctx, x, 0, width, Bounds.InnerHeight, false, 2, 1);
            }

            ctx.SetSourceRGBA(0, 0, 0, 0.5);
            ctx.LineWidth = scaled(2.2);

            int lines = Math.Min(50, (int)((ControlChangePatches.speedMax - ControlChangePatches.speedMin) / lineInterval));

            for (int i = 1; i < lines; i++)
            {
                ctx.NewPath();
                ctx.SetSourceRGBA(0, 0, 0, 0.5);

                double x = Bounds.InnerWidth * i / lines;

                ctx.MoveTo(x, 0);
                ctx.LineTo(x, Math.Max(3, Bounds.InnerHeight - 1));
                ctx.ClosePath();
                ctx.Stroke();
            }

            api.Event.EnqueueMainThreadTask(() =>
            {
                generateTexture(surface, ref barTexture);

                ctx.Dispose();
                surface.Dispose();
            }, "recompstatbar");
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            if (!Visible) return;

            double x = Bounds.renderX;
            double y = Bounds.renderY;

            api.Render.RenderTexture(baseTexture.TextureId, x, y, Bounds.OuterWidthInt + 1, Bounds.OuterHeightInt + 1);

            if (barTexture.TextureId > 0)
            {
                api.Render.RenderTexture(barTexture.TextureId, x, y, Bounds.OuterWidthInt + 1, valueHeight);
            }
        }

        public void SetValue(float value)
        {
            this.value = value;
            RecomposeOverlays();
        }

        public override void Dispose()
        {
            base.Dispose();

            baseTexture.Dispose();
            barTexture.Dispose();
        }
    }
}