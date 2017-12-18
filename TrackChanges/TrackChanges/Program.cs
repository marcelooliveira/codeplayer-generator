using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffPlex.Model;

namespace TrackChanges
{
    class Program
    {
        static void Main(string[] args)
        {
            var differ = new CodeDiffer();
            Console.WriteLine("Listening...");
            Console.WriteLine("(Press any key to exit.)");

            Console.ReadLine();
        }
    }
}
