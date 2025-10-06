using System;
using System.Drawing;
using System.Windows.Forms;

namespace CyberManhuntLocalizer
{
    public class CyberProgressBar : ProgressBar
    {
        public CyberProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = new Rectangle(0, 0, (int)(this.Width * ((double)this.Value / this.Maximum)), this.Height);

            // Фон (тёмный)
            using (var bgBrush = new SolidBrush(Color.FromArgb(30, 30, 40)))
                g.FillRectangle(bgBrush, 0, 0, this.Width, this.Height);

            // Заполнение (пурпурный неон)
            using (var fillBrush = new SolidBrush(Color.FromArgb(100, 0, 255)))
                g.FillRectangle(fillBrush, rect);

            // Рамка (голубой неон)
            using (var pen = new Pen(Color.Cyan, 1))
                g.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
        }
    }
}