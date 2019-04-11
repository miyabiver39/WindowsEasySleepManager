namespace SleepApp
{
	partial class SleepForm
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SleepForm));
            this.label1 = new System.Windows.Forms.Label();
            this.SleepTimeLabel = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("源界明朝", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(230, 32);
            this.label1.TabIndex = 0;
            this.label1.Text = "スリープまで残り";
            // 
            // SleepTimeLabel
            // 
            this.SleepTimeLabel.AutoSize = true;
            this.SleepTimeLabel.Font = new System.Drawing.Font("源界明朝", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.SleepTimeLabel.Location = new System.Drawing.Point(248, 9);
            this.SleepTimeLabel.Name = "SleepTimeLabel";
            this.SleepTimeLabel.Size = new System.Drawing.Size(74, 32);
            this.SleepTimeLabel.TabIndex = 1;
            this.SleepTimeLabel.Text = "00秒";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 44);
            this.progressBar.Maximum = 60;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(308, 23);
            this.progressBar.TabIndex = 2;
            // 
            // SleepForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(332, 79);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.SleepTimeLabel);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SleepForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "スリープタイマー";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SleepForm_FormClosing);
            this.Load += new System.EventHandler(this.SleepForm_Load);
            this.Shown += new System.EventHandler(this.SleepForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label SleepTimeLabel;
		private System.Windows.Forms.ProgressBar progressBar;
    }
}

