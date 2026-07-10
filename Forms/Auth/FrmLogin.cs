using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Housing_rental.BLL;
using Housing_rental.Models;

namespace Housing_rental.Forms.Auth
{
    public partial class FrmLogin : Form
    {
        private readonly AuthService _authService;

        public event EventHandler LoginSucceeded;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int EM_SETMARGINS = 0xd3;
        private const int EC_RIGHTMARGIN = 2;

        public FrmLogin()
        {
            InitializeComponent();
            _authService = new AuthService();

            LoadGraphics();
            SetupButtonAnimations();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetPasswordTextBoxMargins();
        }

        private void SetPasswordTextBoxMargins()
        {
            try
            {
                // Set right margin of password textbox to leave space for the eye toggle button
                int marginWidth = btnTogglePassword.Width + 8; // 26 + 8 = 34 pixels
                SendMessage(txtPassword.Handle, EM_SETMARGINS, EC_RIGHTMARGIN, marginWidth << 16);
            }
            catch (Exception)
            {
                // Fallback in case handle is not created or API call fails
            }
        }

        private void LoadGraphics()
        {
            try
            {
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "login_bg.png");
                if (File.Exists(imagePath))
                {
                    using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        picGraphic.Image = Image.FromStream(fs);
                    }
                }
            }
            catch (Exception)
            {
                // Fallback: leave picturebox background as is if file can't be loaded
            }
        }

        private void SetupButtonAnimations()
        {
            btnLogin.MouseEnter += BtnLogin_MouseEnter;
            btnLogin.MouseLeave += BtnLogin_MouseLeave;

            btnExit.MouseEnter += BtnExit_MouseEnter;
            btnExit.MouseLeave += BtnExit_MouseLeave;

            btnTogglePassword.MouseEnter += BtnTogglePassword_MouseEnter;
            btnTogglePassword.MouseLeave += BtnTogglePassword_MouseLeave;
            btnTogglePassword.Paint += BtnTogglePassword_Paint;
            btnTogglePassword.Click += BtnTogglePassword_Click;
        }

        private void BtnLogin_MouseEnter(object sender, EventArgs e)
        {
            btnLogin.BackColor = Color.FromArgb(29, 78, 216); // darker indigo/blue
        }

        private void BtnLogin_MouseLeave(object sender, EventArgs e)
        {
            btnLogin.BackColor = Color.FromArgb(37, 99, 235); // original indigo/blue
        }

        private void BtnExit_MouseEnter(object sender, EventArgs e)
        {
            btnExit.BackColor = Color.FromArgb(241, 245, 249); // light grey highlight
        }

        private void BtnExit_MouseLeave(object sender, EventArgs e)
        {
            btnExit.BackColor = Color.White;
        }

        private void BtnTogglePassword_MouseEnter(object sender, EventArgs e)
        {
            btnTogglePassword.BackColor = Color.FromArgb(241, 245, 249); // light grey highlight
        }

        private void BtnTogglePassword_MouseLeave(object sender, EventArgs e)
        {
            btnTogglePassword.BackColor = Color.White;
        }

        private void BtnTogglePassword_Click(object sender, EventArgs e)
        {
            if (txtPassword.PasswordChar == '•')
            {
                txtPassword.PasswordChar = '\0'; // Show password
            }
            else
            {
                txtPassword.PasswordChar = '•'; // Hide password
            }
            btnTogglePassword.Invalidate(); // Force repaint to toggle eye slash
        }

        private void BtnTogglePassword_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int w = btnTogglePassword.Width;
            int h = btnTogglePassword.Height;

            // We draw on a 24x24 virtual grid, scaled to fit the button
            float scale = Math.Min(w, h) / 24f;
            e.Graphics.ScaleTransform(scale, scale);

            // Shift to center the drawing inside the button
            float offsetX = (w / scale - 24f) / 2f;
            float offsetY = (h / scale - 24f) / 2f;
            e.Graphics.TranslateTransform(offsetX, offsetY);

            // Sleek slate-500 color (#64748B)
            Color iconColor = Color.FromArgb(100, 116, 139);
            using (Pen pen = new Pen(iconColor, 1.8f))
            {
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                if (txtPassword.PasswordChar != '\0')
                {
                    // Eye-off state (hidden password) - Lucide eye-off design
                    // 1. Diagonal slash line from (3, 3) to (21, 21)
                    e.Graphics.DrawLine(pen, 3f, 3f, 21f, 21f);

                    // 2. Eyelids drawing (using nice smooth curves)
                    // We draw the eyelids but with the slash crossing, which looks very polished
                    e.Graphics.DrawBezier(pen, 
                        new PointF(2, 12), 
                        new PointF(6, 5), 
                        new PointF(18, 5), 
                        new PointF(22, 12));
                    
                    e.Graphics.DrawBezier(pen, 
                        new PointF(2, 12), 
                        new PointF(6, 19), 
                        new PointF(18, 19), 
                        new PointF(22, 12));

                    // 3. Iris outline
                    e.Graphics.DrawEllipse(pen, 9f, 9f, 6f, 6f);
                }
                else
                {
                    // Eye state (visible password) - Lucide eye design
                    // 1. Eyelids
                    e.Graphics.DrawBezier(pen, 
                        new PointF(2, 12), 
                        new PointF(6, 5), 
                        new PointF(18, 5), 
                        new PointF(22, 12));
                    
                    e.Graphics.DrawBezier(pen, 
                        new PointF(2, 12), 
                        new PointF(6, 19), 
                        new PointF(18, 19), 
                        new PointF(22, 12));

                    // 2. Iris outline
                    e.Graphics.DrawEllipse(pen, 9f, 9f, 6f, 6f);

                    // 3. Pupil filled dot
                    using (SolidBrush brush = new SolidBrush(iconColor))
                    {
                        e.Graphics.FillEllipse(brush, 11f, 11f, 2f, 2f);
                    }
                }
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            ServiceResult<User> result = _authService.Login(txtUsername.Text, txtPassword.Text);

            if (!result.IsSuccess)
            {
                MessageBox.Show(result.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OnLoginSucceeded();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OnLoginSucceeded()
        {
            EventHandler handler = LoginSucceeded;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
