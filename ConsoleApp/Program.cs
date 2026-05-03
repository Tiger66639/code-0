using System;
using JaStDev.HAB;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Neural Network Designer Console");
            // Initialize the brain
            var brain = Brain.Current;
            Console.WriteLine("Brain initialized.");
            Console.WriteLine($"Total neurons: {brain.NeuronCount}");
            // Add some basic functionality here
            Console.ReadLine();
        }
    }
}