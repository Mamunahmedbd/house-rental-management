using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using Housing_rental.Models;

namespace Housing_rental.Forms.Payments
{
    public class FrmPaymentReceipt : Form
    {
        private readonly PaymentListItem _payment;
        private readonly IList<PaymentAllocationDetail> _allocations;
        private readonly string _footer;
        private readonly RichTextBox _receiptText;
        private readonly PrintDocument _printDocument;

        public FrmPaymentReceipt(PaymentListItem payment, IList<PaymentAllocationDetail> allocations, string footer)
        {
            if (payment == null)
            {
                throw new ArgumentNullException("payment");
            }

            _payment = payment;
            _allocations = allocations ?? new List<PaymentAllocationDetail>();
            _footer = footer ?? string.Empty;
            _printDocument = new PrintDocument();
            _printDocument.DocumentName = "Receipt " + payment.ReceiptNo;
            _printDocument.PrintPage += PrintDocument_PrintPage;

            Text = "Payment Receipt - " + payment.ReceiptNo;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(620, 650);
            Size = new Size(720, 760);
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9F);

            TableLayoutPanel root = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                RowCount = 2
            };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));

            _receiptText = new RichTextBox
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10F),
                ReadOnly = true,
                Text = BuildReceiptText()
            };
            root.Controls.Add(_receiptText, 0, 0);

            FlowLayoutPanel actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };
            Button close = CreateButton("Close", false);
            close.Click += delegate { Close(); };
            Button print = CreateButton("Print / Preview", true);
            print.Click += Print_Click;
            actions.Controls.Add(close);
            actions.Controls.Add(print);
            root.Controls.Add(actions, 0, 1);
            Controls.Add(root);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _printDocument.Dispose();
            }

            base.Dispose(disposing);
        }

        private string BuildReceiptText()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("HOUSE RENTAL MANAGEMENT SYSTEM");
            text.AppendLine(new string('=', 62));
            text.AppendLine("PAYMENT RECEIPT");
            text.AppendLine();
            text.AppendLine("Receipt No : " + _payment.ReceiptNo);
            text.AppendLine("Payment Date: " + _payment.PaymentDate.ToString("dd MMM yyyy"));
            text.AppendLine("Posted At  : " + _payment.PostedAt.ToString("dd MMM yyyy HH:mm"));
            text.AppendLine("Status     : " + _payment.Status);
            text.AppendLine(new string('-', 62));
            text.AppendLine("Tenant     : " + _payment.TenantName);
            text.AppendLine("Agreement  : " + _payment.AgreementNo);
            text.AppendLine("Property   : " + _payment.PropertyName);
            text.AppendLine("House/Room : " + _payment.HouseName + " / " + _payment.RoomNo);
            text.AppendLine(new string('-', 62));
            text.AppendLine("ALLOCATIONS");

            foreach (PaymentAllocationDetail allocation in _allocations)
            {
                string label = allocation.BillingPeriod.ToString("MMMM yyyy");
                string amount = allocation.CurrencyCode + " " + allocation.AllocatedAmount.ToString("N2");
                text.AppendLine(label.PadRight(42) + amount.PadLeft(20));
            }

            text.AppendLine(new string('-', 62));
            text.AppendLine("TOTAL".PadRight(42) + (_payment.CurrencyCode + " " + _payment.Amount.ToString("N2")).PadLeft(20));
            text.AppendLine("Method     : " + _payment.PaymentMethod);
            if (!string.IsNullOrWhiteSpace(_payment.ExternalReference))
            {
                text.AppendLine("Reference  : " + _payment.ExternalReference);
            }
            text.AppendLine("Collected By: " + _payment.CollectedByName);

            if (_payment.Status == PaymentStatuses.Reversed)
            {
                text.AppendLine();
                text.AppendLine("*** REVERSED ***");
                text.AppendLine("Reason     : " + _payment.ReversalReason);
                text.AppendLine("Reversed By: " + _payment.ReversedByName);
                if (_payment.ReversedAt.HasValue)
                {
                    text.AppendLine("Reversed At: " + _payment.ReversedAt.Value.ToString("dd MMM yyyy HH:mm"));
                }
            }

            text.AppendLine();
            text.AppendLine(_footer);
            return text.ToString();
        }

        private void Print_Click(object sender, EventArgs e)
        {
            using (PrintPreviewDialog preview = new PrintPreviewDialog())
            {
                preview.Document = _printDocument;
                preview.Width = 1000;
                preview.Height = 760;
                preview.ShowDialog(this);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            using (Font font = new Font("Consolas", 10F))
            using (Brush brush = new SolidBrush(Color.Black))
            {
                RectangleF bounds = new RectangleF(e.MarginBounds.Left, e.MarginBounds.Top, e.MarginBounds.Width, e.MarginBounds.Height);
                e.Graphics.DrawString(BuildReceiptText(), font, brush, bounds);
            }

            e.HasMorePages = false;
        }

        private static Button CreateButton(string text, bool primary)
        {
            Button button = new Button
            {
                BackColor = primary ? Color.FromArgb(37, 99, 235) : Color.FromArgb(241, 245, 249),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = primary ? Color.White : Color.FromArgb(51, 65, 85),
                Height = 34,
                Margin = new Padding(8, 0, 0, 0),
                Text = text,
                Width = primary ? 130 : 84
            };
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            return button;
        }
    }
}
