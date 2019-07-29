using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;

// Классы
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

		private int logo2Height;								// Высота второго лого
		private Point[] logo2Form;								// Форма стрелки второго лого
		private SolidBrush logo2Green, logo2Grey;

		private uint phase1 = 1, phase2 = 1;	// Текущие фазы отрисовки
		private Point point1, point2,			// Текущие позиции отрисовки
			point3, point4;
		private double arc1, arc2;				// Переменные для расчёта позиций элементов в полярных координатах

		private Graphics g, g2;					// Объекты-отрисовщики
		private SolidBrush foreBrush, backBrush, backHidingBrush1, backHidingBrush2;
		private Pen backPen;
		private Bitmap logo1, logo2a, logo2b;
		private Bitmap logo2GreyPart, logo2GreenPart, logo2BackPart;
		private Font logo1Font, headerFont, textFont;
		private SizeF logo1Size;				// Графические размеры текста для текущего экрана

		private uint extended = 0;				// Тип расширенного режима

		private uint steps = 0,					// Счётчик шагов
			moves = 0;							// Счётчик движений мыши (используется для корректной обработки движений)

		private const int lineFeed = 40;		// Высота строки текста расширенного режима
		private const int lineLeft = 250;		// Начало строки текста расширенного режима

		private List<List<LogoDrawerString>> extendedStrings1 = new List<List<LogoDrawerString>> ();	// Строки текста расширенного режима
		private List<List<LogoDrawerString>> extendedStrings2 = new List<List<LogoDrawerString>> ();
		private List<List<LogoDrawerString>> extendedStrings3 = new List<List<LogoDrawerString>> ();

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
#if LDDEBUG
			extended = 3;
#else
			extended = ((rnd.Next (10) == 0) ? (uint)rnd.Next (1, 4) : 0);
#endif

			if (extended == 0)
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
			backHidingBrush1 = new SolidBrush (Color.FromArgb (10, this.BackColor.R, this.BackColor.G, this.BackColor.B));
			backHidingBrush2 = new SolidBrush (Color.FromArgb (50, this.BackColor.R, this.BackColor.G, this.BackColor.B));

			g = Graphics.FromHwnd (this.Handle);
			g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;	// Убирает ауру на буквах в Win8

			logo1Font = new Font ("Lucida Sans Unicode", logoFontSize);
			logo1Size = g.MeasureString (logoString1, logo1Font);

			/*logo2Font = new Font ("Hair ‱", logoFontSize);
			if (logo2Font.Name != "Hair ‱")
				extended = 0;*/
			logo2Height = (int)(this.Height / 4.5);

			if (extended == 2)
				{
				// Создание формы стрелки
				logo2Form = new Point[] {
					new Point (0, 0),
					new Point (0, 6 * logo2Height / 7),
					new Point (logo2Height / 7, logo2Height),
					new Point (2 * logo2Height / 7, 6 * logo2Height / 7),
					new Point (2 * logo2Height / 7, 0),
					new Point (logo2Height / 7, logo2Height / 7)
					};

				// Формирование стрелок
				logo2Green = new SolidBrush (Color.FromArgb (0, 160, 80));
				logo2Grey = new SolidBrush (Color.FromArgb (160, 160, 160));

				logo2BackPart = new Bitmap (2 * logo2Height / 7, logo2Height);
				g2 = Graphics.FromImage (logo2BackPart);
				g2.FillRectangle (new SolidBrush (Color.FromArgb (0, 0, 0, 0)), 0, 0, logo2BackPart.Width, logo2BackPart.Height);
				g2.FillPolygon (backBrush, logo2Form);
				g2.Dispose ();

				logo2GreyPart = new Bitmap (logo2BackPart);
				g2 = Graphics.FromImage (logo2GreyPart);
				g2.FillPolygon (logo2Grey, logo2Form);
				g2.Dispose ();

				logo2GreenPart = new Bitmap (logo2BackPart);
				g2 = Graphics.FromImage (logo2GreenPart);
				g2.FillPolygon (logo2Green, logo2Form);
				g2.Dispose ();

				logo2a = new Bitmap (2 * logo2Height / 7, this.Height);
				logo2b = new Bitmap (2 * logo2Height, this.Height);
				}

			headerFont = new Font ("Lucida Console", headerFontSize, FontStyle.Bold);
			textFont = new Font ("Lucida Console", textFontSize);

			// Установка начальных позиций и методов отрисовки
			switch (mode)
				{
				default:
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

			// Текст расширенного режима, вариант 1
			uint headerLetterWidth = (uint)(g.MeasureString ("A", headerFont).Width * 0.8f);
			uint textLetterWidth = (uint)(g.MeasureString ("A", textFont).Width * 0.8f);

			extendedStrings1.Add (new List<LogoDrawerString> ());
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"What does our logo mean?", headerFont, 50, headerLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"Some explanations from RD_AAOW (creator)", textFont, 80, textLetterWidth));

			extendedStrings1.Add (new List<LogoDrawerString> ());
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- What does 'ESHQ' mean?", textFont, 50, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- How d'ya think?", textFont, 80, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- Oh, from arabic it means 'love'. May be...", textFont, 30, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- Hell no! Oh God! Have you ever seen Half-Life?   What kind of love do you assumed to see here?", textFont, 80, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- Some love to machines, I think. Technocracy      madness or somethin'", textFont, 80, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- ...I'm shocked. It's actually an accident. But   you're completely right", textFont, 50, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- OK, keep going", textFont, 100, textLetterWidth));

			extendedStrings1.Add (new List<LogoDrawerString> ());
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"ESHQ:", headerFont, 20, headerLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"• Evil", headerFont, 20, headerLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"• Scientists", headerFont, 20, headerLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"• Head", headerFont, 20, headerLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"• Quarters", headerFont, 40, headerLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- It's a fake name for infrastructure facility     where our plot begins. Our project is about    " +
				"  tech. And about people's inability to use it     properly. We just tried to create something    " +
				"  that we haven't in reality. We got it, I think.  Surrealistic disasters, crushes, mad AI, etc...", textFont, 50, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"  But... Hell yeah! We're sharing 'love'! ♥♥♥", textFont, 100, textLetterWidth));

			extendedStrings1.Add (new List<LogoDrawerString> ());
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- ...OK. And what about this thing?", textFont, 20, textLetterWidth));

			extendedStrings1.Add (new List<LogoDrawerString> ());
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- Offers?", textFont, 80, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- An infinity sign, I think", textFont, 50, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- Yes, it is. But it's 'interrupted' infinity.     Like a life. It may be eternal. But it tends   " +
				"  to break off suddenly... What else?", textFont, 80, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- DNA, may be?", textFont, 50, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- Yes, of course. The code of life. And the        source code that we using just like a painter  " +
				"  uses his brush. It's context dependent", textFont, 80, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- Is Large Hadron Collider also here? Logo looks   like its rays dispersing from center", textFont, 50, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- It's obvious. LHC is about life origin too.      And about tech too", textFont, 100, textLetterWidth));

			extendedStrings1.Add (new List<LogoDrawerString> ());
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- And how about crossing roads?", textFont, 50, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- Of course, it is. An intersection of ways and    fates is the life itself. It's ESHQ itself", textFont, 80, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- Did you invented new Ankh?", textFont, 50, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- No no no. Don't be so pretentious. It's just     possible descriptions. And we just need unique " +
				"  logo. Here it is", textFont, 80, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings1[extendedStrings1.Count - 1].Add (new LogoDrawerString (
				"- Well done!", textFont, 100, textLetterWidth));

			// Текст расширенного режима, вариант 2
			extendedStrings2.Add (new List<LogoDrawerString> ());
			extendedStrings2[extendedStrings2.Count - 1].Add (new LogoDrawerString (
				"What does our logo mean?", headerFont, 50, headerLetterWidth));
			extendedStrings2[extendedStrings2.Count - 1].Add (new LogoDrawerString (
				"Some explanations from RD_AAOW (creator)", textFont, 80, textLetterWidth));

			extendedStrings2[extendedStrings2.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings2[extendedStrings2.Count - 1].Add (new LogoDrawerString (
				"   It's simple. We called our installation       assembly 'Deployment packages'. After that we    " +
				"took 'd' and 'p' and merge them into single      arrow-like logo.", textFont, 70, textLetterWidth));
			extendedStrings2[extendedStrings2.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings2[extendedStrings2.Count - 1].Add (new LogoDrawerString (
				"   ...Although, to be completely honest: we took 'd' and 'p' and after that we called our assembly" +
				"'DP'. We just like the way they merge. And the   right words was easy to find.", textFont, 70, textLetterWidth));
			extendedStrings2[extendedStrings2.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings2[extendedStrings2.Count - 1].Add (new LogoDrawerString (
				"   So it goes", textFont, 100, textLetterWidth));

			// Текст расширенного режима, вариант 3
			extendedStrings3.Add (new List<LogoDrawerString> ());
			extendedStrings3[extendedStrings3.Count - 1].Add (new LogoDrawerString (
				"What does our logo mean?", headerFont, 50, headerLetterWidth));
			extendedStrings3[extendedStrings3.Count - 1].Add (new LogoDrawerString (
				"Some explanations from RD_AAOW (creator)", textFont, 80, textLetterWidth));

			extendedStrings3[extendedStrings3.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings3[extendedStrings3.Count - 1].Add (new LogoDrawerString (
				"   This is the oldest one. When it was born its  text (abbreviation) was a name for pre- pre- pre-" +
				"prototype of SampiHQ from our mod. Proto got new life with new name. And I have a nick.", textFont, 80, textLetterWidth));
			extendedStrings3[extendedStrings3.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings3[extendedStrings3.Count - 1].Add (new LogoDrawerString (
				"   Emerald green stone-like shield also comes    from there. It's was an integrated symbol of     " +
				"stability, strength, protection and power.       In general, it stays so.", textFont, 80, textLetterWidth));
			extendedStrings3[extendedStrings3.Count - 1].Add (new LogoDrawerString (" ", textFont, 0, textLetterWidth));
			extendedStrings3[extendedStrings3.Count - 1].Add (new LogoDrawerString (
				"   As a result, it's simple and unique. Just     what I need ☺", textFont, 120, textLetterWidth));

			// Сброс настроек
			phase1 = phase2 = 1;

			// Запуск таймера
			DrawingTimer.Interval = MovingTimer.Interval = 1;
			PauseTimer.Interval = 2000;
			ExtendedTimer.Interval = 20;
			DrawingTimer.Enabled = true;
			}

		// Таймеры отрисовки

		// Основное лого, вариант 1
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

		// Основное лого, вариант 2
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

		// Завершение отрисовки основного лого, фиксация изображения
		private void CheckTransition ()
			{
			// Остановка таймера по завершению, запуск следующей фазы
			if ((point1.X > this.Width + scale / 10) && (point2.Y > this.Height + scale / 10) ||
				(point3.X < -scale / 10) && (point4.Y < -scale / 10))
				{
				// Остановка
				DrawingTimer.Enabled = false;

				// "Фото" экрана
				Bitmap logo1tmp = new Bitmap (this.Width, this.Height);
				g2 = Graphics.FromImage (logo1tmp);
				g2.CopyFromScreen (0, 0, 0, 0, new Size (this.Width, this.Height), CopyPixelOperation.SourceCopy);
				g2.Dispose ();

				// "Фото" лого
				logo1 = logo1tmp.Clone (new Rectangle (this.Width / 2 - (scale + tailsSize), this.Height / 2 - (int)(scale / 2 + tailsSize),
					(scale + tailsSize) * 2, scale + tailsSize * 2), PixelFormat.Format32bppArgb);
				logo1tmp.Dispose ();

				// В рамочке
				g2 = Graphics.FromImage (logo1);
				g2.DrawRectangle (backPen, 0, 0, logo1.Width, logo1.Height);
				g2.Dispose ();

				// Подготовка следующего таймера
				arc1 = 180.0;

				// Продолжение
				MovingTimer.Enabled = true;
				}
			}

		// Смещение лого и отрисовка подписи
		private void MovingTimer_Tick (object sender, EventArgs e)
			{
			arc1 -= 2.0;

			if (arc1 >= 0.0)
				{
				// Расходящаяся от центра рамка, стирающая лишние линии
				g.DrawRectangle (backPen,
					(int)((this.Width / 2 - (scale + tailsSize)) * ((1 + Cosinus (arc1 + 180.0)) / 2.0)),
					(int)((this.Height / 2 - (int)(scale / 2 + tailsSize)) * ((1 + Cosinus (arc1 + 180.0)) / 2.0)),
					(int)(logo1.Width + (this.Width - logo1.Width) * ((1 + Cosinus (arc1)) / 2.0)),
					(int)(logo1.Height + (this.Height - logo1.Height) * ((1 + Cosinus (arc1)) / 2.0)));

				// Смещающееся влево лого
				g.DrawImage (logo1, (this.Width - logo1.Width) / 2 - (int)(((1 + Cosinus (arc1)) / 2.0) * (this.Width - logo1.Width) / 4),
					(int)((this.Height - (scale + 2 * tailsSize)) / 2));
				}
			else if (arc1 >= -90.0)
				{
				// Отображение текста
				g.DrawString (logoString1.Substring (0, (int)(logoString1.Length * Sinus (-arc1))),
					logo1Font, foreBrush, this.Width - (this.Width - logo1.Width) / 4 - logo1Size.Width,
					this.Height / 2 - logo1Size.Height * 0.7f);
				g.DrawString (logoString2.Substring (0, (int)(logoString2.Length * Sinus (-arc1))),
					logo1Font, foreBrush, this.Width - (this.Width - logo1.Width) / 4 - logo1Size.Width,
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
			// Остановка основного режима
			PauseTimer.Enabled = false;

			if (extended == 0)
				{
				// Выход
				this.Close ();
				return;
				}

			// Инициализация расширенного режима
			phase1 = 1;
			steps = 0;
			switch (extended)
				{
				default:
				case 1:
					ExtendedTimer.Tick += ExtendedTimer1_Tick;
					break;

				case 2:
					ExtendedTimer.Tick += ExtendedTimer2_Tick;
					break;

				case 3:
					ExtendedTimer.Tick += ExtendedTimer3_Tick;
					break;
				}

			// Запуск
			ExtendedTimer.Enabled = true;
			}

		// Таймер расширенного режима отображения, вариант 1
		private void ExtendedTimer1_Tick (object sender, EventArgs e)
			{
			switch (phase1)
				{
				// Начальное затенение
				case 1:
					HideScreen (false);
					break;

				// Отрисовка начальных элементов
				case 2:
					DrawMarker (1);
					break;

				case 3:
				case 14:
					DrawMarker (2);
					break;

				case 4:
				case 15:
					if (steps++ == 0)
						{
						g.DrawImage (new Bitmap (logo1, new Size ((int)((double)logo1.Width / ((double)logo1.Height / 24)),
							(int)((double)logo1.Height / ((double)logo1.Height / 24)))), 110, lineFeed * 2);
						}
					else if (steps > 20)
						{
						steps = 0;
						phase1++;
						}
					break;

				case 5:
					if (steps++ == 0)
						g.DrawString (logoString1, headerFont, foreBrush, 80, lineFeed);
					else if (steps > 20)
						{
						steps = 0;
						point1.X = lineLeft;
						point1.Y = lineFeed;
						phase1++;
						}
					break;

				case 6:
					DrawSplitter ();
					break;

				// Отрисовка текста
				case 7:
				case 9:
				case 11:
				case 13:
				case 17:
				case 19:
					DrawText (extendedStrings1);
					break;

				// Интермиссии
				case 8:
					HideMarker (2);
					break;

				case 16:
					HideMarker (1);
					break;

				// Обновление экрана
				case 10:
				case 12:
				case 18:
					point1.Y = lineFeed;
					phase1 = 100;
					break;

				case 100:
					HideText ();
					break;

				// Завершение
				case 20:
					HideScreen (true);
					break;

				case 21:
					ExtendedTimer.Enabled = false;
					extended = 0;
					mode = DrawModes.Mode2;
					DrawingTimer.Tick -= DrawingTimer_Mode1;
					LogoDrawer_Load (null, null);
					break;
				}
			}

		// Таймер расширенного режима отображения, вариант 2
		private void ExtendedTimer2_Tick (object sender, EventArgs e)
			{
			switch (phase1)
				{
				// Начальное затенение
				case 1:
					HideScreen (true);
					break;

				// Запуск стрелок
				case 2:
					g2 = Graphics.FromImage (logo2a);
					phase1++;
					break;

				case 3:
					steps += 6;

					for (int i = -10; i < -2; i++)
						{
						g2.DrawImage ((i % 2 == 0) ? logo2GreyPart : logo2GreenPart, 0,
							this.Height / 2 + i * 6 * logo2Height / 7 + steps);
						}
					g2.DrawImage (logo2BackPart, 0, this.Height / 2 - 3 * 6 * logo2Height / 7);
					g2.DrawImage (logo2BackPart, 0, this.Height / 2 + 2 * 6 * logo2Height / 7);

					g.DrawImage (logo2a, this.Width / 2 - logo2Height / 7, 0);

					if (steps >= 6 * logo2Height)
						{
						g2.Dispose ();
						g2 = Graphics.FromImage (logo2b);
						steps = 0;
						phase1++;
						}
					break;

				// Запуск круга
				case 4:
					steps += 2;

					g2.FillPie (logo2Grey, 2 * logo2Height / 7, this.Height / 2 - (6 * logo2Height / 7),
						12 * logo2Height / 7, 12 * logo2Height / 7,
						270, (int)(180 * Sinus (steps)));
					g2.FillEllipse (backBrush, 4 * logo2Height / 7, this.Height / 2 - (4 * logo2Height / 7),
						8 * logo2Height / 7, 8 * logo2Height / 7);
					g2.FillPie (logo2Green, 0, this.Height / 2 - (6 * logo2Height / 7),
						12 * logo2Height / 7, 12 * logo2Height / 7,
						90, (int)(180 * Sinus (steps)));
					g2.FillEllipse (backBrush, 2 * logo2Height / 7, this.Height / 2 - (4 * logo2Height / 7),
						8 * logo2Height / 7, 8 * logo2Height / 7);
					g2.DrawImage (logo2a, 6 * logo2Height / 7, 0);

					g.DrawImage (logo2b, this.Width / 2 - logo2Height, 0);

					if (steps > 90)
						{
						g2.Dispose ();
						steps = 0;
						phase1++;
						}
					break;

				case 5:
					if (steps++ > 100)
						{
						steps = 0;
						phase1++;
						}
					break;

				// Затемнение
				case 6:
					HideScreen (false);
					break;

				// Отрисовка начальных элементов
				case 7:
					DrawMarker (1);
					break;

				case 8:
					if (steps++ == 0)
						g.DrawString ("DP", headerFont, foreBrush, 80, lineFeed);
					else if (steps > 20)
						{
						steps = 0;
						point1.X = lineLeft;
						point1.Y = lineFeed;
						phase1++;
						}
					break;

				case 9:
					DrawSplitter ();
					break;

				// Отрисовка текста
				case 10:
					DrawText (extendedStrings2);
					break;

				// Завершение
				case 11:
					HideScreen (true);
					break;

				case 12:
					ExtendedTimer.Enabled = false;
					extended = 0;
					mode = DrawModes.Mode2;
					DrawingTimer.Tick -= DrawingTimer_Mode1;
					LogoDrawer_Load (null, null);
					break;
				}
			}

		// Таймер расширенного режима отображения, вариант 2
		private void ExtendedTimer3_Tick (object sender, EventArgs e)
			{
			switch (phase1)
				{
				// Начальное затенение
				case 1:
					HideScreen (true);
					break;

				// Отрисовка лого
				case 2:
					if (steps++ == 0)
						{
						Bitmap b = new Bitmap (ESHQSetupStub.Properties.Resources.RDAAOW,
							ESHQSetupStub.Properties.Resources.RDAAOW.Width / 2, ESHQSetupStub.Properties.Resources.RDAAOW.Height / 2);
						g.DrawImage (b, (this.Width - b.Width) / 2, (this.Height - b.Height) / 2);
						}
					else if (steps >= 120)
						{
						steps = 0;
						phase1++;
						}
					break;

				// Затемнение
				case 3:
					HideScreen (false);
					break;

				// Отрисовка начальных элементов
				case 4:
					if (steps++ == 0)
						g.DrawString ("RD AAOW", headerFont, foreBrush, 30, lineFeed);
					else if (steps > 20)
						{
						steps = 0;
						point1.X = lineLeft;
						point1.Y = lineFeed;
						phase1++;
						}
					break;

				case 5:
					DrawSplitter ();
					break;

				// Отрисовка текста
				case 6:
					DrawText (extendedStrings3);
					break;

				// Завершение
				case 7:
					HideScreen (true);
					break;

				case 8:
					ExtendedTimer.Enabled = false;
					extended = 0;
					mode = DrawModes.Mode2;
					DrawingTimer.Tick -= DrawingTimer_Mode1;
					LogoDrawer_Load (null, null);
					break;
				}
			}

		// Обслуживающий функционал

		// Закрытие окна
		private void LogoDrawer_FormClosing (object sender, FormClosingEventArgs e)
			{
			// Остановка всех отрисовок
			DrawingTimer.Enabled = MovingTimer.Enabled = PauseTimer.Enabled = ExtendedTimer.Enabled = false;

			// Сброс всех ресурсов
			foreBrush.Dispose ();
			backBrush.Dispose ();
			backHidingBrush1.Dispose ();
			backHidingBrush2.Dispose ();
			backPen.Dispose ();
			logo1Font.Dispose ();
			//logo2Font.Dispose ();
			headerFont.Dispose ();
			textFont.Dispose ();
			g.Dispose ();

			if (extended == 2)
				{
				logo2Green.Dispose ();
				logo2Grey.Dispose ();
				logo2BackPart.Dispose ();
				logo2GreenPart.Dispose ();
				logo2GreyPart.Dispose ();
				}

			if (logo1 != null)
				logo1.Dispose ();
			if (logo2a != null)
				logo2a.Dispose ();
			if (logo2b != null)
				logo2b.Dispose ();
			}

		// Принудительный выход (по любой клавише)
		private void LogoDrawer_KeyDown (object sender, KeyEventArgs e)
			{
#if !LDDEBUG
			this.Close ();
#endif
			}

		private void LogoDrawer_MouseMove (object sender, MouseEventArgs e)
			{
#if !LDDEBUG
			moves++;

			if (moves > 2)
				this.Close ();
#endif
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

		// Метод рисует числовой маркер
		private void DrawMarker (uint Number)
			{
			if (steps++ == 0)
				g.DrawString (Number.ToString () + ".", headerFont, foreBrush, 30, lineFeed * Number);
			else if (steps > 20)
				{
				steps = 0;
				phase1++;
				}
			}

		// Метод рисует вертикальный разделитель
		private void DrawSplitter ()
			{
			steps++;
			g.FillRectangle (foreBrush, 220, this.Height * 0.05f, drawerSize / 2, this.Height * 0.9f * (float)Sinus (steps * 4));

			if (steps > 45)
				{
				steps = 0;
				phase1++;
				}
			}

		// Метод отрисовывает текст
		private void DrawText (List<List<LogoDrawerString>> StringsSet)
			{
			// Последняя строка кончилась
			if (StringsSet[0].Count == 0)
				{
				StringsSet.RemoveAt (0);
				phase1++;
				phase2 = phase1 + 1;
				return;
				}

			// Движение по строке
			if (steps < StringsSet[0][0].StringLength)
				{
				// Одна буква
				g.DrawString (StringsSet[0][0].StringText.Substring ((int)steps++, 1), StringsSet[0][0].StringFont,
					foreBrush, point1);

				// Смещение "каретки"
				point1.X += (int)StringsSet[0][0].LetterSize;

				// Конец строки, перевод "каретки"
				if (point1.X > 1024)
					{
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
				}

			// Кончился текст строки и задержка отображения
			else if (steps > StringsSet[0][0].StringLength + StringsSet[0][0].Pause)
				{
				// Переход к следующей текстовой строке
				StringsSet[0].RemoveAt (0);
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

			// Кончился только текст строки, пауза
			else
				{
				steps++;
				}
			}

		// Метод затеняет указанный маркер
		private void HideMarker (uint Number)
			{
			steps++;
			g.FillRectangle (backHidingBrush1, 0, Number * lineFeed, 220, lineFeed);

			if (steps > 35)
				{
				steps = 0;
				phase1++;	// К отрисовке текста
				}
			}

		// Метод затеняет текстовое поле
		private void HideText ()
			{
			steps++;
			g.FillRectangle (backHidingBrush1, lineLeft, 0, this.Width - lineLeft, this.Height);

			if (steps > 80)
				{
				steps = 0;
				phase1 = phase2;	// К отрисовке текста
				}
			}

		// Метод затеняет экран
		private void HideScreen (bool Full)
			{
			steps++;
			g.FillRectangle (Full ? backHidingBrush2 : backHidingBrush1, 0, 0, this.Width, this.Height);

			if (steps > 65)
				{
				steps = 0;
				phase1++;
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
