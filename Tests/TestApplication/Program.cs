using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    using ReferencedLibrary;
    using ReferencedLibrary2;

    class Program
    {
        static void Main(string[] args)
        {
            var z = "Nothing...";
            Console.WriteLine(z);
            var r = new ReferencedClass();
            r.F();
            var r2 = new ReferencedClass2();
            r2.G();
        }
    }
}
