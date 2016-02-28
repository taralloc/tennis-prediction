using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    static class BinarySearch
    {
        private static Random rng = new Random();

        public static T RicercaDicotomica<T, TKey>(this IList<T> list, Func<T, TKey> keySelector, TKey key) where TKey : IComparable<TKey>
        {
            if (list.Count == 0)
                return default(T);

            int min = 0;
            int max = list.Count - 1;
            while (min <= max)
            {
                int mid = min + ((max - min) / 2);
                T midItem = list[mid];
                TKey midKey = keySelector(midItem);
                int comp = midKey.CompareTo(key);
                if (comp < 0)
                {
                    min = mid + 1;
                }
                else if (comp > 0)
                {
                    max = mid - 1;
                }
                else
                {
                    return midItem;
                }
            }

            return default(T);
        }

        public static List<T> RicercaDicotomicaAll<T, TKey>(this List<T> list, Func<T, TKey> keySelector, TKey key) where TKey : IComparable<TKey>
        {
            if (list.Count == 0)
                return null;

            int min = 0;
            int max = list.Count - 1;
            int index = -1;
            while (min <= max)
            {
                int mid = min + ((max - min) / 2);
                T midItem = list[mid];
                TKey midKey = keySelector(midItem);
                int comp = midKey.CompareTo(key);
                if (comp < 0)
                {
                    min = mid + 1;
                }
                else if (comp > 0)
                {
                    max = mid - 1;
                }
                else
                {
                    index = mid; //Found
                    break;
                }
            }

            if (index == -1) return null; //Not found

            int starting_index, ending_index;
            int i = index - 1;
            while (i >= 0 && keySelector(list[i]).CompareTo(key) == 0) i--;
            starting_index = i + 1;
            i = index + 1;
            while (i < list.Count && keySelector(list[i]).CompareTo(key) == 0) i++;
            ending_index = i - 1;
            return list.GetRange(starting_index, ending_index - starting_index + 1);
        }

        public static T RicercaDicotomicaDic<T, TKey>(this Dictionary<TKey, T> list, TKey key) where TKey : IComparable<TKey>
        {
            if (list.Count == 0)
                return default(T);

            int min = 0;
            int max = list.Count - 1;
            while (min <= max)
            {
                int mid = min + ((max - min) / 2);
                T midItem = list.ElementAt(mid).Value;
                int comp = list.ElementAt(mid).Key.CompareTo(key);
                if (comp < 0)
                {
                    min = mid + 1;
                }
                else if (comp > 0)
                {
                    max = mid - 1;
                }
                else
                {
                    return midItem;
                }
            }

            return default(T);
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static List<T> FindAllLessOrdered<T, TKey>(this List<T> list, Func<T, TKey> keySelector, TKey key) where TKey : IComparable<TKey>
        {
            if (list.Count == 0) return null;

            int start = 0; int i;
            for(i = 0; i < list.Count; i++)
            {
                int cmp = keySelector(list[i]).CompareTo(key);
                if (cmp == 0) break;
            }

            return list.GetRange(start, i - start);
        }


    }
}
