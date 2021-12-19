using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static int FindNextInstruction(ILContext il, Func<Instruction, bool> predicate, int fromIndex = 0, int searchRange = -1)
        {
            if (fromIndex == -1) return -1;
            for (int i = fromIndex; i < il.Instrs.Count; i++)
            {
                if (predicate(il.Instrs[i])) return i;
                else if (i > fromIndex + searchRange) return -1;
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

        public static MethodInfo MethodOf(Action a) => a.Method;
        public static MethodInfo MethodOf<T1>(Action<T1> a) => a.Method;
        public static MethodInfo MethodOf<T1,T2>(Action<T1,T2> a) => a.Method;
        public static MethodInfo MethodOf<T1,T2,T3>(Action<T1,T2,T3> a) => a.Method;

        public static MethodInfo MethodOf<TReturn>(Func<TReturn> a) => a.Method;
        public static MethodInfo MethodOf<T1,TReturn>(Func<T1,TReturn> a) => a.Method;
        public static MethodInfo MethodOf<T1,T2,TReturn>(Func<T1,T2,TReturn> a) => a.Method;
        public static MethodInfo MethodOf<T1,T2,T3,TReturn>(Func<T1,T2, T3,TReturn> a) => a.Method;

    }
}
