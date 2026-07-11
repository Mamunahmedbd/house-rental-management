using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Housing_rental.BLL;
using Housing_rental.Models;

namespace Housing_rental.Forms.Payments
{
    public partial class PaymentManagementControl : UserControl
    {
        private const int EmSetCueBanner = 0x1501;
        private readonly RentPaymentService _paymentService;
        private readonly BindingSource _agreementSource;
        private readonly BindingSource _chargeSource;
        private readonly BindingSource _duesSource;
        private readonly BindingSource _paymentSource;
        private Guid _currentRequestId;
        private bool _loading;

        public PaymentManagementControl()
        {
            _paymentService = new RentPaymentService();
            _agreementSource = new BindingSource();
            _chargeSource = new BindingSource();
            _duesSource = new BindingSource();
            _paymentSource = new BindingSource();
            _currentRequestId = Guid.NewGuid();

            InitializeComponent();
            BindSources();
        }

        private void PaymentManagementControl_Load(object sender, EventArgs e)
        {
            cmbPaymentMethod.Items.AddRange(PaymentMethods.All);
            cmbPaymentMethod.SelectedItem = PaymentMethods.Cash;
            cmbDuesStatus.Items.AddRange(new object[]
            {
                ChargeStatuses.All,
                ChargeStatuses.Due,
                ChargeStatuses.Partial,
                ChargeStatuses.Overdue,
                ChargeStatuses.Paid,
                ChargeStatuses.Waived
            });
            cmbDuesStatus.SelectedItem = ChargeStatuses.All;
            cmbHistoryStatus.Items.AddRange(new object[] { "All", PaymentStatuses.Posted, PaymentStatuses.Reversed });
            cmbHistoryStatus.SelectedIndex = 0;
            dtpPaymentDate.Value = DateTime.Today;
            dtpBillingPeriod.Value = DateTime.Today;
            dtpHistoryFrom.Value = DateTime.Today.AddMonths(-6);
            dtpHistoryTo.Value = DateTime.Today;
            btnGenerateCharges.Visible = CurrentSession.IsAdmin;
            btnReversePayment.Visible = CurrentSession.IsAdmin;
            SetCollectionActionsEnabled(false);
            SetCueBanner(txtAgreementSearch, "Search agreement, tenant, phone, property, house or room");
            SetCueBanner(txtDuesSearch, "Search tenant, agreement, property or room");
            SetCueBanner(txtHistorySearch, "Search receipt, tenant, agreement or reference");

            LoadAllData();
        }

        private void BindSources()
        {
            dgvAgreements.DataSource = _agreementSource;
            dgvCharges.DataSource = _chargeSource;
            dgvDues.DataSource = _duesSource;
            dgvPayments.DataSource = _paymentSource;
        }

        private void LoadAllData()
        {
            LoadAgreementContexts();
            LoadDues();
            LoadPaymentHistory();
        }

        private void LoadAgreementContexts()
        {
            if (_loading)
            {
                return;
            }

            _loading = true;
            try
            {
                ServiceResult<List<PaymentAgreementItem>> result = _paymentService.SearchAgreementContexts(txtAgreementSearch.Text);
                if (!result.IsSuccess)
                {
                    ShowStatus(result.Message, false);
                    return;
                }

                _agreementSource.DataSource = new BindingList<PaymentAgreementItem>(result.Data);
                if (result.Data.Count == 0)
                {
                    _chargeSource.DataSource = new BindingList<RentChargeListItem>();
                    ClearAgreementContext();
                    ShowStatus("No active agreements or outstanding balances matched the search.", true);
                }
                else
                {
                    dgvAgreements.ClearSelection();
                    dgvAgreements.Rows[0].Selected = true;
                    dgvAgreements.CurrentCell = dgvAgreements.Rows[0].Cells[0];
                    LoadSelectedAgreementCharges();
                    ShowStatus(result.Data.Count + " payment agreement(s) loaded.", true);
                }
            }
            finally
            {
                _loading = false;
            }
        }

        private void LoadSelectedAgreementCharges()
        {
            PaymentAgreementItem agreement = GetSelectedAgreement();
            if (agreement == null)
            {
                _chargeSource.DataSource = new BindingList<RentChargeListItem>();
                ClearAgreementContext();
                return;
            }

            lblAgreementContext.Text =
                agreement.AgreementNo + "  |  " + agreement.TenantName + "  |  " + agreement.RoomPath + Environment.NewLine +
                "Term: " + agreement.StartDate.ToString("dd MMM yyyy") + " - " + agreement.EndDate.ToString("dd MMM yyyy") +
                "  |  Contract rent: " + FormatMoney(agreement.MonthlyRent, GetCurrency()) +
                "  |  Outstanding: " + FormatMoney(agreement.TotalBalance, GetCurrency());

            ServiceResult<List<RentChargeListItem>> result = _paymentService.SearchCharges(string.Empty, ChargeStatuses.All, agreement.AgreementId, false);
            if (!result.IsSuccess)
            {
                ShowStatus(result.Message, false);
                return;
            }

            _chargeSource.DataSource = new BindingList<RentChargeListItem>(result.Data);
            ResetPaymentEntry(true);
            SetCollectionActionsEnabled(result.Data.Count > 0);

            if (result.Data.Count == 0)
            {
                string instruction = CurrentSession.IsAdmin
                    ? "No outstanding charge exists for this agreement. Select the billing month above and click Generate Charges."
                    : "No outstanding charge exists for this agreement. Ask an administrator to generate the billing month first.";
                ShowInformation(instruction);
            }
            else
            {
                ShowStatus(result.Data.Count + " outstanding charge(s) loaded.", true);
            }
        }

        private void LoadDues()
        {
            string status = cmbDuesStatus.SelectedItem == null ? ChargeStatuses.All : cmbDuesStatus.SelectedItem.ToString();
            ServiceResult<List<RentChargeListItem>> result = _paymentService.SearchCharges(
                txtDuesSearch.Text,
                status,
                null,
                chkIncludePaid.Checked);

            if (!result.IsSuccess)
            {
                ShowStatus(result.Message, false);
                return;
            }

            _duesSource.DataSource = new BindingList<RentChargeListItem>(result.Data);
        }

        private void LoadPaymentHistory()
        {
            string status = cmbHistoryStatus.SelectedItem == null ? "All" : cmbHistoryStatus.SelectedItem.ToString();
            DateTime? dateFrom = chkUseHistoryDates.Checked ? dtpHistoryFrom.Value.Date : (DateTime?)null;
            DateTime? dateTo = chkUseHistoryDates.Checked ? dtpHistoryTo.Value.Date : (DateTime?)null;

            ServiceResult<List<PaymentListItem>> result = _paymentService.SearchPayments(txtHistorySearch.Text, status, dateFrom, dateTo);
            if (!result.IsSuccess)
            {
                ShowStatus(result.Message, false);
                return;
            }

            _paymentSource.DataSource = new BindingList<PaymentListItem>(result.Data);
            UpdatePaymentActionState();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadAllData();
        }

        private void BtnSearchAgreements_Click(object sender, EventArgs e)
        {
            LoadAgreementContexts();
        }

        private void TxtAgreementSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadAgreementContexts();
                e.SuppressKeyPress = true;
            }
        }

        private void DgvAgreements_SelectionChanged(object sender, EventArgs e)
        {
            if (!_loading)
            {
                LoadSelectedAgreementCharges();
            }
        }

        private void DgvCharges_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvCharges.IsCurrentCellDirty)
            {
                dgvCharges.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvCharges_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _loading)
            {
                return;
            }

            RentChargeListItem item = dgvCharges.Rows[e.RowIndex].DataBoundItem as RentChargeListItem;
            if (item == null)
            {
                return;
            }

            if (dgvCharges.Columns[e.ColumnIndex].Name == "Selected")
            {
                item.AllocationAmount = item.Selected ? item.BalanceAmount : 0m;
                _chargeSource.ResetItem(e.RowIndex);
            }

            RecalculatePaymentAmount();
        }

        private void DgvCharges_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            RentChargeListItem item = dgvCharges.Rows[e.RowIndex].DataBoundItem as RentChargeListItem;
            if (item == null)
            {
                return;
            }

            if (item.AllocationAmount < 0)
            {
                item.AllocationAmount = 0;
            }

            if (item.AllocationAmount > item.BalanceAmount)
            {
                item.AllocationAmount = item.BalanceAmount;
            }

            item.Selected = item.AllocationAmount > 0;
            _chargeSource.ResetItem(e.RowIndex);
            RecalculatePaymentAmount();
        }

        private void BtnAllocateOldest_Click(object sender, EventArgs e)
        {
            decimal requestedAmount = numPaymentAmount.Value;
            if (requestedAmount <= 0)
            {
                ShowStatus("Enter the amount received before allocating it.", false);
                return;
            }

            IList<RentChargeListItem> charges = GetBoundList<RentChargeListItem>(_chargeSource);
            decimal remaining = requestedAmount;

            foreach (RentChargeListItem charge in charges.OrderBy(item => item.DueDate).ThenBy(item => item.ChargeId))
            {
                decimal allocation = Math.Min(charge.BalanceAmount, remaining);
                charge.AllocationAmount = allocation;
                charge.Selected = allocation > 0;
                remaining -= allocation;

                if (remaining <= 0)
                {
                    break;
                }
            }

            if (remaining > 0)
            {
                foreach (RentChargeListItem charge in charges)
                {
                    charge.Selected = false;
                    charge.AllocationAmount = 0;
                }

                _chargeSource.ResetBindings(false);
                ShowStatus("The amount exceeds the selected agreement's outstanding charges. Overpayments are not supported.", false);
                return;
            }

            _chargeSource.ResetBindings(false);
            RecalculatePaymentAmount();
            ShowStatus("Amount allocated to the oldest outstanding charges.", true);
        }

        private void BtnClearAllocation_Click(object sender, EventArgs e)
        {
            ResetPaymentEntry(true);
            ShowStatus("Payment entry cleared.", true);
        }

        private void BtnPostPayment_Click(object sender, EventArgs e)
        {
            PaymentAgreementItem agreement = GetSelectedAgreement();
            if (agreement == null)
            {
                ShowStatus("Select an agreement before posting a payment.", false);
                return;
            }

            List<PaymentAllocationRequest> allocations = GetBoundList<RentChargeListItem>(_chargeSource)
                .Where(item => item.Selected && item.AllocationAmount > 0)
                .Select(item => new PaymentAllocationRequest { ChargeId = item.ChargeId, Amount = item.AllocationAmount })
                .ToList();

            decimal total = allocations.Sum(item => item.Amount);
            if (total <= 0)
            {
                ShowStatus("Select at least one outstanding charge and enter the amount to allocate.", false);
                return;
            }

            PostPaymentRequest request = new PostPaymentRequest
            {
                RequestId = _currentRequestId,
                TenantId = agreement.TenantId,
                AgreementId = agreement.AgreementId,
                PaymentDate = dtpPaymentDate.Value.Date,
                Amount = total,
                CurrencyCode = GetCurrency(),
                PaymentMethod = cmbPaymentMethod.SelectedItem == null ? string.Empty : cmbPaymentMethod.SelectedItem.ToString(),
                ExternalReference = txtExternalReference.Text,
                Remarks = txtRemarks.Text,
                Allocations = allocations
            };

            string confirmation =
                "Post " + FormatMoney(total, request.CurrencyCode) + " for " + agreement.TenantName + "?" + Environment.NewLine +
                "Agreement: " + agreement.AgreementNo + Environment.NewLine +
                "Charges: " + allocations.Count + Environment.NewLine +
                "Method: " + request.PaymentMethod + Environment.NewLine + Environment.NewLine +
                "Posted receipts cannot be edited. Corrections require an administrator reversal.";

            if (MessageBox.Show(confirmation, "Confirm Payment", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            btnPostPayment.Enabled = false;
            try
            {
                ServiceResult<PostPaymentResult> result = _paymentService.PostPayment(request);
                if (!result.IsSuccess)
                {
                    ShowStatus(result.Message, false);
                    return;
                }

                _currentRequestId = Guid.NewGuid();
                ShowStatus(result.Message, true);
                LoadAllData();
                ShowStoredReceipt(result.Data.PaymentId);
            }
            finally
            {
                btnPostPayment.Enabled = _chargeSource.Count > 0;
            }
        }

        private void BtnGenerateCharges_Click(object sender, EventArgs e)
        {
            DateTime period = new DateTime(dtpBillingPeriod.Value.Year, dtpBillingPeriod.Value.Month, 1);
            if (MessageBox.Show("Generate any missing monthly rent charges for " + period.ToString("MMMM yyyy") + "?", "Generate Rent Charges", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            ServiceResult<ChargeGenerationResult> result = _paymentService.GenerateMonthlyCharges(period);
            ShowStatus(result.Message, result.IsSuccess);
            if (result.IsSuccess)
            {
                LoadAllData();
            }
        }

        private void BtnFilterDues_Click(object sender, EventArgs e)
        {
            LoadDues();
        }

        private void DgvDues_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            RentChargeListItem charge = dgvDues.Rows[e.RowIndex].DataBoundItem as RentChargeListItem;
            if (charge == null || charge.BalanceAmount <= 0)
            {
                return;
            }

            tabMain.SelectedTab = tabCollect;
            SelectAgreement(charge.AgreementId);
        }

        private void BtnFilterHistory_Click(object sender, EventArgs e)
        {
            LoadPaymentHistory();
        }

        private void DgvPayments_SelectionChanged(object sender, EventArgs e)
        {
            UpdatePaymentActionState();
        }

        private void BtnViewPayment_Click(object sender, EventArgs e)
        {
            PaymentListItem payment = GetSelectedPayment();
            if (payment == null)
            {
                ShowStatus("Select a payment to view.", false);
                return;
            }

            ShowStoredReceipt(payment.PaymentId);
        }

        private void BtnReversePayment_Click(object sender, EventArgs e)
        {
            PaymentListItem payment = GetSelectedPayment();
            if (payment == null)
            {
                ShowStatus("Select a payment to reverse.", false);
                return;
            }

            if (payment.Status != PaymentStatuses.Posted)
            {
                ShowStatus("Only a posted payment can be reversed.", false);
                return;
            }

            string reason = PromptForReversalReason(payment);
            if (reason == null)
            {
                return;
            }

            ReversePaymentRequest request = new ReversePaymentRequest
            {
                PaymentId = payment.PaymentId,
                Reason = reason
            };

            ServiceResult<PostPaymentResult> result = _paymentService.ReversePayment(request);
            ShowStatus(result.Message, result.IsSuccess);
            if (result.IsSuccess)
            {
                LoadAllData();
            }
        }

        private void CmbPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool requiresReference = cmbPaymentMethod.SelectedItem != null && cmbPaymentMethod.SelectedItem.ToString() != PaymentMethods.Cash;
            txtExternalReference.Enabled = requiresReference;
            if (!requiresReference)
            {
                txtExternalReference.Clear();
            }
        }

        private void RecalculatePaymentAmount()
        {
            decimal total = GetBoundList<RentChargeListItem>(_chargeSource)
                .Where(item => item.Selected && item.AllocationAmount > 0)
                .Sum(item => item.AllocationAmount);

            if (total <= numPaymentAmount.Maximum)
            {
                numPaymentAmount.Value = total;
            }
        }

        private void ResetPaymentEntry(bool clearAmount)
        {
            foreach (RentChargeListItem charge in GetBoundList<RentChargeListItem>(_chargeSource))
            {
                charge.Selected = false;
                charge.AllocationAmount = 0;
            }

            _chargeSource.ResetBindings(false);
            if (clearAmount)
            {
                numPaymentAmount.Value = 0;
            }

            dtpPaymentDate.Value = DateTime.Today;
            cmbPaymentMethod.SelectedItem = PaymentMethods.Cash;
            txtExternalReference.Clear();
            txtRemarks.Clear();
            _currentRequestId = Guid.NewGuid();
        }

        private void ClearAgreementContext()
        {
            lblAgreementContext.Text = "Select an active agreement or an agreement with an outstanding balance.";
            ResetPaymentEntry(true);
            SetCollectionActionsEnabled(false);
        }

        private void SetCollectionActionsEnabled(bool enabled)
        {
            numPaymentAmount.Enabled = enabled;
            dtpPaymentDate.Enabled = enabled;
            cmbPaymentMethod.Enabled = enabled;
            txtRemarks.Enabled = enabled;
            btnAllocateOldest.Enabled = enabled;
            btnClearAllocation.Enabled = enabled;
            btnPostPayment.Enabled = enabled;

            bool requiresReference = enabled
                && cmbPaymentMethod.SelectedItem != null
                && cmbPaymentMethod.SelectedItem.ToString() != PaymentMethods.Cash;
            txtExternalReference.Enabled = requiresReference;
        }

        private void SelectAgreement(int agreementId)
        {
            if (TrySelectAgreement(agreementId))
            {
                return;
            }

            txtAgreementSearch.Clear();
            LoadAgreementContexts();
            if (!TrySelectAgreement(agreementId))
            {
                ShowStatus("The related agreement is no longer available for collection.", false);
            }
        }

        private bool TrySelectAgreement(int agreementId)
        {
            for (int i = 0; i < dgvAgreements.Rows.Count; i++)
            {
                PaymentAgreementItem item = dgvAgreements.Rows[i].DataBoundItem as PaymentAgreementItem;
                if (item == null || item.AgreementId != agreementId)
                {
                    continue;
                }

                dgvAgreements.ClearSelection();
                dgvAgreements.Rows[i].Selected = true;
                dgvAgreements.CurrentCell = dgvAgreements.Rows[i].Cells[0];
                LoadSelectedAgreementCharges();
                return true;
            }

            return false;
        }

        private PaymentAgreementItem GetSelectedAgreement()
        {
            return dgvAgreements.CurrentRow == null ? null : dgvAgreements.CurrentRow.DataBoundItem as PaymentAgreementItem;
        }

        private PaymentListItem GetSelectedPayment()
        {
            return dgvPayments.CurrentRow == null ? null : dgvPayments.CurrentRow.DataBoundItem as PaymentListItem;
        }

        private void UpdatePaymentActionState()
        {
            PaymentListItem selected = GetSelectedPayment();
            btnViewPayment.Enabled = selected != null;
            btnReversePayment.Enabled = CurrentSession.IsAdmin && selected != null && selected.Status == PaymentStatuses.Posted;
        }

        private void ShowStoredReceipt(long paymentId)
        {
            ServiceResult<PaymentListItem> paymentResult = _paymentService.GetPaymentById(paymentId);
            if (!paymentResult.IsSuccess)
            {
                ShowStatus(paymentResult.Message, false);
                return;
            }

            ServiceResult<List<PaymentAllocationDetail>> allocationResult = _paymentService.GetPaymentAllocations(paymentId);
            if (!allocationResult.IsSuccess)
            {
                ShowStatus(allocationResult.Message, false);
                return;
            }

            using (FrmPaymentReceipt receipt = new FrmPaymentReceipt(paymentResult.Data, allocationResult.Data, _paymentService.GetReceiptFooter()))
            {
                receipt.ShowDialog(this);
            }
        }

        private static string PromptForReversalReason(PaymentListItem payment)
        {
            using (Form dialog = new Form())
            using (TextBox input = new TextBox())
            using (Button confirm = new Button())
            using (Button cancel = new Button())
            {
                dialog.Text = "Reverse " + payment.ReceiptNo;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new Size(460, 190);
                dialog.Font = new Font("Segoe UI", 9F);

                Label label = new Label
                {
                    AutoSize = false,
                    Location = new Point(16, 14),
                    Size = new Size(428, 44),
                    Text = "Enter the reason for reversing " + FormatMoney(payment.Amount, payment.CurrencyCode) + ". This action preserves the receipt history."
                };

                input.Location = new Point(16, 62);
                input.Size = new Size(428, 62);
                input.Multiline = true;
                input.MaxLength = 500;

                confirm.Text = "Reverse Payment";
                confirm.DialogResult = DialogResult.OK;
                confirm.Location = new Point(292, 140);
                confirm.Size = new Size(152, 34);
                confirm.BackColor = Color.Firebrick;
                confirm.ForeColor = Color.White;
                confirm.FlatStyle = FlatStyle.Flat;

                cancel.Text = "Cancel";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Location = new Point(194, 140);
                cancel.Size = new Size(88, 34);

                dialog.Controls.Add(label);
                dialog.Controls.Add(input);
                dialog.Controls.Add(confirm);
                dialog.Controls.Add(cancel);
                dialog.AcceptButton = confirm;
                dialog.CancelButton = cancel;

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return null;
                }

                string reason = input.Text.Trim();
                if (reason.Length == 0)
                {
                    MessageBox.Show("A reversal reason is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                return reason;
            }
        }

        private string GetCurrency()
        {
            return _paymentService.GetDefaultCurrency();
        }

        private static string FormatMoney(decimal amount, string currency)
        {
            return (currency ?? "USD") + " " + amount.ToString("N2");
        }

        private static IList<T> GetBoundList<T>(BindingSource source)
        {
            BindingList<T> list = source.DataSource as BindingList<T>;
            if (list == null)
            {
                return new List<T>();
            }

            return list;
        }

        private void ShowStatus(string message, bool success)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = success ? Color.ForestGreen : Color.Firebrick;
        }

        private void ShowInformation(string message)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = Color.DarkGoldenrod;
        }

        private static void SetCueBanner(TextBox textBox, string text)
        {
            if (textBox == null)
            {
                return;
            }

            SendMessage(textBox.Handle, EmSetCueBanner, new IntPtr(1), text);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int message, IntPtr wParam, string lParam);
    }
}
