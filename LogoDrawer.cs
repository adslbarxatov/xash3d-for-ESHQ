using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace ESHQSetupStub
	{
	/// <summary>
	/// Класс обеспечивает отображение логотипа мода
	/// </summary>
	public partial class LogoDrawer:Form
		{
		// Переменные
		private const int scale = 130;							// Главный масштабный множитель
		private const int drawerSize = scale / 8;				// Размер кисти
		private const int tailsSize = (int)(scale * 0.3);		// Длина "хвостов" лого за границами кругов

		private const int frameSpeed = 6;						// Частота смены кадров / отступ кисти в пикселях

		private const int logoFontSize = (int)(scale * 0.95);	// Размер шрифта лого
		private const int headerFontSize = (int)(scale * 0.20);	// Размер шрифта заголовков расширенного режима
		private const int textFontSize = (int)(scale * 0.13);	// Размер шрифта текста расширенного режима
		private const string logoString1 = "ESHQ",				// Тексты лого
			logoString2 = "ES:FA";

		private byte phase1 = 1, phase2 = 1;	// Текущие фазы отрисовки
		private Point point1, point2,			// Текущие позиции отрисовки
			point3, point4;
		private double arc1, arc2;				// Переменные для расчёта позиций элементов в полярных координатах

		private Graphics g;						// Объекты-отрисовщики
		private SolidBrush foreBrush, backBrush;
		private Pen backPen;
		private Bitmap logo;
		private Font logoFont, headerFont, textFont;
		private SizeF logoSize;					// Графические размеры текста для текущего экрана

		private bool extended = false;			// Флаг расширенного режима

		private uint steps = 0,					// Счётчик шагов
			moves = 0;							// Счётчик движений мыши (используется для корректной обработки движений)

		private const int lineFeed = 40;		// Высота строки текста расширенного режима
		private const int lineLeft = 250;		// Начало строки текста расширенного режима

		private List<List<LogoDrawerString>> extendedStrings = new List<List<LogoDrawerString>> ();	// Строки текста расширенного режима

		/// <summary>
		/// Доступные режимы отрисовки лого
		/// </summary>
		public enum DrawModes
			{
			/// <summary>
			/// Режим 1
			/// </summary>
			Mode1 = 0,

			/// <summary>
			/// Режим 2
			/// </summary>
			Mode2 = 1
			}
		private DrawModes mode;

		/// <summary>
		/// Конструктор. Инициализирует экземпляр отрисовщика
		/// </summary>
		public LogoDrawer ()
			{
			// Инициализация
			Random rnd = new Random ();
			extended = (rnd.Next (67) == 0);/*
			extended = true;/* debug */

			if (!extended)
				mode = (DrawModes)rnd.Next (2);
			else
				mode = DrawModes.Mode1;

			InitializeComponent ();
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

			backBrush = new SolidBrush (this.BackColor);
			backPen = new Pen (this.BackColor, drawerSize);

			g = Graphics.FromHwnd (this.Handle);
			g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;	// Убирает ауру на буквах в Win8

			logoFont = new Font ("Lucida Sans Unicode", logoFontSize);
			logoSize = g.MeasureString (logoString1, logoFont);

			// Установка начальных позиций и методов отрисовки
			switch (mode)
				{
				case DrawModes.Mode1:
					point1 = new Point (-(int)scale, (this.Height - (int)scale) / 2);	// Горизонтальная
					point2 = new Point (this.Width / 2 - (int)scale, -(int)scale);		// Вертикальная
					DrawingTimer.Tick += DrawingTimer_Mode1;
					foreBrush = new SolidBrush (this.ForeColor);
					break;

				case DrawModes.Mode2:
					point1 = new Point (this.Width / 2, this.Height / 2);
					point2 = new Point (this.Width / 2, this.Height / 2);
					point3 = new Point (this.Width / 2, this.Height / 2);
					point4 = new Point (this.Width / 2, this.Height / 2);
					arc1 = 0.0;
					DrawingTimer.Tick += DrawingTimer_Mode2;
					foreBrush = new SolidBrush (Color.FromArgb (1, this.ForeColor.R, this.ForeColor.G, this.ForeColor.B));
					break;
				}

			// Сброс настроек
			phase1 = phase2 = 1;

			// Запуск таймера
			DrawingTimer.Interval = MovingTimer.Interval = 1;
			PauseTimer.Interval = 2000;
			ExtendedTimer.Interval = 20;
			DrawingTimer.Enabled = true;
			}

		// Таймеры отрисовки
		private void DrawingTimer_Mode1 (object sender, System.EventArgs e)
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

			// Отрисовка и финальный контроль
			CheckTransition ();
			}

		private void DrawingTimer_Mode2 (object sender, System.EventArgs e)
			{
			// Определение следующей позиции
			switch (phase1)
				{
				// Пауза в центре
				case 1:
					arc1 += 1.0;

					if (arc1 >= 70.0)
						{
						arc1 = 0.0;
						foreBrush = new SolidBrush (this.ForeColor);
						phase1++;
						}

					break;

				// Во всех направлениях от центра
				case 2:
					arc1 += 1.0;

					point1.X = (this.Width + scale) / 2 + (int)(Cosinus (180.0 - arc1) * scale / 2.0);			// Нижняя правая
					point1.Y = this.Height / 2 + (int)(Sinus (180.0 - arc1) * scale / 2.0);
					point2.X = (this.Width + scale) / 2 + (int)(Cosinus (180.0 + arc1 * 2.0) * scale / 2.0);	// Верхняя правая
					point2.Y = this.Height / 2 + (int)(Sinus (180.0 + arc1 * 2.0) * scale / 2.0);

					point3.X = (this.Width - scale) / 2 + (int)(Cosinus (-arc1) * scale / 2.0);					// Верхняя левая
					point3.Y = this.Height / 2 + (int)(Sinus (-arc1) * scale / 2.0);
					point4.X = (this.Width - scale) / 2 + (int)(Cosinus (arc1 * 2.0) * scale / 2.0);			// Нижняя левая
					point4.Y = this.Height / 2 + (int)(Sinus (arc1 * 2.0) * scale / 2.0);

					if (arc1 >= 90.0)
						{
						phase1++;
						}
					break;

				// К краям экрана
				case 3:
					point1.X += frameSpeed;
					point2.Y += frameSpeed;
					point3.X -= frameSpeed;
					point4.Y -= frameSpeed;
					break;
				}

			// Отрисовка
			if (phase1 == 3)
				{
				g.FillRectangle (foreBrush, point1.X - drawerSize / 2, point1.Y - drawerSize / 2, drawerSize, drawerSize);
				g.FillRectangle (foreBrush, point2.X - drawerSize / 2, point2.Y - drawerSize / 2, drawerSize, drawerSize);
				g.FillRectangle (foreBrush, point3.X - drawerSize / 2, point3.Y - drawerSize / 2, drawerSize, drawerSize);
				g.FillRectangle (foreBrush, point4.X - drawerSize / 2, point4.Y - drawerSize / 2, drawerSize, drawerSize);
				}
			else
				{
				g.FillEllipse (foreBrush, point1.X - drawerSize / 2, point1.Y - drawerSize / 2, drawerSize, drawerSize);
				g.FillEllipse (foreBrush, point2.X - drawerSize / 2, point2.Y - drawerSize / 2, drawerSize, drawerSize);
				g.FillEllipse (foreBrush, point3.X - drawerSize / 2, point3.Y - drawerSize / 2, drawerSize, drawerSize);
				g.FillEllipse (foreBrush, point4.X - drawerSize / 2, point4.Y - drawerSize / 2, drawerSize, drawerSize);
				}

			// Отрисовка и финальный контроль
			CheckTransition ();
			}

		// Отрисовка изображения лого
		private void CheckTransition ()
			{
			// Остановка таймера по завершению, запуск следующей фазы
			if ((point1.X > this.Width + scale / 10) && (point2.Y > this.Height + scale / 10) ||
				(point3.X < -scale / 10) && (point4.Y < -scale / 10))
				{
				// Остановка
				DrawingTimer.Enabled = false;

				// "Фото" экрана
				Bitmap logo2 = new Bitmap (this.Width, this.Height);
				Graphics g2 = Graphics.FromImage (logo2);
				g2.CopyFromScreen (0, 0, 0, 0, new Size (this.Width, this.Height), CopyPixelOperation.SourceCopy);
				g2.Dispose ();

				// "Фото" лого
				logo = logo2.Clone (new Rectangle (this.Width / 2 - (scale + tailsSize), this.Height / 2 - (int)(scale / 2 + tailsSize),
					(scale + tailsSize) * 2, scale + tailsSize * 2), PixelFormat.Format32bppArgb);
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

		private void LogoDrawer_MouseMove (object sender, MouseEventArgs e)
			{
			moves++;

			if (moves > 2)
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

		// Таймер задержки лого на экране
		private void PauseTimer_Tick (object sender, EventArgs e)
			{
			// Остановка основного режима
			PauseTimer.Enabled = false;

			if (!extended)
				{
				// Выход
				this.Close ();
				return;
				}

			// Инициализация расширенного режима
			phase1 = 1;
			steps = 0;
			backBrush = new SolidBrush (Color.FromArgb (10, this.BackColor.R, this.BackColor.G, this.BackColor.B));

			headerFont = new Font ("Lucida Console", headerFontSize, FontStyle.Bold);
			textFont = new Font ("Lucida Console", textFontSize);
			uint headerLetterWidth = (uint)(g.MeasureString ("A", headerFont).Width * 0.8f);
			uint textLetterWidth = (uint)(g.MeasureString ("A", textFont).Width * 0.8f);

			// Текст расширенного режима
			extendedStrings.Add (new List<LogoDrawerString> ());
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"What does our logo mean?", headerFont, 50, headerLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"Some explanations from RD_AAOW (creator)", textFont, 80, textLetterWidth));

			extendedStrings.Add (new List<LogoDrawerString> ());
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- What does 'ESHQ' mean?", textFont, 50, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- How d'ya think?", textFont, 80, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- Oh, from arabic it means 'love'. May be...", textFont, 30, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- Hell no! Oh God! Have you ever seen Half-Life?   What kind of love do you assumed to see here?", textFont, 80, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- Some love to machines, I think. Technocracy      madness or somethin'", textFont, 80, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- ...I'm shocked. It's actually an accident. But   you're completely right", textFont, 50, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- OK, keep going", textFont, 100, textLetterWidth));

			extendedStrings.Add (new List<LogoDrawerString> ());
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"ESHQ:", headerFont, 20, headerLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"• Evil", headerFont, 20, headerLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"• Scientists", headerFont, 20, headerLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"• Head", headerFont, 20, headerLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"• Quarters", headerFont, 40, headerLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- It's a fake name for infrastructure facility     where our plot begins. Our project is about    " +
				"  tech. And about people's inability to use it     properly. We just tried to create something    " +
				"  that we haven't in reality. We got it, I think.  Surrealistic disasters, crushes, mad AI, etc...", textFont, 50, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"  But... Hell yeah! We're sharing 'love'! ♥♥♥", textFont, 100, textLetterWidth));

			extendedStrings.Add (new List<LogoDrawerString> ());
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- ...OK. And what about this thing?", textFont, 20, textLetterWidth));

			extendedStrings.Add (new List<LogoDrawerString> ());
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- Offers?", textFont, 80, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- An infinity sign, I think", textFont, 50, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- Yes, it is. But it's 'interrupted' infinity.     Like a life. It may be eternal. But it tends   " +
				"  to break off suddenly... What else?", textFont, 80, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- DNA, may be?", textFont, 50, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- Yes, of course. The code of life. And the        source code that we using just like a painter  " +
				"  uses his brush. It's context dependent", textFont, 80, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- Is Large Hadron Collider also here? Logo looks   like its rays dispersing from center", textFont, 50, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- It's obvious. LHC is about life origin too.      And about tech too", textFont, 100, textLetterWidth));

			extendedStrings.Add (new List<LogoDrawerString> ());
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- And how about crossing roads?", textFont, 50, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- Of course, it is. An intersection of ways and    fates is the life itself. It's ESHQ itself", textFont, 80, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- Did you invented new Ankh?", textFont, 50, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- No no no. Don't be so pretentious. It's just     possible descriptions. And we just need unique " +
				"  logo. Here it is", textFont, 80, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings[extendedStrings.Count - 1].Add (new LogoDrawerString (
				"- Well done!", textFont, 100, textLetterWidth));

			// Запуск
			ExtendedTimer.Enabled = true;
			}

		// Таймер расширенного режима отображения
		private void ExtendedTimer_Tick (object sender, EventArgs e)
			{
			switch (phase1)
				{
				// Начальное затенение
				case 1:
					steps++;
					g.FillRectangle (backBrush, 0, 0, this.Width, this.Height);

					if (steps > 65)
						{
						steps = 0;
						phase1++;
						}
					break;

				// Отрисовка начальных элементов
				case 2:
					if (steps++ == 0)
						g.DrawString ("1.", headerFont, foreBrush, 30, lineFeed);
					else if (steps > 20)
						{
						steps = 0;
						phase1++;
						}
					break;

				case 3:
				case 14:
					if (steps++ == 0)
						g.DrawString ("2.", headerFont, foreBrush, 30, lineFeed * 2);
					else if (steps > 20)
						{
						steps = 0;
						phase1++;
						}
					break;

				case 4:
				case 15:
					if (steps++ == 0)
						{
						g.DrawImage (new Bitmap (logo, new Size ((int)((double)logo.Width / ((double)logo.Height / 24)),
							(int)((double)logo.Height / ((double)logo.Height / 24)))), 110, lineFeed * 2);
						}
					else if (steps > 20)
						{
						steps = 0;
						phase1++;
						}
					break;

				case 5:
					if (steps++ == 0)
						g.DrawString ("ESHQ", headerFont, foreBrush, 80, lineFeed);
					else if (steps > 20)
						{
						steps = 0;
						phase1++;
						}
					break;

				case 6:
					if (steps++ == 0)
						g.FillRectangle (foreBrush, 220, this.Height * 0.05f, drawerSize / 2, this.Height * 0.9f);
					else if (steps > 20)
						{
						steps = 0;
						point1.X = lineLeft;
						point1.Y = lineFeed;
						phase1++;
						}
					break;

				// Отрисовка текста
				case 7:
				case 9:
				case 11:
				case 13:
				case 17:
				case 19:
					if (extendedStrings[0].Count == 0)
						{
						extendedStrings.RemoveAt (0);
						phase1++;
						break;
						}

					if (steps < extendedStrings[0][0].StringLength)
						{
						g.DrawString (extendedStrings[0][0].StringText.Substring ((int)steps++, 1), extendedStrings[0][0].StringFont,
							foreBrush, point1);

						point1.X += (int)extendedStrings[0][0].LetterSize;
						if (point1.X > 1024)
							{
							point1.X = lineLeft;
							point1.Y += lineFeed;
							}
						}
					else if (steps > extendedStrings[0][0].StringLength + extendedStrings[0][0].Pause)
						{
						extendedStrings[0].RemoveAt (0);
						steps = 0;
						point1.X = lineLeft;
						point1.Y += lineFeed;

						// Обработка смены экрана
						if (point1.Y > this.Height * 0.95)
							{
							point1.Y = lineFeed;
							phase2 = phase1;
							phase1 = 100;
							}
						}
					else
						{
						steps++;
						}

					break;

				// Интермиссии
				case 8:
					steps++;
					g.FillRectangle (backBrush, 0, 2 * lineFeed, 220, lineFeed);

					if (steps > 35)
						{
						steps = 0;
						phase1++;	// К отрисовке текста
						}
					break;

				case 16:
					steps++;
					g.FillRectangle (backBrush, 0, lineFeed, 220, lineFeed);

					if (steps > 35)
						{
						steps = 0;
						phase1++;	// К отрисовке текста
						}
					break;

				// Обновление экрана
				case 10:
				case 12:
				case 18:
					point1.Y = lineFeed;
					phase2 = phase1;
					phase2++;
					phase1 = 100;
					break;

				case 100:
					steps++;
					g.FillRectangle (backBrush, lineLeft, 0, this.Width - lineLeft, this.Height);

					if (steps > 80)
						{
						steps = 0;
						phase1 = phase2;	// К отрисовке текста
						}
					break;

				// Завершение
				case 20:
					backBrush = new SolidBrush (Color.FromArgb (50, backBrush.Color.R, backBrush.Color.G, backBrush.Color.B));
					phase1++;
					break;

				case 21:
					steps++;
					g.FillRectangle (backBrush, 0, 0, this.Width, this.Height);

					if (steps > 60)
						{
						ExtendedTimer.Enabled = extended = false;
						mode = DrawModes.Mode2;
						DrawingTimer.Tick -= DrawingTimer_Mode1;
						LogoDrawer_Load (null, null);
						}
					break;
				}
			}
		}

	/// <summary>
	/// Класс описывает строку, предназначенную для вывода на экран в расширенном режиме
	/// </summary>
	public class LogoDrawerString
		{
		/// <summary>
		/// Шрифт строки
		/// </summary>
		public Font StringFont
			{
			get
				{
				return stringFont;
				}
			}
		private Font stringFont;

		/// <summary>
		/// Текст строки
		/// </summary>
		public string StringText
			{
			get
				{
				return stringText;
				}
			}
		private string stringText;

		/// <summary>
		/// Пауза до перехода на следующую строку
		/// </summary>
		public uint Pause
			{
			get
				{
				return pause;
				}
			}
		private uint pause = 0;

		/// <summary>
		/// Ширина буквы строки текста
		/// </summary>
		public uint LetterSize
			{
			get
				{
				return letterSize;
				}
			}
		private uint letterSize = 0;

		/// <summary>
		/// Длина строки текста
		/// </summary>
		public uint StringLength
			{
			get
				{
				return stringLength;
				}
			}
		private uint stringLength = 0;

		/// <summary>
		/// Конструктор. Инициализиует объект-строку (предполагает моноширинный шрифт)
		/// </summary>
		/// <param name="Text">Текст строки</param>
		/// <param name="TextFont">Шрифт строки</param>
		/// <param name="TimeoutPause">Пауза до перехода к следующей строке</param>
		/// <param name="LetterWidth">Ширина отдельной буквы строки</param>
		public LogoDrawerString (string Text, Font TextFont, uint TimeoutPause, uint LetterWidth)
			{
			stringText = Text;
			if ((stringText == null) || (stringText == ""))
				stringText = " ";
			stringLength = (uint)stringText.Length;

			stringFont = TextFont;
			letterSize = LetterWidth;

			pause = TimeoutPause;
			}
		}
	}
