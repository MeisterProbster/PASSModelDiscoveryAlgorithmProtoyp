using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    namespace XEStoPASS_alps.net.api
    {
        public static class EnumerableExtensions
        {
            public static T[][] PowerSet<T>(this IEnumerable<T> set)
            {
                var seq = set.ToArray();
                var powerSet = new T[1 << seq.Length][];
                powerSet[0] = new T[0]; // starting only with empty set
                for (int i = 0; i < seq.Length; i++)
                {
                    var cur = seq[i];
                    int count = 1 << i; // doubling list each time
                    for (int j = 0; j < count; j++)
                    {
                        var source = powerSet[j];
                        var destination = powerSet[count + j] = new T[source.Length + 1];
                        for (int q = 0; q < source.Length; q++)
                            destination[q] = source[q];
                        destination[source.Length] = cur;
                    }
                }
                return powerSet;
            }

            public static bool ContainsAll<T>(this IEnumerable<T> enumerable, IEnumerable<T> collection)
            {
                return collection.All(item => enumerable.Contains(item));
            }
        }
    }
