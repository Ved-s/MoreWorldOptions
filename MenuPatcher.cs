using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using System.Linq;
using Mono.Cecil;
using Terraria.IO;

namespace MoreWorldOptions
{
    internal static class MenuPatcher
    {
        public static void ApplyPatch()
        {
            IL.Terraria.Main.DrawMenu += DrawMenu;
            IL.Terraria.GameContent.UI.Elements.UIWorldListItem.ctor += UIWorldListItem_ctor;
        }

        public static void RemovePatch()
        {
            IL.Terraria.Main.DrawMenu -= DrawMenu;
            IL.Terraria.GameContent.UI.Elements.UIWorldListItem.ctor -= UIWorldListItem_ctor;
        }

        private static void UIWorldListItem_ctor(ILContext il)
        {
            FieldReference deleteButtonLabel = null;

            int pos = Util.FindNextInstruction(il,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(out deleteButtonLabel),
                x => x.MatchLdflda(out _),
                x => x.MatchLdcR4(-30),
                x => x.MatchLdcR4(0)
                );

            // for later
            //il.Instrs[pos + 3].Operand = -54f;

            pos = Util.FindNextInstruction(il, x => x.MatchRet());

            ILCursor c = new ILCursor(il).Goto(pos);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, deleteButtonLabel);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Call, Util.MethodOf<UIElement, UIText, WorldFileData>(ModifyWorldItem));
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
            if (Util.FindNextInstruction(il,
                x => x.MatchLdcI4(1),
                x => x.MatchStsfld("Terraria.ModLoader.ModLoader", "skipLoad"),
                x => x.MatchBr(out endIf)
                ) == -1) return;


            int textOnlyButtons = 0;

            int patchPos = Util.FindNextInstruction(il,
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

            if (Util.FindNextInstruction(il,
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


        private static void ModifyWorldItem(UIElement @this, UIText deleteButtonLabel, WorldFileData data) 
        {
            // for later
            //UIImageButton btn = new UIImageButton(MoreWorldOptions.SmallPlus);
            //btn.VAlign = 1f;
            //btn.HAlign = 1f;
            //btn.Left.Set(-24, 0);
            ////btn.OnClick += this.DeleteButtonClick;
            //btn.OnMouseOver += (ev, ui) =>
            //{
            //    deleteButtonLabel.SetText("More World Options");
            //};
            //btn.OnMouseOut += (ev, ui) =>
            //{
            //    deleteButtonLabel.SetText("");
            //};
            ////btn.
            //@this.Append(btn);
        }

    }
}
