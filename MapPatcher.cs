using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace MoreWorldOptions
{
    internal static class MapPatcher
    {
        public static void ApplyPatch()
        {
            IL.Terraria.Main.DrawMap += PatchMapMinZoom;
            IL.Terraria.Graphics.Capture.CaptureInterface.GetMapCoords += PatchMapMinZoom;
        }

        public static void RemovePatch()
        {
            IL.Terraria.Main.DrawMap -= PatchMapMinZoom;
            IL.Terraria.Graphics.Capture.CaptureInterface.GetMapCoords -= PatchMapMinZoom;
        }

        private static void PatchMapMinZoom(ILContext il)
        {
            int minZoom = -1;

            int pos = Util.FindNextInstruction(il, 
                x => x.MatchLdsfld<Main>("screenWidth"),
                x => x.MatchConvR4(),
                x => x.MatchLdsfld<Main>("maxTilesX"),
                x => x.MatchConvR4(),
                x => x.MatchDiv(),
                x => x.MatchLdcR4(.8f),
                x => x.MatchMul(),
                x => x.MatchStloc(out minZoom)
                );
            if (pos == -1) return;

            for (int i = 0; i < 8; i++) il.Instrs.RemoveAt(pos);

            ILCursor c = new ILCursor(il).Goto(pos);
            c.Emit(OpCodes.Call, Util.MethodOf(GetMinMapZoom));
            c.Emit(OpCodes.Stloc, minZoom);

        }

        private static float GetMinMapZoom() 
        {
            float minZoomH = (Main.screenWidth + 100f) / Main.maxTilesX;
            float minZoomV = (Main.screenHeight + 80f) / Main.maxTilesY;

            return Math.Min(minZoomV, minZoomH) * 0.8f;
        }
    }
}
