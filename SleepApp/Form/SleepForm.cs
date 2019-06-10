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
        private bool isForceVisible = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SleepForm()
        {
            InitializeComponent();

            ToolStripMenuItem menuItemVisible = new ToolStripMenuItem();
            menuItemVisible.Text = "&表示";
            menuItemVisible.Click += new EventHandler(ToolStripMenuItem_ForceVisible);
            contextMenuStrip.Items.Add(menuItemVisible);

            ToolStripMenuItem menuItemExit = new ToolStripMenuItem();
            menuItemExit.Text = "&終了";
            menuItemExit.Click += new EventHandler(ToolStripMenuItem_Close);
            contextMenuStrip.Items.Add(menuItemExit);

            notifyIcon.ContextMenuStrip = contextMenuStrip;

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
        /// ロードイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SleepForm_Load(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        /// <summary>
        /// スリープまでの残り時間表示
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
                    // 0.5秒スリープ
                    System.Threading.Thread.Sleep(500);

                    // キャンセルの場合
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }

                    // プログレスバー適用前に値が範囲内に当てはまるようにする
                    int progressBarVal = progressBar.Maximum - sleepController.SleepElapsedTime;

                    // プログレスバー以上ならば、プログレスバーの最大値を指定
                    if (progressBar.Maximum < progressBarVal)
                    {
                        progressBarVal = progressBar.Maximum;
                    }
                    // プログレスバー以下ならば、プログレスバーの最低値を指定
                    else if (progressBarVal < progressBar.Minimum)
                    {
                        progressBarVal = progressBar.Minimum;
                    }

                    // 残り時間
                    string displaysleepElapsedTime;

                    // 残り時間が9999秒以上ならば
                    if (sleepController.SleepElapsedTime > 9999)
                    {
                        displaysleepElapsedTime = "+9999";
                    }
                    // それ以外
                    else {
                        displaysleepElapsedTime = sleepController.SleepElapsedTime.ToString();
                    }

                    // 画面の残り時間を反映
                    Invoke((MethodInvoker)delegate
                    {
                        SleepTimeLabel.Text = displaysleepElapsedTime;
                    });

                    // 通知アイコンの残り時間を反映
                    notifyIcon.Text = "スリープまで残り" + displaysleepElapsedTime + "秒";

                    // 画面操作
                    Invoke((MethodInvoker)delegate
                    {
                        // プログレスバーの値変更
                        progressBar.Value = progressBarVal;
                    });

                    // 強制表示状態ならば
                    if (isForceVisible)
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            // 画面操作
                            if (!this.Visible)
                            {
                                this.Visible = true;
                            }
                        });
                        continue;
                    }

                    // 表示範囲内ならば画面を表示
                    if (Program.SleepVisibleTime >= sleepController.SleepElapsedTime)
                    {
                        // 画面操作
                        Invoke((MethodInvoker)delegate
                        {
                            // 表示状態に変更
                            if (!this.Visible)
                            {
                                this.Visible = true;
                            }

                            // 最前面に表示
                            if (!this.TopMost)
                            {
                                this.TopMost = true;
                            }
                        });
                    }
                    // 表示範囲外ならば
                    else
                    {
                        // 画面操作
                        Invoke((MethodInvoker)delegate
                        {
                            // 非表示状態に変更
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
        /// ワーカー終了時の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // 再起動させる
            Program.logger.Info("情報：ReStart");
            sleepController.IsCancel = true;
            sleepController.DoStartAsync();
            backgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// フォームを閉じたときのイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SleepForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isForceDown)
            {
                e.Cancel = true;
                this.Visible = false;
                isForceVisible = false;
            }
        }

        /// <summary>
        /// フォームが表示された時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SleepForm_Shown(object sender, EventArgs e)
        {
            this.Visible = false;
            isForceVisible = false;

            Invoke((MethodInvoker)delegate
            {
                // プログレスバー最大値設定
                progressBar.Maximum = sleepController.SleepElapsedTime;
            });
        }

        /// <summary>
        /// メニュー 閉じるイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItem_Close(object sender, EventArgs e)
        {
            isForceDown = true;
            Application.Exit();
        }

        /// <summary>
        /// メニュー 表示イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItem_ForceVisible(object sender, EventArgs e)
        {
            isForceVisible = true;
        }

        /// <summary>
        /// 通知アイコンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
