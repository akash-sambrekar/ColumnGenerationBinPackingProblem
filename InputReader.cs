using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BPP
{
    class InputReader
    {
        public static  Dictionary<int,double> ReadDataFile(ref double Bincap)
        {
            Dictionary<int, double> Data = new Dictionary<int, double>();
            //var ReadLines = File.ReadAllLines(@"C:\Users\Akash Sambrekar\source\repos\BPP\bin\Debug\input_data.txt", Encoding.UTF8);
            var ReadLines = File.ReadAllLines(@"C: \Users\Akash Sambrekar\source\repos\BPP\input_data.txt", Encoding.UTF8);     
            var firstline = ReadLines[0].Split(' ').ToArray();
            int num_items = Convert.ToInt32(firstline[0]);
            Bincap = Convert.ToDouble(firstline[1]);

            for(int i=1;i<=num_items;i++)
            {
                Data.Add(i, Convert.ToInt32(ReadLines[i]));

            }

            return Data;
        }
    }

    class Set 
    {
        public List<int> member;

        public Set(List<int> list)
        {
            this.member = list;
        }
    
    }




}
