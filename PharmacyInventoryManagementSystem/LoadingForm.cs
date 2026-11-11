using System;
using System.Drawing;
using System.Windows.Forms;

namespace PharmacyInventoryManagementSystem;

public class LoadingForm : Form
{
    private readonly System.Windows.Forms.Timer timer = new();

    private readonly DoubleBufferedPanel progressTrack = new();
    private int progressValue = 0;
    private readonly Random rand = new();

    public LoadingForm(string statusMessage = "Loading...")
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(550, 320);
        BackColor = Color.FromArgb(24, 42, 75);
        ShowInTaskbar = false;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(40, 20, 40, 20) 
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90)); 
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

        root.Controls.Add(new Label
        {
            Text = "+",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomCenter,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 45, FontStyle.Bold)
        }, 0, 0);

        root.Controls.Add(new Label
        {
            Text = "PHARMACARE",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 20, FontStyle.Bold)
        }, 0, 1);

        root.Controls.Add(new Label
        {
            Text = "PHARMACY INVENTORY MANAGEMENT SYSTEM",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopCenter,
            ForeColor = Color.LightSteelBlue,
            Font = new Font("Segoe UI", 9)
        }, 0, 2);

        var bottomPanel = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };

        var lblStatus = new Label
        {
            Text = statusMessage,
            ForeColor = Color.LightSteelBlue,
            Font = new Font("Segoe UI", 9, FontStyle.Italic),
            AutoSize = true,
            Location = new Point(0, 0)
        };

        progressTrack.Size = new Size(470, 24);
        progressTrack.Location = new Point(0, 25);
        progressTrack.BackColor = Color.FromArgb(15, 30, 55);

        progressTrack.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using var pen = new Pen(Color.FromArgb(55, 75, 105), 2);
            e.Graphics.DrawRectangle(pen, 1, 1, progressTrack.Width - 2, progressTrack.Height - 2);

            int totalBlocks = 15;
            int spacing = 4;
            int blockWidth = (progressTrack.Width - (spacing * (totalBlocks + 1))) / totalBlocks;
            int filledBlocks = (int)((progressValue / 100.0) * totalBlocks);

            for (int i = 0; i < totalBlocks; i++)
            {
                Rectangle rect = new Rectangle(spacing + i * (blockWidth + spacing), 4, blockWidth, progressTrack.Height - 8);

                if (i < filledBlocks)
                {
                    using var brush = new SolidBrush(Color.FromArgb(19, 145, 135));
                    e.Graphics.FillRectangle(brush, rect);
                }
            }
        };

        bottomPanel.Controls.Add(lblStatus);
        bottomPanel.Controls.Add(progressTrack);

        root.Controls.Add(bottomPanel, 0, 4);
        Controls.Add(root);

        timer.Interval = 40;
        timer.Tick += (_, _) =>
        {
            progressValue += rand.Next(3, 8);

            if (progressValue >= 100)
            {
                progressValue = 100;
                progressTrack.Invalidate();
                timer.Stop();

                var delay = new System.Windows.Forms.Timer { Interval = 150 };
                delay.Tick += (sender, ev) =>
                {
                    delay.Stop();
                    DialogResult = DialogResult.OK;
                };
                delay.Start();
            }
            else
            {
                progressTrack.Invalidate();
            }
        };
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        timer.Start();
    }
}

public class DoubleBufferedPanel : Panel
{
    public DoubleBufferedPanel()
    {
        DoubleBuffered = true;
        UpdateStyles();
    }
}