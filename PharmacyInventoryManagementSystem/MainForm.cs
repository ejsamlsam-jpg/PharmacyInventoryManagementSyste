using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PharmacyInventoryManagementSystem;

public class MainForm : Form
{
    private readonly TextBox txtName = new();
    private readonly TextBox txtBrand = new();
    private readonly TextBox txtSupplier = new();
    private readonly TextBox txtSearch = new();

    private readonly ComboBox cmbCategory = new();
    private readonly ComboBox cmbCategoryFilter = new();
    private readonly NumericUpDown numQty = new();
    private readonly NumericUpDown numPrice = new();
    private readonly DateTimePicker dtExpiry = new();

    private readonly DataGridView grid = new();

    private readonly Label lblTotal = new();
    private readonly Label lblLow = new();
    private readonly Label lblExpired = new();
    private readonly Label lblItemCount = new();

    private readonly Color navy = Color.FromArgb(24, 42, 75);
    private readonly Color teal = Color.FromArgb(19, 145, 135);
    private readonly Color background = Color.FromArgb(244, 247, 250);

    private int selectedId;
    private readonly string currentUser;

    public bool LogoutRequested { get; private set; }

    public MainForm(
        string loggedInName = "Administrator",
        string role = "Admin")
    {
        currentUser = loggedInName;

        Text = $"PharmaCare | Inventory Management System ({role})";
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        MinimumSize = new Size(1100, 720);
        ClientSize = new Size(1280, 780);
        Font = new Font("Segoe UI", 9);
        BackColor = background;

        BuildUI(loggedInName, role);

        Shown += (_, _) => RunSafely(() => LoadMedicines());
    }

    private void BuildUI(string user, string role)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = background,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        root.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Controls.Add(root);

        root.Controls.Add(BuildHeader(user, role), 0, 0);
        root.Controls.Add(BuildStats(), 0, 1);

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(18, 0, 18, 18),
            Margin = Padding.Empty
        };

        body.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 340));

        body.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        body.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(body, 0, 2);

        body.Controls.Add(BuildEditor(), 0, 0);
        body.Controls.Add(BuildInventory(), 1, 0);
    }

    private Control BuildHeader(string user, string role)
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4, 
            RowCount = 1,
            BackColor = navy,
            Padding = new Padding(22, 10, 22, 10),
            Margin = Padding.Empty
        };

        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, role == "Admin" ? 140 : 0));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

        header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var branding = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };

        branding.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        branding.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

        branding.Controls.Add(new Label
        {
            Text = "PHARMACARE",
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 19, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);

        branding.Controls.Add(new Label
        {
            Text = "PHARMACY INVENTORY MANAGEMENT SYSTEM",
            Dock = DockStyle.Fill,
            ForeColor = Color.LightSteelBlue,
            Font = new Font("Segoe UI", 8),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 1);

        var signedIn = new Label
        {
            Text = $"Signed in: {user} ({role})",
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleRight,
            AutoEllipsis = true,
            Margin = new Padding(5, 0, 15, 0)
        };

        if (role == "Admin")
        {
            var btnAudit = MakeButton("📜 AUDIT LOGS", teal, Color.White);
            btnAudit.Anchor = AnchorStyles.None;
            btnAudit.Size = new Size(130, 36);
            btnAudit.Click += (_, _) =>
            {
                using var auditForm = new AuditLogForm();
                auditForm.ShowDialog(this);
            };
            header.Controls.Add(btnAudit, 2, 0);
        }

        var logout = MakeButton("🚪 LOG OUT", Color.FromArgb(55, 75, 105), Color.White);
        logout.Anchor = AnchorStyles.None;
        logout.Size = new Size(100, 36);

        logout.Click += (_, _) =>
        {
            DialogResult answer = MessageBox.Show(
                this,
                "Do you want to log out?",
                "Log out",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
                return;

            LogoutRequested = true;
            Close();
        };

        header.Controls.Add(branding, 0, 0);
        header.Controls.Add(signedIn, 1, 0);
        header.Controls.Add(logout, 3, 0);

        return header;
    }

    private Control BuildStats()
    {
        var stats = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(18, 12, 18, 12),
            Margin = Padding.Empty
        };

        for (int i = 0; i < 3; i++)
        {
            stats.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F / 3));
        }

        stats.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        stats.Controls.Add(
            CreateStat("TOTAL MEDICINES", lblTotal, navy), 0, 0);

        stats.Controls.Add(
            CreateStat("LOW STOCK (10 OR FEWER)", lblLow,
                Color.DarkGoldenrod), 1, 0);

        stats.Controls.Add(
            CreateStat("EXPIRED MEDICINES", lblExpired,
                Color.Firebrick), 2, 0);

        return stats;
    }

    private Control CreateStat(
        string title,
        Label value,
        Color accent)
    {
        var card = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            ColumnCount = 2,
            RowCount = 2,
            Margin = new Padding(0, 0, 10, 0)
        };

        card.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 6));

        card.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        card.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 27));

        card.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        var stripe = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = accent,
            Margin = Padding.Empty
        };

        card.Controls.Add(stripe, 0, 0);
        card.SetRowSpan(stripe, 2);

        card.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            TextAlign = ContentAlignment.BottomLeft,
            Margin = new Padding(12, 0, 4, 0)
        }, 1, 0);

        value.Text = "0";
        value.Dock = DockStyle.Fill;
        value.ForeColor = navy;
        value.Font = new Font("Segoe UI", 21, FontStyle.Bold);
        value.Margin = new Padding(12, 0, 4, 0);

        card.Controls.Add(value, 1, 1);

        return card;
    }

    private Control BuildEditor()
    {
        var host = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            AutoScroll = true,
            Margin = new Padding(0, 0, 14, 0)
        };

        var fields = new FlowLayoutPanel
        {
            Location = new Point(16, 16),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = Padding.Empty
        };

        host.Controls.Add(fields);

        fields.Controls.Add(new Label
        {
            Text = "MEDICINE DETAILS",
            AutoSize = true,
            ForeColor = navy,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 18)
        });

        AddEditorField(fields, "Medicine Name *", txtName);

        cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCategory.Items.AddRange(new object[]
        {
            "Tablet",
            "Capsule",
            "Syrup",
            "Ointment / Cream",
            "Drops",
            "Medical Supply",
            "Other"
        });
        cmbCategory.SelectedIndex = 0;

        AddEditorField(fields, "Category *", cmbCategory);
        AddEditorField(fields, "Brand", txtBrand);

        numQty.Maximum = 100000;
        numQty.Minimum = 0;
        numQty.DecimalPlaces = 0;

        AddEditorField(fields, "Quantity *", numQty);

        numPrice.Maximum = 1000000;
        numPrice.Minimum = 0;
        numPrice.DecimalPlaces = 2;
        numPrice.ThousandsSeparator = true;

        AddEditorField(fields, "Unit Price (PHP) *", numPrice);

        dtExpiry.Format = DateTimePickerFormat.Short;
        dtExpiry.Value = DateTime.Today;

        AddEditorField(fields, "Expiration Date *", dtExpiry);
        AddEditorField(fields, "Supplier", txtSupplier);

        var buttons = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 2,
            Size = new Size(280, 88),
            Margin = new Padding(0, 4, 0, 18)
        };

        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        var add = MakeButton("➕ ADD", teal, Color.White);
        var update = MakeButton("💾 UPDATE", navy, Color.White);
        var clear = MakeButton("🧹 CLEAR", Color.Gainsboro, navy);
        var delete = MakeButton("🗑️ DELETE", Color.Firebrick, Color.White);

        add.Click += (_, _) => RunSafely(AddMedicine);
        update.Click += (_, _) => RunSafely(UpdateMedicine);
        clear.Click += (_, _) => ClearForm();
        delete.Click += (_, _) => RunSafely(DeleteMedicine);

        buttons.Controls.Add(add, 0, 0);
        buttons.Controls.Add(update, 1, 0);
        buttons.Controls.Add(clear, 0, 1);
        buttons.Controls.Add(delete, 1, 1);

        fields.Controls.Add(buttons);

        return host;
    }

    private static void AddEditorField(
        FlowLayoutPanel parent,
        string caption,
        Control input)
    {
        parent.Controls.Add(new Label
        {
            Text = caption,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4)
        });

        input.Width = 280;
        input.Margin = new Padding(0, 0, 0, 12);

        parent.Controls.Add(input);
    }

    private Control BuildInventory()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(16),
            Margin = Padding.Empty
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

        panel.Controls.Add(new Label
        {
            Text = "MEDICINE INVENTORY",
            Dock = DockStyle.Fill,
            ForeColor = navy,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 0, 0);

        var searchBar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = Padding.Empty
        };

        searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));

        txtSearch.PlaceholderText = "Search medicine, brand, or supplier...";
        txtSearch.Dock = DockStyle.Fill;
        txtSearch.Margin = new Padding(0, 7, 8, 7);
        txtSearch.TextChanged += (_, _) => FilterInventory();

        cmbCategoryFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCategoryFilter.Items.Add("All Categories");
        cmbCategoryFilter.Items.AddRange(new object[]
        {
            "Tablet",
            "Capsule",
            "Syrup",
            "Ointment / Cream",
            "Drops",
            "Medical Supply",
            "Other"
        });
        cmbCategoryFilter.SelectedIndex = 0;
        cmbCategoryFilter.Dock = DockStyle.Fill;
        cmbCategoryFilter.Margin = new Padding(0, 8, 8, 7);
        cmbCategoryFilter.SelectedIndexChanged += (_, _) => FilterInventory();

        var refresh = MakeButton("🔄 REFRESH", Color.Gainsboro, navy);
        refresh.Margin = new Padding(0, 3, 8, 7);
        refresh.Click += (_, _) =>
        {
            txtSearch.Clear();
            cmbCategoryFilter.SelectedIndex = 0;
            LoadMedicines();
        };

        var export = MakeButton("📥 EXPORT", teal, Color.White);
        export.Margin = new Padding(0, 3, 0, 7);
        export.Click += (_, _) => RunSafely(ExportToCSV);

        searchBar.Controls.Add(txtSearch, 0, 0);
        searchBar.Controls.Add(cmbCategoryFilter, 1, 0);
        searchBar.Controls.Add(refresh, 2, 0);
        searchBar.Controls.Add(export, 3, 0);

        panel.Controls.Add(searchBar, 0, 1);

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
        grid.ColumnHeadersDefaultCellStyle.Font =
            new Font("Segoe UI", 9, FontStyle.Bold);

        grid.ColumnHeadersHeight = 38;
        grid.RowTemplate.Height = 32;
        grid.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(245, 248, 251);

        grid.DefaultCellStyle.SelectionBackColor = teal;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;

        grid.CellClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
                RunSafely(SelectRow);
        };

        grid.Paint += Grid_Paint;

        panel.Controls.Add(grid, 0, 2);

        var footerPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty
        };
        footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));

        var hintLabel = new Label
        {
            Text = "Select a row to edit or delete. Low stock: 10 units or fewer.",
            Dock = DockStyle.Fill,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 8.5F),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        lblItemCount.Text = "Showing 0 items";
        lblItemCount.Dock = DockStyle.Fill;
        lblItemCount.ForeColor = Color.Gray;
        lblItemCount.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblItemCount.TextAlign = ContentAlignment.MiddleRight;

        footerPanel.Controls.Add(hintLabel, 0, 0);
        footerPanel.Controls.Add(lblItemCount, 1, 0);

        panel.Controls.Add(footerPanel, 0, 3);

        return panel;
    }

    private void Grid_Paint(object? sender, PaintEventArgs e)
    {
        if (grid.Rows.Count == 0)
        {
            string message = "🔍 No medicine inventory records found.\nAdd a new medicine using the form on the left or clear your filters.";
            using Font font = new("Segoe UI", 10, FontStyle.Italic);
            using Brush brush = new SolidBrush(Color.Gray);
            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            e.Graphics.DrawString(message, font, brush, grid.ClientRectangle, format);
        }
    }

    private static Button MakeButton(
        string text,
        Color back,
        Color fore)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            BackColor = back,
            ForeColor = fore,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(3)
        };

        button.FlatAppearance.BorderSize = 0;

        return button;
    }

    private void RunSafely(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                "Unable to complete the operation:\n" + ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private bool ValidateMedicine()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show(this, "Enter the medicine name.");
            txtName.Focus();
            return false;
        }

        if (cmbCategory.SelectedIndex < 0)
        {
            MessageBox.Show(this, "Select a category.");
            return false;
        }

        if (numPrice.Value <= 0)
        {
            MessageBox.Show(this, "Unit price must be greater than zero.");
            numPrice.Focus();
            return false;
        }

        return true;
    }

    private void AddParameters(SqliteCommand command)
    {
        command.Parameters.AddWithValue("$name", txtName.Text.Trim());
        command.Parameters.AddWithValue("$cat", cmbCategory.Text);
        command.Parameters.AddWithValue("$brand", txtBrand.Text.Trim());
        command.Parameters.AddWithValue("$qty", (int)numQty.Value);
        command.Parameters.AddWithValue("$price", (double)numPrice.Value);

        command.Parameters.AddWithValue(
            "$expiry",
            dtExpiry.Value.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture));

        command.Parameters.AddWithValue(
            "$supplier",
            txtSupplier.Text.Trim());
    }

    private void LogActivity(string action, string details)
    {
        using var con = Database.GetConnection();
        con.Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = """
            INSERT INTO AuditLogs (Timestamp, Username, Action, Details) 
            VALUES ($timestamp, $user, $action, $details);
            """;
        cmd.Parameters.AddWithValue("$timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("$user", currentUser);
        cmd.Parameters.AddWithValue("$action", action);
        cmd.Parameters.AddWithValue("$details", details);
        cmd.ExecuteNonQuery();
    }

    private void AddMedicine()
    {
        if (!ValidateMedicine())
            return;

        using (var con = Database.GetConnection())
        {
            con.Open();

            using var command = con.CreateCommand();

            command.CommandText = """
                INSERT INTO Medicines (
                    MedicineName, Category, Brand, Quantity,
                    UnitPrice, ExpiryDate, Supplier, DateAdded
                )
                VALUES (
                    $name, $cat, $brand, $qty,
                    $price, $expiry, $supplier, $added
                );
                """;

            AddParameters(command);

            command.Parameters.AddWithValue(
                "$added",
                DateTime.Today.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture));

            command.ExecuteNonQuery();
            LogActivity("ADD", $"Added medicine: {txtName.Text.Trim()}");
        }

        ClearForm();
        LoadMedicines();

        MessageBox.Show(this, "Medicine added successfully.");
    }

    private void UpdateMedicine()
    {
        if (selectedId == 0)
        {
            MessageBox.Show(this, "Select a medicine first.");
            return;
        }

        if (!ValidateMedicine())
            return;

        int affected;

        using (var con = Database.GetConnection())
        {
            con.Open();

            using var command = con.CreateCommand();

            command.CommandText = """
                UPDATE Medicines
                SET MedicineName = $name,
                    Category = $cat,
                    Brand = $brand,
                    Quantity = $qty,
                    UnitPrice = $price,
                    ExpiryDate = $expiry,
                    Supplier = $supplier
                WHERE Id = $id;
                """;

            AddParameters(command);
            command.Parameters.AddWithValue("$id", selectedId);

            affected = command.ExecuteNonQuery();
            if (affected > 0)
            {
                LogActivity("UPDATE", $"Updated medicine: {txtName.Text.Trim()} (ID: {selectedId})");
            }
        }

        ClearForm();
        LoadMedicines();

        MessageBox.Show(
            this,
            affected > 0
                ? "Medicine updated successfully."
                : "Medicine no longer exists.");
    }

    private void DeleteMedicine()
    {
        if (selectedId == 0)
        {
            MessageBox.Show(this, "Select a medicine first.");
            return;
        }

        string targetMedicine = txtName.Text.Trim();

        DialogResult answer = MessageBox.Show(
            this,
            $"Delete '{targetMedicine}'?",
            "Confirm delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (answer != DialogResult.Yes)
            return;

        int affected;

        using (var con = Database.GetConnection())
        {
            con.Open();

            using var command = con.CreateCommand();

            command.CommandText =
                "DELETE FROM Medicines WHERE Id = $id;";

            command.Parameters.AddWithValue("$id", selectedId);

            affected = command.ExecuteNonQuery();
            if (affected > 0)
            {
                LogActivity("DELETE", $"Deleted medicine: {targetMedicine} (ID: {selectedId})");
            }
        }

        ClearForm();
        LoadMedicines();

        MessageBox.Show(
            this,
            affected > 0
                ? "Medicine deleted successfully."
                : "Medicine no longer exists.");
    }

    private void LoadMedicines()
    {
        var table = new DataTable();

        using (var con = Database.GetConnection())
        {
            con.Open();

            using var command = con.CreateCommand();

            command.CommandText = """
                SELECT
                    Id,
                    MedicineName,
                    Category,
                    Brand,
                    Quantity,
                    UnitPrice,
                    ExpiryDate,
                    Supplier
                FROM Medicines
                ORDER BY MedicineName;
                """;

            using var reader = command.ExecuteReader();
            table.Load(reader);
        }

        var previousTable = grid.DataSource as DataTable;
        grid.DataSource = table;
        previousTable?.Dispose();

        ConfigureGridColumns();
        FilterInventory();
        UpdateStats();
    }

    private void ConfigureGridColumns()
    {
        if (grid.Columns["Id"] is DataGridViewColumn idColumn)
            idColumn.Visible = false;

        if (grid.Columns["MedicineName"] is DataGridViewColumn nameColumn)
        {
            nameColumn.HeaderText = "Medicine";
            nameColumn.FillWeight = 140;
        }

        if (grid.Columns["Quantity"] is DataGridViewColumn qtyColumn)
            qtyColumn.HeaderText = "Qty";

        if (grid.Columns["UnitPrice"] is DataGridViewColumn priceColumn)
        {
            priceColumn.HeaderText = "Price (PHP)";
            priceColumn.DefaultCellStyle.Format = "N2";
        }

        if (grid.Columns["ExpiryDate"] is DataGridViewColumn expiryColumn)
            expiryColumn.HeaderText = "Expiry";
    }

    private void FilterInventory()
    {
        if (grid.DataSource is not DataTable table) return;

        string searchText = txtSearch.Text.Trim().ToLower();
        string selectedCategory = cmbCategoryFilter.SelectedIndex > 0 ? cmbCategoryFilter.SelectedItem?.ToString() ?? "All" : "All";

        var dv = table.DefaultView;
        string filterStr = "";

        if (!string.IsNullOrEmpty(searchText))
        {
            filterStr += $"(MedicineName LIKE '%{searchText}%' OR Brand LIKE '%{searchText}%' OR Supplier LIKE '%{searchText}%' OR Category LIKE '%{searchText}%')";
        }

        if (selectedCategory != "All")
        {
            if (!string.IsNullOrEmpty(filterStr)) filterStr += " AND ";
            filterStr += $"Category = '{selectedCategory}'";
        }

        dv.RowFilter = filterStr;

        foreach (DataGridViewRow row in grid.Rows)
        {
            string expiryText = Convert.ToString(row.Cells["ExpiryDate"].Value) ?? "";
            int.TryParse(row.Cells["Quantity"].Value?.ToString(), out int qty);

            bool isExpired = DateTime.TryParseExact(
                expiryText,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime expiry) && expiry.Date < DateTime.Today;

            if (isExpired)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                row.DefaultCellStyle.ForeColor = Color.DarkRed;
            }
            else if (qty <= 10)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 230);
                row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 100, 0);
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = Color.Black;
            }
        }

        lblItemCount.Text = $"Showing {dv.Count} of {table.Rows.Count} items";
    }

    private void ExportToCSV()
    {
        if (grid.Rows.Count == 0)
        {
            MessageBox.Show(this, "No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv",
            FileName = $"Inventory_Report_{DateTime.Now:yyyyMMdd}.csv",
            Title = "Export Inventory Data"
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

                MessageBox.Show(this, "Data successfully exported!", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error exporting data: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void UpdateStats()
    {
        using var con = Database.GetConnection();
        con.Open();

        using var command = con.CreateCommand();

        command.CommandText = """
            SELECT
                COUNT(*),
                COALESCE(
                    SUM(CASE WHEN Quantity <= 10 THEN 1 ELSE 0 END),
                    0
                ),
                COALESCE(
                    SUM(
                        CASE
                            WHEN date(ExpiryDate) < date($today)
                            THEN 1
                            ELSE 0
                        END
                    ),
                    0
                )
            FROM Medicines;
            """;

        command.Parameters.AddWithValue(
            "$today",
            DateTime.Today.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture));

        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            long total = reader.GetInt64(0);
            long low = reader.GetInt64(1);
            long expired = reader.GetInt64(2);

            lblTotal.Text = total.ToString();

            lblLow.Text = low.ToString();
            lblLow.ForeColor = low > 0 ? Color.DarkGoldenrod : navy;

            lblExpired.Text = expired.ToString();
            lblExpired.ForeColor = expired > 0 ? Color.Firebrick : navy;
        }
    }

    private void SelectRow()
    {
        if (grid.CurrentRow == null || grid.CurrentRow.IsNewRow)
            return;

        DataGridViewRow row = grid.CurrentRow;

        if (row.Cells["Id"].Value is null or DBNull)
            return;

        selectedId = Convert.ToInt32(row.Cells["Id"].Value);

        txtName.Text =
            Convert.ToString(row.Cells["MedicineName"].Value) ?? "";

        string category =
            Convert.ToString(row.Cells["Category"].Value) ?? "Other";

        int categoryIndex = cmbCategory.FindStringExact(category);

        cmbCategory.SelectedIndex = categoryIndex >= 0
            ? categoryIndex
            : cmbCategory.Items.IndexOf("Other");

        txtBrand.Text =
            Convert.ToString(row.Cells["Brand"].Value) ?? "";

        decimal quantity =
            Convert.ToDecimal(row.Cells["Quantity"].Value);

        decimal price =
            Convert.ToDecimal(row.Cells["UnitPrice"].Value);

        numQty.Value = Math.Clamp(
            quantity,
            numQty.Minimum,
            numQty.Maximum);

        numPrice.Value = Math.Clamp(
            price,
            numPrice.Minimum,
            numPrice.Maximum);

        string expiryText =
            Convert.ToString(row.Cells["ExpiryDate"].Value) ?? "";

        if (DateTime.TryParseExact(
            expiryText,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime expiry))
        {
            dtExpiry.Value = expiry < dtExpiry.MinDate
                ? dtExpiry.MinDate
                : expiry > dtExpiry.MaxDate
                    ? dtExpiry.MaxDate
                    : expiry;
        }
        else
        {
            dtExpiry.Value = DateTime.Today;
        }

        txtSupplier.Text =
            Convert.ToString(row.Cells["Supplier"].Value) ?? "";
    }

    private void ClearForm()
    {
        selectedId = 0;

        txtName.Clear();
        txtBrand.Clear();
        txtSupplier.Clear();

        cmbCategory.SelectedIndex = 0;
        numQty.Value = 0;
        numPrice.Value = 0;
        dtExpiry.Value = DateTime.Today;

        grid.ClearSelection();
        grid.CurrentCell = null;
    }
}