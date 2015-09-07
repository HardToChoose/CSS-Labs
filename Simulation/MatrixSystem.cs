using Language;
using Parallel.Tree;

using System;
using System.Linq;
using System.Collections.Generic;

namespace Simulation
{
    public class MatrixSystem
    {
        public int ProcessorCount { get; set; }

        public MatrixSystem(int processorCount)
        {
            this.ProcessorCount = processorCount;
        }

        public SimulationResult SimulateCalculation(Tree expression)
        {
            var result = new SimulationResult(this.ProcessorCount);
            var tree = expression.Clone() as Tree;
            var leaveOps = tree.LeaveOperations;

            while (leaveOps.Count > 0)
            {
                /* Group tree leaf operations */
                var groups = leaveOps.GroupBy(node => node.Value)
                                     .OrderByDescending(group => group.Count());

                /* Check for a group of operations of length ProcessorCount */
                var selection = groups.FirstOrDefault(group => group.Count() == this.ProcessorCount);

                /* Select the most common operation otherwise */
                if (selection == null)
                {
                    /* Prefer multiplication and division operations */
                    selection = groups.FirstOrDefault(group => group.Count() == groups.First().Count() &&
                                                               (group.Key == "*" || group.Key == "/"));
                    if (selection == null)
                    {
                        selection = groups.First();
                    }
                }

                /* Obtain the number of processors to occupy on the current step */
                int stepSize = Math.Min(selection.Count(), this.ProcessorCount);
                int offset = (ProcessorCount - stepSize) / 2;
                
                var step = new string[ProcessorCount];
                var nodes = selection.ToArray().Select(node => expression.FindNode(node.Path));

                for (int proc = 0; proc < stepSize; proc++)
                {
                    Node operation = selection.ElementAt(proc);
                    step[proc + offset] = string.Join(' ' + operation.Value + ' ',
                                                      operation.Children.Select(child => child.Value));

                    /* Mark operation node as visited */
                    operation.Children.Clear();
                    operation.Value = "  ";
                }

                result.AddStep(step, nodes.ToArray(), Utils.GetOperationComplexity(selection.Key));
                leaveOps = tree.LeaveOperations;
            }

            return result;
        }
    }
}
