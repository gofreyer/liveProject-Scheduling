using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using System.Windows.Shapes;
using gantt_charts.Properties;
using System.Collections;

namespace gantt_charts
{
    internal class Task
    {
        public const double MARGIN = 10;
        private const double SIDE = 60;
        private const double GAP = 40;
        private const double FONT_SIZE = 11;
        public Point Center { get; set; }
        public Rect Bounds { get; set; }

        public string Name { get; set; }
        public int Index { get; set; }
        public int Duration { get; set; }
        public int PrereqCount { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public bool IsCritical { get; set; }
        public List<int> PrereqNumbers { get; set; }
        public List<Task> PrereqTasks { get; set; }
        public List<Task> FollowerTasks { get; set; }
        public int SortIndex { get; set; }
        public Task(int _index, string _name, int _duration, List<int> _prereqNumbers)
        {
            Index = _index;
            Name = _name;
            Duration = _duration;
            PrereqNumbers = _prereqNumbers;
            Center = new Point(0, 0);
            Bounds = new Rect(0, 0, 0, 0);
            StartTime = 0;
            EndTime = 0;
            IsCritical = false;
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
            return $"{Index.ToString()} - {Name.ToString()} - ({Duration.ToString()}) - [{temp}]";

        }
        public void Init()
        {
            PrereqCount = PrereqTasks.Count;
            FollowerTasks = new List<Task>();
            IsCritical = false; 
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
            int lineWidth = 1;
            Brush lineBrush = Brushes.Black;
            Point ptTo = new Point(Center.X-SIDE/2,Center.Y);
            foreach (Task task in PrereqTasks)
            {
                Point ptFrom = new Point(task.Center.X + SIDE / 2, task.Center.Y);
                lineWidth = 1;
                lineBrush = Brushes.Black;
                
                if (task.EndTime == StartTime)
                {
                    lineWidth = 3;
                }
                
                if (IsCritical && task.EndTime == StartTime)
                {
                    lineBrush = Brushes.Red;
                }
                
                Line line = _canvas.DrawLine(ptFrom, ptTo, lineBrush, lineWidth);
            }
        }
        public void DrawTaskBox(Canvas _canvas)
        {
            Brush boxFill = Brushes.LightBlue;
            Brush boxStroke = Brushes.Black;
            Brush labelColor = Brushes.Black;
            if (IsCritical)
            {
                boxFill = Brushes.Pink;
                boxStroke = Brushes.Red;
                labelColor = Brushes.Red;
            }
            _canvas.DrawRectangle(Bounds,boxFill,boxStroke, 2);
            string label = $"Task: {Index.ToString()}\nDur: {Duration.ToString()}\nStart: {StartTime.ToString()}\nEnd: {EndTime.ToString()}";
            _canvas.DrawLabel(Bounds,label, Brushes.Transparent, labelColor, (HorizontalAlignment)1, (VerticalAlignment)1, FONT_SIZE, 0);
        }

        public void DrawTaskGanttBox(Canvas _canvas, double _offset, int _margin, int _side)
        {
            Brush boxFill = Brushes.LightBlue;
            Brush boxStroke = Brushes.Blue;
            if (IsCritical)
            {
                boxFill = Brushes.Pink;
                boxStroke = Brushes.Red;
            }

            double width = Duration == 0 ? 2 : Duration*_side;
            double height = _side*0.5;
            Rect Bounds = new Rect(
                _offset + (StartTime+1) * _side,
                _margin + (Index+1) * _side + 0.25*_side,
                width,
                height);

            _canvas.DrawRectangle(Bounds, boxFill, boxStroke, 1);
        }
        public void DrawGanttLinesToPrereqs(Canvas _canvas, double _offset, int _margin, int _side, bool critical=true)
        {
            double lineWidth = 1;
            Brush lineBrush = Brushes.Gray;
            Point ptTo;
            int counter = 0;
            foreach (Task task in PrereqTasks)
            {
                double width = task.Duration == 0 ? 2 : task.Duration * _side;
                if (task.Index < Index)
                {
                    // top
                    ptTo = new Point(5+_offset + (StartTime + 1) * _side + 4 * counter, _margin + (Index + 1) * _side + 0.25 * _side);
                }
                else
                {
                    //bottom
                    ptTo = new Point(5+_offset + (StartTime + 1) * _side + 4 * counter, _margin + (Index + 2) * _side - 0.25 * _side);
                }
                Point ptFrom = new Point(_offset + (task.StartTime + 1) * _side + width, _margin + (task.Index + 1.5) * _side);
                Point ptVia = new Point(5 + _offset + (StartTime + 1) * _side + 4 * counter, _margin + (task.Index + 1.5) * _side);
                lineWidth = 1;
                lineBrush = Brushes.Gray;

                if (task.EndTime == StartTime)
                {
                    lineWidth = 3;
                    lineBrush = Brushes.Black;
                }

                if (IsCritical && task.EndTime == StartTime)
                {
                    lineBrush = Brushes.Red;
                    lineWidth = 1.5;
                }
                if ( !(critical ^ IsCritical) )
                {
                    _canvas.DrawLine(ptFrom, ptVia, lineBrush, lineWidth);
                    _canvas.DrawLine(ptVia, ptTo, lineBrush, lineWidth);
                }
                

                counter++;
            }
        }

        public void SetTimes()
        {
            int maxEndTime = 0;
            foreach (Task task in PrereqTasks)
            {
                if (task.EndTime > maxEndTime)
                {
                    maxEndTime = task.EndTime;
                }
            }
            StartTime = maxEndTime;
            EndTime = StartTime + Duration;
        }
        public void MarkIsCritical()
        {
            IsCritical = true;
            foreach (Task task in PrereqTasks)
            {
                if (task.EndTime == StartTime)
                {
                    task.MarkIsCritical();
                }
            }
        }
        public double GetMargin()
        {
            return MARGIN;
        }
    }
}