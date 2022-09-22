using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using System.Windows.Shapes;

namespace pert_charts
{
    internal class Task
    {
        private const double MARGIN = 10;
        private const double SIDE = 30;
        private const double GAP = 40;
        private const double FONT_SIZE = 12;
        public Point Center { get; set; }
        public Rect Bounds { get; set; }

        public string Name { get; set; }
        public int Index { get; set; }
        public int PrereqCount { get; set; }
        public List<int> PrereqNumbers { get; set; }
        public List<Task> PrereqTasks { get; set; }
        public List<Task> FollowerTasks { get; set; }
        public int SortIndex { get; set; }
        public Task(int _index, string _name, List<int> _prereqNumbers)
        {
            Index = _index;
            Name = _name;
            PrereqNumbers = _prereqNumbers;
            Center = new Point(0, 0);
            Bounds = new Rect(0, 0, 0, 0);
        }

        public override string ToString()
        {
            //return Name.ToString();

            // for debugging

            string temp = "";
            int numPrereqs = PrereqNumbers.Count;
            for (int i = 0; i < numPrereqs; i++)
            {
                temp += " " + PrereqNumbers[i].ToString();
            }
            return $"{Index.ToString()} - {Name.ToString()} - [{temp}]";

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
        public void SetBounds(int _col, int _row)
        {
            Center = new Point(MARGIN + _col * (SIDE + GAP) + SIDE / 2,
                MARGIN + _row * (SIDE + MARGIN) + SIDE / 2);
            Bounds = new Rect(Center.X - SIDE / 2, Center.Y - SIDE / 2, SIDE, SIDE);
        }
        public void DrawLinesToPrereqs(Canvas _canvas)
        {
            Point ptTo = new Point(Center.X-SIDE/2,Center.Y);
            foreach (Task task in PrereqTasks)
            {
                Point ptFrom = new Point(task.Center.X + SIDE / 2, task.Center.Y); 
                Line line = _canvas.DrawLine(ptFrom, ptTo, Brushes.Green, 1);
            }
        }
        public void DrawTaskBox(Canvas _canvas)
        {
            _canvas.DrawRectangle(Bounds,Brushes.Yellow, Brushes.Blue, 2);
            _canvas.DrawLabel(Bounds, Index.ToString(), Brushes.Transparent, Brushes.Green, (HorizontalAlignment)1, (VerticalAlignment)1, FONT_SIZE, 0);
        }
    }
}

