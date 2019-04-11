using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SleepApp
{
	class SleepController
	{
		MouceController _mouceController;

		/// <summary>
		/// マウス移動チェック間隔
		/// </summary>
		private int _sleepCheckInterval;

		/// <summary>
		/// スリープ開始までの時間
		/// </summary>
		private int _sleepIntervalTime;

		/// <summary>
		/// スリープまでの残り時間
		/// </summary>
		private int _sleepElapsedTime;

		/// <summary>
		/// キャンセル
		/// </summary>
		private bool _isCancel;

		private eStatus _status;
		private eResult _result;

		private Object ThreadSleepTimeLock = new Object();
		private Object SleepIntervalTimeLock = new Object();
		private Object IsCancelLock = new Object();
		private Object SleepElapsedTimeLock = new Object();

		public int SleepElapsedTime
		{
			get
			{
				lock (SleepElapsedTimeLock)
				{
					return _sleepElapsedTime;
				}
			}
		}

		/// <summary>
		/// スリープ開始までの時間
		/// </summary>
		public int SleepIntervalTime
		{
			get {
				lock (SleepIntervalTimeLock)
				{
					return _sleepIntervalTime;
				}
			}
			set
			{
				lock (SleepIntervalTimeLock)
				{
					_sleepIntervalTime = value;
				}
			}
		}

		/// <summary>
		/// スレッドスリープ時間
		/// </summary>
		public int ThreadSleepTime
		{
			get
			{
				lock (ThreadSleepTimeLock)
				{
					return _sleepCheckInterval;
				}
			}
			set
			{
				lock (ThreadSleepTimeLock)
				{
					_sleepCheckInterval = value;
				}
			}
		}

		/// <summary>
		/// キャンセル
		/// </summary>
		public bool IsCancel
		{
			get
			{
				lock (IsCancelLock)
				{
					return _isCancel;
				}
			}
			set
			{
				lock (IsCancelLock)
				{
					_isCancel = value;
				}
			}
		}

		public eStatus Status
		{
			get { return _status; }
			set { _status = value; }
		}
		public eResult Result
		{
			get { return _result; }
			set { _result = value; }
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public SleepController(int threadSleepTime = 100, int sleepIntervalTime = 70)
		{
			_sleepCheckInterval = threadSleepTime;
			_sleepIntervalTime = sleepIntervalTime;
			_sleepElapsedTime = _sleepIntervalTime;
			_mouceController = new MouceController();
			_status = eStatus.None;
			_result = eResult.None;
			IsCancel = false;
		}

		/// <summary>
		/// 
		/// </summary>
		public async void DoStartAsync()
		{
			System.Diagnostics.Stopwatch _stopWatch;
			_stopWatch = new System.Diagnostics.Stopwatch();
			_status = eStatus.Init;
			_result = eResult.None;
			IsCancel = false;
			bool isBeforeMove = false;

			try
			{
				await Task.Run(() =>
				{

					while (!IsCancel)
					{
						_mouceController.MouceMoveCheck();

						// 移動したならタイマー停止
						if (_mouceController.IsMove)
						{
							// 前回移動したならばタイマーリセット
							if (!isBeforeMove)
							{
								_stopWatch.Reset();
								isBeforeMove = true;
								_status = eStatus.Pause;
							}
						}
						// 移動していないならばタイマー開始
						else
						{
							// 処理開始していないならば
							if (_status != eStatus.Process)
							{
								_stopWatch.Start();
								_status = eStatus.Process;
								isBeforeMove = false;
							}
							// スリープチェック中ならば
							else
							{

								// スリープ時間経過したならば
								if (SleepIntervalTime <= (int)_stopWatch.Elapsed.TotalSeconds)
								{
									_result = eResult.Success;
									break;
								}
							}
						}
						// スレッドスリープ
						System.Threading.Thread.Sleep(ThreadSleepTime);

						lock (SleepElapsedTimeLock)
						{
							_sleepElapsedTime = SleepIntervalTime - _stopWatch.Elapsed.Seconds;
						}
					}

					// ループの終了がキャンセルならば
					if (IsCancel)
					{
						return;
					}
					// ループの終了がスリープチェックならば
					else
					{
						SystemSleep();
					}
				});
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
		public void SystemSleep()
		{
			Application.SetSuspendState(Program.SleepMode, false, false);
		}

		/// <summary>
		/// 
		/// </summary>
		public enum eStatus
		{
			None,
			Init,
			Pause,
			Process,
		}

		/// <summary>
		/// 
		/// </summary>
		public enum eResult
		{
			None,
			Failed,
			Success
		}
	}
}
