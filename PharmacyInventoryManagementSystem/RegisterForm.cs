using System;
using System.Drawing;
using System.Windows.Forms;

namespace PharmacyInventoryManagementSystem;

public class RegisterForm : Form
{
    private readonly TextBox txtFullName = new();
    private readonly TextBox txtUser = new();
    private readonly TextBox txtPass = new();
    private readonly TextBox txtConfirm = new();

    public string RegisteredUsername { get; private set; } = "";

    public RegisterForm()
    {
        Text = "PharmaCare | Create Account";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(920, 700);
        Font = new Font("Segoe UI", 10);
        BackColor = Color.White;

        BuildUI();

        Shown += (_, _) => txtFullName.Focus();
    }

    private void BuildUI()
    {
        FlowLayoutPanel content = AuthUi.CreatePage(
            this,
            "Create Account",
            "Enter your details to register as staff.");

        AuthUi.AddField(content, "FULL NAME", txtFullName, false, 0);
        AuthUi.AddField(content, "USERNAME", txtUser, false, 1);
        AuthUi.AddField(content, "PASSWORD", txtPass, true, 2);
        AuthUi.AddField(content, "CONFIRM PASSWORD", txtConfirm, true, 3);

        var chkShow = new CheckBox
        {
            Text = "Show passwords",
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20),
            TabIndex = 4
        };

        chkShow.CheckedChanged += (_, _) =>
        {
            txtPass.UseSystemPasswordChar = !chkShow.Checked;
            txtConfirm.UseSystemPasswordChar = !chkShow.Checked;
        };

        content.Controls.Add(chkShow);

        var btnRegister = AuthUi.CreateButton("CREATE ACCOUNT");
        btnRegister.TabIndex = 5;
        btnRegister.Click += (_, _) => Register();

        content.Controls.Add(btnRegister);

        var back = new LinkLabel
        {
            Text = "Back to sign in",
            AutoSize = true,
            LinkColor = AuthUi.Teal,
            Margin = new Padding(0, 18, 0, 0),
            TabIndex = 6
        };

        back.LinkClicked += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        content.Controls.Add(back);

        AcceptButton = btnRegister;
    }

    private void Register()
    {
        if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
            string.IsNullOrWhiteSpace(txtUser.Text) ||
            string.IsNullOrWhiteSpace(txtPass.Text) ||
            string.IsNullOrWhiteSpace(txtConfirm.Text))
        {
            MessageBox.Show(
                this,
                "Pakikumpleto ang lahat ng field.",
                "Missing information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (txtPass.Text != txtConfirm.Text)
        {
            MessageBox.Show(
                this,
                "Hindi magkapareho ang password at confirm password.",
                "Password mismatch",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            txtConfirm.Clear();
            txtConfirm.Focus();
            return;
        }

        try
        {
            bool success = Database.RegisterUser(
                txtUser.Text,
                txtPass.Text,
                txtFullName.Text,
                out string error);

            if (!success)
            {
                MessageBox.Show(
                    this,
                    error,
                    "Registration failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            RegisteredUsername = txtUser.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                "Unable to create account:\n" + ex.Message,
                "Registration error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}