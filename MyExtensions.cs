using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtensionMethods
{

    public static class MyExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        public static unsafe List<uint> IntersectSortedUnsafe(this uint[] source, uint[] target)
        {
            var ints = new List<uint>(Math.Min(source.Length, target.Length));

            fixed (uint* ptSrc = source)
            {
                var maxSrcAdr = ptSrc + source.Length;

                fixed (uint* ptTar = target)
                {
                    var maxTarAdr = ptTar + source.Length;

                    var currSrc = ptSrc;
                    var currTar = ptTar;

                    while (currSrc < maxSrcAdr && currTar < maxTarAdr)
                    {
                        switch ((*currSrc).CompareTo(*currTar))
                        {
                            case -1:
                                currSrc++;
                                continue;
                            case 1:
                                currTar++;
                                continue;
                            default:
                                ints.Add(*currSrc);
                                currSrc++;
                                currTar++;
                                continue;
                        }
                    }
                }
            }

            ints.TrimExcess();
            return ints;
        }

        public static List<uint> IntersectSorted(this List<uint> source, List<uint> target)
        {
            // Set initial capacity to a "full-intersection" size
            // This prevents multiple re-allocations
            var ints = new List<uint>(Math.Min(source.Count, target.Count));     

            var i = 0;
            var j = 0;

            while (i < source.Count && j < target.Count)
            {
                // Compare only once and let compiler optimize the switch-case
                switch (source[i].CompareTo(target[j]))
                {
                    case -1:
                        i++;

                        // Saves us a JMP instruction
                        continue;
                    case 1:
                        j++;

                        // Saves us a JMP instruction
                        continue;
                    default:
                        ints.Add(source[i++]);
                        j++;

                        // Saves us a JMP instruction
                        continue;
                }
            }

            // Free unused memory (sets capacity to actual count)            
            //ints.TrimExcess(); //Could remove for possible performance increase at cost of memory

            return ints;
        }

        public static IEnumerable<T> IntersectSorted<T>(this IEnumerable<T> sequence1, IEnumerable<T> sequence2, IComparer<T> comparer)
        {
            using (var cursor1 = sequence1.GetEnumerator())
            using (var cursor2 = sequence2.GetEnumerator())
            {
                if (!cursor1.MoveNext() || !cursor2.MoveNext())
                {
                    yield break;
                }
                var value1 = cursor1.Current;
                var value2 = cursor2.Current;

                while (true)
                {
                    int comparison = comparer.Compare(value1, value2);                   
                    if (comparison < 0)
                    {
                        if (!cursor1.MoveNext())
                        {
                            yield break;
                        }
                        value1 = cursor1.Current;
                    }
                    else if (comparison > 0)
                    {
                        if (!cursor2.MoveNext())
                        {
                            yield break;
                        }
                        value2 = cursor2.Current;
                    }
                    else
                    {
                        yield return value1;
                        if (!cursor1.MoveNext() || !cursor2.MoveNext())
                        {
                            yield break;
                        }
                        value1 = cursor1.Current;
                        value2 = cursor2.Current;
                    }
                }
            }
        }


        internal static IEnumerable<T> MyIntersect<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {         
            var firstHashset = first as HashSet<T>;
            var secondHashset = second as HashSet<T>;
            if (firstHashset != null && secondHashset != null)
            {
                return (firstHashset.Count > secondHashset.Count)
                          ? firstHashset.Intersect(second)
                          : secondHashset.Intersect(first);
            }
            if (firstHashset != null) { return firstHashset.Intersect(second); }
            if (secondHashset != null) { return secondHashset.Intersect(first); }
            return first.Intersect(second);
        }

        static IEnumerable<T> Intersect<T>(this HashSet<T> firstHashset, IEnumerable<T> second)
        {
            foreach (var tmp in second)
            {
                if (firstHashset.Contains(tmp)) { yield return tmp; }
            }
        }
        

    }

    
    
}
