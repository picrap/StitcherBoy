using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{

#if no
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
#endif
}

class Program
{
    static void Main()
    {
        Async().GetAwaiter().GetResult();
        Non_Async();
    }

    static async Task Async()
    {
        var foo = "bar";
        var foo2 = foo;
        // can see foo in debug
        // but not after dnlib read/write :'(
    }

    static void Non_Async()
    {
        var foo = "bar";
        // can see foo in debug
    }
}
