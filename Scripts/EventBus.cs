using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Com.Frozenbullets.Libraries.Eventbus
{
	public class IEvent
	{
		public object Payload;
	}

	public interface IObserver
	{
		void OnEvent(IEvent pEvent);
	}

	public class EventBus
	{
		private static readonly EventBus m_UiBus = new EventBus();
		private static readonly EventBus m_Bus = new EventBus();
		private static readonly EventBus m_NetworkBus = new EventBus();
		private static readonly Dictionary<string, EventBus> m_BusDict = new Dictionary<string, EventBus>();
        
        private string i_Identifier;
        
        public string Identifier
        {
            get { return i_Identifier; }
            set { i_Identifier = value; } 
        }

		private EventBus()
		{
			m_observers = new Dictionary<Type, List<IObserver>>();
		}

		private Dictionary<Type, List<IObserver>> m_observers;

		/// <summary>
		/// Registers <paramref name="pObserver"/> for event(s) <paramref name="pEventsObserved"/> so it will receive those events whenever they are raised.
		/// Good practice for MonoBehaviours: Register in OnEnable, Unregister in OnDisable.
		/// </summary>
		/// <exception cref="System.NotImplementedException">Thrown when <paramref name="pObserver"/> doesnot implement <see cref="Core.Bus.IObserver"/>.</exception>
		/// <exception cref="System.NotImplementedException">Thrown when <paramref name="pEventsObserved"/> doesnot implement <see cref="Core.Bus.IEvent"/>.</exception>
		/// <param name="pObserver">The object that will receive events.</param>
		/// <param name="pEventsObserved">The event types that <paramref name="pObserver"/> will receive.</param>
		public void Register(object pObserver, params Type[] pEventsObserved)
		{
			if (pObserver is IObserver)
			{
				foreach (Type _eventType in pEventsObserved)
				{
					if (typeof(IEvent).IsAssignableFrom(_eventType))
					{
						if (!m_observers.ContainsKey(_eventType))
							m_observers.Add(_eventType, new List<IObserver>());

						m_observers[_eventType].Add(pObserver as IObserver);
					}
					else
					{
						throw new System.NotImplementedException(string.Concat(_eventType, " doesn't implement IEvent"));
					}
				}

				return;
			}

			throw new System.NotImplementedException(string.Concat(pObserver, " doesn't implement IObserver"));
		}

		/// <summary>
		/// Unregisters <paramref name="pObserver"/> for event(s) <paramref name="pEventsObserved"/> so it will not receive these events anymore.
		/// Good practice for MonoBehaviours: Register in OnEnable, Unregister in OnDisable.
		/// </summary>
		/// <exception cref="System.NotImplementedException">Thrown when <paramref name="pObserver"/> doesnot implement <see cref="Core.Bus.IObserver"/>.</exception>
		/// <param name="pObserver">The object that will receive events.</param>
		/// <param name="pEventsObserved">The event types that <paramref name="pObserver"/> will receive.</param>
		public void Unregister(object pObserver, params Type[] pEventsObserved)
		{
			if (pObserver is IObserver)
			{
				foreach (Type _eventType in pEventsObserved)
				{
					if (m_observers.ContainsKey(_eventType))
					{
						m_observers[_eventType].Remove(pObserver as IObserver);
					}
				}

				return;
			}

			throw new System.NotImplementedException(string.Concat(pObserver, " doesn't implement IObserver"));
		}

		/// <summary>
		/// Raises the event <paramref name="pEvent"/> to every object that registered for.
		/// </summary>
		/// <param name="pEvent">The event being raised.</param>
		public void Send(IEvent pEvent)
		{
            string _data = JsonUtility.ToJson(pEvent);
            if (string.IsNullOrEmpty(_data))
                _data = "[PARSE ERROR]";
            
			if (m_observers.ContainsKey(pEvent.GetType()))
			{
				List<IObserver> _list = new List<IObserver>(m_observers[pEvent.GetType()]);

				foreach (IObserver observer in _list)
				{
					observer.OnEvent(pEvent);
				}

				_list = null;
			}
		}

		/// <summary>
		/// Get the event bus with the name <paramref name="pBusName"/>.
		/// If eventBus doesnot exist already, this will automatically create it.
		/// </summary>
		/// <param name="pBusName">Name of the eventbus, used as a dictionary key.</param>
		public static EventBus Get(string pBusName)
		{
			if (m_BusDict.ContainsKey(pBusName))
                return m_BusDict[pBusName];

			EventBus _eventBus = new EventBus();
            _eventBus.Identifier = pBusName;

			m_BusDict.Add(pBusName, _eventBus);

			return _eventBus;
		}

		/// <summary>
		/// Get the event bus created for UI calls
		/// </summary>
		public static EventBus GetUiBus()
		{
			return m_UiBus;
		}

		/// <summary>
		/// Get the event bus created for Network calls
		/// </summary>
		public static EventBus GetNetworkBus()
		{
			return m_NetworkBus;
		}

		/// <summary>
		/// Get the event bus created for Generic calls
		/// </summary>
		public static EventBus GetBus()
		{
			return m_Bus;
		}
	}

}

