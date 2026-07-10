using System.Drawing;
using System.Windows.Forms;

namespace Housing_rental.Forms.Common
{
    public class ModulePlaceholderControl : UserControl
    {
        public ModulePlaceholderControl(string moduleName, string description)
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9F);

            Panel content = new Panel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White,
                Location = new Point(24, 24),
                Size = new Size(860, 210)
            };

            Label title = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                Location = new Point(32, 28),
                Text = moduleName
            };

            Label body = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.DimGray,
                Location = new Point(36, 86),
                Size = new Size(720, 120),
                Text = description
            };

            content.Controls.Add(title);
            content.Controls.Add(body);
            Controls.Add(content);
        }
    }
}
