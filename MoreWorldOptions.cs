using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;

namespace MoreWorldOptions
{
    public class MoreWorldOptions : Mod
    {
        public static MoreWorldOptions Instance;

        public static Texture2D SmallPlus;

        public MoreWorldOptions() { Instance = this; }

        public override void Load()
        {
            base.Load();

            SmallPlus = GetTexture("SmallPlus");

            MenuPatcher.ApplyPatch();
            WorldGenPatches.ApplyPatches();
            MapPatcher.ApplyPatch();
            CapturePatcher.ApplyPatch();

            On.Terraria.IO.WorldFileData.SetWorldSize += WorldFileData_SetWorldSize;

            
        }

        public override void Unload()
        {
            base.Unload();
            MenuPatcher.RemovePatch();
            WorldGenPatches.RemovePatches();
            GeneratorPatches.RemovePatches();
            MapPatcher.RemovePatch();
            CapturePatcher.RemovePatch();

            On.Terraria.IO.WorldFileData.SetWorldSize -= WorldFileData_SetWorldSize;

            Instance = null;
        }

        

        public static bool DrawMenuHook(
            Main main,
            int selectedMenu,
            string[] buttonNames,
            float[] buttonScales,
            int[] buttonVerticalSpacing,
            bool[] textOnlyButtons,
            ref int offY,
            ref int spacing,
            ref int numButtons,
            ref bool backButtonDown)
        {
            if (Main.menuMode == 16)
            {
                offY = 200;
                spacing = 60;
                buttonVerticalSpacing[1] = 30;
                buttonVerticalSpacing[2] = 30;
                buttonVerticalSpacing[3] = 30;
                buttonVerticalSpacing[4] = 30;
                buttonVerticalSpacing[5] = 70;
                textOnlyButtons[0] = true;
                buttonNames[0] = Language.GetTextValue("LegacyMenu.91");
                buttonNames[1] = Language.GetTextValue("LegacyMenu.92");
                buttonNames[2] = Language.GetTextValue("LegacyMenu.93");
                buttonNames[3] = Language.GetTextValue("LegacyMenu.94");
                buttonNames[4] = Language.ActiveCulture == GameCulture.Russian ? "Настраиваемый" : "Custom";
                buttonNames[5] = Language.GetTextValue("LegacyMenu.5");
                numButtons = 6;
                if (selectedMenu == 5 || backButtonDown)
                {
                    backButtonDown = false;
                    Main.menuMode = 6;
                    Main.PlaySound(SoundID.MenuClose, -1, -1, 1, 1f, 0f);
                }
                else if (selectedMenu == 4)
                {
                    Main.MenuUI.SetState(new UI.UICustomWorld());
                    Main.PlaySound(SoundID.MenuOpen, -1, -1, 1, 1f, 0f);
                    Main.menuMode = 888;
                }
                else if (selectedMenu > 0)
                {
                    switch (selectedMenu)
                    {
                        case 1:
                            Main.maxTilesX = 4200;
                            Main.maxTilesY = 1200;
                            break;
                        case 2:
                            Main.maxTilesX = 6400;
                            Main.maxTilesY = 1800;
                            break;
                        case 3:
                            Main.maxTilesX = 8400;
                            Main.maxTilesY = 2400;
                            break;
                    }
                    Main.clrInput();
                    Main.menuMode = -7;
                    Main.PlaySound(SoundID.MenuOpen, -1, -1, 1, 1f, 0f);
                    WorldGen.setWorldSize();
                }
                return true;
            }
            return false;
        }

        public static void ResizeWorld()
        {
            int right = (Main.maxTilesX + 1) * 16;
            int bottom = (Main.maxTilesY + 1) * 16;
            if (right > Main.rightWorld || bottom > Main.bottomWorld)
            {
                Main.rightWorld = right;
                Main.topWorld = 0f;
                Main.bottomWorld = bottom;
                Main.maxSectionsX = Main.maxTilesX / 200;
                Main.maxSectionsY = Main.maxTilesY / 150;
                Main.Map = new WorldMap(Main.maxTilesX, Main.maxTilesY);
                Main.tile = new Tile[Main.maxTilesX, Main.maxTilesY];

                Main.mapTargetX = (int)Math.Ceiling((float)Main.maxTilesX / Main.textureMaxWidth) + 1;
                Main.mapTargetY = (int)Math.Ceiling((float)Main.maxTilesY / Main.textureMaxHeight) + 1;

                Main.initMap = new bool[Main.mapTargetX, Main.mapTargetY];
                Main.mapWasContentLost = new bool[Main.mapTargetX, Main.mapTargetY];

                for (int i = 0; i < Main.instance.mapTarget.GetLength(0); i++)
                    for (int j = 0; j < Main.instance.mapTarget.GetLength(1); j++)
                        if (Main.instance.mapTarget[i, j] != null && !Main.instance.mapTarget[i, j].IsDisposed)
                            Main.instance.mapTarget[i, j].Dispose();

                Main.instance.mapTarget = new RenderTarget2D[Main.mapTargetX, Main.mapTargetY];

                typeof(WorldGen).GetField("lastMaxTilesX", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, 0);
                typeof(WorldGen).GetField("lastMaxTilesY", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, 0);

                GC.Collect();
            }
        }

        private void WorldFileData_SetWorldSize(On.Terraria.IO.WorldFileData.orig_SetWorldSize orig, Terraria.IO.WorldFileData self, int x, int y)
        {
            orig(self, x, y);

            if ((x == 4200 && y == 1200)
                || (x == 6400 && y == 1800)
                || (x == 8400 && y == 2400)) return;

            self._worldSizeName = (LocalizedText)Activator.CreateInstance(
                typeof(LocalizedText), 
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { "", $"{x} x {y}" },
                null);
        }

    }
}