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
using System.Globalization;

namespace gantt_charts
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

            Task finishTask = null;
            while (readyTasks.Count > 0)
            {
                Task readyTask = readyTasks.Dequeue();
                finishTask = readyTask;
                readyTask.SetTimes();
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
            if (finishTask != null)
            {
                finishTask.MarkIsCritical();
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
            double maxX = 0;
            double maxY = 0;
            foreach (Task task in Tasks)
            {
                task.DrawTaskBox(_canvas);
                if (task.Bounds.Left+task.Bounds.Width+task.GetMargin() > maxX) maxX = task.Bounds.Left+task.Bounds.Width+task.GetMargin();
                if (task.Bounds.Top + task.Bounds.Height + task.GetMargin() > maxY) maxY = task.Bounds.Top + task.Bounds.Height+task.GetMargin();
            }
            _canvas.Width = maxX;
            _canvas.Height = maxY;
        }

        public static Size PrintedSize(string s, FontFamily font, double fontSize)
        {
            FormattedText formatted = new FormattedText(s,
                    CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    font.GetTypefaces().First(),
                    fontSize,
                    Brushes.Black);
            return new Size(formatted.Width, formatted.Height);
        }

        public void DrawGanttChart(Canvas _canvas)
        {
            FontFamily font = new FontFamily("Segoe UI");
            
            const int MARGIN = 10;
            const int FONT_SIZE = 11;
            const int LINEHEIGHT = FONT_SIZE + 13;
            Brush labelColor = Brushes.Black;
            Brush gridColor = Brushes.LightGray;

            _canvas.Children.Clear();

            Label label;
            string taskName = "";
            double maxLength = 0;
            Size size;
            Rect Bounds = new Rect(0,0,0,0);
            int colStartTime = int.MaxValue;
            int colEndTime = 0;
            
            // Get TextSize and ColumnsCount
            for (int row = 0; row < Tasks.Count; row++)
            {
                taskName = $"{Tasks[row].Index.ToString()}. {Tasks[row].Name}";
                size = PrintedSize(taskName, font, FONT_SIZE);
                if (size.Width+MARGIN > maxLength)
                {
                    maxLength = size.Width+MARGIN;
                }
                /*
                if (taskName.Length * FONT_SIZE * 0.6 > maxLength)
                {
                    maxLength = taskName.Length * FONT_SIZE * 0.6;  
                }
                */
                if (Tasks[row].StartTime < colStartTime) colStartTime = Tasks[row].StartTime;
                if (Tasks[row].EndTime > colEndTime) colEndTime = Tasks[row].EndTime;
            }
            // Draw TaskNames
            for (int row = 0; row < Tasks.Count; row++)
            {
                taskName = $"{Tasks[row].Index.ToString()}. {Tasks[row].Name}";
                
                Bounds = new Rect(MARGIN, MARGIN + (row+1) * LINEHEIGHT, maxLength, LINEHEIGHT);
                label = _canvas.DrawLabel(Bounds, taskName, Brushes.Transparent, labelColor, HorizontalAlignment.Left, VerticalAlignment.Center, FONT_SIZE, 0);
                label.FontFamily = font;
                //_canvas.DrawRectangle(Bounds, Brushes.Transparent, labelColor, 1);
            }
            // Draw ColumnHeaders
            for (int col = colStartTime; col <= colEndTime+1; col++)
            {
                Bounds = new Rect(MARGIN+maxLength+(col-colStartTime)*LINEHEIGHT, MARGIN, LINEHEIGHT, LINEHEIGHT);
                label = _canvas.DrawLabel(Bounds, col.ToString(), Brushes.Transparent, labelColor, HorizontalAlignment.Center, VerticalAlignment.Center, FONT_SIZE, 0);
                label.FontFamily = font;
                //_canvas.DrawRectangle(Bounds, Brushes.Transparent, labelColor, 1);
            }
            //Draw Grid
            for (int row = 0; row <= Tasks.Count+1; row++)
            {
                _canvas.DrawLine(new Point(MARGIN + maxLength, MARGIN + row * LINEHEIGHT),
                    new Point(MARGIN + maxLength + LINEHEIGHT * (colEndTime - colStartTime+2), MARGIN + row * LINEHEIGHT), gridColor, 1);
            }
            for (int col = colStartTime; col <= colEndTime+2; col++) 
            {
                _canvas.DrawLine(new Point(MARGIN + maxLength + (col - colStartTime ) * LINEHEIGHT, MARGIN),
                    new Point(MARGIN + maxLength + (col - colStartTime ) * LINEHEIGHT, MARGIN + (Tasks.Count+1) * LINEHEIGHT), gridColor, 1);
            }
            // Draw TaskBoxes
            foreach (Task task in Tasks)
            {
                task.DrawTaskGanttBox(_canvas,MARGIN+maxLength,MARGIN,LINEHEIGHT);
            }
            // Draw TaskLines
            foreach (Task task in Tasks)
            {
                // first all non critical paths...
                task.DrawGanttLinesToPrereqs(_canvas, MARGIN + maxLength, MARGIN, LINEHEIGHT,false);
            }
            foreach (Task task in Tasks)
            {
                // ...then all critical paths
                task.DrawGanttLinesToPrereqs(_canvas, MARGIN + maxLength, MARGIN, LINEHEIGHT, true);
            }

            _canvas.Width = 2*MARGIN + maxLength + LINEHEIGHT * (colEndTime - colStartTime + 2);
            _canvas.Height = 2 * MARGIN + (Tasks.Count + 1) * LINEHEIGHT;
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
                        } while (position > 0 && count < 3);
                        if (count == 3)
                        {
                            tokens.Add(line.Substring(start).Trim());

                            List<int> pre = new List<int>();

                            string[] prenumbers = tokens[3].Split(',');
                            foreach (string num in prenumbers)
                            {
                                string num2 = num.Replace("[", "").Replace("]", "").Trim();
                                if (num2 != "")
                                {
                                    pre.Add(int.Parse(num2));
                                }
                            }

                            Task task = new Task(int.Parse(tokens[0]), tokens[2], int.Parse(tokens[1]), pre);

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
