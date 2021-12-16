using System.Reflection;
using Terraria;

namespace MoreWorldOptions
{
    internal static class WorldGenPatches
    {

        public static void ApplyPatches()
        {
            On.Terraria.WorldGen.PlacePot += WorldGen_PlacePot;
            On.Terraria.WorldGen.placeTrap += WorldGen_placeTrap;
            On.Terraria.WorldGen.clearWorld += WorldGen_clearWorld;
            On.Terraria.WorldGen.AddGenerationPass_string_WorldGenLegacyMethod += WorldGen_AddGenerationPass;

            IL.Terraria.WorldGen.makeTemple += WorldGen_makeTemple;
        }

        

        public static void RemovePatches()
        {
            On.Terraria.WorldGen.PlacePot -= WorldGen_PlacePot;
            On.Terraria.WorldGen.placeTrap -= WorldGen_placeTrap;
            On.Terraria.WorldGen.clearWorld -= WorldGen_clearWorld;
            On.Terraria.WorldGen.AddGenerationPass_string_WorldGenLegacyMethod -= WorldGen_AddGenerationPass;

            IL.Terraria.WorldGen.makeTemple -= WorldGen_makeTemple;
        }

        private static void WorldGen_clearWorld(On.Terraria.WorldGen.orig_clearWorld orig)
        {
            MoreWorldOptions.ResizeWorld();
            orig();
        }
        private static bool WorldGen_placeTrap(On.Terraria.WorldGen.orig_placeTrap orig, int x2, int y2, int type)
        {
            int n = y2;
            while (true)
            {
                if (!WorldGen.InWorld(x2, n, 1)) return false;
                if (WorldGen.SolidTile(x2, n)) break;
                n++;
                if (n >= Main.maxTilesY - 300)
                {
                    return false;
                }
            }
            n--;
            return orig(x2, n, type);
        }
        private static bool WorldGen_PlacePot(On.Terraria.WorldGen.orig_PlacePot orig, int x, int y, ushort type, int style)
        {
            if (x < 2 || y < 2 || x > Main.maxTilesX - 3 || y > Main.maxTilesY - 3) return false;
            return orig(x, y, type, style);
        }
        private static void WorldGen_AddGenerationPass(On.Terraria.WorldGen.orig_AddGenerationPass_string_WorldGenLegacyMethod orig, string name, Terraria.GameContent.Generation.WorldGenLegacyMethod method)
        {
            orig(name, method);
            if (name == "Jungle") GeneratorPatches.ApplyJunglePass(method.GetMethodInfo());
        }
        private static void WorldGen_makeTemple(MonoMod.Cil.ILContext il)
        {
            GeneratorPatches.Patch4200(il);
        }
    }
}
