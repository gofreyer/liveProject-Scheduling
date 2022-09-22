using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Windows.Controls;

namespace pert_charts
{
    class PoSorter
    {
        public List<Task> Tasks { get; set; }
        public List<Task> SortedTasks { get; set; }
        public List<List<Task>> Columns { get; set; }
        public PoSorter()
        {
            Tasks = null;
            SortedTasks = null;
            Columns = null;
        }
        public void PrepareTasks()
        {
            foreach (Task task in Tasks)
            {
                task.Init();
            }
            foreach (Task task in Tasks)
            {
                task.AddToFollowerLists();
            }
        }
        public void TopoSort()
        {
            if (Tasks == null)
            {
                return;
            }
            SortedTasks = new List<Task>();
            Queue<Task> readyTasks = new Queue<Task>();

            PrepareTasks();
            
            foreach (Task task in Tasks)
            {
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
        public void BuildPertChart()
        {
            if (Tasks == null)
            {
                return;
            }
            SortedTasks = new List<Task>();
            Queue<Task> readyTasks = new Queue<Task>();
            Queue<Task> newReadyTasks = new Queue<Task>();

            PrepareTasks();

            foreach (Task task in Tasks)
            {
                if (task.PrereqCount == 0)
                {
                    readyTasks.Enqueue(task);
                }
            }

            Columns = new List<List<Task>>();
            List<Task> Column = new List<Task>();
            Columns.Add(Column);
            foreach (Task task in readyTasks)
            {
                Column.Add(task);
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
                        newReadyTasks.Enqueue(task);
                    }
                }
                if (readyTasks.Count == 0)
                {
                    if (newReadyTasks.Count > 0)
                    {
                        Column = new List<Task>();
                        Columns.Add(Column);
                        foreach (Task task in newReadyTasks)
                        {
                            Column.Add(task);
                        }
                    }
                    readyTasks = newReadyTasks;
                    newReadyTasks = new Queue<Task>();
                }
            }
        }
        public void DrawPertChart(Canvas _canvas)
        {
            _canvas.Children.Clear();

            for (int col = 0; col < Columns.Count; col++)
            {
                for (int row = 0; row < Columns[col].Count; row++)
                {
                    Task task = Columns[col][row];
                    task.SetBounds(col, row);
                }
            }

            foreach (Task task in Tasks)
            {
                task.DrawLinesToPrereqs(_canvas);
            }
            foreach (Task task in Tasks)
            {
                task.DrawTaskBox(_canvas);
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
