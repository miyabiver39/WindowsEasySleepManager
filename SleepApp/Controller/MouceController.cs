using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SleepApp
{
	class MouceController
	{
		/// <summary>
		/// X座標
		/// </summary>
		private int _moucePointX;

		/// <summary>
		/// Y座標
		/// </summary>
		private int _moucePointY;

		/// <summary>
		/// 移動したか
		/// </summary>
		private bool _isMove;

        /// <summary>
        /// X座標許容範囲
        /// </summary>
        private int _permissibleRangeX;

        /// <summary>
        /// Y座標許容範囲
        /// </summary>
        private int _permissibleRangeY;

        /// <summary>
        /// 移動したか
        /// </summary>
        public bool IsMove
		{
			get { return _isMove; }
        }

        /// <summary>
        /// X座標許容範囲
        /// </summary>
        public int PermissibleRangeX
        {
            set { _permissibleRangeX = value; }
        }
        
        /// <summary>
        /// Y座標許容範囲
        /// </summary>
        public int PermissibleRangeY
        {
            set { _permissibleRangeY = value; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MouceController ()
		{
			_isMove = false;
			_moucePointX = System.Windows.Forms.Cursor.Position.X;
			_moucePointY = System.Windows.Forms.Cursor.Position.Y;
            _permissibleRangeX = Program.PermissibleRangeX;
            _permissibleRangeY = Program.PermissibleRangeY;

        }

		/// <summary>
		/// 移動チェック
		/// </summary>
		public void  MouceMoveCheck()
		{
			int new_mouce_point_x = System.Windows.Forms.Cursor.Position.X;
			int new_mouce_point_y = System.Windows.Forms.Cursor.Position.Y;
            
            // X座標移動量チェック
            if (_moucePointX - _permissibleRangeX <= new_mouce_point_x && new_mouce_point_x <= _moucePointX + _permissibleRangeX &&
                _moucePointY - _permissibleRangeY <= new_mouce_point_y && new_mouce_point_y <= _moucePointY + _permissibleRangeY)
            {
                _isMove = false;
            }
            else
            {
                _isMove = true;
                _moucePointX = new_mouce_point_x;
                _moucePointY = new_mouce_point_y;
            }
        }
	}
}
