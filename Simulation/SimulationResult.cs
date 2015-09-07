using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Language;
using Parallel.Tree;

namespace Simulation
{
    public class Step
    {
        public int Tick { get; private set; }
        public double Time { get; private set; }

        public string[] ProcessorOperations { get; private set; }
        public Node[] GraphNodes { get; private set; }

        public Step(int tick, string[] procOps, Node[] graphNodes, double time)
        {
            this.Tick = tick;
            this.Time = time;

            this.ProcessorOperations = procOps;
            this.GraphNodes = graphNodes;
        }
    }

    public struct Location
    {
        public int Step;
        public int Processor;

        public Location(int step, int processor)
        {
            this.Step = step;
            this.Processor = processor;
        }

        public override string ToString()
        {
 	         return string.Format("{0}:{1}", Step, Processor);
        }

        public static Location Parse(string location)
        {
            var parts = location.Split(':');
            return new Location(int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }

    public class Link
    {
        public Location From;
        public Location To;
        public bool RightSide;
    }

    public class SimulationResult
    {
        private int processorCount;

        public List<Step> Steps { get; private set; }
        public double TotalTime { get; private set; }

        public SimulationResult() : this(0) { }

        public SimulationResult(int processorCount)
        {
            this.processorCount = processorCount;

            this.Steps = new List<Step>();
        }

        public void AddStep(string[] procesorOperations, Node[] graphNodes, double stepTime)
        {
            Contract.Requires(procesorOperations != null &&
                              procesorOperations.Length == processorCount);

            Steps.Add(new Step(Steps.Count + 1, procesorOperations, graphNodes, stepTime));
            TotalTime += stepTime;
        }
    }
}
