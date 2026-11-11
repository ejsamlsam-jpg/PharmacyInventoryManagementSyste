using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PharmacyInventoryManagementSystem;

public class AuditLogForm : Form
{
    private readonly DataGridView grid = new();

    private readonly Color navy = Color.FromArgb(24, 42, 75);
    private readonly Color teal = Color.FromArgb(19, 145, 135);
    private readonly Color background = Color.FromArgb(244, 247, 250);

    public AuditLogForm()
    {
        Text = "PharmaCare | System Audit Logs";
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        MinimumSize = new Size(800, 500);
        ClientSize = new Size(950, 600);
        Font = new Font("Segoe UI", 9);
        BackColor = background;
        ShowIcon = false;
        ShowInTaskbar = false;

        BuildUI();
        LoadLogs();
    }

    private void BuildUI()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(20),
            Margin = Padding.Empty
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Controls.Add(root);

        var topBar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty
        };

        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); 
        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130)); 

        var title = new Label
        {
            Text = "SYSTEM AUDIT LOGS",
            Dock = DockStyle.Fill,
            ForeColor = navy,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var btnRefresh = new Button
        {
            Text = "🔄 REFRESH",
            Dock = DockStyle.Fill,
            BackColor = Color.Gainsboro,
            ForeColor = navy,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 5, 10, 10)
        };
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.Click += (_, _) => LoadLogs();

        var btnExport = new Button
        {
            Text = "📥 EXPORT",
            Dock = DockStyle.Fill,
            BackColor = teal,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 5, 0, 10)
        };
        btnExport.FlatAppearance.BorderSize = 0;
        btnExport.Click += (_, _) => ExportToCSV();

        topBar.Controls.Add(title, 0, 0);
        topBar.Controls.Add(btnRefresh, 1, 0);
        topBar.Controls.Add(btnExport, 2, 0);

        root.Controls.Add(topBar, 0, 0);

        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.EnableHeadersVisualStyles = false;

        grid.ColumnHeadersDefaultCellStyle.BackColor = navy;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        grid.ColumnHeadersHeight = 38;
        grid.RowTemplate.Height = 32;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 251);
        grid.DefaultCellStyle.SelectionBackColor = teal;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;

        root.Controls.Add(grid, 0, 1);
    }

    private void LoadLogs()
    {
        try
        {
            var table = new DataTable();
            using var con = Database.GetConnection();
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = """
                SELECT 
                    Timestamp AS 'Date & Time', 
                    Username AS 'User', 
                    Action, 
                    Details 
                FROM AuditLogs 
                ORDER BY Id DESC;
                """;

            using var reader = cmd.ExecuteReader();
            table.Load(reader);

            grid.DataSource = table;

            if (grid.Columns.Count > 0)
            {
                grid.Columns["Date & Time"].FillWeight = 120;
                grid.Columns["User"].FillWeight = 100;
                grid.Columns["Action"].FillWeight = 80;
                grid.Columns["Details"].FillWeight = 300;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"Failed to load audit logs:\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ExportToCSV()
    {
        if (grid.Rows.Count == 0)
        {
            MessageBox.Show(this, "No audit logs to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv",
            FileName = $"AuditLogs_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
            Title = "Export Audit Logs"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                using var sw = new StreamWriter(sfd.FileName);

                var headers = grid.Columns.Cast<DataGridViewColumn>()
                                          .Where(c => c.Visible)
                                          .Select(c => $"\"{c.HeaderText}\"");
                sw.WriteLine(string.Join(",", headers));

                foreach (DataGridViewRow row in grid.Rows)
                {
                    var cells = row.Cells.Cast<DataGridViewCell>()
                                         .Where(c => c.OwningColumn.Visible)
                                         .Select(c =>
                                         {
                                             var cellValue = c.Value?.ToString() ?? "";
                                             return $"\"{cellValue.Replace("\"", "\"\"")}\"";
                                         });
                    sw.WriteLine(string.Join(",", cells));
                }

                MessageBox.Show(this, "Audit logs successfully exported!", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error exporting data: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}