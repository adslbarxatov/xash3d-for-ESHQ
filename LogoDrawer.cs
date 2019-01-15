using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ESHQSetupStub
	{
	/// <summary>
	/// Класс обеспечивает отображение логотипа мода
	/// </summary>
	public partial class LogoDrawer:Form
		{
		// Масштабы картинки
		private const int scale = 130;
		private const int drawerSize = scale / 8;
		private const int fontSize = (int)(scale * 0.95);
		private const int tailsSize = (int)(scale * 0.3);

		private const int frameSpeed = 6;
		private const string logoString1 = "ESHQ",
			logoString2 = "ES:FA";

		// Переменные
		private byte phase1 = 1, phase2 = 1;	// Текущие фазы отрисовки
		private Point point1, point2;			// Текущие позиции отрисовки
		private SizeF logoSize;					// Графические размеры текста для текущего экрана
		private double arc1, arc2;				// Переменные для расчёта позиций элементов в полярных координатах

		private Graphics g;						// Объекты-отрисовщики
		private SolidBrush foreBrush, backBrush;
		private Pen backPen;
		private Bitmap logo;
		private Font logoFont;

		/// <summary>
		/// Конструктор. Инициализирует экземпляр отрисовщика
		/// </summary>
		public LogoDrawer ()
			{
			// Инициализация
			InitializeComponent ();

			// Запуск
			this.ShowDialog ();
			}

		private void LogoDrawer_Load (object sender, System.EventArgs e)
			{
			// Если запрос границ экрана завершается ошибкой, отменяем отображение
			this.Left = this.Top = 0;
			try
				{
				this.Width = Screen.PrimaryScreen.Bounds.Width;
				this.Height = Screen.PrimaryScreen.Bounds.Height;
				}
			catch
				{
				this.Close ();
				return;
				}

			// Настройка окна
			this.BackColor = ProgramDescription.MasterBackColor;
			this.ForeColor = ProgramDescription.MasterTextColor;

			backBrush = new SolidBrush (ProgramDescription.MasterBackColor);
			foreBrush = new SolidBrush (ProgramDescription.MasterTextColor);
			backPen = new Pen (ProgramDescription.MasterBackColor, drawerSize);

			g = Graphics.FromHwnd (this.Handle);
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;	// Убирает ауру на буквах в Win8

			logoFont = new Font ("Lucida Sans Unicode", fontSize);
			logoSize = g.MeasureString (logoString1, logoFont);

			// Установка начальных позиций
			point1 = new Point (-(int)scale, (this.Height - (int)scale) / 2);	// Горизонтальная
			point2 = new Point (this.Width / 2 - (int)scale, -(int)scale);		// Вертикальная

			// Запуск таймера
			DrawingTimer.Interval = MovingTimer.Interval = 1;
			PauseTimer.Interval = 2000;
			DrawingTimer.Enabled = true;
			}

		// Таймер отрисовки
		private void DrawingTimer_Tick (object sender, System.EventArgs e)
			{
			// Определение следующей позиции
			switch (phase1)
				{
				// Горизонтально вправо
				case 1:
					point1.X += frameSpeed;

					if (point1.X >= (this.Width - scale) / 2)
						{
						arc1 = -90.0;
						phase1++;
						}
					break;

				// Вниз по выпуклой вверх четвертьдуге
				case 2:
					arc1 += 2.0;

					point1.X = (this.Width - scale) / 2 + (int)(Cosinus (arc1) * scale / 2.0);
					point1.Y = this.Height / 2 + (int)(Sinus (arc1) * scale / 2.0);

					if (arc1 >= 0.0)
						{
						arc1 = -180.0;
						phase1++;
						}
					break;

				// Вниз по выпуклой вниз четвертьдуге
				case 3:
					arc1 -= 2.0;

					point1.X = (this.Width + scale) / 2 + (int)(Cosinus (arc1) * scale / 2.0);
					point1.Y = this.Height / 2 + (int)(Sinus (arc1) * scale / 2.0);

					if (arc1 <= -270.0)
						{
						phase1++;
						}
					break;

				// Горизонтально вправо
				case 4:
					point1.X += frameSpeed;
					break;
				}

			switch (phase2)
				{
				// Вертикально вниз
				case 1:
					point2.Y += frameSpeed;

					if (point2.Y >= this.Height / 2)
						{
						arc2 = 180.0;
						phase2++;
						}
					break;

				// Выпуклая вниз полудуга
				case 2:
					arc2 -= 2.0;

					point2.X = (this.Width - scale) / 2 + (int)(Cosinus (arc2) * scale / 2.0);
					point2.Y = this.Height / 2 + (int)(Sinus (arc2) * scale / 2.0);

					if (arc2 <= 0.0)
						{
						arc2 = 180.0;
						phase2++;
						}
					break;

				// Выпуклая вверх полудуга
				case 3:
					arc2 += 2.0;

					point2.X = (this.Width + scale) / 2 + (int)(Cosinus (arc2) * scale / 2.0);
					point2.Y = this.Height / 2 + (int)(Sinus (arc2) * scale / 2.0);

					if (arc2 >= 360.0)
						{
						phase2++;
						}
					break;

				// Вертикально вниз
				case 4:
					point2.Y += frameSpeed;
					break;
				}

			// Отрисовка
			if ((phase1 == 1) || (phase1 == 4))
				g.FillRectangle (foreBrush, point1.X - drawerSize / 2, point1.Y - drawerSize / 2, drawerSize, drawerSize);
			else
				g.FillEllipse (foreBrush, point1.X - drawerSize / 2, point1.Y - drawerSize / 2, drawerSize, drawerSize);

			if ((phase2 == 1) || (phase2 == 4))
				g.FillRectangle (foreBrush, point2.X - drawerSize / 2, point2.Y - drawerSize / 2, drawerSize, drawerSize);
			else
				g.FillEllipse (foreBrush, point2.X - drawerSize / 2, point2.Y - drawerSize / 2, drawerSize, drawerSize);

			// Остановка таймера по завершению, запуск следующей фазы
			if ((point1.X > this.Width + scale / 10) && (point2.Y > this.Height + scale / 10))
				{
				// Остановка
				DrawingTimer.Enabled = false;

				// "Фото" экрана
				Bitmap logo2 = new Bitmap (this.Width, this.Height);
				Graphics g2 = Graphics.FromImage (logo2);
				g2.CopyFromScreen (0, 0, 0, 0, new Size (this.Width, this.Height));
				g2.Dispose ();

				// "Фото" лого
				logo = logo2.Clone (new Rectangle (this.Width / 2 - (scale + tailsSize), this.Height / 2 - (int)(scale / 2 + tailsSize),
					(scale + tailsSize) * 2, scale + tailsSize * 2), PixelFormat.Format24bppRgb);
				logo2.Dispose ();

				// В рамочке
				g2 = Graphics.FromImage (logo);
				g2.DrawRectangle (backPen, 0, 0, logo.Width, logo.Height);
				g2.Dispose ();

				// Подготовка следующего таймера
				arc1 = 180.0;

				// Продолжение
				MovingTimer.Enabled = true;
				}
			}

		// Таймер формирования конечного изображения
		private void MovingTimer_Tick (object sender, EventArgs e)
			{
			arc1 -= 2.0;

			if (arc1 >= 0.0)
				{
				// Расходящаяся от центра рамка, стирающая лишние линии
				g.DrawRectangle (backPen,
					(int)((this.Width / 2 - (scale + tailsSize)) * ((1 + Cosinus (arc1 + 180.0)) / 2.0)),
					(int)((this.Height / 2 - (int)(scale / 2 + tailsSize)) * ((1 + Cosinus (arc1 + 180.0)) / 2.0)),
					(int)(logo.Width + (this.Width - logo.Width) * ((1 + Cosinus (arc1)) / 2.0)),
					(int)(logo.Height + (this.Height - logo.Height) * ((1 + Cosinus (arc1)) / 2.0)));

				// Смещающееся влево лого
				g.DrawImage (logo, (this.Width - logo.Width) / 2 - (int)(((1 + Cosinus (arc1)) / 2.0) * (this.Width - logo.Width) / 4),
					(int)((this.Height - (scale + 2 * tailsSize)) / 2));
				}
			else if (arc1 >= -90.0)
				{
				// Отображение текста
				/*g.DrawString (logoString1.Substring (0, (int)(logoString1.Length * Sinus (-arc1))),
					logoFont, foreBrush, this.Width - (this.Width - logo.Width) / 4 - logoSize.Width,
					(this.Height - logoSize.Height * 0.9f) / 2.0f);*/
				g.DrawString (logoString1.Substring (0, (int)(logoString1.Length * Sinus (-arc1))),
					logoFont, foreBrush, this.Width - (this.Width - logo.Width) / 4 - logoSize.Width,
					this.Height / 2 - logoSize.Height * 0.7f);
				g.DrawString (logoString2.Substring (0, (int)(logoString2.Length * Sinus (-arc1))),
					logoFont, foreBrush, this.Width - (this.Width - logo.Width) / 4 - logoSize.Width,
					this.Height / 2);
				}
			else
				{
				MovingTimer.Enabled = false;
				PauseTimer.Enabled = true;
				}
			}

		// Таймер задержки лого на экране
		private void PauseTimer_Tick (object sender, EventArgs e)
			{
			this.Close ();
			}

		// Закрытие окна
		private void LogoDrawer_FormClosing (object sender, FormClosingEventArgs e)
			{
			// Остановка всех отрисовок
			DrawingTimer.Enabled = MovingTimer.Enabled = PauseTimer.Enabled = false;

			// Сброс всех ресурсов
			foreBrush.Dispose ();
			backBrush.Dispose ();
			backPen.Dispose ();
			logoFont.Dispose ();
			g.Dispose ();

			if (logo != null)
				logo.Dispose ();
			}

		// Принудительный выход (по любой клавише)
		private void LogoDrawer_KeyDown (object sender, KeyEventArgs e)
			{
			this.Close ();
			}

		private void LogoDrawer_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		/// <summary>
		/// Метод переводит градусы в радианы
		/// </summary>
		/// <param name="Phi">Градусная величина угла</param>
		/// <returns>Радианная величина угла</returns>
		private double D2R (double Phi)
			{
			return Math.PI * Phi / 180.0;
			}

		/// <summary>
		/// Метод возвращает значение синуса угла, представленного в градусах
		/// </summary>
		/// <param name="ArcInDegrees">Градусная величина угла</param>
		/// <returns>Синус угла</returns>
		private double Sinus (double ArcInDegrees)
			{
			return Math.Sin (D2R (ArcInDegrees));
			}

		/// <summary>
		/// Метод возвращает значение косинуса угла, представленного в градусах
		/// </summary>
		/// <param name="ArcInDegrees">Градусная величина угла</param>
		/// <returns>Косинус угла</returns>
		private double Cosinus (double ArcInDegrees)
			{
			return Math.Cos (D2R (ArcInDegrees));
			}
		}
	}
