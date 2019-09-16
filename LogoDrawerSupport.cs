﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace ESHQSetupStub
	{
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
		/// Тип строки (дополнительное поле)
		/// </summary>
		public uint StringType
			{
			get
				{
				return stringType;
				}
			}
		private uint stringType = 0;

		/// <summary>
		/// Конструктор. Инициализиует объект-строку (предполагает моноширинный шрифт)
		/// </summary>
		/// <param name="Text">Текст строки</param>
		/// <param name="TextFont">Шрифт строки</param>
		/// <param name="TimeoutPause">Пауза до перехода к следующей строке</param>
		/// <param name="LetterWidth">Ширина отдельной буквы строки</param>
		public LogoDrawerString (string Text, Font TextFont, uint TimeoutPause, uint LetterWidth)
			{
			LogoDrawerStringInit (Text, TextFont, TimeoutPause, LetterWidth, 0);
			}

		/// <summary>
		/// Конструктор. Инициализиует объект-строку (предполагает моноширинный шрифт)
		/// </summary>
		/// <param name="Text">Текст строки</param>
		/// <param name="TextFont">Шрифт строки</param>
		/// <param name="TimeoutPause">Пауза до перехода к следующей строке</param>
		/// <param name="LetterWidth">Ширина отдельной буквы строки</param>
		/// <param name="Type">Тип строки (для поддержки дополнительных функций)</param>
		public LogoDrawerString (string Text, Font TextFont, uint TimeoutPause, uint LetterWidth, uint Type)
			{
			LogoDrawerStringInit (Text, TextFont, TimeoutPause, LetterWidth, Type);
			}

		private void LogoDrawerStringInit (string Text, Font TextFont, uint TimeoutPause, uint LetterWidth, uint Type)
			{
			stringText = Text;
			if ((stringText == null) || (stringText == ""))
				stringText = " ";
			stringLength = (uint)stringText.Length;

			stringFont = TextFont;
			letterSize = LetterWidth;
			pause = TimeoutPause;
			stringType = Type;
			}
		}

	/// <summary>
	/// Класс описывает визуальный объект 'сфера' для вывода на экран в расширенном режиме
	/// </summary>
	public class LogoDrawerSphere:IDisposable, ILogoDrawerObject
		{
		// Переменные и константы
		private Random rnd;				// ГПСЧ (обязательно извне)
		private int maxFluctuation;		// Максимальный дребезг скорости
		private int endX;				// Конечная позиция по горизонтали
		private int endY;				// Конечная позиция по вертикали
		private int speedX;				// Горизонтальная скорость
		private int speedY;				// Вертикальная скорость
		private int ρ;					// Радиус описанной окружности для объекта
		private SolidBrush objectBrush;	// Кисть для отрисовки объекта

		/// <summary>
		/// Статус инициализации объекта
		/// </summary>
		public bool IsInited
			{
			get
				{
				return isInited;
				}
			}
		private bool isInited = false;

		/// <summary>
		/// Координата X центра объекта
		/// </summary>
		public int X
			{
			get
				{
				return x;
				}
			}
		private int x;

		/// <summary>
		/// Координата Y центра объекта
		/// </summary>
		public int Y
			{
			get
				{
				return y;
				}
			}
		private int y;

		/// <summary>
		/// Изображение объекта
		/// </summary>
		public Bitmap Image
			{
			get
				{
				if (!isInited)
					return null;

				return image;
				}
			}
		private Bitmap image;

		/// <summary>
		/// Метод освобождает ресурсы, занятые объектом
		/// </summary>
		public void Dispose ()
			{
			if (image != null)
				image.Dispose ();
			if (objectBrush != null)
				objectBrush.Dispose ();
			isInited = false;
			}

		/// <summary>
		/// Метод создаёт визуальный объект
		/// </summary>
		/// <param name="ScreenWidth">Ширина экрана</param>
		/// <param name="ScreenHeight">Высота экрана</param>
		/// <param name="Randomizer">Внешний ГПСЧ</param>
		/// <param name="Metrics">Метрики генерации объекта</param>
		public LogoDrawerSphere (uint ScreenWidth, uint ScreenHeight, Random Randomizer, LogoDrawerObjectMetrics Metrics)
			{
			// Контроль
			LogoDrawerObjectMetrics metrics = LogoDrawerSupport.AlingMetrics (Metrics);

			// Получение изображения
			rnd = Randomizer;
			maxFluctuation = (int)metrics.MaxSpeedFluctuation;
			ρ = (metrics.MinSize == metrics.MaxSize) ? (int)metrics.MinSize : rnd.Next ((int)metrics.MinSize, (int)metrics.MaxSize);

			objectBrush = new SolidBrush (Color.FromArgb (10,
				(metrics.MinRed == metrics.MaxRed) ? metrics.MinRed : rnd.Next (metrics.MinRed, (int)metrics.MaxRed + 1),
				(metrics.MinGreen == metrics.MaxGreen) ? metrics.MinGreen : rnd.Next (metrics.MinGreen, (int)metrics.MaxGreen + 1),
				(metrics.MinBlue == metrics.MaxBlue) ? metrics.MinBlue : rnd.Next (metrics.MinBlue, (int)metrics.MaxBlue + 1)));

			// Получение координат
			switch (metrics.StartupPosition)
				{
				case LogoDrawerObjectStartupPositions.Left:
				case LogoDrawerObjectStartupPositions.LeftFlat:
				case LogoDrawerObjectStartupPositions.Right:
				case LogoDrawerObjectStartupPositions.RightFlat:
					speedX = (metrics.MinSpeed == metrics.MaxSpeed) ? (int)metrics.MinSpeed :
						rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
					speedY = 0;

					if (LogoDrawerSupport.IsLeft (metrics.StartupPosition))
						{
						x = -ρ;
						endX = (int)ScreenWidth + ρ;
						}
					else
						{
						x = (int)ScreenWidth + ρ;
						endX = -ρ;
						speedX *= -1;
						}

					if (!LogoDrawerSupport.IsFlat (metrics.StartupPosition))
						endY = y = rnd.Next ((int)ScreenHeight + ρ) - ρ / 2;
					else
						endY = y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));
					break;

				case LogoDrawerObjectStartupPositions.Top:
				case LogoDrawerObjectStartupPositions.Bottom:
					speedX = 0;
					speedY = (metrics.MinSpeed == metrics.MaxSpeed) ? (int)metrics.MinSpeed :
						rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

					if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.Top)
						{
						y = -ρ;
						endY = (int)ScreenHeight + ρ;
						}
					else
						{
						y = (int)ScreenHeight + ρ;
						endY = -ρ;
						speedY *= -1;
						}

					endX = x = rnd.Next ((int)ScreenWidth + ρ) - ρ / 2;
					break;

				case LogoDrawerObjectStartupPositions.CenterRandom:
				case LogoDrawerObjectStartupPositions.Random:
				case LogoDrawerObjectStartupPositions.CenterFlat:
				default:
					while ((speedX == 0) && (speedY == 0) ||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat) && (speedX == 0))
						{
						speedX = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat)
							speedY = -rnd.Next (LogoDrawerSupport.AccelerationBorder + 1);
						else
							speedY = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

						if (metrics.MaxSpeed == 0)
							break;
						}

					if (LogoDrawerSupport.IsCenter (metrics.StartupPosition))
						{
						x = (int)ScreenWidth / 2;
						if (LogoDrawerSupport.IsFlat (metrics.StartupPosition))
							y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));
						else
							y = (int)(ScreenHeight / 2);
						}
					else
						{
						x = rnd.Next ((int)ScreenWidth + ρ) - ρ / 2;
						y = rnd.Next ((int)ScreenHeight + ρ) - ρ / 2;
						}

					endX = (speedX > 0) ? ((int)ScreenWidth + ρ + speedX + maxFluctuation) :
						(-ρ + speedX - maxFluctuation);		// Фикс против преждевременного
					endY = (speedY > 0) ? ((int)ScreenHeight + ρ + speedY + maxFluctuation) :
						(-ρ + speedY - maxFluctuation);		// 'перепрыгивания' порога

					break;
				}

			// Успешно
			isInited = true;
			Move (false, 0);		// Генерация изображения
			}

		/// <summary>
		/// Метод выполняет смещение объекта
		/// </summary>
		/// <param name="Acceleration">Движение с ускорением</param>
		/// <param name="Enlarging">Увеличение (+)/уменьшение (-) при движении</param>
		public void Move (bool Acceleration, int Enlarging)
			{
			// Отсечка
			if (!isInited)
				return;

			// Смещение  с ускорением
			x += (speedX + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			if (Acceleration)
				{
				if (speedX > 0)
					speedX++;
				else if (speedX < 0)
					speedX--;
				}

			y += (speedY + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			if (Acceleration)
				{
				if (speedY > LogoDrawerSupport.AccelerationBorder)
					speedY++;
				else if (speedY < -LogoDrawerSupport.AccelerationBorder)
					speedY--;
				}

			if ((Enlarging > 0) || (Enlarging < 0) && (ρ > -Enlarging + 2))
				ρ += Enlarging;

			// Отрисовка
			if (image != null)
				image.Dispose ();

			if (ρ > 2)
				{
				image = new Bitmap (ρ, ρ);
				Graphics g = Graphics.FromImage (image);

				for (int i = ρ / 2; i >= 0; i -= 3)
					{
					g.FillEllipse (objectBrush, ρ / 2 - i, ρ / 2 - i, 2 * i, 2 * i);
					}
				g.Dispose ();
				}
			else
				image = new Bitmap (2, 2);

			// Контроль
			if ((speedX > 0) && (x > endX) ||
				(speedX < 0) && (x < endX) ||
				(speedY > 0) && (y > endY) ||
				(speedY < 0) && (y < endY))
				{
				isInited = false;
				}
			}
		}

	/// <summary>
	/// Структура описывает параметры генерации объектов
	/// </summary>
	public struct LogoDrawerObjectMetrics
		{
		/// <summary>
		/// Начальная позиция движения
		/// </summary>
		public LogoDrawerObjectStartupPositions StartupPosition;

		/// <summary>
		/// Минимальная скорость
		/// </summary>
		public uint MinSpeed;

		/// <summary>
		/// Максимальная скорость
		/// </summary>
		public uint MaxSpeed;

		/// <summary>
		/// Дребезг скорости при движении
		/// </summary>
		public uint MaxSpeedFluctuation;

		/// <summary>
		/// Минимальный размер
		/// </summary>
		public uint MinSize;

		/// <summary>
		/// Максимальный размер
		/// </summary>
		public uint MaxSize;

		/// <summary>
		/// Минимальное значение красного канала
		/// </summary>
		public byte MinRed;

		/// <summary>
		/// Максимальное значение красного канала
		/// </summary>
		public byte MaxRed;

		/// <summary>
		/// Минимальное значение зелёного канала
		/// </summary>
		public byte MinGreen;

		/// <summary>
		/// Максимальное значение зелёного канала
		/// </summary>
		public byte MaxGreen;

		/// <summary>
		/// Минимальное значение синего канала
		/// </summary>
		public byte MinBlue;

		/// <summary>
		/// Максимальное значение синего канала
		/// </summary>
		public byte MaxBlue;

		/// <summary>
		/// Тип объектов
		/// </summary>
		public LogoDrawerObjectTypes ObjectsType;

		/// <summary>
		/// Количество одновременно существующих объектов
		/// </summary>
		public byte ObjectsCount;

		/// <summary>
		/// Количество сторон многоугольников
		/// </summary>
		public byte PolygonsSidesCount;

		/// <summary>
		/// Флаг сохранения следов движения объектов
		/// </summary>
		public bool KeepTracks;

		/// <summary>
		/// Флаг преобразования многоугольников в звёзды
		/// </summary>
		public bool AsStars;

		/// <summary>
		/// Флаг вращения объектов
		/// </summary>
		public bool Rotation;

		/// <summary>
		/// Флаг ускорения движения объектов
		/// </summary>
		public bool Acceleration;

		/// <summary>
		/// Флаг увеличения / уменьшения объектов при движении
		/// </summary>
		public int Enlarging;
		}

	/// <summary>
	/// Возможные начальные позиции объектов
	/// </summary>
	public enum LogoDrawerObjectStartupPositions
		{
		/// <summary>
		/// Слева
		/// </summary>
		Left = 1,

		/// <summary>
		/// Справа
		/// </summary>
		Right = 2,

		/// <summary>
		/// Сверху
		/// </summary>
		Top = 3,

		/// <summary>
		/// Снизу
		/// </summary>
		Bottom = 4,

		/// <summary>
		/// От центра во все стороны
		/// </summary>
		CenterRandom = 5,

		/// <summary>
		/// От центра по горизонтали
		/// </summary>
		CenterFlat = 6,

		/// <summary>
		/// Слева по горизонтали
		/// </summary>
		LeftFlat = 7,

		/// <summary>
		/// Справа по горизонтали
		/// </summary>
		RightFlat = 8,

		/// <summary>
		/// Случайная
		/// </summary>
		Random = 0
		}

	/// <summary>
	/// Возможные типы генерируемых объектов
	/// </summary>
	public enum LogoDrawerObjectTypes
		{
		/// <summary>
		/// Сферы
		/// </summary>
		Spheres = 0,

		/// <summary>
		/// Многоугольники
		/// </summary>
		Polygons = 4,

		/// <summary>
		/// Звёзды
		/// </summary>
		Stars = 5,

		/// <summary>
		/// Картинки
		/// </summary>
		Pictures = 7,

		/// <summary>
		/// Буквы
		/// </summary>
		Letters = 8,

		/// <summary>
		/// Вращающиеся многоугольники
		/// </summary>
		RotatingPolygons = 1,

		/// <summary>
		/// Вращающиеся звёзды
		/// </summary>
		RotatingStars = 2,

		/// <summary>
		/// Вращающиеся картинки
		/// </summary>
		RotatingPictures = 6,

		/// <summary>
		/// Вращающиеся буквы
		/// </summary>
		RotatingLetters = 3,
		}

	/// <summary>
	/// Класс описывает отрисовочный слой
	/// </summary>
	public class LogoDrawerLayer:IDisposable
		{
		// Переменные
		private bool isInited = false;

		/// <summary>
		/// Возвращает изображение слоя
		/// </summary>
		public Bitmap Layer
			{
			get
				{
				return (isInited ? layer : null);
				}
			}
		private Bitmap layer;

		/// <summary>
		/// Возвращает дескриптор для изменения слоя
		/// </summary>
		public Graphics Descriptor
			{
			get
				{
				return (isInited ? descriptor : null);
				}
			}
		private Graphics descriptor;

		/// <summary>
		/// Метод освобождает занятые экземпляром ресурсы
		/// </summary>
		public void Dispose ()
			{
			if (isInited)
				{
				descriptor.Dispose ();
				layer.Dispose ();
				isInited = false;
				}
			}

		/// <summary>
		/// Левое смещение слоя
		/// </summary>
		public uint Left
			{
			get
				{
				return left;
				}
			}
		private uint left;

		/// <summary>
		/// Верхнее смещение слоя
		/// </summary>
		public uint Top
			{
			get
				{
				return top;
				}
			}
		private uint top;

		/// <summary>
		/// Конструктор. Создаёт новый отрисовочный слой
		/// </summary>
		/// <param name="Width">Ширина слоя</param>
		/// <param name="Height">Высота слоя</param>
		/// <param name="LeftOffset">Левое смещение</param>
		/// <param name="TopOffset">Верхнее смещение</param>
		public LogoDrawerLayer (uint LeftOffset, uint TopOffset, uint Width, uint Height)
			{
			left = LeftOffset;
			top = TopOffset;
			layer = new Bitmap ((Width == 0) ? 1 : (int)Width, (Height == 0) ? 1 : (int)Height);
			descriptor = Graphics.FromImage (layer);
			isInited = true;
			}
		}

	/// <summary>
	/// Класс описывает визуальный объект 'многоугольник' для вывода на экран в расширенном режиме
	/// </summary>
	public class LogoDrawerSquare:IDisposable, ILogoDrawerObject
		{
		// Переменные и константы
		private Random rnd;				// ГПСЧ (обязательно извне)
		private int maxFluctuation;		// Максимальный дребезг скорости
		private int speedX;				// Скорость горизонтального смещения
		private int speedY;				// Скорость вертикального смещения
		private int speedOfRotation;	// Скорость вращения
		private uint sidesCount;		// Количество сторон многоугольника
		private int endX;				// Конечная позиция по горизонтали
		private int endY;				// Конечная позиция по вертикали
		private bool star;				// Флаг преобразования многоугольника в звезду
		private int ρ;					// Радиус описанной окружности для объекта
		private int φ;					// Угол поворота объекта
		private SolidBrush objectBrush;	// Кисть для отрисовки объекта
		private bool rotation;			// Флаг вращения объекта

		/// <summary>
		/// Статус инициализации объекта
		/// </summary>
		public bool IsInited
			{
			get
				{
				return isInited;
				}
			}
		private bool isInited = false;

		/// <summary>
		/// Координата X центра объекта
		/// </summary>
		public int X
			{
			get
				{
				return x;
				}
			}
		private int x;

		/// <summary>
		/// Координата Y центра объекта
		/// </summary>
		public int Y
			{
			get
				{
				return y;
				}
			}
		private int y;

		/// <summary>
		/// Изображение объекта
		/// </summary>
		public Bitmap Image
			{
			get
				{
				return image;
				}
			}
		private Bitmap image;

		/// <summary>
		/// Метод освобождает ресурсы, занятые объектом
		/// </summary>
		public void Dispose ()
			{
			if (image != null)
				image.Dispose ();
			if (objectBrush != null)
				objectBrush.Dispose ();
			isInited = false;
			}

		/// <summary>
		/// Метод создаёт визуальный объект 'правильный многоугольник'
		/// </summary>
		/// <param name="ScreenWidth">Ширина экрана</param>
		/// <param name="ScreenHeight">Высота экрана</param>
		/// <param name="Randomizer">Внешний ГПСЧ</param>
		/// <param name="Metrics">Метрики генерации объекта</param>
		public LogoDrawerSquare (uint ScreenWidth, uint ScreenHeight, Random Randomizer, LogoDrawerObjectMetrics Metrics)
			{
			// Контроль
			LogoDrawerObjectMetrics metrics = LogoDrawerSupport.AlingMetrics (Metrics);

			// Генерация параметров изображения
			rnd = Randomizer;
			star = metrics.AsStars;
			rotation = metrics.Rotation;
			sidesCount = metrics.PolygonsSidesCount;
			ρ = (metrics.MinSize == metrics.MaxSize) ? (int)metrics.MinSize : rnd.Next ((int)metrics.MinSize, (int)metrics.MaxSize);

			if (rotation)
				{
				φ = rnd.Next (0, 360);
				speedOfRotation = (metrics.MinSpeed == metrics.MaxSpeed) ? (int)metrics.MinSpeed :
					rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
				speedOfRotation *= ((rnd.Next (2) == 0) ? -1 : 1);
				}
			else
				{
				φ = (360 * 3) / (metrics.PolygonsSidesCount * 4);	// Ориентация вверх
				speedOfRotation = 0;
				}

			maxFluctuation = (int)metrics.MaxSpeedFluctuation;

			objectBrush = new SolidBrush (Color.FromArgb (255,
				(metrics.MinRed == metrics.MaxRed) ? metrics.MinRed : rnd.Next (metrics.MinRed, (int)metrics.MaxRed + 1),
				(metrics.MinGreen == metrics.MaxGreen) ? metrics.MinGreen : rnd.Next (metrics.MinGreen, (int)metrics.MaxGreen + 1),
				(metrics.MinBlue == metrics.MaxBlue) ? metrics.MinBlue : rnd.Next (metrics.MinBlue, (int)metrics.MaxBlue + 1)));

			// Получение координат
			switch (metrics.StartupPosition)
				{
				case LogoDrawerObjectStartupPositions.Left:
				case LogoDrawerObjectStartupPositions.LeftFlat:
				case LogoDrawerObjectStartupPositions.Right:
				case LogoDrawerObjectStartupPositions.RightFlat:
					speedX = (metrics.MinSpeed == metrics.MaxSpeed) ? (int)metrics.MinSpeed :
						rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
					speedY = 0;

					if (LogoDrawerSupport.IsLeft (metrics.StartupPosition))
						{
						x = -2 * ρ;
						endX = (int)ScreenWidth + 2 * ρ;
						}
					else
						{
						x = (int)ScreenWidth + 2 * ρ;
						endX = -2 * ρ;
						speedX *= -1;
						}

					if (!LogoDrawerSupport.IsFlat (metrics.StartupPosition))
						endY = y = rnd.Next ((int)ScreenHeight + 2 * ρ) - ρ;
					else
						endY = y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));
					break;

				case LogoDrawerObjectStartupPositions.Top:
				case LogoDrawerObjectStartupPositions.Bottom:
					speedX = 0;
					speedY = (metrics.MinSpeed == metrics.MaxSpeed) ? (int)metrics.MinSpeed :
						rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

					if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.Top)
						{
						y = -2 * ρ;
						endY = (int)ScreenHeight + 2 * ρ;
						}
					else
						{
						y = (int)ScreenHeight + 2 * ρ;
						endY = -2 * ρ;
						speedY *= -1;
						}

					endX = x = rnd.Next ((int)ScreenWidth + 2 * ρ) - ρ;
					break;

				case LogoDrawerObjectStartupPositions.CenterRandom:
				case LogoDrawerObjectStartupPositions.Random:
				case LogoDrawerObjectStartupPositions.CenterFlat:
				default:
					while ((speedX == 0) && (speedY == 0) ||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat) && (speedX == 0))
						{
						speedX = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat)
							speedY = -rnd.Next (LogoDrawerSupport.AccelerationBorder + 1);
						else
							speedY = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

						if (metrics.MaxSpeed == 0)
							break;
						}

					if (LogoDrawerSupport.IsCenter (metrics.StartupPosition))
						{
						x = (int)ScreenWidth / 2;
						if (LogoDrawerSupport.IsFlat (metrics.StartupPosition))
							y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));
						else
							y = (int)(ScreenHeight / 2);
						}
					else
						{
						x = rnd.Next ((int)ScreenWidth + 2 * ρ) - ρ;
						y = rnd.Next ((int)ScreenHeight + 2 * ρ) - ρ;
						}

					endX = (speedX > 0) ? ((int)ScreenWidth + 8 * ρ + speedX + maxFluctuation) :
						(-8 * ρ + speedX - maxFluctuation);
					endY = (speedY > 0) ? ((int)ScreenHeight + 8 * ρ + speedY + maxFluctuation) :
						(-8 * ρ + speedY - maxFluctuation);
					break;
				}

			// Успешно
			isInited = true;
			Move (false, 0);			// Инициализация отрисовки
			}

		/// <summary>
		/// Метод выполняет смещение объекта
		/// </summary>
		/// <param name="Acceleration">Движение с ускорением</param>
		/// <param name="Enlarging">Увеличение (+)/уменьшение (-) при движении</param>
		public void Move (bool Acceleration, int Enlarging)
			{
			// Отсечка
			if (!isInited)
				return;

			// Смещение  с ускорением
			x += (speedX + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			if (Acceleration)
				{
				if (speedX > 0)
					speedX++;
				else if (speedX < 0)
					speedX--;
				}

			y += (speedY + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			if (Acceleration)
				{
				if (speedY > LogoDrawerSupport.AccelerationBorder)
					speedY++;
				else if (speedY < -LogoDrawerSupport.AccelerationBorder)
					speedY--;
				}

			if (rotation)
				{
				φ += speedOfRotation;
				while (φ < 0)
					φ += 360;
				while (φ > 359)
					φ -= 360;
				}

			if ((Enlarging > 0) || (Enlarging < 0) && (ρ > -Enlarging))
				ρ += Enlarging;

			// Сброс предыдущего изображения
			if (image != null)
				image.Dispose ();

			// Сборка фрейма
			List<Point> points = new List<Point> ();
			for (int i = 0; i < sidesCount; i++)
				{
				points.Add (new Point (ρ - (int)(ρ * LogoDrawerSupport.Cosinus ((double)φ +
					(double)i * 360.0 / (double)sidesCount)),
					ρ - (int)(ρ * LogoDrawerSupport.Sinus ((double)φ +
					(double)i * 360.0 / (double)sidesCount))));

				if (star)
					points.Add (new Point (ρ - (int)(ρ * 0.25 * LogoDrawerSupport.Cosinus ((double)φ +
						((double)i + 0.5) * 360.0 / (double)sidesCount)),
						ρ - (int)(ρ * 0.25 * LogoDrawerSupport.Sinus ((double)φ +
						((double)i + 0.5) * 360.0 / (double)sidesCount))));
				}

			Point[] res = points.ToArray ();
			points.Clear ();

			// Формирование изображения
			if (ρ == 0)
				image = new Bitmap (2, 2);
			else
				image = new Bitmap (2 * ρ, 2 * ρ);
			Graphics g = Graphics.FromImage (image);
			if (ρ != 0)
				g.FillPolygon (objectBrush, res);
			g.Dispose ();

			// Контроль
			if ((speedX > 0) && (x > endX) ||
				(speedX < 0) && (x < endX) ||
				(speedY > 0) && (y > endY) ||
				(speedY < 0) && (y < endY))
				{
				isInited = false;
				}
			}
		}

	/// <summary>
	/// Вспомогательный класс программы
	/// </summary>
	public static class LogoDrawerSupport
		{
		/// <summary>
		/// Граница разрешения/запрета ускорения
		/// </summary>
		public const int AccelerationBorder = 0;

		/// <summary>
		/// Доля поля текста
		/// </summary>
		public const double TextFieldPart = 3.0 / 8.0;				// Часть поля отрисовки, занимаемая текстом

		/// <summary>
		/// Метод приводит исходные метрики объекта к допустимым диапазонам
		/// </summary>
		/// <param name="OldMetrics">Исходные метрики</param>
		/// <returns>Приведённые метрики</returns>
		public static LogoDrawerObjectMetrics AlingMetrics (LogoDrawerObjectMetrics OldMetrics)
			{
			LogoDrawerObjectMetrics metrics;

			metrics.MinRed = (OldMetrics.MinRed > OldMetrics.MaxRed) ? OldMetrics.MaxRed : OldMetrics.MinRed;
			metrics.MaxRed = (OldMetrics.MinRed > OldMetrics.MaxRed) ? OldMetrics.MinRed : OldMetrics.MaxRed;
			metrics.MinGreen = (OldMetrics.MinGreen > OldMetrics.MaxGreen) ? OldMetrics.MaxGreen : OldMetrics.MinGreen;
			metrics.MaxGreen = (OldMetrics.MinGreen > OldMetrics.MaxGreen) ? OldMetrics.MinGreen : OldMetrics.MaxGreen;
			metrics.MinBlue = (OldMetrics.MinBlue > OldMetrics.MaxBlue) ? OldMetrics.MaxBlue : OldMetrics.MinBlue;
			metrics.MaxBlue = (OldMetrics.MinBlue > OldMetrics.MaxBlue) ? OldMetrics.MinBlue : OldMetrics.MaxBlue;

			metrics.MinSize = (OldMetrics.MinSize > OldMetrics.MaxSize) ? OldMetrics.MaxSize : OldMetrics.MinSize;
			metrics.MaxSize = (OldMetrics.MinSize > OldMetrics.MaxSize) ? OldMetrics.MinSize : OldMetrics.MaxSize;
			if (metrics.MinSize < MinObjectSize)
				metrics.MinSize = MinObjectSize;
			if (metrics.MinSize > MaxObjectSize)
				metrics.MinSize = MaxObjectSize;
			if (metrics.MaxSize < metrics.MinSize)
				metrics.MaxSize = metrics.MinSize;
			if (metrics.MaxSize > MaxObjectSize)
				metrics.MaxSize = MaxObjectSize;

			metrics.MinSpeed = (OldMetrics.MinSpeed > OldMetrics.MaxSpeed) ? OldMetrics.MaxSpeed : OldMetrics.MinSpeed;
			metrics.MaxSpeed = (OldMetrics.MinSpeed > OldMetrics.MaxSpeed) ? OldMetrics.MinSpeed : OldMetrics.MaxSpeed;
			if (metrics.MinSpeed < MinObjectSpeed)
				metrics.MinSpeed = MinObjectSpeed;
			if (metrics.MinSpeed > MaxObjectSpeed)
				metrics.MinSpeed = MaxObjectSpeed;
			if (metrics.MaxSpeed < metrics.MinSpeed)
				metrics.MaxSpeed = metrics.MinSpeed;
			if (metrics.MaxSpeed > MaxObjectSpeed)
				metrics.MaxSpeed = MaxObjectSpeed;

			metrics.MaxSpeedFluctuation = OldMetrics.MaxSpeedFluctuation;
			if (metrics.MaxSpeedFluctuation > MaxObjectSpeed)
				metrics.MaxSpeedFluctuation = MaxObjectSpeed;
			metrics.StartupPosition = OldMetrics.StartupPosition;

			metrics.ObjectsType = OldMetrics.ObjectsType;

			metrics.ObjectsCount = OldMetrics.ObjectsCount;
			if (metrics.ObjectsCount > MaxObjectsCount)
				metrics.ObjectsCount = (byte)MaxObjectsCount;

			metrics.PolygonsSidesCount = OldMetrics.PolygonsSidesCount;
			if (metrics.PolygonsSidesCount > MaxPolygonsSidesCount)
				metrics.PolygonsSidesCount = (byte)MaxPolygonsSidesCount;
			if (metrics.PolygonsSidesCount < MinPolygonsSidesCount)
				metrics.PolygonsSidesCount = (byte)MinPolygonsSidesCount;

			metrics.KeepTracks = OldMetrics.KeepTracks;
			metrics.AsStars = OldMetrics.AsStars;
			metrics.Rotation = OldMetrics.Rotation;
			metrics.Acceleration = OldMetrics.Acceleration;

			metrics.Enlarging = OldMetrics.Enlarging;
			if (metrics.Enlarging < -MaxEnlarge)
				metrics.Enlarging = -MaxEnlarge;
			if (metrics.Enlarging > MaxEnlarge)
				metrics.Enlarging = MaxEnlarge;

			return metrics;
			}

		/// <summary>
		/// Минимально допустимый размер шрифта
		/// </summary>
		public const uint MinFontSize = 10;

		/// <summary>
		/// Максимально допустимый размер шрифта
		/// </summary>
		public const uint MaxFontSize = 100;

		/// <summary>
		/// Максимально допустимое количество объектов
		/// </summary>
		public const uint MaxObjectsCount = 20;

		/// <summary>
		/// Минимально допустимое количество сторон многоугольников
		/// </summary>
		public const uint MinPolygonsSidesCount = 3;

		/// <summary>
		/// Максимально допустимое количество сторон многоугольников
		/// </summary>
		public const uint MaxPolygonsSidesCount = 16;

		/// <summary>
		/// Максимально допустимый коэффициент увеличения / уменьшения
		/// </summary>
		public const int MaxEnlarge = 10;

		/// <summary>
		/// Минимально допустимая скорость объекта
		/// </summary>
		public const uint MinObjectSpeed = 0;

		/// <summary>
		/// Максимально допустимая скорость объекта
		/// </summary>
		public const uint MaxObjectSpeed = 20;

		/// <summary>
		/// Минимально допустимый размер объекта
		/// </summary>
		public const uint MinObjectSize = 1;

		/// <summary>
		/// Максимально допустимый размер объекта
		/// </summary>
		public const uint MaxObjectSize = 400;

		/// <summary>
		/// Метод переводит градусы в радианы
		/// </summary>
		/// <param name="Φ">Градусная величина угла</param>
		/// <returns>Радианная величина угла</returns>
		public static double D2R (double Φ)
			{
			return Math.PI * Φ / 180.0;
			}

		/// <summary>
		/// Метод возвращает значение синуса угла, представленного в градусах
		/// </summary>
		/// <param name="ArcInDegrees">Градусная величина угла</param>
		/// <returns>Синус угла</returns>
		public static double Sinus (double ArcInDegrees)
			{
			return Math.Sin (D2R (ArcInDegrees));
			}

		/// <summary>
		/// Метод возвращает значение косинуса угла, представленного в градусах
		/// </summary>
		/// <param name="ArcInDegrees">Градусная величина угла</param>
		/// <returns>Косинус угла</returns>
		public static double Cosinus (double ArcInDegrees)
			{
			return Math.Cos (D2R (ArcInDegrees));
			}

		/// <summary>
		/// Метод возвращает true, если указанная позиция относится к разновидности Flat
		/// </summary>
		/// <param name="Position">Стартовая позиция объекта</param>
		/// <returns></returns>
		public static bool IsFlat (LogoDrawerObjectStartupPositions Position)
			{
			return (Position == LogoDrawerObjectStartupPositions.LeftFlat) ||
				(Position == LogoDrawerObjectStartupPositions.RightFlat) ||
				(Position == LogoDrawerObjectStartupPositions.CenterFlat);
			}

		/// <summary>
		/// Метод возвращает true, если указанная позиция относится к разновидности Left
		/// </summary>
		/// <param name="Position">Стартовая позиция объекта</param>
		/// <returns></returns>
		public static bool IsLeft (LogoDrawerObjectStartupPositions Position)
			{
			return (Position == LogoDrawerObjectStartupPositions.Left) ||
				(Position == LogoDrawerObjectStartupPositions.LeftFlat);
			}

		/// <summary>
		/// Метод возвращает true, если указанная позиция относится к разновидности Center
		/// </summary>
		/// <param name="Position">Стартовая позиция объекта</param>
		/// <returns></returns>
		public static bool IsCenter (LogoDrawerObjectStartupPositions Position)
			{
			return (Position == LogoDrawerObjectStartupPositions.CenterFlat) ||
				(Position == LogoDrawerObjectStartupPositions.CenterRandom);
			}
		}

	/// <summary>
	/// Класс описывает визуальный объект 'буква' для вывода на экран в расширенном режиме
	/// </summary>
	public class LogoDrawerLetter:IDisposable, ILogoDrawerObject
		{
		// Переменные и константы
		private Random rnd;				// ГПСЧ (обязательно извне)
		private int maxFluctuation;		// Максимальный дребезг скорости
		private int speedX;				// Скорость горизонтального смещения
		private int speedY;				// Скорость вертикального смещения
		private int speedOfRotation;	// Скорость вращения
		private int endX;				// Конечная позиция по горизонтали
		private int endY;				// Конечная позиция по вертикали
		private int φ;					// Угол поворота объекта

		/// <summary>
		/// Статус инициализации объекта
		/// </summary>
		public bool IsInited
			{
			get
				{
				return isInited;
				}
			}
		private bool isInited = false;

		/// <summary>
		/// Координата X центра объекта
		/// </summary>
		public int X
			{
			get
				{
				return x;
				}
			}
		private int x;

		/// <summary>
		/// Координата Y центра объекта
		/// </summary>
		public int Y
			{
			get
				{
				return y;
				}
			}
		private int y;

		/// <summary>
		/// Изображение объекта
		/// </summary>
		public Bitmap Image
			{
			get
				{
				return resultImage;
				}
			}
		private Bitmap resultImage, sourceImage;

		/// <summary>
		/// Метод освобождает ресурсы, занятые объектом
		/// </summary>
		public void Dispose ()
			{
			if (resultImage != null)
				resultImage.Dispose ();
			if (sourceImage != null)
				sourceImage.Dispose ();
			isInited = false;
			}

		/// <summary>
		/// Метод создаёт визуальный объект 'правильный многоугольник'
		/// </summary>
		/// <param name="ScreenWidth">Ширина экрана</param>
		/// <param name="ScreenHeight">Высота экрана</param>
		/// <param name="Randomizer">Внешний ГПСЧ</param>
		/// <param name="Metrics">Метрики генерации объекта</param>
		public LogoDrawerLetter (uint ScreenWidth, uint ScreenHeight, Random Randomizer, LogoDrawerObjectMetrics Metrics)
			{
			// Контроль
			LogoDrawerObjectMetrics metrics = LogoDrawerSupport.AlingMetrics (Metrics);

			// Генерация параметров изображения
			rnd = Randomizer;

			φ = rnd.Next (0, 360);
			if (metrics.Rotation)
				{
				speedOfRotation = (metrics.MinSpeed == metrics.MaxSpeed) ? (int)metrics.MinSpeed :
					rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
				speedOfRotation *= ((rnd.Next (2) == 0) ? -1 : 1);
				}
			else
				{
				speedOfRotation = 0;
				}
			maxFluctuation = (int)metrics.MaxSpeedFluctuation;

			// Генерация изображения
			SolidBrush sb = new SolidBrush (Color.FromArgb (rnd.Next (128, 256),
				(metrics.MinRed == metrics.MaxRed) ? metrics.MinRed : rnd.Next (metrics.MinRed, (int)metrics.MaxRed + 1),
				(metrics.MinGreen == metrics.MaxGreen) ? metrics.MinGreen : rnd.Next (metrics.MinGreen, (int)metrics.MaxGreen + 1),
				(metrics.MinBlue == metrics.MaxBlue) ? metrics.MinBlue : rnd.Next (metrics.MinBlue, (int)metrics.MaxBlue + 1)));
			int size = (metrics.MinSize == metrics.MaxSize) ? (int)metrics.MinSize :
				rnd.Next ((int)metrics.MinSize, (int)metrics.MaxSize + 1);

			sourceImage = new Bitmap (size * 2, size * 2);
			Graphics g = Graphics.FromImage (sourceImage);

			Font f = new Font ("Arial Black", size, FontStyle.Bold);
			string s = Encoding.GetEncoding (1251).GetString (new byte[] { (byte)rnd.Next (192, 192 + 32) });
			SizeF sz = g.MeasureString (s, f);

			g.DrawString (s, f, sb, (sourceImage.Width - sz.Width) / 2, (sourceImage.Height - sz.Height) / 2);

			g.Dispose ();
			f.Dispose ();
			sb.Dispose ();

			// Получение координат
			switch (metrics.StartupPosition)
				{
				case LogoDrawerObjectStartupPositions.Left:
				case LogoDrawerObjectStartupPositions.LeftFlat:
				case LogoDrawerObjectStartupPositions.Right:
				case LogoDrawerObjectStartupPositions.RightFlat:
					speedX = (metrics.MinSpeed == metrics.MaxSpeed) ? (int)metrics.MinSpeed :
						rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
					speedY = 0;

					if (LogoDrawerSupport.IsLeft (metrics.StartupPosition))
						{
						x = -sourceImage.Width;
						endX = (int)ScreenWidth + sourceImage.Width;
						}
					else
						{
						x = (int)ScreenWidth + sourceImage.Width;
						endX = -sourceImage.Width;
						speedX *= -1;
						}

					if (!LogoDrawerSupport.IsFlat (metrics.StartupPosition))
						endY = y = rnd.Next ((int)ScreenHeight + sourceImage.Height) - sourceImage.Height / 2;
					else
						endY = y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));
					break;

				case LogoDrawerObjectStartupPositions.Top:
				case LogoDrawerObjectStartupPositions.Bottom:
					speedX = 0;
					speedY = (metrics.MinSpeed == metrics.MaxSpeed) ? (int)metrics.MinSpeed :
						rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

					if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.Top)
						{
						y = -sourceImage.Height;
						endY = (int)ScreenHeight + sourceImage.Height;
						}
					else
						{
						y = (int)ScreenHeight + sourceImage.Height;
						endY = -sourceImage.Height;
						speedY *= -1;
						}

					endX = x = rnd.Next ((int)ScreenWidth + sourceImage.Width) - sourceImage.Width / 2;
					break;

				case LogoDrawerObjectStartupPositions.CenterRandom:
				case LogoDrawerObjectStartupPositions.Random:
				case LogoDrawerObjectStartupPositions.CenterFlat:
				default:
					while ((speedX == 0) && (speedY == 0) ||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat) && (speedX == 0))
						{
						speedX = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat)
							speedY = -rnd.Next (LogoDrawerSupport.AccelerationBorder + 1);
						else
							speedY = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

						if (metrics.MaxSpeed == 0)
							break;
						}

					if (LogoDrawerSupport.IsCenter (metrics.StartupPosition))
						{
						x = (int)ScreenWidth / 2;
						if (LogoDrawerSupport.IsFlat (metrics.StartupPosition))
							y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));
						else
							y = (int)(ScreenHeight / 2);
						}
					else
						{
						x = rnd.Next ((int)ScreenWidth + sourceImage.Width) - sourceImage.Width / 2;
						y = rnd.Next ((int)ScreenHeight + sourceImage.Height) - sourceImage.Height / 2;
						}

					endX = (speedX > 0) ? ((int)ScreenWidth + sourceImage.Width + speedX + maxFluctuation) :
						(-sourceImage.Width + speedX - maxFluctuation);
					endY = (speedY > 0) ? ((int)ScreenHeight + sourceImage.Height + speedY + maxFluctuation) :
						(-sourceImage.Height + speedY - maxFluctuation);

					break;
				}

			// Успешно
			isInited = true;
			Move (false, 0);			// Инициализация отрисовки
			}

		/// <summary>
		/// Метод выполняет смещение объекта
		/// </summary>
		/// <param name="Acceleration">Движение с ускорением</param>
		/// <param name="Enlarging">Увеличение (+)/уменьшение (-) при движении (неактивен)</param>
		public void Move (bool Acceleration, int Enlarging)
			{
			// Отсечка
			if (!isInited)
				return;

			// Смещение  с ускорением
			x += (speedX + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			if (Acceleration)
				{
				if (speedX > 0)
					speedX++;
				else if (speedX < 0)
					speedX--;
				}

			y += (speedY + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			if (Acceleration)
				{
				if (speedY > LogoDrawerSupport.AccelerationBorder)
					speedY++;
				else if (speedY < -LogoDrawerSupport.AccelerationBorder)
					speedY--;
				}

			φ += speedOfRotation;
			while (φ < 0)
				φ += 360;
			while (φ > 359)
				φ -= 360;

			// Перерисовка
			if (resultImage != null)
				resultImage.Dispose ();

			resultImage = new Bitmap (sourceImage.Width, sourceImage.Height);
			Graphics g = Graphics.FromImage (resultImage);

			g.TranslateTransform (sourceImage.Width / 2, sourceImage.Height / 2);			// Центровка поворота
			g.RotateTransform (φ);
			g.DrawImage (sourceImage, -sourceImage.Width / 2, -sourceImage.Height / 2);
			g.Dispose ();

			// Контроль
			if ((speedX > 0) && (x > endX) ||
				(speedX < 0) && (x < endX) ||
				(speedY > 0) && (y > endY) ||
				(speedY < 0) && (y < endY))
				{
				isInited = false;
				}
			}
		}

	/// <summary>
	/// Интерфейс описывает визуальный объект
	/// </summary>
	public interface ILogoDrawerObject
		{
		/// <summary>
		/// Метод освобождает ресурсы, занятые объектом
		/// </summary>
		void Dispose ();

		/// <summary>
		/// Изображение объекта
		/// </summary>
		Bitmap Image
			{
			get;
			}

		/// <summary>
		/// Статус инициализации объекта
		/// </summary>
		bool IsInited
			{
			get;
			}

		/// <summary>
		/// Метод выполняет смещение объекта
		/// </summary>
		/// <param name="Acceleration">Движение с ускорением</param>
		/// <param name="Enlarging">Увеличение (+)/уменьшение (-) при движении</param>
		void Move (bool Acceleration, int Enlarging);

		/// <summary>
		/// Координата X центра объекта
		/// </summary>
		int X
			{
			get;
			}

		/// <summary>
		/// Координата Y центра объекта
		/// </summary>
		int Y
			{
			get;
			}
		}

	/// <summary>
	/// Класс описывает визуальный объект 'картинка' для вывода на экран в расширенном режиме
	/// </summary>
	public class LogoDrawerPicture:IDisposable, ILogoDrawerObject
		{
		// Переменные и константы
		private Random rnd;				// ГПСЧ (обязательно извне)
		private int maxFluctuation;		// Максимальный дребезг скорости
		private int speedX;				// Скорость горизонтального смещения
		private int speedY;				// Скорость вертикального смещения
		private int speedOfRotation;	// Скорость вращения
		private int endX;				// Конечная позиция по горизонтали
		private int endY;				// Конечная позиция по вертикали
		private int φ;					// Угол поворота объекта

		/// <summary>
		/// Статус инициализации объекта
		/// </summary>
		public bool IsInited
			{
			get
				{
				return isInited;
				}
			}
		private bool isInited = false;

		/// <summary>
		/// Координата X центра объекта
		/// </summary>
		public int X
			{
			get
				{
				return x;
				}
			}
		private int x;

		/// <summary>
		/// Координата Y центра объекта
		/// </summary>
		public int Y
			{
			get
				{
				return y;
				}
			}
		private int y;

		/// <summary>
		/// Изображение объекта
		/// </summary>
		public Bitmap Image
			{
			get
				{
				return resultImage;
				}
			}
		private Bitmap resultImage, sourceImage;

		/// <summary>
		/// Метод освобождает ресурсы, занятые объектом
		/// </summary>
		public void Dispose ()
			{
			if (resultImage != null)
				resultImage.Dispose ();
			if (sourceImage != null)
				sourceImage.Dispose ();
			isInited = false;
			}

		/// <summary>
		/// Метод создаёт визуальный объект 'картинка' на основе случайно выбранного изображения
		/// </summary>
		/// <param name="ScreenWidth">Ширина экрана</param>
		/// <param name="ScreenHeight">Высота экрана</param>
		/// <param name="Randomizer">Внешний ГПСЧ</param>
		/// <param name="Metrics">Метрики генерации объекта</param>
		/// <param name="PicturesPath">Директория с изображениями</param>
		public LogoDrawerPicture (uint ScreenWidth, uint ScreenHeight, Random Randomizer, LogoDrawerObjectMetrics Metrics,
			string PicturesPath)
			{
			// Контроль
			LogoDrawerObjectMetrics metrics = LogoDrawerSupport.AlingMetrics (Metrics);

			// Генерация параметров изображения
			rnd = Randomizer;

			if (metrics.Rotation)
				{
				speedOfRotation = (metrics.MinSpeed == metrics.MaxSpeed) ? (int)metrics.MinSpeed :
					rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
				speedOfRotation *= ((rnd.Next (2) == 0) ? -1 : 1);
				φ = rnd.Next (0, 360);
				}
			else
				{
				speedOfRotation = 0;
				φ = 0;
				}

			maxFluctuation = (int)metrics.MaxSpeedFluctuation;

			// Получение изображения
			try
				{
				string[] files = Directory.GetFiles (PicturesPath);
				Bitmap b = (Bitmap)Bitmap.FromFile (files[(files.Length > 1) ? rnd.Next (files.Length) : 0]);
				int size = rnd.Next ((int)metrics.MinSize, (int)metrics.MaxSize);
				Bitmap b2 = new Bitmap (b, (int)((double)size * (double)b.Width / (double)b.Height), size);

				sourceImage = new Bitmap (2 * b2.Width, 2 * b2.Height);
				Graphics g = Graphics.FromImage (sourceImage);

				g.DrawImage (b2, (sourceImage.Width - b2.Width) / 2, (sourceImage.Height - b2.Height) / 2);

				g.Dispose ();
				b.Dispose ();
				b2.Dispose ();
				}
			catch
				{
				sourceImage = new Bitmap (2, 2);
				}

			// Получение координат
			switch (metrics.StartupPosition)
				{
				case LogoDrawerObjectStartupPositions.Left:
				case LogoDrawerObjectStartupPositions.LeftFlat:
				case LogoDrawerObjectStartupPositions.Right:
				case LogoDrawerObjectStartupPositions.RightFlat:
					speedX = (metrics.MinSpeed == metrics.MaxSpeed) ? (int)metrics.MinSpeed :
						rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
					speedY = 0;

					if (LogoDrawerSupport.IsLeft (metrics.StartupPosition))
						{
						x = -sourceImage.Width;
						endX = (int)ScreenWidth + sourceImage.Width;
						}
					else
						{
						x = (int)ScreenWidth + sourceImage.Width;
						endX = -sourceImage.Width;
						speedX *= -1;
						}

					if (!LogoDrawerSupport.IsFlat (metrics.StartupPosition))
						endY = y = rnd.Next ((int)ScreenHeight + sourceImage.Height) - sourceImage.Height / 2;
					else
						endY = y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));
					break;

				case LogoDrawerObjectStartupPositions.Top:
				case LogoDrawerObjectStartupPositions.Bottom:
					speedX = 0;
					speedY = (metrics.MinSpeed == metrics.MaxSpeed) ? (int)metrics.MinSpeed :
						rnd.Next ((int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

					if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.Top)
						{
						y = -sourceImage.Height;
						endY = (int)ScreenHeight + sourceImage.Height;
						}
					else
						{
						y = (int)ScreenHeight + sourceImage.Height;
						endY = -sourceImage.Height;
						speedY *= -1;
						}

					endX = x = rnd.Next ((int)ScreenWidth + sourceImage.Width) - sourceImage.Width / 2;
					break;

				case LogoDrawerObjectStartupPositions.CenterRandom:
				case LogoDrawerObjectStartupPositions.Random:
				case LogoDrawerObjectStartupPositions.CenterFlat:
				default:
					while ((speedX == 0) && (speedY == 0) ||
						(metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat) && (speedX == 0))
						{
						speedX = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);
						if (metrics.StartupPosition == LogoDrawerObjectStartupPositions.CenterFlat)
							speedY = -rnd.Next (LogoDrawerSupport.AccelerationBorder + 1);
						else
							speedY = rnd.Next (-(int)metrics.MinSpeed, (int)metrics.MaxSpeed + 1);

						if (metrics.MaxSpeed == 0)
							break;
						}

					if (LogoDrawerSupport.IsCenter (metrics.StartupPosition))
						{
						x = (int)ScreenWidth / 2;
						if (LogoDrawerSupport.IsFlat (metrics.StartupPosition))
							y = (int)(ScreenHeight * (1.0 - LogoDrawerSupport.TextFieldPart));
						else
							y = (int)(ScreenHeight / 2);
						}
					else
						{
						x = rnd.Next ((int)ScreenWidth + sourceImage.Width) - sourceImage.Width / 2;
						y = rnd.Next ((int)ScreenHeight + sourceImage.Height) - sourceImage.Height / 2;
						}

					endX = (speedX > 0) ? ((int)ScreenWidth + sourceImage.Width + speedX + maxFluctuation) :
						(-sourceImage.Width + speedX - maxFluctuation);
					endY = (speedY > 0) ? ((int)ScreenHeight + sourceImage.Height + speedY + maxFluctuation) :
						(-sourceImage.Height + speedY - maxFluctuation);

					break;
				}

			// Успешно
			isInited = true;
			Move (false, 0);			// Инициализация отрисовки
			}

		/// <summary>
		/// Метод выполняет смещение объекта
		/// </summary>
		/// <param name="Acceleration">Движение с ускорением</param>
		/// <param name="Enlarging">Увеличение (+)/уменьшение (-) при движении (неактивен)</param>
		public void Move (bool Acceleration, int Enlarging)
			{
			// Отсечка
			if (!isInited)
				return;

			// Смещение  с ускорением
			x += (speedX + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			if (Acceleration)
				{
				if (speedX > 0)
					speedX++;
				else if (speedX < 0)
					speedX--;
				}

			y += (speedY + rnd.Next (-maxFluctuation, maxFluctuation + 1));
			if (Acceleration)
				{
				if (speedY > LogoDrawerSupport.AccelerationBorder)
					speedY++;
				else if (speedY < -LogoDrawerSupport.AccelerationBorder)
					speedY--;
				}

			φ += speedOfRotation;
			while (φ < 0)
				φ += 360;
			while (φ > 359)
				φ -= 360;

			// Перерисовка
			if (resultImage != null)
				resultImage.Dispose ();

			resultImage = new Bitmap (sourceImage.Width, sourceImage.Height);
			Graphics g = Graphics.FromImage (resultImage);

			g.TranslateTransform (sourceImage.Width / 2, sourceImage.Height / 2);			// Центровка поворота
			g.RotateTransform (φ);
			/*if ((Enlarging > 0) || (Enlarging < 0) && (sourceImage.Width > -Enlarging) && (sourceImage.Height > -Enlarging))
				g.DrawImage (sourceImage, -sourceImage.Width / 2, -sourceImage.Height / 2, sourceImage.Width + Enlarging ,
					sourceImage.Height + (int)((double)sourceImage.Height * (double)Enlarging / (double)sourceImage.Width));
			else*/
			g.DrawImage (sourceImage, -sourceImage.Width / 2, -sourceImage.Height / 2);

			g.Dispose ();

			// Контроль
			if ((speedX > 0) && (x > endX) ||
				(speedX < 0) && (x < endX) ||
				(speedY > 0) && (y > endY) ||
				(speedY < 0) && (y < endY))
				{
				isInited = false;
				}
			}
		}
	}
