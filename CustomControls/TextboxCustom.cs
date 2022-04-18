using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WandSyncFile.CustomControls
{
    public partial class TextboxCustom : UserControl
    {

        private Color borderColor = Color.MediumSlateBlue;
        private Color borderFocusColor = Color.MediumSlateBlue;
        private Color inputColor = Color.MediumSlateBlue;
        private int borderSize = 2;
        private bool underlinedStype = false;
        private bool isFocused = false;
        private bool isMultiline = false;
        private bool isPassWord = false;
        private int borderRadius = 0;
        private string value = null;
        private bool isFocus = false;

        public TextboxCustom()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            value = textBox1.Text;
        }

        private void CustomTextBox_Load(object sender, EventArgs e)
        {
        }

        public bool IsFocus
        {
            get { return isFocus; }
            set
            {
                isFocus = value;
                this.ActiveControl = textBox1;
            }
        }
        public Color BottomColor
        {
            get { return borderColor; }
            set
            {
                borderColor = value;
                this.Invalidate();
            }
        }

        public string TextBox1Value
        {
            get
            {
                return textBox1.Text;
            }
        }

        [Category("Wand Input Password Char")]
        public bool IsPassWord
        {
            get { return isPassWord; }
            set
            {
                isPassWord = value;
                this.Invalidate();
            }
        }

        [Category("Input Color")]
        public Color InpuColor
        {
            get { return inputColor; }
            set
            {
                inputColor = value;
                this.Invalidate();
            }
        }

        [Category("Wand Border Focus Color")]
        public Color BorderFocusColor
        {
            get { return borderFocusColor; }
            set
            {
                borderFocusColor = value;
                this.Invalidate();
            }
        }

        [Category("Wand Border Size")]
        public int BorderSize
        {
            get { return borderSize; }
            set
            {
                borderSize = value;
                this.Invalidate();
            }
        }

        [Category("Wand Underlined")]
        public bool UnderlinedStype
        {
            get { return underlinedStype; }
            set
            {
                underlinedStype = value;
                this.Invalidate();
            }
        }

        [Category("Wand Border Radius")]
        public int BonusRadius
        {
            get
            {
                return borderRadius;
            }
            set
            {
                if (value >= 0)
                {
                    borderRadius = value;
                    this.Invalidate();
                }

            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graph = e.Graphics;

            if (isPassWord)
            {
                textBox1.PasswordChar = '*';
            }

            textBox1.BackColor = inputColor;

            if (borderRadius > 1)
            {
                var rectBorderSmooth = this.ClientRectangle;
                var rectBorder = Rectangle.Inflate(rectBorderSmooth, -borderSize, -borderSize);
                int smoothSize = borderSize > 0 ? borderSize : 1;

                using (GraphicsPath pathBorderSmooth = GetFigurePath(rectBorderSmooth, borderRadius))
                using (GraphicsPath pathBorder = GetFigurePath(rectBorder, borderRadius - borderSize))
                using (Pen penBorderSmooth = new Pen(this.Parent.BackColor, smoothSize))
                using (Pen penBorder = new Pen(borderColor, borderSize))
                {
                    this.Region = new Region(pathBorderSmooth);

                    if (borderRadius > 15)
                    {
                        SetTextBoxRoundedRegion();
                    }

                    graph.SmoothingMode = SmoothingMode.AntiAlias;
                    penBorder.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
                    if (isFocused)
                    {
                        penBorder.Color = borderFocusColor;
                    }
                    if (underlinedStype)
                    {
                        graph.DrawPath(penBorderSmooth, pathBorderSmooth);
                        graph.SmoothingMode = SmoothingMode.None;
                        graph.DrawLine(penBorder, 0, this.Height - 1, this.Width, this.Height - 1);
                    }
                    else
                    {
                        graph.DrawPath(penBorderSmooth, pathBorderSmooth);
                        graph.DrawPath(penBorder, pathBorder);
                    }
                }
            }
            else
            {
                using (Pen penBorder = new Pen(borderColor, borderSize))
                {
                    this.Region = new Region(this.ClientRectangle);
                    penBorder.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;

                    if (underlinedStype)
                    {
                        graph.DrawLine(penBorder, 0, this.Height - 1, this.Width, this.Height - 1);
                    }
                    else
                    {
                        graph.DrawRectangle(penBorder, 0, 0, this.Width - 0.5F, this.Height - 0.5F);
                    }
                }
            }
        }

        private void SetTextBoxRoundedRegion()
        {
            GraphicsPath pathTxt;

            if (isMultiline)
            {
                pathTxt = GetFigurePath(textBox1.ClientRectangle, borderRadius - borderSize);
            }
            else
            {
                pathTxt = GetFigurePath(textBox1.ClientRectangle, borderSize * 2);
            }

            textBox1.Region = new Region(pathTxt);
        }

        private GraphicsPath GetFigurePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float size = radius * 2F;

            path.StartFigure();

            path.AddArc(rect.X, rect.Y, size, size, 180, 90);
            path.AddArc((rect.X + (rect.Width - size)),
                  rect.Y, size, size, 270, 90);
            path.AddArc((rect.X + (rect.Width - size)),
                 (rect.Y + (rect.Height - size)),
                 size, size, 0, 90);
            path.AddArc(rect.X, (rect.Y +
                 (rect.Height - size)), size, size, 90, 90);

            path.CloseFigure();

            return path;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateControlHeight();
            if (this.DesignMode)
            {
                UpdateControlHeight();
            }
        }

        private void UpdateControlHeight()
        {
            if (textBox1.Multiline == false)
            {
                int txtHeight = TextRenderer.MeasureText("Text", this.Font).Height + 1;
                textBox1.Multiline = true;
                textBox1.MinimumSize = new Size(0, txtHeight);
                textBox1.Multiline = false;

                this.Height = textBox1.Height + this.Padding.Top + this.Padding.Bottom;
            }
        }

        private void TextboxCustom_Load(object sender, EventArgs e)
        {

        }

        private void OnInputKeyDown(object sender, KeyEventArgs e)
        {
           // MessageBox.Show("123");
        }
    }
}
