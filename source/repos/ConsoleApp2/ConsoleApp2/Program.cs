using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace staticexample
{
    class Program
    {
        public static int i;
        public static void display()
        {
           // i = 10;
            Console.WriteLine(i);
        }
        public void demo()
        {
            int j = 20;
            Console.WriteLine(j);
            i = 100;
            Console.WriteLine(i);
            //Console.Read();
        }


        static void Main(string[] args)
        {
            Program obj = new Program();
            Program.display();

           // obj.display();
            obj.demo();
            Console.ReadKey();
        }
    }
}