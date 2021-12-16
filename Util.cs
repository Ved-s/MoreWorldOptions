using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreWorldOptions
{
    public static class Util
    {
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
