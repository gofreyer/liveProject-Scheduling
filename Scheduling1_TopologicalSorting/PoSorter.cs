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
                                tokens.Add(line.Substring(start, position - start + 1).Trim());
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
                                pre.Add(int.Parse(num.Replace("[", "").Replace("]", "")));
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
