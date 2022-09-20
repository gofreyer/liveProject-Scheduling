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
        public int PrereqCount { get;set; }
        public List<int> PrereqNumbers { get; set; }
        public List<Task> PrereqTasks { get; set; }
        public List<Task> FollowerTasks { get; set; }
        public int SortIndex { get; set; }
        public Task(int _index, string _name, List<int> _prereqNumbers)
        {
            Index = _index;
            Name = _name;
            PrereqNumbers = _prereqNumbers;
        }

        public override string ToString()
        {
            return Name.ToString();
            
            // for debugging
            /*
            string temp = "";
            int numPrereqs = PrereqNumbers.Count;
            for (int i = 0; i < numPrereqs; i++)
            {
                temp += " "+PrereqNumbers[i].ToString();
            }
            return $"{Index.ToString()} - {Name.ToString()} - [{temp}]";
            */
        }
        public void Init()
        {
            PrereqCount = PrereqTasks.Count;
            FollowerTasks = new List<Task>();
        }
        public void AddToFollowerLists()
        { 
            foreach (Task task in PrereqTasks)
            {
                task.FollowerTasks.Add(this);
            }
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

