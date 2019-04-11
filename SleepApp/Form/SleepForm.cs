using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SleepApp
{
	public partial class SleepForm : Form
	{
		private SleepController sleepController;
		private BackgroundWorker backgroundWorker;

        private bool isForceDown = false;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public SleepForm()
		{
			InitializeComponent();

            ToolStripMenuItem menuItem = new ToolStripMenuItem();
            menuItem.Text = "&終了";
            menuItem.Click += new EventHandler(Close_Click);
            //contextMenuStrip.Items.Add(menuItem);
            //notifyIcon.ContextMenuStrip = contextMenuStrip;

            // ワーカースレッド
            backgroundWorker = new BackgroundWorker();
			backgroundWorker.WorkerSupportsCancellation = true;
			backgroundWorker.WorkerReportsProgress = true;

			backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_doWork);
			backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);

			sleepController = new SleepController(100, Program.SleepCheckTime);
			sleepController.DoStartAsync();

			backgroundWorker.RunWorkerAsync();

			SystemEvents.PowerModeChanged += (sender, e) =>
			{
				switch (e.Mode)
				{
					case PowerModes.Suspend:
						// スリープ直前
						sleepController.IsCancel = true;
						break;
					case PowerModes.Resume:
						sleepController.DoStartAsync();
						// 復帰直後
						break;
					case PowerModes.StatusChange:
						// バッテリーや電源に関する通知があった
						break;
				}
			};
		}

		/// <summary>
		///  ロードイベント
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SleepForm_Load(object sender, EventArgs e)
		{
			this.Visible = false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void backgroundWorker_doWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				BackgroundWorker worker = sender as BackgroundWorker;

				while (true)
				{
					// 0.1秒スリープ
					System.Threading.Thread.Sleep(100);
					
					// キャンセルの場合
					if (worker.CancellationPending)
					{
						e.Cancel = true;
						break;
					}

                    //notifyIcon.Text = "スリープまで残り" + sleepController.SleepElapsedTime.ToString() + "秒";

                    // プログレスバーの表示範囲内ならば画面を表示
                    if (progressBar.Maximum >= sleepController.SleepElapsedTime)
					{
                        // プログレスバー適用前にチェック
                        int val = progressBar.Maximum - sleepController.SleepElapsedTime;
                        if (progressBar.Maximum < val)
                        {
                            val = progressBar.Maximum;
                        }
                        else if (val < progressBar.Minimum)
                        {
                            val = progressBar.Minimum;
                        }

                        // 画面操作
                        Invoke((MethodInvoker)delegate
                        {
                            // プログレスバーの値変更
                            progressBar.Value = val;
                            SleepTimeLabel.Text = sleepController.SleepElapsedTime.ToString();

                            // 表示状態に変更
                            this.Visible = true;
                        });
                    }
					else
					{
						// 画面操作
						Invoke((MethodInvoker)delegate
						{
							if (this.Visible)
							{
								this.Visible = false;
							}
						});
						continue;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
                Program.logger.Error("障害：" + ex.Message);
                // 何もしない
            }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Program.logger.Info("情報：ReStart");
            sleepController.IsCancel = true;
            sleepController.DoStartAsync();
            backgroundWorker.RunWorkerAsync();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SleepForm_FormClosing(object sender, FormClosingEventArgs e)
		{
            if (!isForceDown)
            {
                e.Cancel = true;
                this.Visible = false;
            }
		}

		private void SleepForm_Shown(object sender, EventArgs e)
		{
			this.Visible = false;
		}

        private void Close_Click(object sender, EventArgs e)
        {
            isForceDown = true;
            Application.Exit();
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            // 表示する必要はないので無効
#if false
            if (this.Visible)
            {
                this.Visible = false;
            }
            else
            {
                this.Visible = true;
            }
#endif
        }
	}
}
