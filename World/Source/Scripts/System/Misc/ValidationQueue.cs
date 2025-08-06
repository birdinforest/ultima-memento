using System.Collections.Generic;

namespace Server
{
	public interface IValidate
	{
		void Validate();
	}

	public delegate void ValidationEventHandler();

	public static class ValidationQueue
	{
		public static event ValidationEventHandler StartValidation;

		public static void Initialize()
		{
			if ( StartValidation != null )
				StartValidation();

			StartValidation = null;
		}

		public static void Add<T>(T obj) where T : IValidate
		{
			ValidationQueue<T>.Add(obj);
		}
	}

	public static class ValidationQueue<T> where T : IValidate
	{
		private static List<T> m_Queue;

		static ValidationQueue()
		{
			m_Queue = new List<T>();
			ValidationQueue.StartValidation += new ValidationEventHandler( ValidateAll );
		}

		public static void Add( T obj )
		{
			m_Queue.Add( obj );
		}

		private static void ValidateAll()
		{
			if (m_Queue == null) return;

			for ( int i = 0; i < m_Queue.Count; i++ )
			{
				var item = m_Queue[i];
				item.Validate();
			}

			m_Queue.Clear();
			m_Queue = null;
		}
	}
}