using System;
using System.Security.Cryptography;

namespace Question3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int zero = 0;
            for(int i = 0; i < 2; ++i)
            {
                try
                {
                    try
                    {
                        Console.WriteLine("{0}: Hello World!", i + 1);
                        _ = 42 / zero;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Processing exception ...");
                        if (i == 0)
                        {
                            throw new Exception(e.Message);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
