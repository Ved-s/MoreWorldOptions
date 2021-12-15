using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria;

namespace MoreWorldOptions
{
    internal static class MenuPatcher
    {
        public static void ApplyPatch()
        {
            IL.Terraria.Main.DrawMenu += DrawMenu;
        }

        public static void RemovePatch()
        {
            IL.Terraria.Main.DrawMenu -= DrawMenu;
        }

        private static void DrawMenu(ILContext il)
        {
            /*
             * Patch 
             *   if (loadedEverything)
             *   {
             *     GamepadMainMenuHandler.CanRun = true;
             *   }
             * to
             *   if (loadedEverything)
             *   {
             *     GamepadMainMenuHandler.CanRun = true;
             *     if (MoreWorldOptions.DrawMenuHook(this, this.selectedMenu, array9, array7, array4, ref num2, ref num4, ref num5, ref flag5)) goto IL_57BB;
             *   }             
             */

            ILLabel endIf = null;
            ILLabel startIf = null;
            if (FindNextInstruction(il,
                x => x.MatchLdcI4(1),
                x => x.MatchStsfld("Terraria.ModLoader.ModLoader", "skipLoad"),
                x => x.MatchBr(out endIf)
                ) == -1) return;


            int textOnlyButtons = 0;

            int patchPos = FindNextInstruction(il,
                x => x.MatchDup(),              // Removed
                x => x.MatchBrfalse(out _),     // Redirected
                x => x.MatchLdcI4(1),
                x => x.MatchStsfld("Terraria.UI.Gamepad.GamepadMainMenuHandler", "CanRun"),
                // Patch here
                x => x.MatchBrtrue(out startIf), // Removed
                x => x.MatchLdloc(out textOnlyButtons)
                );
            if (patchPos == -1) return;

            int buttonNames = 0,
                buttonScales = 0,
                buttonVerticalSpacing = 0,
                offY = 0,
                spacing = 0,
                numButtons = 0,
                backButtonDown = 0;

            if (FindNextInstruction(il,
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Main>("selectedMenu"),
                x => x.MatchLdloc(out buttonNames),
                x => x.MatchLdloc(out buttonScales),
                x => x.MatchLdloc(out buttonVerticalSpacing),
                x => x.MatchLdloca(out offY),
                x => x.MatchLdloca(out spacing),
                x => x.MatchLdloca(out numButtons),
                x => x.MatchLdloca(out backButtonDown),
                x => x.MatchCall("Terraria.ModLoader.UI.Interface", "ModLoaderMenus")
                ) == -1) return;


            il.Instrs.RemoveAt(patchPos + 4); // brtrue
            il.Instrs.RemoveAt(patchPos);     // dup
            il.Instrs.RemoveAt(patchPos);     // brfalse

            ILCursor c = new ILCursor(il).Goto(patchPos + 2);

            ILLabel plabel = c.MarkLabel();
            c.Goto(patchPos);
            c.Emit(OpCodes.Brfalse_S, plabel);

            c.Goto(patchPos + 3);

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit<Main>(OpCodes.Ldfld, "selectedMenu");
            c.Emit(OpCodes.Ldloc_S, (byte)buttonNames);
            c.Emit(OpCodes.Ldloc_S, (byte)buttonScales);
            c.Emit(OpCodes.Ldloc_S, (byte)buttonVerticalSpacing);
            c.Emit(OpCodes.Ldloc_S, (byte)textOnlyButtons);
            c.Emit(OpCodes.Ldloca_S, (byte)offY);
            c.Emit(OpCodes.Ldloca_S, (byte)spacing);
            c.Emit(OpCodes.Ldloca_S, (byte)numButtons);
            c.Emit(OpCodes.Ldloca_S, (byte)backButtonDown);
            c.Emit<MoreWorldOptions>(OpCodes.Call, "DrawMenuHook");
            c.Emit(OpCodes.Brtrue, endIf);
            c.Emit(OpCodes.Br_S, startIf);
        }

        public static int FindNextInstruction(ILContext il, params Func<Instruction, bool>[] predicates)
        {
            int pIndex = 0;
            for (int i = 0; i < il.Instrs.Count; i++)
            {
                if (pIndex == predicates.Length)
                {
                    return i - predicates.Length;
                }

                else if (predicates[pIndex](il.Instrs[i])) pIndex++;
                else pIndex = 0;
            }
            return -1;
        }
        public static IEnumerable<int> FindNextInstructions(ILContext il, params Func<Instruction, bool>[] predicates)
        {
            int pIndex = 0;
            for (int i = 0; i < il.Instrs.Count; i++)
            {
                if (pIndex == predicates.Length)
                {
                    Instruction current = il.Instrs[i];
                    yield return i - predicates.Length;
                    int newIndex = il.Instrs.IndexOf(current);
                    i = newIndex == -1 ? i : newIndex;
                    pIndex = 0;
                }

                else if (predicates[pIndex](il.Instrs[i])) pIndex++;
                else pIndex = 0;
            }
        }


    }
}
