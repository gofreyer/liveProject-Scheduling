using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace topological_sorting
{
    internal class Task
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public List<int> PrereqNumbers { get; set; }
        public List<Task> PrereqTasks { get; set; }
        public Task(int _index, string _name, List<int> _prereqNumbers)
        {
            Index = _index;
            Name = _name;
            PrereqNumbers = _prereqNumbers;
        }

        public override string ToString()
        {
            return Name.ToString();
        }
        public void NumbersToTasks(List<Task> tasks)
        {
            int numPrereqs = PrereqNumbers.Count;
            PrereqTasks = new List<Task>();
            for (int i = 0; i < numPrereqs; i++)
            {
                PrereqTasks.Add(tasks[PrereqNumbers[i]]);
            }
        }
    }
}

