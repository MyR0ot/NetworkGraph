using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Сетевой_график
{
	public class Work
	{
		private static double e = 5.0;          // производительность труда

		public int i { get; }                   // начальное событие
		public int j { get; }                   // конечное событие

		public int d { get; }                    // объем работы
		public int r { get; private set; }      // кол-во человек

		public double T { get; private set; }   // время работы

		public double Tp { get; set; }      // Ранний возможный срок начала работы
		public double Tn { get; set; }      // Поздний допустимый срок начала работы
		public double Tpo { get; set; }     // Ранний возможный срок окончания работы
		public double Tno { get; set; }     // Поздний допустимый срок окончания работы
		public double Pn { get; set; }      // Полный резерв времени
		public double Pc { get; set; }      // Свободный резерв времени

		public bool IsExist { get; }			// Существует работа?


		/// <summary>
		/// Конструктор создания работы
		/// </summary>
		/// <param name="i">Номер события до работы</param>
		/// <param name="j">Номер события после работы</param>
		/// <param name="d">Кол-во едениц работы</param>
		/// <param name="r">Кол-во человек</param>
		public Work(int i, int j, int d, int r, bool exist = true)
		{
			this.d = d;
			this.r = r;
			this.T = (double)d / (r * e);
			this.i = i;
			this.j = j;
			IsExist = exist;
		}

		public Work(bool exist)
		{
			IsExist = exist;
		}

		/// <summary>
		/// Добавить людей на выполнение данной работы
		/// </summary>
		/// <param name="n">Кол-во людей</param>
		public void AddPeople(int n)
		{
			this.r += n;
			T = (double)d / (r * e);
		}


		/// <summary>
		/// Убрать людей с текущей работы
		/// </summary>
		/// <param name="n">Кол-во людей</param>
		public void RemovePeople(int n)
		{
			this.r -= n;
			if (this.r < 0)
				throw new Exception("Недопустимое кол-во человек!");
			this.T = (double)d / (r * e);
		}

	}
}
