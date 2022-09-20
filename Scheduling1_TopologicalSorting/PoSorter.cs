using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows;

namespace topological_sorting
{
    class PoSorter
    {
        public List<Task> Tasks { get; set; }
        public List<Task> SortedTasks { get; set; }
        public PoSorter()
        {
            Tasks = null;
            SortedTasks = null;
        }
        public void TopoSort()
        {
            if (Tasks == null)
            {
                return;
            }
            SortedTasks = new List<Task>();
            Queue<Task> readyTasks = new Queue<Task>();
            
            foreach (Task task in Tasks)
            {
                task.Init();
            }
            foreach (Task task in Tasks)
            {
                task.AddToFollowerLists();
                if (task.PrereqCount == 0)
                {
                    readyTasks.Enqueue(task);
                }
            }

            while (readyTasks.Count > 0)
            {
                Task readyTask = readyTasks.Dequeue();
                SortedTasks.Add(readyTask);
                foreach (Task task in readyTask.FollowerTasks)
                {
                    task.PrereqCount--;
                    if (task.PrereqCount == 0)
                    {
                        readyTasks.Enqueue(task);
                    }
                }
            }
        }
        
        public void VerifySort()
        {
            if (SortedTasks == null)
            {
                return;
            }
            int countSorted = 0;
            int countTasks = 0;
            for (int i = 0; i < Tasks.Count; i++)
            {
                Tasks[i].SortIndex = -1;
            }
            for (int i = 0; i < SortedTasks.Count; i++)
            {
                SortedTasks[i].SortIndex = i;
            }
            foreach (Task task in Tasks)
            {
                countTasks++;
                if (task.PrereqTasks.TrueForAll(t => t.SortIndex < task.SortIndex))
                {
                    countSorted++;
                }
            }
            if (countSorted < countTasks)
            {
                MessageBox.Show($"Sorted only {countSorted} out of {countTasks} tasks.");
            }
            else
            {
                MessageBox.Show($"Successfully sorted {countSorted} out of {countTasks} tasks.");
            }
            
        }
        public Task ReadTask(StreamReader _sr)
        {
            while (true)
            {
                string line = _sr.ReadLine();
                if (line == null)
                {
                    return null;
                }
                else
                {
                    line = line.Trim();
                    if (line.Length > 0)
                    {
                        var tokens = new List<String>();
                        int start = 0;
                        int count = 0;
                        int position;
                        do
                        {
                            position = line.IndexOf(',', start);
                            if (position >= 0)
                            {
                                count++;
                                tokens.Add(line.Substring(start, position - start).Trim());
                                start = position + 1;
                            }
                        } while (position > 0 && count < 2);
                        if (count == 2)
                        {
                            tokens.Add(line.Substring(start).Trim());

                            List<int> pre = new List<int>();

                            string[] prenumbers = tokens[2].Split(',');
                            foreach (string num in prenumbers)
                            {
                                string num2 = num.Replace("[", "").Replace("]", "").Trim();
                                if (num2 != "")
                                {
                                    pre.Add(int.Parse(num2));
                                }
                            }

                            Task task = new Task(int.Parse(tokens[0]), tokens[1], pre);

                            return task;
                        }
                                               
                        break;
                    }
                }
            }
            return null;
        }
        public void LoadPoFile(string _filename)
        {
            Tasks = new List<Task>();
            try
            {
                using (StreamReader sr = new StreamReader(_filename))
                {
                    Task task;
                    while ((task = ReadTask(sr)) != null)
                    {
                        Tasks.Add(task);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            foreach(Task task in Tasks)
            {
                task.NumbersToTasks(Tasks);
            }
        }
    }
}
