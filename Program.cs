using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using ExtensionMethods;

namespace BlockSolve
{

    class Set_info 
    {
        public HashSet<uint> chemical_set = new HashSet<uint>();        
        public HashSet<uint> protein_set = new HashSet<uint>();
    }

    class Input_info
    {
        public HashSet<uint> input_chemicals = new HashSet<uint>();
        public uint index = 0;      
    }

    public class MyComparer : IComparer<uint>
    {
        public int Compare(uint x, uint y)
        {
            if (x > y) return 1;
            else if (x < y) return -1;
            else return 0;
        }
    }



    class Program
    {
        static Dictionary<uint, Input_info> main_map = new Dictionary<uint, Input_info>();
        static ConcurrentDictionary<String, Set_info> all_sets = new ConcurrentDictionary<String, Set_info>();

        static void Main(string[] args)
        {
            //sort_file();
            //return;

            // Note: A's chemical list should be bigger than B's in the file
            load_input();
                                
            //uint skip = 0;
            MyComparer comparer = new MyComparer();

            //foreach (KeyValuePair<uint, Input_info> a_kvp in main_map)
            //{
            Parallel.ForEach(main_map, a_kvp =>
            {
                //Console.WriteLine("A Key " + a_kvp.Key +  " Index " + a_kvp.Value.index);
                //skip += 1;
                //Console.WriteLine("Loop A key " + a_kvp.Key + " skip " + skip);
                //foreach (KeyValuePair<uint, HashSet<uint>> b_kvp in main_map.Where(x => x.Key != a_kvp.Key))
                foreach (KeyValuePair<uint, Input_info> b_kvp in main_map.Where(x => x.Value.index > a_kvp.Value.index))
                {
                    //Console.WriteLine("A Key " + a_kvp.Key + " B Key " + b_kvp.Key + " Index " + b_kvp.Value.index);
                    //Console.WriteLine("Loop B A key " + a_kvp.Key + " B key " + b_kvp.Key + " skip " + skip);

                    //var num_elements = Math.Min(a_kvp.Value.input_chemicals.Count,b_kvp.Value.input_chemicals.Count);
                    //var tempList = new List<uint>(a_kvp.Value.input_chemicals);
                    //foreach (var item in a_kvp.Value.input_chemicals)
                    //{
                    //    tempList.Add(item);
                    //}
                    //var a_v = tempList.ToHashSet();
                    //tempList.Clear();

                   // HashSet<uint> a_v = new HashSet<uint>(a_kvp.Value.input_chemicals);
                  //  a_v.IntersectWith(b_kvp.Value.input_chemicals);


                   // var a_chems = new List<uint>(a_kvp.Value.input_chemicals);
                   // var b_chems = new List<uint>(b_kvp.Value.input_chemicals);

                    //int i = 0;
                    //foreach (var item in a_kvp.Value.input_chemicals)
                    //{
                    //    a_chems[i] = item;
                    //    i++;
                    //}
                    //i = 0;
                    //foreach (var item in b_kvp.Value.input_chemicals)
                    //{
                    //    b_chems[i] = item;
                    //    i++;
                    //}

                   
                    //var a_v = a_kvp.Value.input_chemicals.IntersectSorted(b_kvp.Value.input_chemicals, comparer);                    
                    //var a_v = a_kvp.Value.input_chemicals.ToList().IntersectSorted(b_kvp.Value.input_chemicals.ToList());                                        
                    //a_v.IntersectWith(b_kvp.Value.input_chemicals);

                    var a_v = a_kvp.Value.input_chemicals.MyIntersect(b_kvp.Value.input_chemicals);
                    

                    if (a_v.Count() > 1)
                    {
                        // Made a block 

                        StringBuilder key_b = new StringBuilder();
                        foreach (var item in a_v)
                        {
                            key_b.Append(item);
                        }
                        string key = key_b.ToString();

                        if (all_sets.ContainsKey(key))
                        {
                            Set_info old_set_info = new Set_info();
                            all_sets.TryGetValue(key, out old_set_info);

                            Set_info new_set_info = old_set_info;
                            new_set_info.protein_set.Add(a_kvp.Key);
                            new_set_info.protein_set.Add(b_kvp.Key);
                            all_sets.TryUpdate(key, new_set_info, old_set_info);
                        }
                        else
                        {
                            Set_info set_info = new Set_info();
                            set_info.chemical_set = a_v.ToHashSet();
                            set_info.protein_set.Add(a_kvp.Key);
                            set_info.protein_set.Add(b_kvp.Key);
                            all_sets.TryAdd(key, set_info);
                        }

                        //Console.WriteLine("set [" +key +"]" + " proteins [" + a_kvp.Key + " " + b_kvp.Key + "]");
                    }
                }
            });
            //}
          // }       

            //Round 2. Assign any left out subsets         
            //foreach (KeyValuePair<String, Set_info> a_kvp in all_sets)
            Parallel.ForEach(all_sets, a_kvp =>
            {
                int a_len = a_kvp.Value.chemical_set.Count;

                foreach (KeyValuePair<String, Set_info> b_kvp in all_sets)
                {
                    // The A chem set should be less in length than the B chem set for the possibility of a subset
                    if (a_len < b_kvp.Value.chemical_set.Count && !a_kvp.Value.chemical_set.Except(b_kvp.Value.chemical_set).Any())
                    {
                        // Make sure all the proteins of the superset are also in the subset
                        a_kvp.Value.protein_set.UnionWith(b_kvp.Value.protein_set);
                    }
                }
                });
           // }


            int block_size = 0;
            foreach (var item in all_sets.OrderBy(x => x.Value.chemical_set.Count * x.Value.protein_set.Count))
            {
                //Weird occurence where there are "0" items as proteins
                item.Value.protein_set.RemoveWhere(x => x < 1);

                if (block_size != item.Value.protein_set.Count * item.Value.chemical_set.Count)
                {
                    //Console.WriteLine("\n Block Size " + item.Value.protein_set.Count * item.Value.chemical_set.Count);
                    block_size = item.Value.protein_set.Count * item.Value.chemical_set.Count;
                }


                Console.WriteLine("C[" + string.Join(" ", item.Value.chemical_set.OrderBy(x => x)) + "] P[" + string.Join(" ", item.Value.protein_set.OrderBy(x => x)) + "]");
            }

        }

        static void load_input()
        {
            string[] lines = System.IO.File.ReadAllLines(@"input.txt");

            uint index = 0;
            foreach (string line in lines)
            {
                //Console.WriteLine("\t" + line);
                string[] split = line.Split(',');

                uint protein = uint.Parse(split[0]);
                HashSet<uint> c_set = new HashSet<uint>();


                foreach (string s in split.Skip(1))
                {
                    c_set.Add(uint.Parse(s));
                }

                Input_info input_info = new Input_info();
                input_info.input_chemicals = c_set;
                input_info.index = index++;

                main_map.Add(protein, input_info);
            }
        }

        static void sort_file()
        {
            string[] lines = System.IO.File.ReadAllLines(@"sort_me.txt");
            Dictionary<string,int> lines2 = new Dictionary<string,int>();
       
            foreach (string line in lines)
            {   
                lines2.Add(line, line.Length);   
            }

            foreach (var line in lines2.OrderBy(x=>x.Value).Reverse())
            {
                Console.WriteLine(line.Key);
            }

        }





    }
}
