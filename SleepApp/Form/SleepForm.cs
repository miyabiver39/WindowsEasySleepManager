using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SleepApp
{
    /// <summary>
    /// スリープまでの残り時間を表示するフォーム
    /// </summary>
    public partial class SleepForm : Form
    {
        private SleepController sleepController;
        private BackgroundWorker formUpdateWorker;

        private bool isForceDown = false;
        private bool isForceVisible = false;

        /// <summary>
        /// 残り時間ラベルに表示可能な最大値
        /// </summary>
        private readonly int ELAPSED_LABEL_DISPLAY_MAX_TIME = 9999;

        /// <summary>
        /// 通知アイコンの残り時間に表示可能な最大値
        /// </summary>
        private readonly int NOTIFY_ICON_DISPLAY_MAX_TIME = 9999;

        /// <summary>
        /// 画面更新処理の更新チェック間隔
        /// </summary>
        private readonly int WORKER_THREAD_SLEEP_TIME = 500;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SleepForm()
        {
            InitializeComponent();

            initializeNotifyIcon();

            initializeFormUpdateWorker();
        }

        /// <summary>
        /// 起動状態の変更イベントを
        /// </summary>
        private void initializePowerModeChangeEvent()
        {
            SystemEvents.PowerModeChanged += (sender, e) =>
            {
                switch (e.Mode)
                {
                    // スリープ直前
                    case PowerModes.Suspend:
                        sleepController.IsCancel = true;
                        break;
                    // 復帰直後
                    case PowerModes.Resume:
                        sleepController.DoStartAsync();
                        break;
                    // バッテリーや電源に関する通知があった
                    case PowerModes.StatusChange:
                        break;
                    default:
                        break;
                }
            };
        }

        /// <summary>
        /// 通知アイコンの初期化
        /// </summary>
        private void initializeNotifyIcon()
        {
            ToolStripMenuItem menuItemVisible = new ToolStripMenuItem();
            menuItemVisible.Text = "&表示";
            menuItemVisible.Click += new EventHandler(ToolStripMenuItem_ForceVisible);
            contextMenuStrip.Items.Add(menuItemVisible);

            ToolStripMenuItem menuItemExit = new ToolStripMenuItem();
            menuItemExit.Text = "&終了";
            menuItemExit.Click += new EventHandler(ToolStripMenuItem_Close);
            contextMenuStrip.Items.Add(menuItemExit);

            notifyIcon.ContextMenuStrip = contextMenuStrip;
        }

        /// <summary>
        /// フォーム更新ワーカーの初期化
        /// </summary>
        private void initializeFormUpdateWorker()
        {
            // ワーカースレッド
            formUpdateWorker = new BackgroundWorker();
            formUpdateWorker.WorkerSupportsCancellation = true;
            formUpdateWorker.WorkerReportsProgress = true;

            formUpdateWorker.DoWork += new DoWorkEventHandler(backgroundWorker_doWork);
            formUpdateWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);

            sleepController = new SleepController(100, Program.SleepCheckTime);
            sleepController.DoStartAsync();

            formUpdateWorker.RunWorkerAsync();
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
                    // スリープ
                    System.Threading.Thread.Sleep(WORKER_THREAD_SLEEP_TIME);

                    // キャンセルの場合はループ終了
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }


                    // プログレスバーに値を設定
                    updateProgressBar(sleepController.SleepElapsedTime);

                    // 残り時間ラベルに値を反映
                    updateElapsedTimeLabel(sleepController.SleepElapsedTime);

                    // 通知アイコンの残り時間に値を反映
                    updateElapsedTimeNotifyIcon(sleepController.SleepElapsedTime);


                    // 強制表示状態ならば
                    if (isForceVisible)
                    {
                        changeVisibleForm(true);

                        continue;
                    }

                    // 表示範囲内ならば画面を表示
                    if (Program.SleepVisibleTime >= sleepController.SleepElapsedTime)
                    {
                        changeVisibleForm(true);

                        changeTopMostForm(true);
                    }
                    // 表示範囲外ならば
                    else
                    {
                        changeVisibleForm(false);

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
            Program.logger.Info("情報：ReStart");

            // 再起動させる
            sleepController.IsCancel = true;
            sleepController.DoStartAsync();
            formUpdateWorker.RunWorkerAsync();
        }


        #region プライベートメソッド
        /// <summary>
        /// プログレスバーを更新する
        /// </summary>
        /// <param name="value">更新する値</param>
        private void updateProgressBar(int value)
        {
            // プログレスバー適用前に値が範囲内に当てはまるようにする
            int progressBarValue = progressBar.Maximum - value;

            // プログレスバー以上ならば、プログレスバーの最大値を指定
            if (progressBar.Maximum < value)
            {
                progressBarValue = progressBar.Maximum;
            }
            // プログレスバー以下ならば、プログレスバーの最低値を指定
            else if (progressBarValue < progressBar.Minimum)
            {
                progressBarValue = progressBar.Minimum;
            }

            // 画面操作
            Invoke((MethodInvoker)delegate
            {
                // プログレスバーの値変更
                progressBar.Value = progressBarValue;
            });
        }

        /// <summary>
        /// 残り時間ラベルを更新する
        /// </summary>
        /// <param name="value">更新する値</param>
        private void updateElapsedTimeLabel(int value)
        {
            // 残り時間
            string displaysleepElapsedTime = value.ToString();

            // 残り時間が既定値以上ならば
            if (value > ELAPSED_LABEL_DISPLAY_MAX_TIME)
            {
                displaysleepElapsedTime = "+" + ELAPSED_LABEL_DISPLAY_MAX_TIME.ToString();
            }

            // 画面の残り時間を反映
            Invoke((MethodInvoker)delegate
            {
                SleepTimeLabel.Text = displaysleepElapsedTime;
            });
        }

        /// <summary>
        /// 通知アイコンの残り時間ラベルを更新する
        /// </summary>
        /// <param name="value">更新する値</param>
        private void updateElapsedTimeNotifyIcon(int value)
        {
            string displaysleepElapsedTime = value.ToString();

            // 残り時間が既定値以上ならば
            if (value > NOTIFY_ICON_DISPLAY_MAX_TIME)
            {
                displaysleepElapsedTime = "+" + NOTIFY_ICON_DISPLAY_MAX_TIME.ToString();
            }

            // 画面の残り時間を反映
            Invoke((MethodInvoker)delegate
            {
                // 通知アイコンの残り時間を反映
                notifyIcon.Text = "スリープまで残り" + displaysleepElapsedTime + "秒";
            });
        }

        /// <summary>
        /// フォームを表示する
        /// </summary>
        /// <param name="value">更新する値</param>
        private void changeVisibleForm(bool value)
        {
            Invoke((MethodInvoker)delegate
            {
                if (!this.Visible)
                {
                    this.Visible = value;
                }

            });
        }

        /// <summary>
        /// 最前面に表示する
        /// </summary>
        /// <param name="value">更新する値</param>
        private void changeTopMostForm(bool value)
        {
            Invoke((MethodInvoker)delegate
            {
                if (!this.TopMost)
                {
                    this.TopMost = value;
                }

            });
        }
        #endregion


        #region イベント
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

        #endregion
    }
}
