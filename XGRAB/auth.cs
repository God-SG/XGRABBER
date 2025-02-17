using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Memory;
using XGRAB.Properties;

namespace XGRAB;

public class auth : Form
{
	public Mem m = new Mem();

	private bool attached;

	private bool TokenRetrieved;

	private IContainer components;

	private Label label1;

	private BackgroundWorker backgroundWorker1;

	private NotifyIcon notifyIcon1;

	private Panel panel2;

	private Label label4;

	private Label label2;

	private TextBox textBox1;

	public auth()
	{
		InitializeComponent();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		base.OnPaint(e);
		GraphicsPath path = new GraphicsPath();
		int radius = 15;
		path.AddArc(0, 0, radius * 2, radius * 2, 180f, 90f);
		path.AddArc(base.Width - radius * 2, 0, radius * 2, radius * 2, 270f, 90f);
		path.AddArc(base.Width - radius * 2, base.Height - radius * 2, radius * 2, radius * 2, 0f, 90f);
		path.AddArc(0, base.Height - radius * 2, radius * 2, radius * 2, 90f, 90f);
		path.CloseFigure();
		base.Region = new Region(path);
	}

	private void LoadingScreen_Load(object sender, EventArgs e)
	{
		backgroundWorker1.RunWorkerAsync();
		base.TopMost = true;
		foreach (Process process in from p in Process.GetProcesses()
			where p.ProcessName.Contains("xbox", StringComparison.OrdinalIgnoreCase)
			select p)
		{
			try
			{
				process.Kill();
				process.WaitForExit();
			}
			catch (Exception)
			{
			}
		}
	}

	private async Task GetToken()
	{
		Thread.Sleep(3000);
		if (TokenRetrieved)
		{
			return;
		}
		string mostCommon = null;
		try
		{
			string[] XBLStrings = (await m.AoBScan("58 42 4C 33 2E 30 20 78 3D", writable: true)).Select((long address) => m.ReadString(address.ToString("X"), "", 10000)).ToArray();
			Dictionary<string, int> frequency = new Dictionary<string, int>();
			string[] array = XBLStrings;
			foreach (string str in array)
			{
				if (!frequency.ContainsKey(str))
				{
					frequency[str] = 1;
				}
				else
				{
					frequency[str]++;
				}
			}
			mostCommon = XBLStrings.FirstOrDefault();
			int highestFrequency = 0;
			foreach (KeyValuePair<string, int> pair in frequency)
			{
				if (pair.Value > highestFrequency)
				{
					mostCommon = pair.Key;
					highestFrequency = pair.Value;
				}
			}
			if (highestFrequency < 3)
			{
				Application.Restart();
				return;
			}
		}
		catch (Exception)
		{
			backgroundWorker1.CancelAsync();
			MessageBox.Show("Couldn't find XBL. There was either a software error or the console app has changed its storage methods. This app will now close after you close this message.", "X-GRAB", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			Application.Exit();
		}
		try
		{
			if (!TokenRetrieved)
			{
				label1.Text = "Xbox Token Found!";
				TokenRetrieved = true;
				textBox1.Text = mostCommon.ToString();
				File.WriteAllText(Application.StartupPath + "\\xbl.txt", mostCommon.ToString());
				Application.Exit();
			}
		}
		catch
		{
			backgroundWorker1.CancelAsync();
			MessageBox.Show("Couldn't find XBL. There was either a software error or the console app has changed its storage methods. This app will now close after you close this message.", "X-GRAB", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			Application.Exit();
		}
	}

	private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
	{
		while (true)
		{
			if (m.OpenProcess("XboxPcApp"))
			{
				attached = true;
				Thread.Sleep(1000);
				backgroundWorker1.ReportProgress(0);
				continue;
			}
			attached = false;
			try
			{
				string str = "Start-Process xbox://";
				ProcessStartInfo processStartInfo = new ProcessStartInfo
				{
					FileName = "powershell.exe",
					Arguments = "-Command \"" + str + "\"",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				};
				using Process process = new Process();
				process.StartInfo = processStartInfo;
				process.Start();
				process.WaitForExit();
			}
			catch (Exception)
			{
			}
			Thread.Sleep(1000);
		}
	}

	private async void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		if (attached && !TokenRetrieved)
		{
			await Task.Delay(2000);
			await GetToken();
		}
	}

	private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		backgroundWorker1.RunWorkerAsync();
	}

	private void label1_Click(object sender, EventArgs e)
	{
	}

	private void label3_Click(object sender, EventArgs e)
	{
	}

	private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
	{
	}

	private void textBox1_TextChanged(object sender, EventArgs e)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Expected O, but got Unknown
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Expected O, but got Unknown
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Expected O, but got Unknown
		//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Expected O, but got Unknown
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XGRAB.auth));
		this.label1 = new System.Windows.Forms.Label();
		this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
		this.label2 = new System.Windows.Forms.Label();
		this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
		this.panel2 = new System.Windows.Forms.Panel();
		this.label4 = new System.Windows.Forms.Label();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.panel2.SuspendLayout();
		base.SuspendLayout();
		this.label1.BackColor = System.Drawing.Color.Transparent;
		this.label1.Font = new Font("Yu Gothic UI", 9.75f, (FontStyle)0, (GraphicsUnit)3);
		this.label1.Location = new System.Drawing.Point(12, 185);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(763, 82);
		this.label1.TabIndex = 0;
		this.label1.Text = "Please Open the Xbox App";
		this.label1.TextAlign = (ContentAlignment)2;
		this.label1.Click += new System.EventHandler(label1_Click);
		this.backgroundWorker1.WorkerReportsProgress = true;
		this.backgroundWorker1.WorkerSupportsCancellation = true;
		this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker1_DoWork);
		this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
		this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
		this.label2.AutoSize = true;
		this.label2.Font = new Font("Segoe UI", 36f, (FontStyle)0, (GraphicsUnit)3);
		this.label2.ForeColor = System.Drawing.Color.FromArgb(16, 124, 16);
		this.label2.Location = new System.Drawing.Point(4, 0);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(56, 65);
		this.label2.TabIndex = 1;
		this.label2.Text = "X";
		this.label2.TextAlign = (ContentAlignment)64;
		this.notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
		this.notifyIcon1.Text = "Xbox";
		this.notifyIcon1.Visible = true;
		this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(notifyIcon1_MouseDoubleClick);
		this.panel2.BackColor = System.Drawing.Color.Transparent;
		this.panel2.Controls.Add(this.label4);
		this.panel2.Controls.Add(this.label2);
		this.panel2.Location = new System.Drawing.Point(306, 124);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(182, 65);
		this.panel2.TabIndex = 3;
		this.label4.AutoSize = true;
		this.label4.Font = new Font("Segoe UI", 36f, (FontStyle)0, (GraphicsUnit)3);
		this.label4.ForeColor = System.Drawing.Color.FromArgb(0, 98, 184);
		this.label4.Location = new System.Drawing.Point(43, 1);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(149, 65);
		this.label4.TabIndex = 3;
		this.label4.Text = "GRAB";
		this.label4.TextAlign = (ContentAlignment)64;
		this.textBox1.Location = new System.Drawing.Point(190, 228);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(425, 23);
		this.textBox1.TabIndex = 4;
		this.textBox1.Text = "XBL3.0 x=...";
		this.textBox1.TextChanged += new System.EventHandler(textBox1_TextChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(18, 18, 18);
		this.BackgroundImage = (Image?)(object)XGRAB.Properties.Resources.RewardsSignUpBanner;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		base.ClientSize = new System.Drawing.Size(787, 319);
		base.Controls.Add(this.textBox1);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.panel2);
		this.DoubleBuffered = true;
		this.ForeColor = System.Drawing.Color.White;
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Icon = (Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MdiChildrenMinimizedAnchorBottom = false;
		base.MinimizeBox = false;
		base.Name = "auth";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Authenticator";
		base.WindowState = System.Windows.Forms.FormWindowState.Minimized;
		base.Load += new System.EventHandler(LoadingScreen_Load);
		this.panel2.ResumeLayout(false);
		this.panel2.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
