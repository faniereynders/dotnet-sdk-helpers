using System;
using System.Threading;

namespace dotnet.helpers.sdk
{
    public class Animations
    {
        private int counter = 0;
        
        public void Turn(string loadingText, int top)
        {
            Console.SetCursorPosition(0, top);

            switch (counter % 4)
            {
                case 0: Console.WriteLine($"{loadingText} /  "); break;
                case 1: Console.WriteLine($"{loadingText} -  "); break;
                case 2: Console.WriteLine($"{loadingText} \\ "); break;
                case 3: Console.WriteLine($"{loadingText} |  "); break;
            }
        }

        public void Ready()
        {
            counter++;
            Thread.Sleep(100);
        }
    }
}
