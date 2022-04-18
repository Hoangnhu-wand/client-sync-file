using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WandSyncFile.CustomControls
{
    public class ButtonCustom : Button
    {
        private int borderSize = 0;
        private int borderRadius = 6;

        private Color borderColor = Color.Red;

        public ButtonCustom()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Size = new Size(150, 40);
            this.BackColor = Color.MediumAquamarine;
            this.ForeColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF rectSurFace = new RectangleF(0, 0, this.Width, this.Height);
            RectangleF rectSurBorder = new RectangleF(1, 1, this.Width - 0.8F, this.Height - 1);

            if (borderRadius > 2)
            {
                using (GraphicsPath pathSurFace = GetFigurePath(rectSurFace, borderRadius))
                using (GraphicsPath pathBorder = GetFigurePath(rectSurBorder, borderRadius - 1F))
                using (Pen penSurFace = new Pen(this.Parent.BackColor, 2))
                using (Pen penBorder = new Pen(borderColor, borderSize))
                {
                    penBorder.Alignment = PenAlignment.Inset;
                    this.Region = new Region(pathSurFace);
                    e.Graphics.DrawPath(penSurFace, pathSurFace);

                    if (borderSize > 1)
                    {
                        e.Graphics.DrawPath(penBorder, pathBorder);
                    }
                }
            }
            else
            {
                this.Region = new Region(rectSurFace);
                if (borderSize > 1)
                {
                    using (Pen penBorder = new Pen(borderColor, borderSize))
                    {
                        penBorder.Alignment = PenAlignment.Inset;
                        e.Graphics.DrawRectangle(penBorder, 0, 0, this.Width - 1, this.Height - 1);
                    }
                }
            }

        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            this.Parent.BackColorChanged += new EventHandler(Container_BackColorChanged);
        }

        private void Container_BackColorChanged(object sender, EventArgs e)
        {
            if (this.DesignMode)
            {
                this.Invalidate();
            }
        }

        private GraphicsPath GetFigurePath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float size = radius * 2F;

            path.StartFigure();


            path.AddArc(rect.X, rect.Y, size, size, 180, 90);
            path.AddArc((rect.X + (rect.Width - size)), rect.Y, size, size, 270, 90);
            path.AddArc((rect.X + (rect.Width - size)), (rect.Y + (rect.Height - size)), size, size, 0, 90);
            path.AddArc(rect.X, (rect.Y + (rect.Height - size)), size, size, 90, 90);

            path.CloseFigure();

            return path;
        }
    }
}
