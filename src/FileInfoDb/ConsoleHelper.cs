using System;
using System.Text;

namespace FileInfoDb
{
    static class ConsoleHelper
    {
        /// <summary>
        /// Reads a string from the console masking the input with asterisks
        /// </summary>
        public static string ReadPassword()
        {
            var resultBuilder = new StringBuilder();
            while(true)
            {
                var input = Console.ReadKey(intercept: true);
                
                if(input.Key == ConsoleKey.Enter)
                {
                    Console.Write("\n");
                    break;
                }
                else if(input.Key == ConsoleKey.Backspace && resultBuilder.Length > 0)
                {
                    resultBuilder.Remove(resultBuilder.Length - 1, 1);
                    Console.Write("\b \b");                        
                }
                else if (!char.IsControl(input.KeyChar))
                {                    
                    resultBuilder.Append(input.KeyChar);                    
                    Console.Write("*");
                }
            }
            return resultBuilder.ToString();
        }
    }
}
