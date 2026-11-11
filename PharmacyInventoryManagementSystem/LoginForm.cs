using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PharmacyInventoryManagementSystem;

public class LoginForm : Form
{
    private readonly TextBox txtUser = new();
    private readonly TextBox txtPass = new();
    private readonly CheckBox chkRemember = new();
    private readonly Button btnLogin = AuthUi.CreateButton("SIGN IN");

    private static readonly string SessionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.txt");

    public string LoggedInName { get; private set; } = "";
    public string LoggedInRole { get; private set; } = "";

    public LoginForm()
    {
        Text = "PharmaCare | Sign In";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(920, 600);
        Font = new Font("Segoe UI", 10);
        BackColor = Color.White;

        BuildUI();

        Shown += (_, _) =>
        {
            if (string.IsNullOrEmpty(txtUser.Text))
                txtUser.Focus();
            else
                txtPass.Focus();
        };
    }

    private void BuildUI()
    {
        FlowLayoutPanel content = AuthUi.CreatePage(
            this,
            "Welcome Back",
            "Sign in to access your pharmacy inventory.");

        txtUser.PlaceholderText = "Enter your username";
        txtPass.PlaceholderText = "Enter your password";

        AuthUi.AddField(content, "USERNAME", txtUser, false, 0);
        AuthUi.AddField(content, "PASSWORD", txtPass, true, 1);

        var optionsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 20)
        };

        var chkShow = new CheckBox
        {
            Text = "Show password",
            AutoSize = true,
            Margin = new Padding(0, 0, 25, 0),
            TabIndex = 2
        };

        chkShow.CheckedChanged += (_, _) =>
            txtPass.UseSystemPasswordChar = !chkShow.Checked;

        chkRemember.Text = "Keep me signed in";
        chkRemember.AutoSize = true;
        chkRemember.TabIndex = 3;

        optionsPanel.Controls.Add(chkShow);
        optionsPanel.Controls.Add(chkRemember);
        content.Controls.Add(optionsPanel);

        btnLogin.TabIndex = 4;
        btnLogin.Click += (_, _) => SignIn();
        content.Controls.Add(btnLogin);

        content.Controls.Add(new Label
        {
            Text = "Don't have an account?",
            AutoSize = true,
            ForeColor = Color.Gray,
            Margin = new Padding(0, 16, 0, 3)
        });

        var registerLink = new LinkLabel
        {
            Text = "Register here",
            AutoSize = true,
            LinkColor = AuthUi.Teal,
            Margin = new Padding(0, 0, 0, 18),
            TabIndex = 5
        };

        registerLink.LinkClicked += (_, _) =>
        {
            using var register = new RegisterForm();

            if (register.ShowDialog(this) != DialogResult.OK)
                return;

            using var splash = new LoadingForm("Setting up your account...");
            splash.ShowDialog(this);

            txtUser.Text = register.RegisteredUsername;
            txtPass.Clear();

            MessageBox.Show(
                this,
                "Registration successful! You can now log in.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            txtPass.Focus();
        };

        content.Controls.Add(registerLink);

        content.Controls.Add(new Label
        {
            Text = "Demo: admin  |  Password: admin123",
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 8.5F),
            Margin = Padding.Empty
        });

        AcceptButton = btnLogin;
    }

    private void SaveRememberedUser()
    {
        try
        {
            if (chkRemember.Checked)
            {
                File.WriteAllLines(SessionPath, new[] { LoggedInName, LoggedInRole });
            }
            else if (File.Exists(SessionPath))
            {
                File.Delete(SessionPath);
            }
        }
        catch { }
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        ClientSize = new Size(648, 413);
        Name = "LoginForm";
        ResumeLayout(false);
    }

    private void SignIn()
    {
        if (string.IsNullOrWhiteSpace(txtUser.Text) ||
            string.IsNullOrWhiteSpace(txtPass.Text))
        {
            MessageBox.Show(
                this,
                "Please enter your username and password.",
                "Missing information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        btnLogin.Text = "Signing in...";
        btnLogin.Enabled = false;
        Cursor = Cursors.WaitCursor;
        Application.DoEvents();

        try
        {
            bool valid = Database.ValidateLogin(
                txtUser.Text,
                txtPass.Text,
                out string fullName,
                out string role);

            if (!valid)
            {
                MessageBox.Show(
                    this,
                    "Invalid username or password.",
                    "Login failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                txtPass.Clear();
                txtPass.Focus();
                return;
            }

            LoggedInName = fullName;
            LoggedInRole = role;

            SaveRememberedUser();

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                "Unable to sign in:\n" + ex.Message,
                "Database error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            btnLogin.Text = "SIGN IN";
            btnLogin.Enabled = true;
            Cursor = Cursors.Default;
        }
    }
}

internal static class AuthUi
{
    internal static readonly Color Navy = Color.FromArgb(24, 42, 75);
    internal static readonly Color Teal = Color.FromArgb(19, 145, 135);

    internal static FlowLayoutPanel CreatePage(
        Form form,
        string title,
        string subtitle)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        form.Controls.Add(root);

        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Navy,
            ColumnCount = 1,
            RowCount = 5,
            Margin = Padding.Empty,
            Padding = new Padding(15)
        };

        left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        left.Controls.Add(new Label
        {
            Text = "+",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 60, FontStyle.Bold),
            Margin = Padding.Empty
        }, 0, 1);

        left.Controls.Add(new Label
        {
            Text = "PHARMACARE",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 21, FontStyle.Bold),
            Margin = Padding.Empty
        }, 0, 2);

        left.Controls.Add(new Label
        {
            Text = "PHARMACY INVENTORY\r\nMANAGEMENT SYSTEM",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.LightSteelBlue,
            Font = new Font("Segoe UI", 10),
            Margin = Padding.Empty
        }, 0, 3);

        var right = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            AutoScroll = true,
            Margin = Padding.Empty,
            Padding = new Padding(40, 35, 30, 25)
        };

        var content = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Location = new Point(40, 35),
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        content.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            ForeColor = Navy,
            Font = new Font("Segoe UI", 25, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 6)
        });

        content.Controls.Add(new Label
        {
            Text = subtitle,
            AutoSize = true,
            MaximumSize = new Size(440, 0),
            ForeColor = Color.Gray,
            Margin = new Padding(0, 0, 0, 26)
        });

        right.Controls.Add(content);
        root.Controls.Add(left, 0, 0);
        root.Controls.Add(right, 1, 0);

        return content;
    }

    internal static void AddField(
        FlowLayoutPanel parent,
        string caption,
        TextBox box,
        bool password,
        int tabIndex)
    {
        parent.Controls.Add(new Label
        {
            Text = caption,
            AutoSize = true,
            ForeColor = Color.FromArgb(75, 85, 100),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 6)
        });

        box.Width = 440;
        box.Font = new Font("Segoe UI", 11);
        box.UseSystemPasswordChar = password;
        box.Margin = new Padding(0, 0, 0, 17);
        box.TabIndex = tabIndex;

        parent.Controls.Add(box);
    }

    internal static Button CreateButton(string text)
    {
        var button = new Button
        {
            Text = text,
            Size = new Size(440, 43),
            BackColor = Teal,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = Padding.Empty
        };

        button.FlatAppearance.BorderSize = 0;

        return button;
    }
}