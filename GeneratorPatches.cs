using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace MoreWorldOptions
{
    internal static class GeneratorPatches
    {
        static MethodInfo JungleGeneratorPass = null;

        public static void RemovePatches() 
        {
            if (JungleGeneratorPass != null) 
            {
                HookEndpointManager.Unmodify(JungleGeneratorPass, (ILContext.Manipulator)Patch4200);
                JungleGeneratorPass = null;
            }
        }

        public static void ApplyJunglePass(MethodInfo info) 
        {
            if (JungleGeneratorPass != null) return;
            JungleGeneratorPass = info;

            HookEndpointManager.Modify(JungleGeneratorPass, (ILContext.Manipulator)Patch4200);
        }

        internal static void Patch4200(ILContext il) 
        {
            int numId = 0;

            int pos = Util.FindNextInstruction(il,
                x => x.MatchLdsfld<Main>("maxTilesX"),
                x => x.MatchLdcI4(4200),
                x => x.MatchDiv(),
                x => x.MatchConvR4(),
                x => x.MatchStloc(out numId));
            if (pos == -1) return;

            ILCursor c = new ILCursor(il).Goto(pos + 5);

            ILLabel numNotZero = c.DefineLabel();

            c.Emit(OpCodes.Ldloc, numId);
            c.Emit(OpCodes.Ldc_R4, 0f);
            c.Emit(OpCodes.Bne_Un, numNotZero);
            c.Emit(OpCodes.Ldc_R4, 1f);
            c.Emit(OpCodes.Stloc, numId);
            c.MarkLabel(numNotZero);
        }
    }
}
